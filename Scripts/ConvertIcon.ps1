# Convert BMP to ICO
Add-Type -AssemblyName System.Drawing

$bmpPath = "C:\Users\Rod\Desktop\2\M59AdminTool\icon1.bmp"
$icoPath = "C:\Users\Rod\Desktop\2\M59AdminTool\icon1.ico"

# Load BMP
$bmp = [System.Drawing.Bitmap]::new($bmpPath)

# Convert to Icon
$icon = [System.Drawing.Icon]::FromHandle($bmp.GetHicon())

# Save as ICO
$file = [System.IO.File]::Create($icoPath)
$icon.Save($file)
$file.Close()

# Cleanup
$bmp.Dispose()

Write-Host "Icon converted successfully: $icoPath"
