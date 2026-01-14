/*
 Adapted from Meridian59 .NET project
 Copyright (c) 2012-2013 Clint Banzhaf
 Licensed under GPL v3
*/

using System;
using System.Text;
using System.Security.Cryptography;

namespace M59AdminTool.Protocol
{
    /// <summary>
    /// Provides MD5 hashes in M59 style (replaced zero bytes).
    /// Zero bytes are replaced with 0x01 to avoid null termination issues.
    /// </summary>
    public static class MeridianMD5
    {
        /// <summary>
        /// MD5 creator from .NET
        /// </summary>
        private static readonly MD5 md5 = MD5.Create();

        /// <summary>
        /// Default encoding used in Meridian 59 (Latin1/ISO-8859-1, compatible with Windows-1252)
        /// </summary>
        public static readonly Encoding Encoding = Encoding.Latin1;

        /// <summary>
        /// Generates a MD5 in M59 style from bytes input
        /// </summary>
        /// <param name="Input">Bytes to generate a M59 MD5 from</param>
        /// <returns>MD5 bytes with replaced 0x00</returns>
        public static byte[] ComputeMD5(byte[] Input)
        {
            // get MD5
            byte[] bytes = md5.ComputeHash(Input);

            // replace zero bytes with 0x01 to work around misinterpreting
            // them as termination zeros
            for (int i = 0; i < bytes.Length; i++)
                if (bytes[i] == 0x00)
                    bytes[i] = 0x01;

            return bytes;
        }

        /// <summary>
        /// Generates a MD5 in M59 style from string input
        /// </summary>
        /// <param name="Input">A string to generate a M59 MD5 from</param>
        /// <returns>MD5 bytes with replaced 0x00</returns>
        public static byte[] ComputeMD5(string Input)
        {
            // get bytes of string using Windows-1252 encoding
            byte[] bytes = Encoding.GetBytes(Input);
            return ComputeMD5(bytes);
        }
    }
}
