# download-tools.ps1
# Downloads yt-dlp executable for bundling with YTDownloaderUI

$ErrorActionPreference = "Stop"

$toolsDir = Join-Path $PSScriptRoot "src\YTDownloaderUI\tools"
$licensesDir = Join-Path $toolsDir "LICENSES"

# Create directories if they don't exist
if (-not (Test-Path $toolsDir)) {
    New-Item -ItemType Directory -Path $toolsDir -Force | Out-Null
}
if (-not (Test-Path $licensesDir)) {
    New-Item -ItemType Directory -Path $licensesDir -Force | Out-Null
}

Write-Host "Downloading tools to: $toolsDir" -ForegroundColor Cyan

# Download yt-dlp
Write-Host "`nDownloading yt-dlp..." -ForegroundColor Yellow
$ytDlpUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe"
$ytDlpPath = Join-Path $toolsDir "yt-dlp.exe"
Invoke-WebRequest -Uri $ytDlpUrl -OutFile $ytDlpPath
Write-Host "  Downloaded: $ytDlpPath" -ForegroundColor Green

# Download yt-dlp license
Write-Host "  Downloading yt-dlp license..." -ForegroundColor Yellow
$ytDlpLicenseUrl = "https://raw.githubusercontent.com/yt-dlp/yt-dlp/master/LICENSE"
$ytDlpLicensePath = Join-Path $licensesDir "yt-dlp-LICENSE.txt"
Invoke-WebRequest -Uri $ytDlpLicenseUrl -OutFile $ytDlpLicensePath
Write-Host "  Downloaded: $ytDlpLicensePath" -ForegroundColor Green

Write-Host "`nDone! yt-dlp downloaded to: $toolsDir" -ForegroundColor Cyan
Write-Host "`nFiles:"
Get-ChildItem -Path $toolsDir -File | ForEach-Object { Write-Host "  - $($_.Name)" }
Write-Host "`nLicenses:"
Get-ChildItem -Path $licensesDir -File | ForEach-Object { Write-Host "  - $($_.Name)" }

Write-Host "`n--- FFmpeg (Optional) ---" -ForegroundColor Yellow
Write-Host "For audio extraction (mp3/aac presets), download FFmpeg separately:"
Write-Host "  1. Visit: https://www.ffmpeg.org/download.html"
Write-Host "  2. Download the static build for Windows."
Write-Host "  3. Extract ffmpeg.exe and ffprobe.exe to: $toolsDir"
