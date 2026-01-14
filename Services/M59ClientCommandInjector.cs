using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace M59AdminTool.Services
{
    /// <summary>
    /// Meridian 59 Client Command Injector
    /// Sends commands directly to the running client via Named Pipe.
    /// Replaces unreliable SendKeys methods.
    /// </summary>
    public class M59ClientCommandInjector : IDisposable
    {
        private int? _pid;
        private NamedPipeClientStream? _pipeClient;
        private string? _pipeName;

        /// <summary>
        /// Creates a new command injector
        /// </summary>
        /// <param name="pid">Optional: Process ID of the client. If null, will auto-detect.</param>
        public M59ClientCommandInjector(int? pid = null)
        {
            _pid = pid;
        }

        /// <summary>
        /// Finds the Process ID of the running Meridian59 client
        /// </summary>
        public int? FindClientPid()
        {
            var processes = Process.GetProcesses();
            var clientNames = new[] { "meridian", "clientd3d", "client" };

            foreach (var proc in processes)
            {
                try
                {
                    var processName = proc.ProcessName.ToLower();
                    if (clientNames.Any(name => processName.Contains(name)))
                    {
                        Debug.WriteLine($"[M59ClientCommandInjector] ✓ Client found: PID {proc.Id}");
                        return proc.Id;
                    }
                }
                catch
                {
                    // Ignore Access Denied errors
                }
            }

            return null;
        }

        /// <summary>
        /// Connects to the client via Named Pipe
        /// </summary>
        /// <returns>True if connection successful</returns>
        public bool Connect()
        {
            if (_pid == null)
            {
                _pid = FindClientPid();
            }

            if (_pid == null)
            {
                Debug.WriteLine("[M59ClientCommandInjector] ✗ No running Meridian 59 client found!");
                return false;
            }

            _pipeName = $"Meridian59_Command_{_pid}";

            try
            {
                Debug.WriteLine($"[M59ClientCommandInjector] → Connecting to pipe: \\\\.\\pipe\\{_pipeName}");

                _pipeClient = new NamedPipeClientStream(
                    ".",                    // Local server
                    _pipeName,              // Pipe name
                    PipeDirection.Out       // Write-only
                );

                // Try to connect (5 seconds timeout)
                _pipeClient.Connect(5000);

                Debug.WriteLine("[M59ClientCommandInjector] ✓ Connected to client!");
                return true;
            }
            catch (TimeoutException)
            {
                Debug.WriteLine("[M59ClientCommandInjector] ✗ Connection timeout!");
                Debug.WriteLine("[M59ClientCommandInjector] Note: Client must be compiled with Command Pipe support!");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[M59ClientCommandInjector] ✗ Connection error: {ex.Message}");
                Debug.WriteLine("[M59ClientCommandInjector] Note: Client must be compiled with Command Pipe support!");
                return false;
            }
        }

        /// <summary>
        /// Sends a command to the client
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <returns>True if command was sent successfully</returns>
        public bool SendCommand(string command)
        {
            if (_pipeClient == null || !_pipeClient.IsConnected)
            {
                Debug.WriteLine("[M59ClientCommandInjector] ✗ Not connected!");
                return false;
            }

            try
            {
                // Add newline
                byte[] buffer = Encoding.UTF8.GetBytes(command + "\n");

                // Send command
                _pipeClient.Write(buffer, 0, buffer.Length);
                _pipeClient.Flush();

                Debug.WriteLine($"[M59ClientCommandInjector] ✓ Command sent: {command}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[M59ClientCommandInjector] ✗ Send error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the client
        /// </summary>
        public void Disconnect()
        {
            if (_pipeClient != null)
            {
                _pipeClient.Close();
                _pipeClient.Dispose();
                _pipeClient = null;
                Debug.WriteLine("[M59ClientCommandInjector] ✓ Disconnected");
            }
        }

        /// <summary>
        /// Checks if the client is connected
        /// </summary>
        public bool IsConnected => _pipeClient?.IsConnected ?? false;

        public void Dispose()
        {
            Disconnect();
        }
    }
}
