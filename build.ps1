# AutomationProfileManager Build Script
# Compiles the project and creates the .pext package

$ErrorActionPreference = "Stop"

Write-Host "=== AutomationProfileManager Build Script ===" -ForegroundColor Cyan
Write-Host ""

# Read version from extension.yaml
Write-Host "Reading version from extension.yaml..." -ForegroundColor Yellow
$versionLine = Select-String -Path "extension.yaml" -Pattern "^\s*Version\s*:" | Select-Object -First 1
if (-not $versionLine) {
    Write-Host "ERROR: Version not found in extension.yaml" -ForegroundColor Red
    exit 1
}
$version = ($versionLine.Line -split ":", 2)[1].Trim()
if (-not $version) {
    Write-Host "ERROR: Parsed version is empty" -ForegroundColor Red
    exit 1
}
Write-Host "Version: $version" -ForegroundColor Green
Write-Host ""

# Find MSBuild
Write-Host "Locating MSBuild..." -ForegroundColor Yellow
$msbuildPaths = @(
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
)

$msbuild = $null
foreach ($path in $msbuildPaths) {
    if (Test-Path $path) {
        $msbuild = $path
        break
    }
}

if (-not $msbuild) {
    Write-Host "ERROR: MSBuild not found. Please install Visual Studio." -ForegroundColor Red
    exit 1
}

Write-Host "Found MSBuild: $msbuild" -ForegroundColor Green
Write-Host ""

# Build the project
Write-Host "Building project (Release configuration)..." -ForegroundColor Yellow
& $msbuild "AutomationProfileManager.csproj" /t:Rebuild /p:Configuration=Release /v:minimal /nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host ""

# Check if output exists
$releaseDir = "bin\Release"
if (-not (Test-Path $releaseDir)) {
    Write-Host "ERROR: Release output folder not found: $releaseDir" -ForegroundColor Red
    exit 1
}

$dllPath = Join-Path $releaseDir "AutomationProfileManager.dll"
if (-not (Test-Path $dllPath)) {
    Write-Host "ERROR: AutomationProfileManager.dll not found in $releaseDir" -ForegroundColor Red
    exit 1
}

# Create .pext package
Write-Host "Creating .pext package..." -ForegroundColor Yellow

$pextName = "AutomationProfileManager_v$version.pext"
$zipName = "AutomationProfileManager_v$version.zip"

# Remove old files if they exist
if (Test-Path $zipName) {
    Remove-Item $zipName -Force
    Write-Host "Removed old zip file" -ForegroundColor Gray
}
if (Test-Path $pextName) {
    Remove-Item $pextName -Force
    Write-Host "Removed old .pext file" -ForegroundColor Gray
}

# Collect files to package
$files = @(
    $dllPath,
    "extension.yaml"
)

if (Test-Path "icon.png") {
    $files += "icon.png"
    Write-Host "Including icon.png" -ForegroundColor Gray
}

# Create zip archive
Write-Host "Compressing files..." -ForegroundColor Yellow
Compress-Archive -Path $files -DestinationPath $zipName -Force

# Rename to .pext
Rename-Item -Path $zipName -NewName $pextName -Force

# Verify
if (-not (Test-Path $pextName)) {
    Write-Host "ERROR: .pext file was not created!" -ForegroundColor Red
    exit 1
}

$pextFile = Get-Item $pextName
Write-Host ""
Write-Host "=== Build Complete ===" -ForegroundColor Cyan
Write-Host "Package: $pextName" -ForegroundColor Green
Write-Host "Size: $([math]::Round($pextFile.Length / 1KB, 2)) KB" -ForegroundColor Green
Write-Host "Location: $($pextFile.FullName)" -ForegroundColor Green
Write-Host ""
