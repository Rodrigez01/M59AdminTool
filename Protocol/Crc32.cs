/*
 This file was adopted from http://damieng.com/blog/2006/08/08/calculating_crc32_in_c_and_net
 All credits & copyright go to the original author: Damien Guard (damieng@gmail.com)

 Adapted for M59AdminTool from Meridian59 .NET project
*/

using System;
using System.Security.Cryptography;

namespace M59AdminTool.Protocol
{
    /// <summary>
    /// CRC32 hash generator for Meridian 59 protocol
    /// </summary>
    public class Crc32 : HashAlgorithm
    {
        public const uint DEFAULTPOLYNOMIAL = 0xEDB88320;
        public const uint DEFAULTSEED = 0xFFFFFFFF;

        protected static uint[]? defaultTable;

        protected uint hash;
        protected uint seed;
        protected uint[] table;

        public Crc32()
        {
            table = InitializeTable(DEFAULTPOLYNOMIAL);
            seed = DEFAULTSEED;
            Initialize();
        }

        public Crc32(uint Polynomial, uint Seed)
        {
            table = InitializeTable(Polynomial);
            this.seed = Seed;
            Initialize();
        }

        public override void Initialize()
        {
            hash = seed;
        }

        protected override void HashCore(byte[] Buffer, int Start, int Length)
        {
            hash = CalculateHash(table, hash, Buffer, Start, Length);
        }

        protected override byte[] HashFinal()
        {
            byte[] hashBuffer = UInt32ToBigEndianBytes(~hash);
            this.HashValue = hashBuffer;
            return hashBuffer;
        }

        public override int HashSize
        {
            get { return 32; }
        }

        public static uint Compute(byte[] Buffer)
        {
            return ~CalculateHash(InitializeTable(DEFAULTPOLYNOMIAL), DEFAULTSEED, Buffer, 0, Buffer.Length);
        }

        public static uint Compute(uint Seed, byte[] Buffer)
        {
            return ~CalculateHash(InitializeTable(DEFAULTPOLYNOMIAL), Seed, Buffer, 0, Buffer.Length);
        }

        public static uint Compute(uint Polynomial, uint Seed, byte[] Buffer)
        {
            return ~CalculateHash(InitializeTable(Polynomial), Seed, Buffer, 0, Buffer.Length);
        }

        private static uint[] InitializeTable(uint Polynomial)
        {
            if (Polynomial == DEFAULTPOLYNOMIAL && defaultTable != null)
                return defaultTable;

            uint[] createTable = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                uint entry = (uint)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ Polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (Polynomial == DEFAULTPOLYNOMIAL)
                defaultTable = createTable;

            return createTable;
        }

        private static uint CalculateHash(uint[] Table, uint Seed, byte[] Buffer, int Start, int Size)
        {
            uint crc = Seed;
            for (int i = Start; i < Size; i++)
            {
                unchecked
                {
                    crc = (crc >> 8) ^ Table[Buffer[i] ^ crc & 0xFF];
                }
            }

            return crc;
        }

        private byte[] UInt32ToBigEndianBytes(uint X)
        {
            return new byte[]
            {
                (byte)((X >> 24) & 0xFF),
                (byte)((X >> 16) & 0xFF),
                (byte)((X >> 8) & 0xFF),
                (byte)(X & 0xFF)
            };
        }
    }
}
