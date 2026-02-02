#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Quick test script for Screen Capture Agent

.DESCRIPTION
    Runs a series of tests to verify the Screen Capture Agent is working correctly
#>

$ErrorActionPreference = 'Continue'

Write-Host @"
╔══════════════════════════════════════════════════════════════╗
║        Screen Capture Agent - Test Suite                    ║
╚══════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan

# Find the executable
$ExePath = ""
if (Test-Path ".\ScreenCapture.exe") {
    $ExePath = ".\ScreenCapture.exe"
} elseif (Test-Path ".\publish\Release\ScreenCapture.exe") {
    $ExePath = ".\publish\Release\ScreenCapture.exe"
} else {
    Write-Host "❌ ScreenCapture.exe not found!" -ForegroundColor Red
    Write-Host "   Please build the project first using build.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found executable: $ExePath" -ForegroundColor Green
Write-Host ""

# Create test directory
$TestDir = Join-Path $env:TEMP "ScreenCaptureTests"
if (Test-Path $TestDir) {
    Remove-Item $TestDir -Recurse -Force
}
New-Item -ItemType Directory -Path $TestDir | Out-Null

Write-Host "Test directory: $TestDir" -ForegroundColor Cyan
Write-Host ""

# Test counter
$TestCount = 0
$PassCount = 0
$FailCount = 0

function Test-Capture {
    param(
        [string]$Name,
        [string[]]$Arguments
    )
    
    $script:TestCount++
    Write-Host "Test $TestCount - $Name" -ForegroundColor Yellow
    Write-Host "  Command: ScreenCapture.exe $($Arguments -join ' ')" -ForegroundColor Gray
    
    $output = & $ExePath @Arguments 2>&1
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -eq 0) {
        Write-Host "  ✓ PASS (Exit Code: $exitCode)" -ForegroundColor Green
        $script:PassCount++
        return $true
    } else {
        Write-Host "  ✗ FAIL (Exit Code: $exitCode)" -ForegroundColor Red
        Write-Host "  Output: $output" -ForegroundColor Gray
        $script:FailCount++
        return $false
    }
}

# Run tests
Write-Host "Running Tests..." -ForegroundColor Cyan
Write-Host ("-" * 70) -ForegroundColor Gray
Write-Host ""

# Test 1: Help
Test-Capture "Display Help" @("--help")

# Test 2: List Windows
Write-Host ""
Test-Capture "List Available Windows" @("--list-windows")

# Test 3: Full Screen Capture
Write-Host ""
Test-Capture "Full Screen Capture" @("-m", "full", "-d", $TestDir, "-p", "test_fullscreen")

# Test 4: Active Window Capture
Write-Host ""
Test-Capture "Active Window Capture" @("-m", "active", "-d", $TestDir, "-p", "test_active")

# Test 5: Multi-Monitor Capture
Write-Host ""
Test-Capture "Multi-Monitor Capture" @("-m", "multi", "-d", $TestDir, "-p", "test_multi")

# Test 6: Custom Output Path
Write-Host ""
$customPath = Join-Path $TestDir "custom_screenshot.png"
Test-Capture "Custom Output Path" @("-o", $customPath)

# Test 7: Verbose Mode
Write-Host ""
Test-Capture "Verbose Mode" @("-m", "full", "-d", $TestDir, "-p", "test_verbose", "-v")

# Test 8: With Delay
Write-Host ""
Test-Capture "Capture with Delay" @("-m", "full", "-d", $TestDir, "-p", "test_delay", "--delay", "1000")

# Test 9: GDI Fallback
Write-Host ""
Test-Capture "GDI Fallback Mode" @("-m", "full", "-d", $TestDir, "-p", "test_gdi", "--no-winrt")

Write-Host ""
Write-Host ("-" * 70) -ForegroundColor Gray
Write-Host ""

# Show results
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "  Total Tests: $TestCount" -ForegroundColor White
Write-Host "  Passed: $PassCount" -ForegroundColor Green
Write-Host "  Failed: $FailCount" -ForegroundColor Red
Write-Host ""

# Check captured files
$capturedFiles = Get-ChildItem $TestDir -Filter "*.png"
Write-Host "Captured Screenshots: $($capturedFiles.Count)" -ForegroundColor Cyan

if ($capturedFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "Sample screenshots:" -ForegroundColor Yellow
    foreach ($file in $capturedFiles | Select-Object -First 5) {
        $size = [math]::Round($file.Length / 1KB, 2)
        Write-Host "  - $($file.Name) ($size KB)" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "📁 Open test directory: $TestDir" -ForegroundColor Cyan
    
    # Optionally open the folder
    $openFolder = Read-Host "Open test directory? (y/n)"
    if ($openFolder -eq 'y') {
        Start-Process explorer.exe $TestDir
    }
}

Write-Host ""
if ($FailCount -eq 0) {
    Write-Host "✅ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Some tests failed. Please review the output above." -ForegroundColor Yellow
}

Write-Host ""
