[CmdletBinding()]
param(
  [string]$OutputDir = '.\npc_images',
  [int]$Start = 0,
  [int]$PageSize = 20,
  [switch]$AllPages,
  [int]$MaxPages = 500,
  [int]$DelayMs = 150
)

$ErrorActionPreference = 'Stop'

$BaseUrlTemplate = 'https://wiki.meridian59.de/index.php?show=npc_list&s_link=99&o_id=8&h_id=48&h_name=NPCs&s_meng={0}&s_id=196&start={1}&u_name=NPC%20nach%20Name'

try { [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 } catch {}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

function Get-ExtFromContentType([string]$ct) {
  if ([string]::IsNullOrWhiteSpace($ct)) { return '.bin' }
  $ct = $ct.Split(';')[0].Trim().ToLowerInvariant()
  switch ($ct) {
    'image/png'  { '.png' }
    'image/jpeg' { '.jpg' }
    'image/jpg'  { '.jpg' }
    'image/gif'  { '.gif' }
    'image/webp' { '.webp' }
    'image/bmp'  { '.bmp' }
    default      { '.bin' }
  }
}

function Sanitize-FileName([string]$name) {
  $invalid = [IO.Path]::GetInvalidFileNameChars()
  foreach ($c in $invalid) { $name = $name.Replace($c, '_') }
  return $name.Trim()
}

$BgfUrlRegex = [regex]@'
https?://bgf\.meridian59\.de/[^\s"'<>]+
'@

$seen = New-Object 'System.Collections.Generic.HashSet[string]'
$totalDownloaded = 0
$page = 0
$currentStart = $Start

while ($true) {

  if ($page -ge $MaxPages) {
    Write-Warning ('MaxPages ({0}) erreicht – Abbruch.' -f $MaxPages)
    break
  }

  $listUrl = [string]::Format($BaseUrlTemplate, $PageSize, $currentStart)
  Write-Host ('==> Seite laden: start={0}' -f $currentStart) -ForegroundColor Cyan

  $resp = Invoke-WebRequest -Uri $listUrl -MaximumRedirection 5
  Start-Sleep -Milliseconds $DelayMs

  $imgLinks = ($BgfUrlRegex.Matches($resp.Content) | ForEach-Object { $_.Value }) | Select-Object -Unique

  $newLinks = @()
  foreach ($u in $imgLinks) {
    if ($seen.Add($u)) { $newLinks += $u }
  }

  if (-not $newLinks -or $newLinks.Count -eq 0) {
    Write-Host 'Keine neuen Bildlinks gefunden.' -ForegroundColor Yellow
    break
  }

  Write-Host ('Gefunden: {0} neue Bildlinks' -f $newLinks.Count) -ForegroundColor Green

  $i = 0
  foreach ($imgUrl in $newLinks) {
    $i++
    $tmp = $null

    try {
      $uri = [Uri]$imgUrl
      $lastSeg = $uri.Segments[-1].TrimEnd('/')
      if ([string]::IsNullOrWhiteSpace($lastSeg)) { $lastSeg = 'image' }

      $tmp = Join-Path $OutputDir ('_tmp_{0:000000}' -f ($totalDownloaded + 1))
      $downloadResp = Invoke-WebRequest -Uri $imgUrl -OutFile $tmp -PassThru -MaximumRedirection 5

      $ext = Get-ExtFromContentType $downloadResp.Headers['Content-Type']
      $safeBase = Sanitize-FileName $lastSeg

      $prefix = ('npc_{0:000000}_' -f ($totalDownloaded + 1))
      $finalName = $prefix + $safeBase + $ext
      $finalPath = Join-Path $OutputDir $finalName

      Move-Item -Force -Path $tmp -Destination $finalPath
      $totalDownloaded++

      Write-Host ('  [{0}/{1}] OK -> {2}' -f $i, $newLinks.Count, $finalName)
      Start-Sleep -Milliseconds $DelayMs
    }
    catch {
      Write-Warning ('  [{0}/{1}] FEHLER bei {2}: {3}' -f $i, $newLinks.Count, $imgUrl, $_.Exception.Message)
      if ($tmp -and (Test-Path $tmp)) { Remove-Item -Force $tmp -ErrorAction SilentlyContinue }
    }
  }

  if (-not $AllPages) { break }

  $page++
  $currentStart += $PageSize
}

Write-Host ''
Write-Host ('Fertig. Insgesamt heruntergeladen: {0}' -f $totalDownloaded) -ForegroundColor Magenta
Write-Host ('Zielordner: {0}' -f (Resolve-Path $OutputDir).Path) -ForegroundColor Magenta
