/*
 Adapted from Meridian59 .NET project
 Protocol Identifier (PI) encryption/decryption for Meridian 59 protocol
*/

using System;
using System.Text;

namespace M59AdminTool.Protocol
{
    /// <summary>
    /// Encodes/Decodes message types (PI) for Meridian 59 protocol
    /// </summary>
    public class PIEncryption
    {
        #region Constants
        public const string StaticFallbackHashString = "BLAKSTON: Greenwich Q Zjiria";
        public const byte SeedByte = 0;
        public const byte XORValue = 0xED;
        public const byte ANDValue = 0x7F;
        #endregion

        private byte[] _hashString;
        private short _cursor;
        private bool _enabled;
        private byte _currentByte;

        public PIEncryption()
        {
            // Use Latin1/ISO-8859-1 encoding (compatible with Windows-1252 for ASCII range)
            // The fallback string only contains ASCII characters, so Latin1 is sufficient
            _hashString = Encoding.Latin1.GetBytes(StaticFallbackHashString);
            Reset();
        }

        public void Reset()
        {
            _currentByte = 0x00;
            _cursor = 0;
            _enabled = false;
        }

        /// <summary>
        /// Encode message type byte
        /// </summary>
        public byte Encode(byte messageType)
        {
            byte encodedPI = (byte)(messageType ^ _currentByte);

            if (_enabled)
            {
                if (_cursor == _hashString.Length)
                    _cursor = 0;

                _currentByte += (byte)(_hashString[_cursor] & ANDValue);
                _cursor++;
            }

            return encodedPI;
        }

        /// <summary>
        /// Decode message type byte
        /// </summary>
        public byte Decode(byte encodedPI)
        {
            byte decodedPI = (byte)(encodedPI ^ _currentByte);

            if (_enabled)
            {
                if (_cursor == _hashString.Length)
                    _cursor = 0;

                _currentByte += (byte)(_hashString[_cursor] & ANDValue);
                _cursor++;
            }

            return decodedPI;
        }

        /// <summary>
        /// Enable encryption after initial handshake
        /// </summary>
        public void Enable()
        {
            _currentByte = SeedByte ^ XORValue;
            // Use Latin1/ISO-8859-1 encoding (compatible with Windows-1252 for ASCII range)
            _hashString = Encoding.Latin1.GetBytes(StaticFallbackHashString);
            _cursor = 0;
            _enabled = true;
        }
    }
}
