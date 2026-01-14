using System;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using M59AdminTool.Protocol;
using static M59AdminTool.Services.DebugLogger;

namespace M59AdminTool.Services
{
    /// <summary>
    /// TCP-Verbindung zum Meridian 59 Server mit vollständigem Protokoll-Support
    /// </summary>
    public class M59ServerConnection : IDisposable
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private bool _isConnected;
        private readonly string _host;
        private readonly int _port;
        private readonly PIEncryption _piEncryption;
        private bool _isAdminMode;
        private string _currentUsername = string.Empty;
        private readonly StringBuilder _adminTextBuffer;

        // Queue für eingehende Server-Antworten
        public ConcurrentQueue<string> ResponseQueue { get; } = new();

        // Event wenn eine Antwort empfangen wurde
        public event EventHandler<string>? ResponseReceived;

        public bool IsConnected => _isConnected && _client?.Connected == true;

        public M59ServerConnection(string host = "127.0.0.1", int port = 5959)
        {
            _host = host;
            _port = port;
            _piEncryption = new PIEncryption();
            _isAdminMode = false;
            _currentUsername = string.Empty;
            _adminTextBuffer = new StringBuilder();
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                Log($"[M59ServerConnection] Connecting to {_host}:{_port}...");

                _client = new TcpClient();
                await _client.ConnectAsync(_host, _port);
                _stream = _client.GetStream();
                _isConnected = true;

                Log("[M59ServerConnection] ✓ Connected successfully!");

                // Enable PI encryption after connection
                _piEncryption.Enable();

                // Start listening for responses in background
                _ = ListenForResponsesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Log($"[M59ServerConnection] ✗ Connection failed: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        public async Task<bool> ConnectAndLoginAsync(string username, string password, string secretKey)
        {
            try
            {
                _currentUsername = username;
                Log($"[M59ServerConnection] Connecting to {_host}:{_port}...");

                _client = new TcpClient();
                await _client.ConnectAsync(_host, _port);
                _stream = _client.GetStream();

                Log("[M59ServerConnection] ✓ TCP Connected!");
                _isAdminMode = false;
                _piEncryption.Reset();
                _adminTextBuffer.Clear();

                // Wait for GetLoginMessage from server with timeout
                Log("[M59ServerConnection] Waiting for GetLoginMessage...");

                _client.ReceiveTimeout = 10000; // 10 second timeout
                byte[] buffer = new byte[8192];

                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                Log($"[M59ServerConnection] ReadAsync completed, bytes: {bytesRead}");

                if (bytesRead > 0)
                {
                    Log($"[M59ServerConnection] Received {bytesRead} bytes from server");

                    // Parse header
                    if (bytesRead >= 7)
                    {
                        ushort len1 = (ushort)(buffer[0] | (buffer[1] << 8));
                        ushort crc = (ushort)(buffer[2] | (buffer[3] << 8));
                        ushort len2 = (ushort)(buffer[4] | (buffer[5] << 8));
                        byte ss = buffer[6];

                        Log($"[M59ServerConnection] Header: LEN1={len1}, CRC={crc}, LEN2={len2}, SS={ss}");

                        // Validate header
                        if (len1 == len2 && bytesRead >= 7 + len1)
                        {
                            // Extract body
                            byte[] body = new byte[len1];
                            Array.Copy(buffer, 7, body, 0, len1);

                // Get PI byte (first byte of body) - NOT ENCRYPTED in LoginMode!
                byte messageType = body[0];
                Log($"[M59ServerConnection] Message Type: {messageType} (expecting GetLogin = 21)");

                            if (messageType == (byte)MessageTypeLoginMode.GetLogin)
                            {
                                Log("[M59ServerConnection] ✓ Received GetLoginMessage!");

                                // Send LoginMessage
                                Log($"[M59ServerConnection] Sending LoginMessage for user: {username}");
                                var loginMsg = new LoginMessage(username, password, secretKey);
                                byte[] loginData = loginMsg.BuildMessage();

                                // Debug: Show first 50 bytes of message
                                string hexDump = BitConverter.ToString(loginData, 0, Math.Min(50, loginData.Length));
                                Log($"[M59ServerConnection] LoginMessage HEX (first 50 bytes): {hexDump}");

                                await _stream.WriteAsync(loginData, 0, loginData.Length);
                                await _stream.FlushAsync();
                                Log($"[M59ServerConnection] ✓ LoginMessage sent ({loginData.Length} bytes)");

                                // Wait for response (LoginOK or LoginFailed) with timeout
                                Log("[M59ServerConnection] Waiting for server response (LoginOK/Failed)...");

                                // Give server more time to process and respond
                                await Task.Delay(500); // Increase to 500ms

                                // Check if data is available
                                if (_stream.DataAvailable)
                                {
                                    Log($"[M59ServerConnection] Data available: {_client.Available} bytes");
                                }
                                else
                                {
                                    Log("[M59ServerConnection] ⚠ No data available after 500ms delay");
                                    // Wait a bit longer
                                    await Task.Delay(1000);
                                    if (_stream.DataAvailable)
                                    {
                                        Log($"[M59ServerConnection] Data available after 1.5s: {_client.Available} bytes");
                                    }
                                    else
                                    {
                                        Log("[M59ServerConnection] ✗ Still no data available after 1.5s - server not responding");
                                    }
                                }

                                bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                                Log($"[M59ServerConnection] Response received: {bytesRead} bytes");

                                if (bytesRead >= 7)
                                {
                                    len1 = (ushort)(buffer[0] | (buffer[1] << 8));
                                    crc = (ushort)(buffer[2] | (buffer[3] << 8));
                                    len2 = (ushort)(buffer[4] | (buffer[5] << 8));
                                    Log($"[M59ServerConnection] Response Header: LEN1={len1}, CRC={crc}, LEN2={len2}");

                                    if (len1 == len2 && bytesRead >= 7 + len1)
                                    {
                                        // Extract response body
                                        byte[] responseBody = new byte[len1];
                                        Array.Copy(buffer, 7, responseBody, 0, len1);

                                        // Get PI byte - NOT ENCRYPTED in LoginMode!
                                        messageType = responseBody[0];
                                        Log($"[M59ServerConnection] Response Message Type: {messageType} (LoginOK=23, LoginFailed=24)");

                                        if (messageType == (byte)MessageTypeLoginMode.LoginOK)
                                        {
                                            Log("[M59ServerConnection] ✓✓✓ LOGIN SUCCESS! ✓✓✓");
                                            _isConnected = true;

                                            // Request admin mode in login protocol (AP_REQ_ADMIN) so admin commands can run without characters
                                            await SendAdminHandshakeAsync();

                                            // Start listening for responses in background
                                            _ = ListenForResponsesAsync();

                                            return true;
                                        }
                                        else if (messageType == (byte)MessageTypeLoginMode.LoginFailed)
                                        {
                                            Log("[M59ServerConnection] ✗ LOGIN FAILED - Invalid credentials");
                                            _isConnected = false;
                                            return false;
                                        }
                                        else
                                        {
                                            Log($"[M59ServerConnection] ✗ Unexpected message type: {messageType}");
                                            Log($"[M59ServerConnection] Raw PI byte: 0x{responseBody[0]:X2}");
                                            Log($"[M59ServerConnection] First 10 bytes: {BitConverter.ToString(responseBody, 0, Math.Min(10, responseBody.Length))}");
                                            _isConnected = false;
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        Log($"[M59ServerConnection] ✗ Invalid response header or incomplete data");
                                        Log($"[M59ServerConnection] LEN1={len1}, LEN2={len2}, BytesRead={bytesRead}, Expected={7 + len1}");
                                    }
                                }
                                else
                                {
                                    Log($"[M59ServerConnection] ✗ Response too short: {bytesRead} bytes (need at least 7)");
                                }
                            }
                            else
                            {
                                Log($"[M59ServerConnection] ✗ Expected GetLogin but got message type: {messageType}");
                            }
                        }
                    }
                }

                Log("[M59ServerConnection] ✗ Login sequence failed");
                _isConnected = false;
                return false;
            }
            catch (Exception ex)
            {
                Log($"[M59ServerConnection] ✗ Login failed with exception: {ex.Message}");
                Log($"[M59ServerConnection] Stack trace: {ex.StackTrace}");
                _isConnected = false;
                return false;
            }
        }

        public async Task<bool> SendAdminCommandAsync(string command)
        {
            if (!IsConnected)
            {
                Log("[M59ServerConnection] ✗ Not connected!");
                return false;
            }
            try
            {
                Log($"[M59ServerConnection] Sending admin command: {command}");

                // Use admin login-mode channel only
                if (!_isAdminMode)
                {
                    Log("[M59ServerConnection] Admin mode not active, requesting AP_REQ_ADMIN...");
                    await SendAdminHandshakeAsync();
                    await Task.Delay(50);
                }

                if (!_isAdminMode)
                {
                    Log("[M59ServerConnection] ✗ Admin mode not granted yet. Aborting command.");
                    return false;
                }

                await SendAdminTextAsync(command);
                return true;
            }
            catch (Exception ex)
            {
                Log($"[M59ServerConnection] ✗ Send failed: {ex.Message}");
                Log($"[M59ServerConnection] Exception detail: {ex}");
                return false;
            }
        }

        private async Task ListenForResponsesAsync()
        {
            if (_stream == null) return;

            try
            {
                byte[] buffer = new byte[8192];
                int bufferOffset = 0;

                while (IsConnected)
                {
                    // Admin mode uses line-based text without headers
                    if (_isAdminMode)
                    {
                        int bytesReadAdmin = await _stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesReadAdmin > 0)
                        {
                            for (int i = 0; i < bytesReadAdmin; i++)
                            {
                                char ch = (char)buffer[i];
                                if (ch == '\r' || ch == '\n')
                                {
                                    if (_adminTextBuffer.Length > 0)
                                    {
                                        string line = _adminTextBuffer.ToString();
                                        _adminTextBuffer.Clear();
                                        Log($"[M59ServerConnection] Admin text: {line}");
                                        ResponseQueue.Enqueue(line);
                                        ResponseReceived?.Invoke(this, line);
                                    }
                                }
                                else
                                {
                                    _adminTextBuffer.Append(ch);
                                }
                            }
                        }
                        continue;
                    }

                    int bytesRead = await _stream.ReadAsync(buffer, bufferOffset, buffer.Length - bufferOffset);

                    if (bytesRead > 0)
                    {
                        bufferOffset += bytesRead;

                        // Try to parse messages from buffer
                        while (bufferOffset >= 7) // Minimum: 7 byte header
                        {
                            // Read header
                            ushort len1 = (ushort)(buffer[0] | (buffer[1] << 8));
                            ushort crc = (ushort)(buffer[2] | (buffer[3] << 8));
                            ushort len2 = (ushort)(buffer[4] | (buffer[5] << 8));
                            byte ss = buffer[6];

                            // Validate header
                            if (len1 != len2)
                            {
                                Log("[M59ServerConnection] ✗ Invalid header: LEN1 != LEN2");
                                bufferOffset = 0;
                                break;
                            }

                            // Check if we have the complete message
                            if (bufferOffset < 7 + len1)
                                break; // Wait for more data

                            // Extract body
                            byte[] body = new byte[len1];
                            Array.Copy(buffer, 7, body, 0, len1);

                            // Verify CRC
                            uint calculatedCrc = Crc32.Compute(body);
                            ushort calculatedCrc16 = (ushort)(calculatedCrc & 0xFFFF);

                            if (calculatedCrc16 != crc)
                            {
                                Log($"[M59ServerConnection] ✗ CRC mismatch! Expected: {crc}, Got: {calculatedCrc16}");
                            }

                            // Decode PI depending on mode (login mode: plain PI, game mode: encrypted PI)
                            byte piByte = body[0];
                            byte messageType = piByte;

                            Log($"[M59ServerConnection] Received message type: {messageType}");

                            // Check for admin acknowledgement in login mode
                            if (messageType == (byte)MessageTypeLoginMode.Admin)
                            {
                                Log("[M59ServerConnection] ✓ Received AP_ADMIN (login mode).");
                                _isAdminMode = true;
                            }

                            // Parse AdminMessage (type 162) in game mode
                            if (messageType == (byte)MessageTypeLoginMode.Admin)
                            {
                                // Already handled above
                            }
                            else if (messageType == (byte)MessageType.Admin)
                            {
                                var adminMessage = AdminMessage.Parse(body, 1); // Skip PI byte
                                Log($"[M59ServerConnection] Admin response: {adminMessage.Message}");

                                ResponseQueue.Enqueue(adminMessage.Message);
                                ResponseReceived?.Invoke(this, adminMessage.Message);
                            }

                            // Remove processed message from buffer
                            int messageLength = 7 + len1;
                            Array.Copy(buffer, messageLength, buffer, 0, bufferOffset - messageLength);
                            bufferOffset -= messageLength;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"[M59ServerConnection] Listen error: {ex.Message}");
                _isConnected = false;
            }
        }

        public void Disconnect()
        {
            Log("[M59ServerConnection] Disconnecting...");

            _isConnected = false;
            _isAdminMode = false;
            _adminTextBuffer.Clear();
            _stream?.Close();
            _client?.Close();

            Log("[M59ServerConnection] ✓ Disconnected");
        }

        public void Dispose()
        {
            Disconnect();
            _stream?.Dispose();
            _client?.Dispose();
        }


        /// <summary>
        /// Sends AP_REQ_GAME to enter game protocol mode.
        /// </summary>
        private async Task<bool> SendReqGameAsync(string username)
        {
            if (_stream == null) return false;

            byte[] message = BuildReqGameMessage(username);

            try
            {
                await _stream.WriteAsync(message, 0, message.Length);
                await _stream.FlushAsync();
                Log($"[M59ServerConnection] ✓ ReqGame sent (AP_REQ_GAME, {message.Length} bytes)");
                return true;
            }
            catch (Exception ex)
            {
                Log($"[M59ServerConnection] ✗ Failed to send ReqGame: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Sends AP_REQ_ADMIN (login mode) to switch server session into admin state.
        /// </summary>
        private async Task<bool> SendAdminHandshakeAsync()
        {
            if (_stream == null) return false;

            byte[] message = BuildLoginModeMessage((byte)MessageTypeLoginMode.ReqAdmin);

            try
            {
                await _stream.WriteAsync(message, 0, message.Length);
                await _stream.FlushAsync();
                Log($"[M59ServerConnection] ✓ Admin handshake sent (AP_REQ_ADMIN, {message.Length} bytes)");
                return true;
            }
            catch (Exception ex)
            {
                Log($"[M59ServerConnection] ✗ Failed to send admin handshake: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Builds a simple login-mode message with only a PI byte (no payload).
        /// Login-mode PI bytes are unencrypted.
        /// </summary>
        private byte[] BuildLoginModeMessage(byte messageType)
        {
            byte[] body = new byte[1];
            body[0] = messageType;

            uint crc32 = Crc32.Compute(body);
            ushort crc16 = (ushort)(crc32 & 0xFFFF);

            ushort len = (ushort)body.Length;
            byte[] fullMessage = new byte[7 + len];

            // Header: LEN1 (2) + CRC (2) + LEN2 (2) + SS (1)
            Array.Copy(BitConverter.GetBytes(len), 0, fullMessage, 0, 2);
            Array.Copy(BitConverter.GetBytes(crc16), 0, fullMessage, 2, 2);
            Array.Copy(BitConverter.GetBytes(len), 0, fullMessage, 4, 2);
            fullMessage[6] = 0;

            // Body
            Array.Copy(body, 0, fullMessage, 7, body.Length);

            return fullMessage;
        }

        /// <summary>
        /// Sends plain text admin command in STATE_ADMIN (login/admin mode).
        /// </summary>
        private async Task SendAdminTextAsync(string command)
        {
            if (_stream == null) return;

            // Admin mode expects CR-terminated ASCII
            string line = command + "\r\n";
            byte[] bytes = Encoding.Latin1.GetBytes(line);
            await _stream.WriteAsync(bytes, 0, bytes.Length);
            await _stream.FlushAsync();
            Log($"[M59ServerConnection] ✓ Admin text sent ({bytes.Length} bytes)");
        }

        /// <summary>
        /// Build AP_REQ_GAME message (login mode).
        /// Fields: last_download_time (int), encodednum (int), hostname (len + bytes).
        /// </summary>
        private byte[] BuildReqGameMessage(string username)
        {
            // Encode version check value like EnterGame() in login.c
            // encodednum = (((MAJOR_REV * 100) + MINOR_REV) * P_CATCH) + P_CATCH
            const byte major = 8;
            const byte minor = 0;
            const int P_CATCH = 3;
            int encodedNum = (((major * 100) + minor) * P_CATCH) + P_CATCH; // 2403 for 8.0

            // Hostname string (classic clients send "cheater" here)
            string hostString = "cheater";
            byte[] hostBytes = MeridianMD5.Encoding.GetBytes(hostString);

            int bodySize = 1 + 4 + 4 + 2 + hostBytes.Length; // PI + last_download + encoded + len + host
            byte[] body = new byte[bodySize];

            int cursor = 0;
            body[cursor++] = (byte)MessageTypeLoginMode.ReqGame;
            Array.Copy(BitConverter.GetBytes(0), 0, body, cursor, 4); // last_download_time = 0
            cursor += 4;
            Array.Copy(BitConverter.GetBytes(encodedNum), 0, body, cursor, 4); // encodednum
            cursor += 4;
            Array.Copy(BitConverter.GetBytes((ushort)hostBytes.Length), 0, body, cursor, 2);
            cursor += 2;
            Array.Copy(hostBytes, 0, body, cursor, hostBytes.Length);

            uint crc32 = Crc32.Compute(body);
            ushort crc16 = (ushort)(crc32 & 0xFFFF);

            ushort len = (ushort)body.Length;
            byte[] fullMessage = new byte[7 + len];

            Array.Copy(BitConverter.GetBytes(len), 0, fullMessage, 0, 2);
            Array.Copy(BitConverter.GetBytes(crc16), 0, fullMessage, 2, 2);
            Array.Copy(BitConverter.GetBytes(len), 0, fullMessage, 4, 2);
            fullMessage[6] = 0;
            Array.Copy(body, 0, fullMessage, 7, body.Length);

            return fullMessage;
        }
    }
}
