using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace M59AdminTool.Services
{
    /// <summary>
    /// Writes BGF (BlakSton Graphics Format) files from bitmap data.
    /// Based on the Meridian 59 writebgf.c implementation.
    /// </summary>
    public class BGFWriter
    {
        private const int BGF_VERSION = 10;
        private static readonly byte[] MAGIC = { 0x42, 0x47, 0x46, 0x11 }; // "BGF" + 0x11
        private const int MAX_BITMAPNAME = 32;

        public class BitmapData
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int XOffset { get; set; }
            public int YOffset { get; set; }
            public byte[] PixelData { get; set; } = Array.Empty<byte>();
            public byte NumHotspots { get; set; }
            public HotspotData[] Hotspots { get; set; } = Array.Empty<HotspotData>();
        }

        public class HotspotData
        {
            public byte Number { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class BGFOptions
        {
            public string Name { get; set; } = "texture";
            public int ShrinkFactor { get; set; } = 1;
            public bool Compress { get; set; } = true;
            public bool Rotate { get; set; } = false;
        }

        /// <summary>
        /// Writes a single bitmap to BGF format.
        /// </summary>
        public static void WriteBGF(string outputPath, BitmapData bitmap, BGFOptions options)
        {
            var bitmaps = new BitmapData[] { bitmap };
            WriteBGF(outputPath, bitmaps, options);
        }

        /// <summary>
        /// Writes multiple bitmaps to BGF format with animation groups.
        /// </summary>
        public static void WriteBGF(string outputPath, BitmapData[] bitmaps, BGFOptions options)
        {
            using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            using var writer = new BinaryWriter(fs);

            // Write magic number
            writer.Write(MAGIC);

            // Write version
            writer.Write(BGF_VERSION);

            // Write bitmap name (32 bytes, null-padded)
            byte[] nameBytes = new byte[MAX_BITMAPNAME];
            byte[] nameData = Encoding.ASCII.GetBytes(options.Name);
            Array.Copy(nameData, nameBytes, Math.Min(nameData.Length, MAX_BITMAPNAME - 1));
            writer.Write(nameBytes);

            // Write number of bitmaps
            writer.Write(bitmaps.Length);

            // Write number of groups (1 group with all bitmaps)
            writer.Write(1);

            // Write max indices (all bitmaps in one group)
            writer.Write(bitmaps.Length);

            // Write shrink factor
            writer.Write(options.ShrinkFactor);

            // Write each bitmap
            foreach (var bitmap in bitmaps)
            {
                WriteBitmapData(writer, bitmap, options);
            }

            // Write group indices (one group with all bitmaps: 0, 1, 2, ...)
            writer.Write(bitmaps.Length); // num_indices in group
            for (int i = 0; i < bitmaps.Length; i++)
            {
                writer.Write(i); // bitmap index
            }
        }

        private static void WriteBitmapData(BinaryWriter writer, BitmapData bitmap, BGFOptions options)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            // Write dimensions (swap if rotated)
            if (options.Rotate)
            {
                writer.Write(height); // width becomes height
                writer.Write(width);  // height becomes width
            }
            else
            {
                writer.Write(width);
                writer.Write(height);
            }

            // Write offsets
            writer.Write(bitmap.XOffset);
            writer.Write(bitmap.YOffset);

            // Write hotspots
            writer.Write(bitmap.NumHotspots);
            foreach (var hotspot in bitmap.Hotspots)
            {
                writer.Write(hotspot.Number);
                writer.Write(hotspot.X);
                writer.Write(hotspot.Y);
            }

            // Prepare pixel data (rotate if needed)
            byte[] pixelData = bitmap.PixelData;
            if (options.Rotate)
            {
                pixelData = RotatePixelData(bitmap.PixelData, width, height);
            }

            // Compress if enabled
            if (options.Compress)
            {
                byte[]? compressed = CompressData(pixelData);

                // Check if compression is beneficial
                if (compressed != null && compressed.Length < pixelData.Length)
                {
                    writer.Write((byte)1); // compressed flag
                    writer.Write(compressed.Length);
                    writer.Write(compressed);
                }
                else
                {
                    // Compression not beneficial, write uncompressed
                    writer.Write((byte)0); // uncompressed flag
                    writer.Write(0); // length = 0 for uncompressed
                    writer.Write(pixelData);
                }
            }
            else
            {
                // Uncompressed
                writer.Write((byte)0);
                writer.Write(0);
                writer.Write(pixelData);
            }
        }

        private static byte[] RotatePixelData(byte[] data, int width, int height)
        {
            byte[] rotated = new byte[data.Length];

            // Rotate 90 degrees clockwise (for wall textures)
            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < height; row++)
                {
                    int srcIndex = row * width + col;
                    int dstIndex = col * height + row;
                    rotated[dstIndex] = data[srcIndex];
                }
            }

            return rotated;
        }

        private static byte[]? CompressData(byte[] data)
        {
            try
            {
                using var output = new MemoryStream();
                using (var deflate = new DeflaterOutputStream(output, new Deflater(Deflater.BEST_COMPRESSION)))
                {
                    deflate.Write(data, 0, data.Length);
                    deflate.Finish();
                }
                return output.ToArray();
            }
            catch
            {
                return null;
            }
        }
    }
}
