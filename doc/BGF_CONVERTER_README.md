# BGF Converter - BMP to BGF Conversion Tool

## Overview

The BGF Converter is an integrated tool in the M59AdminTool that converts BMP images to the BGF (BlakSton Graphics Format) used by Meridian 59 for textures and graphics.

## Features

✅ **Batch Conversion** - Convert multiple BMP files at once
✅ **Live Preview** - Preview selected BMP files
✅ **Palette Validation** - Automatic validation of 256-color palette
✅ **zlib Compression** - Optional compression for smaller BGF files
✅ **90° Rotation** - For wall textures
✅ **Shrink Factor** - Scaling 1-8
✅ **Offsets** - X/Y offsets for bitmap positioning
✅ **Progress Tracking** - Progress bar for batch conversion

## BMP File Requirements

BGF files require special BMP formats:

### ✅ Required:
- **8-bit indexed colors** (Format8bppIndexed)
- **Exactly 256 colors** in the palette
- Palette must match Meridian 59 standard palette

### ❌ Not Supported:
- 24-bit RGB BMPs
- 16-bit BMPs
- BMPs with less than 256 colors
- BMPs with more than 256 colors

## Preparing BMP Files

### Option 1: With Photoshop/GIMP

1. Open your image
2. **Image → Mode → Indexed Color**
3. Select "256 colors"
4. **Load Color Table** → Load `Data\blakston.pal`
5. **File → Export → BMP**
6. Select "8-bit" and save

### Option 2: With Paint.NET

1. Open your image
2. **Image → Flatten**
3. **Image → Reduce Colors → 256**
4. **File → Save As → BMP**
5. Select "8-bit"

### Palette File

The Meridian 59 standard palette is located at:
```
C:\Users\Rod\Desktop\2\M59AdminTool\Data\blakston.pal
```

## Usage

### 1. Add BMP Files

1. Click **➕ Add...**
2. Select one or more BMP files
3. Files appear in the list with validation status

### 2. Check Preview

- Click on a file in the list
- Preview appears in the middle panel
- Status shows:
  - ✓ **256 colors** (valid)
  - ✗ **Error message** (invalid format)

### 3. Set Output Directory

- Default: `BGF_Output` folder in application directory
- Click **...** to choose a different directory

### 4. Configure Options

#### **Compress (zlib)**
- ☑ **Enabled**: BGF will be compressed (smaller file, recommended)
- ☐ **Disabled**: BGF uncompressed (larger)

#### **Rotate 90°**
- ☑ **Enabled**: For wall textures
- ☐ **Disabled**: Normal (for floor textures, objects)

#### **Shrink**
- Value: 1-8
- Default: 1 (no scaling)
- Higher values = stronger reduction

#### **Name**
- Name for the BGF (max 32 characters)
- Default: "texture"

#### **X/Y Offsets**
- Offsets for bitmap positioning
- Default: 0, 0

### 5. Convert

1. Click **🚀 Convert All Files**
2. Progress bar shows conversion status
3. After completion: Success message with statistics

## Output

For each BMP file, a BGF file is created:

```
Input:  mushroom.bmp
Output: mushroom.bgf
```

BGF files are saved in the selected output directory.

## BGF Format Specification

### File Structure

```
Header:
- Magic: 0x42, 0x47, 0x46, 0x11 ("BGF" + 0x11)
- Version: 10
- Name: 32 Bytes (Null-terminated)
- Num Bitmaps: int32
- Num Groups: int32
- Max Indices: int32
- Shrink Factor: int32

For each Bitmap:
- Width: int32
- Height: int32
- X Offset: int32
- Y Offset: int32
- Num Hotspots: byte
- Hotspot Data: (Number, X, Y) × Num Hotspots
- Compressed Flag: byte (0=uncompressed, 1=compressed)
- Compressed Size: int32
- Pixel Data: byte[]

Groups:
- Num Indices: int32
- Indices: int32[]
```

### Pixel Data

- **8-bit indices** into 256-color palette
- **Row-major** order (row by row, left to right)
- **Optionally compressed** with zlib (DEFLATE)

## Error Handling

### "✗ Bitmap must be 8-bit indexed"

**Problem:** BMP is not in 8-bit indexed mode
**Solution:** Convert BMP to 8-bit indexed (see "Preparing BMP Files")

### "✗ Bitmap must have exactly 256 colors"

**Problem:** BMP doesn't have exactly 256 colors
**Solution:**
1. Reduce colors to 256 in image editor
2. Load Meridian 59 palette (`Data\blakston.pal`)

### "Output directory does not exist"

**Problem:** Selected directory doesn't exist
**Solution:** Choose existing directory or create it

### "Failed to load bitmap"

**Problem:** BMP file is corrupted or invalid format
**Solution:** Open and save BMP again in image editor

## Examples

### Single Floor Texture

1. Add `floor_stone.bmp`
2. Options:
   - ☑ Compress
   - ☐ Rotate
   - Shrink: 1
   - Name: "Stone Floor"
3. Convert → `floor_stone.bgf`

### Wall Texture (90° rotated)

1. Add `wall_brick.bmp`
2. Options:
   - ☑ Compress
   - ☑ Rotate
   - Shrink: 1
   - Name: "Brick Wall"
3. Convert → `wall_brick.bgf`

### Batch: 10 Textures

1. Add all 10 BMPs
2. Set options as needed
3. Convert All
4. All 10 BGFs are created

## Technical Details

### Used Libraries

- **Xein.SharpZipLib** - zlib compression
- **System.Drawing** - BMP processing
- **.NET 8** - Runtime
- **WPF** - UI Framework

### Classes

#### **BGFWriter**
- Writes BGF files
- Supports compression and rotation
- Multi-bitmap support (for animations)

#### **DIBHelper**
- Loads and validates BMPs
- Extracts pixel data
- Palette checks

#### **BGFConverterViewModel**
- MVVM pattern
- Async batch conversion
- Progress tracking
- Error handling

## Location

The BGF Converter is located in the M59AdminTool:

```
Tab: "BGF Converter"
Path: C:\Users\Rod\Desktop\2\M59AdminTool\
```

## Credits

Based on the original Meridian 59 `makebgf` tool by Andrew Kirmse and Chris Kirmse.

Ported and extended for M59AdminTool (C# .NET 8 / WPF).

## Version History

### Version 1.0 (2026-01-14)
- ✅ Initial Release
- ✅ Batch conversion
- ✅ Live preview
- ✅ Palette validation
- ✅ zlib compression
- ✅ 90° rotation
- ✅ Shrink factor
- ✅ X/Y offsets
- ✅ Progress tracking
- ✅ WPF UI with MVVM pattern
