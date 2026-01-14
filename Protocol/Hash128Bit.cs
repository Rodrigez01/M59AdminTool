/*
 Adapted from Meridian59 .NET project
 Copyright (c) 2012-2013 Clint Banzhaf
 Licensed under GPL v3
*/

namespace M59AdminTool.Protocol
{
    /// <summary>
    /// A struct to store 128-Bit hashes in 4x 32-Bit blocks.
    /// Used for MD5 password hashing in M59 protocol.
    /// </summary>
    public struct Hash128Bit
    {
        public uint HASH1;
        public uint HASH2;
        public uint HASH3;
        public uint HASH4;
    }
}
