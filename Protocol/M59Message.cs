/*
 Meridian 59 Protocol Implementation
 TCP Message Format
*/

using System;
using System.Text;

namespace M59AdminTool.Protocol
{
    /// <summary>
    /// Message types for GameMode protocol
    /// </summary>
    public enum MessageType : byte
    {
        ReqAdmin = 60,      // Client -> Server: Admin command request
        Admin = 162,        // Server -> Client: Admin response
        // More message types can be added here as needed
    }

    /// <summary>
    /// Base class for Meridian 59 protocol messages
    /// </summary>
    public abstract class M59Message
    {
        public MessageType Type { get; protected set; }

        protected M59Message(MessageType type)
        {
            Type = type;
        }

        /// <summary>
        /// Serialize message body to bytes (without header)
        /// </summary>
        public abstract byte[] ToBytes();

        /// <summary>
        /// Calculate total message length (header + body)
        /// </summary>
        public int TotalLength => 7 + ToBytes().Length; // 7 = header size

        /// <summary>
        /// Build complete TCP message with header
        /// </summary>
        public byte[] BuildMessage(PIEncryption piEncryption)
        {
            byte[] body = ToBytes();
            // Build body with encrypted PI
            byte[] fullBody = new byte[1 + body.Length];
            // Encode message type (PI)
            byte encodedPI = piEncryption.Encode((byte)Type);
            fullBody[0] = encodedPI;
            Array.Copy(body, 0, fullBody, 1, body.Length);

            // Header (7 bytes) + full body (PI + payload)
            byte[] message = new byte[7 + fullBody.Length];

            // Calculate CRC
            uint crc = Crc32.Compute(fullBody);
            ushort crc16 = (ushort)(crc & 0xFFFF); // Take lower 16 bits

            ushort bodyLength = (ushort)fullBody.Length;

            System.Diagnostics.Debug.WriteLine($"[M59Message] Building message:");
            System.Diagnostics.Debug.WriteLine($"  Type: {Type} ({(byte)Type})");
            System.Diagnostics.Debug.WriteLine($"  Encoded PI: {encodedPI}");
            System.Diagnostics.Debug.WriteLine($"  Body length: {bodyLength}");
            System.Diagnostics.Debug.WriteLine($"  CRC: {crc16:X4}");

            // Build header (7 bytes)
            // LEN1 (2 bytes)
            message[0] = (byte)(bodyLength & 0xFF);
            message[1] = (byte)((bodyLength >> 8) & 0xFF);

            // CRC (2 bytes)
            message[2] = (byte)(crc16 & 0xFF);
            message[3] = (byte)((crc16 >> 8) & 0xFF);

            // LEN2 (2 bytes) - duplicate of LEN1
            message[4] = (byte)(bodyLength & 0xFF);
            message[5] = (byte)((bodyLength >> 8) & 0xFF);

            // SS (1 byte) - ServerSave, we send 0
            message[6] = 0;

            // Copy body
            Array.Copy(fullBody, 0, message, 7, fullBody.Length);

            // Debug: Show hex dump
            System.Diagnostics.Debug.WriteLine($"  Message bytes: {BitConverter.ToString(message)}");

            return message;
        }
    }

    /// <summary>
    /// Admin command request (Client -> Server)
    /// Message Type: 60 (ReqAdmin)
    /// </summary>
    public class ReqAdminMessage : M59Message
    {
        public string Command { get; set; }

        public ReqAdminMessage(string command) : base(MessageType.ReqAdmin)
        {
            Command = command;
        }

        public override byte[] ToBytes()
        {
            byte[] commandBytes = Encoding.UTF8.GetBytes(Command);
            byte[] data = new byte[2 + commandBytes.Length];

            // Length prefix (2 bytes)
            ushort len = (ushort)commandBytes.Length;
            data[0] = (byte)(len & 0xFF);
            data[1] = (byte)((len >> 8) & 0xFF);

            // Command string
            Array.Copy(commandBytes, 0, data, 2, commandBytes.Length);

            return data;
        }
    }

    /// <summary>
    /// Admin response (Server -> Client)
    /// Message Type: 162 (Admin)
    /// </summary>
    public class AdminMessage : M59Message
    {
        public string Message { get; set; }

        public AdminMessage(string message) : base(MessageType.Admin)
        {
            Message = message;
        }

        public override byte[] ToBytes()
        {
            // Not used for parsing server responses
            throw new NotImplementedException("AdminMessage is for parsing only");
        }

        /// <summary>
        /// Parse AdminMessage from bytes (after header and PI are removed)
        /// </summary>
        public static AdminMessage Parse(byte[] data, int offset = 0)
        {
            // Read length prefix (2 bytes)
            ushort len = (ushort)(data[offset] | (data[offset + 1] << 8));

            // Read message string
            string message = Encoding.UTF8.GetString(data, offset + 2, len);

            return new AdminMessage(message);
        }
    }
}
