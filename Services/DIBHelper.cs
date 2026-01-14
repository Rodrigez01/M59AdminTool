using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace M59AdminTool.Services
{
    /// <summary>
    /// Helper class for Device Independent Bitmap (DIB) operations.
    /// Handles palette validation and pixel data extraction.
    /// </summary>
    public class DIBHelper
    {
        public class BitmapInfo
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int ColorCount { get; set; }
            public bool Is256Colors { get; set; }
            public bool IsIndexed { get; set; }
            public PixelFormat Format { get; set; }
            public string FormatDescription { get; set; } = string.Empty;
        }

        /// <summary>
        /// Gets detailed information about a bitmap file.
        /// </summary>
        public static BitmapInfo GetBitmapInfo(string filePath)
        {
            try
            {
                using var bitmap = new Bitmap(filePath);
                return GetBitmapInfo(bitmap);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load bitmap: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets detailed information about a bitmap.
        /// </summary>
        public static BitmapInfo GetBitmapInfo(Bitmap bitmap)
        {
            var info = new BitmapInfo
            {
                Width = bitmap.Width,
                Height = bitmap.Height,
                Format = bitmap.PixelFormat,
                IsIndexed = (bitmap.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed
            };

            if (info.IsIndexed && bitmap.Palette != null)
            {
                info.ColorCount = bitmap.Palette.Entries.Length;
                info.Is256Colors = (info.ColorCount == 256);
            }
            else
            {
                info.ColorCount = 0;
                info.Is256Colors = false;
            }

            info.FormatDescription = GetPixelFormatDescription(bitmap.PixelFormat);

            return info;
        }

        /// <summary>
        /// Validates if a bitmap meets BGF requirements (8-bit indexed, 256 colors).
        /// </summary>
        public static bool ValidateBitmap(string filePath, out string errorMessage)
        {
            try
            {
                using var bitmap = new Bitmap(filePath);
                return ValidateBitmap(bitmap, out errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to load bitmap: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Validates if a bitmap meets BGF requirements.
        /// </summary>
        public static bool ValidateBitmap(Bitmap bitmap, out string errorMessage)
        {
            // Check if indexed
            if ((bitmap.PixelFormat & PixelFormat.Indexed) != PixelFormat.Indexed)
            {
                errorMessage = $"Bitmap must be indexed color (8-bit). Current format: {GetPixelFormatDescription(bitmap.PixelFormat)}";
                return false;
            }

            // Check if 8-bit
            if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                errorMessage = $"Bitmap must be 8-bit indexed. Current format: {GetPixelFormatDescription(bitmap.PixelFormat)}";
                return false;
            }

            // Check palette size
            if (bitmap.Palette == null || bitmap.Palette.Entries.Length != 256)
            {
                int count = bitmap.Palette?.Entries.Length ?? 0;
                errorMessage = $"Bitmap must have exactly 256 colors. Found: {count} colors";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Extracts raw pixel data from a bitmap (8-bit indexed).
        /// </summary>
        public static byte[] GetPixelData(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                throw new ArgumentException("Bitmap must be 8-bit indexed format");
            }

            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[] pixels = new byte[width * height];

            BitmapData? bmpData = null;
            try
            {
                // Lock bitmap for reading
                bmpData = bitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format8bppIndexed);

                int stride = Math.Abs(bmpData.Stride);
                int bytesPerRow = width; // 8-bit = 1 byte per pixel
                IntPtr ptr = bmpData.Scan0;

                // Copy pixel data row by row
                for (int row = 0; row < height; row++)
                {
                    // Calculate offset (handle bottom-up bitmaps)
                    int srcOffset = row * stride;
                    int dstOffset = row * width;

                    // Copy row
                    Marshal.Copy(
                        IntPtr.Add(ptr, srcOffset),
                        pixels,
                        dstOffset,
                        bytesPerRow);
                }

                return pixels;
            }
            finally
            {
                if (bmpData != null)
                {
                    bitmap.UnlockBits(bmpData);
                }
            }
        }

        /// <summary>
        /// Loads a bitmap and extracts pixel data for BGF conversion.
        /// </summary>
        public static BGFWriter.BitmapData LoadBitmapForBGF(string filePath, int xOffset, int yOffset)
        {
            using var bitmap = new Bitmap(filePath);

            // Validate
            if (!ValidateBitmap(bitmap, out string error))
            {
                throw new Exception($"Bitmap validation failed: {error}");
            }

            // Extract pixel data
            byte[] pixels = GetPixelData(bitmap);

            // Create BGF bitmap data
            return new BGFWriter.BitmapData
            {
                Width = bitmap.Width,
                Height = bitmap.Height,
                XOffset = xOffset,
                YOffset = yOffset,
                PixelData = pixels,
                NumHotspots = 0,
                Hotspots = Array.Empty<BGFWriter.HotspotData>()
            };
        }

        private static string GetPixelFormatDescription(PixelFormat format)
        {
            return format switch
            {
                PixelFormat.Format1bppIndexed => "1-bit Indexed (2 colors)",
                PixelFormat.Format4bppIndexed => "4-bit Indexed (16 colors)",
                PixelFormat.Format8bppIndexed => "8-bit Indexed (256 colors)",
                PixelFormat.Format16bppRgb555 => "16-bit RGB (32K colors)",
                PixelFormat.Format16bppRgb565 => "16-bit RGB (64K colors)",
                PixelFormat.Format24bppRgb => "24-bit RGB (16M colors)",
                PixelFormat.Format32bppRgb => "32-bit RGB",
                PixelFormat.Format32bppArgb => "32-bit ARGB (with alpha)",
                _ => format.ToString()
            };
        }
    }
}
