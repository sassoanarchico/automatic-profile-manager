# Build PEXT Script per Automation Profile Manager

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Paths
$ProjectDir = $PSScriptRoot
$OutputDir = Join-Path $ProjectDir "bin\$Configuration"
$PextOutputDir = Join-Path $ProjectDir "pext-output"
$PextFileName = "AutomationProfileManager.pext"

Write-Host "=== Building Automation Profile Manager Extension ===" -ForegroundColor Cyan

# Step 1: Verify build output exists
Write-Host "`n[1/4] Verifying build output..." -ForegroundColor Yellow
if (-not (Test-Path (Join-Path $OutputDir "AutomationProfileManager.dll"))) {
    Write-Host "Build output not found! Please build the project first." -ForegroundColor Red
    exit 1
}
Write-Host "Build output found!" -ForegroundColor Green

# Step 2: Prepare output directory
Write-Host "`n[2/4] Preparing output directory..." -ForegroundColor Yellow
if (Test-Path $PextOutputDir) {
    Remove-Item $PextOutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $PextOutputDir | Out-Null

# Step 3: Copy required files
Write-Host "`n[3/4] Copying files to pext package..." -ForegroundColor Yellow

# Copy main DLL
Copy-Item (Join-Path $OutputDir "AutomationProfileManager.dll") $PextOutputDir
Write-Host "  - AutomationProfileManager.dll" -ForegroundColor Gray

# Copy extension.yaml
Copy-Item (Join-Path $OutputDir "extension.yaml") $PextOutputDir
Write-Host "  - extension.yaml" -ForegroundColor Gray

# Copy Newtonsoft.Json (dependency)
$newtonsoftPath = Join-Path $OutputDir "Newtonsoft.Json.dll"
if (Test-Path $newtonsoftPath) {
    Copy-Item $newtonsoftPath $PextOutputDir
    Write-Host "  - Newtonsoft.Json.dll" -ForegroundColor Gray
}

# Step 4: Create ZIP (rename to .pext)
Write-Host "`n[4/4] Creating .pext package..." -ForegroundColor Yellow

$ZipPath = Join-Path $ProjectDir $PextFileName
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

# Create the zip
$TempZipPath = Join-Path $ProjectDir "AutomationProfileManager.zip"
Compress-Archive -Path "$PextOutputDir\*" -DestinationPath $TempZipPath -Force
Rename-Item $TempZipPath $PextFileName

Write-Host "`n=== Build Complete ===" -ForegroundColor Cyan
Write-Host "Output file: $ZipPath" -ForegroundColor Green
Write-Host "`nTo install:" -ForegroundColor Yellow
Write-Host "  1. Open Playnite" -ForegroundColor Gray
Write-Host "  2. Go to Menu > Add-ons... > Install from file" -ForegroundColor Gray
Write-Host "  3. Select: $ZipPath" -ForegroundColor Gray

# Cleanup temp folder
Remove-Item $PextOutputDir -Recurse -Force

Write-Host "`nDone!" -ForegroundColor Green
