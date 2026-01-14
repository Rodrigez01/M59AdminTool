/*
 Adapted from Meridian59 .NET project
 Copyright (c) 2012-2013 Clint Banzhaf
 Licensed under GPL v3
*/

using System;

namespace M59AdminTool.Protocol
{
    /// <summary>
    /// First message sent from the client to the server in response to GetLogin.
    /// Used to identify the client including account credentials.
    /// </summary>
    [Serializable]
    public class LoginMessage
    {
        #region Constants
        public const ushort CPUTYPE_PENTIUM = 586;
        public const ushort WINTYPE_NT      = 2;
        #endregion

        public MessageTypeLoginMode MessageType => MessageTypeLoginMode.Login;

        // Client version
        public byte MajorClientVersion { get; set; } = 8;
        public byte MinorClientVersion { get; set; } = 0;

        // Windows info
        public uint WindowsType { get; set; } = WINTYPE_NT;
        public uint WindowsMajorVersion { get; set; } = 10;
        public uint WindowsMinorVersion { get; set; } = 0;

        // System info
        public uint RamSize { get; set; } = 16384; // 16GB in MB
        public ushort CpuType { get; set; } = CPUTYPE_PENTIUM;
        public ushort ClientExecutableCRC { get; set; } = 0;

        // Display info
        public ushort HorizontalSystemResolution { get; set; } = 1920;
        public ushort VerticalSystemResolution { get; set; } = 1080;
        public uint Display { get; set; } = 0;
        public uint Bandwidth { get; set; } = 0;
        public byte ColorDepth { get; set; } = 32;

        // Partner/unused fields
        public byte PartnerNr { get; set; } = 0;
        public ushort Unused { get; set; } = 0;

        // Credentials
        public string Username { get; set; } = "";
        public Hash128Bit PasswordHash { get; protected set; }
        public string SecretKey { get; set; }

        /// <summary>
        /// Sets the password to use. This will update property 'PasswordHash'
        /// </summary>
        public string Password
        {
            set
            {
                if (value != null)
                {
                    // Calculate the MD5 hash in M59 format
                    byte[] md5hash = MeridianMD5.ComputeMD5(value);

                    Hash128Bit pwHash = new Hash128Bit();
                    pwHash.HASH1 = BitConverter.ToUInt32(md5hash, 0);
                    pwHash.HASH2 = BitConverter.ToUInt32(md5hash, 4);
                    pwHash.HASH3 = BitConverter.ToUInt32(md5hash, 8);
                    pwHash.HASH4 = BitConverter.ToUInt32(md5hash, 12);

                    this.PasswordHash = pwHash;
                }
            }
        }

        /// <summary>
        /// Constructor by values
        /// </summary>
        public LoginMessage(string username, string password)
            : this(username, password, "347")
        {
        }

        public LoginMessage(string username, string password, string secretKey)
        {
            this.Username = username;
            this.Password = password;
            this.SecretKey = string.IsNullOrWhiteSpace(secretKey) ? "347" : secretKey;
        }

