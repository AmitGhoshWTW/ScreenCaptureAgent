#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build script for Screen Capture Agent

.DESCRIPTION
    Builds the Screen Capture Agent as a self-contained single executable

.PARAMETER Configuration
    Build configuration (Debug or Release)

.PARAMETER Clean
    Clean before building

.EXAMPLE
    .\build.ps1 -Configuration Release
#>

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'

Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║        Screen Capture Agent - Build Script                  ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$SolutionDir = $PSScriptRoot
$ProjectPath = Join-Path $SolutionDir "ScreenCaptureAgent\ScreenCaptureAgent.csproj"
$OutputDir = Join-Path $SolutionDir "bin\$Configuration"

# Clean if requested
if ($Clean) {
    Write-Host "🧹 Cleaning..." -ForegroundColor Yellow
    dotnet clean "$SolutionDir\ScreenCaptureAgent.sln" --configuration $Configuration
    if (Test-Path $OutputDir) {
        Remove-Item $OutputDir -Recurse -Force
    }
}

# Restore dependencies
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore "$SolutionDir\ScreenCaptureAgent.sln"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Restore failed!" -ForegroundColor Red
    exit 1
}

# Build
Write-Host "🔨 Building $Configuration configuration..." -ForegroundColor Yellow
dotnet build "$SolutionDir\ScreenCaptureAgent.sln" --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Publish as single-file executable
Write-Host "📦 Publishing self-contained executable..." -ForegroundColor Yellow
$PublishDir = Join-Path $SolutionDir "publish\$Configuration"

dotnet publish $ProjectPath `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    --output $PublishDir `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Publish failed!" -ForegroundColor Red
    exit 1
}

# Show results
Write-Host ""
Write-Host "✅ Build completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Output directory: $PublishDir" -ForegroundColor Cyan

$ExePath = Join-Path $PublishDir "ScreenCapture.exe"
if (Test-Path $ExePath) {
    $FileInfo = Get-Item $ExePath
    Write-Host "Executable: ScreenCapture.exe" -ForegroundColor Cyan
    Write-Host "Size: $([math]::Round($FileInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
    Write-Host "Path: $ExePath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📋 Quick test commands:" -ForegroundColor Yellow
    Write-Host "  cd `"$PublishDir`"" -ForegroundColor Gray
    Write-Host "  .\ScreenCapture.exe --help" -ForegroundColor Gray
    Write-Host "  .\ScreenCapture.exe --list-windows" -ForegroundColor Gray
    Write-Host "  .\ScreenCapture.exe -m active -v" -ForegroundColor Gray
}

Write-Host ""