        /// <summary>
        /// Serialize to network buffer
        /// </summary>
        public byte[] BuildMessage()
        {
            byte[] secretBytes = MeridianMD5.Encoding.GetBytes(SecretKey ?? string.Empty);

            // Calculate size
            int bodySize = TypeSizes.BYTE + TypeSizes.BYTE +  // versions
                TypeSizes.INT + TypeSizes.INT + TypeSizes.INT +  // windows
                TypeSizes.INT + TypeSizes.SHORT + TypeSizes.SHORT +  // ram, cpu, crc
                TypeSizes.SHORT + TypeSizes.SHORT +  // resolution
                TypeSizes.INT + TypeSizes.INT +  // display, bandwidth
                TypeSizes.BYTE + TypeSizes.BYTE + TypeSizes.SHORT +  // color, partner, unused
                TypeSizes.SHORT + Username.Length +  // username length + data
                TypeSizes.SHORT + 16 +  // password hash length + hash (128 bit = 16 bytes)
                TypeSizes.SHORT + secretBytes.Length;  // secret key length + data

            byte[] body = new byte[bodySize + 1];  // +1 for PI byte

            int cursor = 0;

            // PI (Protocol Identifier) - UNENCRYPTED for OUTGOING messages!
            // Only INCOMING messages have encrypted PI bytes!
            body[cursor] = (byte)MessageTypeLoginMode.Login;
            cursor++;

            // Client versions
            body[cursor++] = MajorClientVersion;
            body[cursor++] = MinorClientVersion;

            // Windows info
            Array.Copy(BitConverter.GetBytes(WindowsType), 0, body, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;
            Array.Copy(BitConverter.GetBytes(WindowsMajorVersion), 0, body, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;
            Array.Copy(BitConverter.GetBytes(WindowsMinorVersion), 0, body, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;

            // System info
            Array.Copy(BitConverter.GetBytes(RamSize), 0, body, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;
            Array.Copy(BitConverter.GetBytes(CpuType), 0, body, cursor, TypeSizes.SHORT);
            cursor += TypeSizes.SHORT;
            Array.Copy(BitConverter.GetBytes(ClientExecutableCRC), 0, body, cursor, TypeSizes.SHORT);
            cursor += TypeSizes.SHORT;

            // Display info
            Array.Copy(BitConverter.GetBytes(HorizontalSystemResolution), 0, body, cursor, TypeSizes.SHORT);
            cursor += TypeSizes.SHORT;
            Array.Copy(BitConverter.GetBytes(VerticalSystemResolution), 0, body, cursor, TypeSizes.SHORT);
            cursor += TypeSizes.SHORT;
            Array.Copy(BitConverter.GetBytes(Display), 0, body, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;
            Array.Copy(BitConverter.GetBytes(Bandwidth), 0, body, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;

            // Color/partner
            body[cursor++] = ColorDepth;
            body[cursor++] = PartnerNr;
            Array.Copy(BitConverter.GetBytes(Unused), 0, body, cursor, TypeSizes.SHORT);
            cursor += TypeSizes.SHORT;

            // Username
            Array.Copy(BitConverter.GetBytes((ushort)Username.Length), 0, body, cursor, TypeSizes.SHORT);
            cursor += TypeSizes.SHORT;
            Array.Copy(MeridianMD5.Encoding.GetBytes(Username), 0, body, cursor, Username.Length);
            cursor += Username.Length;

            // Password hash
            Array.Copy(BitConverter.GetBytes((ushort)16), 0, body, cursor, TypeSizes.SHORT);
            cursor += TypeSizes.SHORT;
            Array.Copy(BitConverter.GetBytes(PasswordHash.HASH1), 0, body, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;
            Array.Copy(BitConverter.GetBytes(PasswordHash.HASH2), 0, body, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;
            Array.Copy(BitConverter.GetBytes(PasswordHash.HASH3), 0, body, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;
            Array.Copy(BitConverter.GetBytes(PasswordHash.HASH4), 0, body, cursor, TypeSizes.INT);
            cursor += TypeSizes.INT;

            // Secret key (server uses this to authorize clients)
            Array.Copy(BitConverter.GetBytes((ushort)secretBytes.Length), 0, body, cursor, TypeSizes.SHORT);
            cursor += TypeSizes.SHORT;
            Array.Copy(secretBytes, 0, body, cursor, secretBytes.Length);
            cursor += secretBytes.Length;

            // Calculate CRC
            uint crc32 = Crc32.Compute(body);
            ushort crc16 = (ushort)(crc32 & 0xFFFF);

            // Build full message with header
            ushort len = (ushort)body.Length;
            byte[] fullMessage = new byte[7 + len];

            // Header: LEN1 (2) + CRC (2) + LEN2 (2) + SS (1) + BODY
            Array.Copy(BitConverter.GetBytes(len), 0, fullMessage, 0, 2);
            Array.Copy(BitConverter.GetBytes(crc16), 0, fullMessage, 2, 2);
            Array.Copy(BitConverter.GetBytes(len), 0, fullMessage, 4, 2);
            fullMessage[6] = 0;  // SS (session byte, unused here)
            Array.Copy(body, 0, fullMessage, 7, body.Length);

            return fullMessage;
        }
    }
}
