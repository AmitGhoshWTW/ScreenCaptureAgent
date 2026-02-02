# Screen Capture Agent

**Enterprise-Grade Screenshot Automation Tool for Ticketing Systems**

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## 🎯 Overview

Screen Capture Agent is a production-ready, self-contained executable designed for automatic screenshot capture in agent ticketing workflows. Built with .NET 8 and WinRT GraphicsCapture API, it provides reliable screen capture capabilities with no black screens in Chrome/Edge browsers.

### ✨ Key Features

✅ **Full-screen capture** - Capture entire primary screen  
✅ **Active window capture** - Automatically capture the focused window  
✅ **Window by title** - Find and capture specific windows  
✅ **Region capture** - Capture custom rectangular areas  
✅ **Multi-monitor support** - Capture all screens or specific monitors  
✅ **PNG output** - High-quality, lossless image format  
✅ **WinRT GraphicsCapture API** - Modern, reliable capture technology  
✅ **No black screens** - Properly captures Chrome, Edge, and other browsers  
✅ **Single EXE** - Self-contained, no dependencies required  
✅ **Browser tab capture** - Captures browser content correctly  
✅ **Command-line automation** - Perfect for scripting and integration  
✅ **Exit codes** - Proper error reporting for automation workflows  

## 📋 Requirements

- **Operating System**: Windows 10 (version 1903+) or Windows 11
- **Runtime**: None required - self-contained executable
- **.NET SDK**: Only required for building (not for running the .exe)

## 🚀 Quick Start

### Download & Run

1. Download `ScreenCapture.exe` from the releases
2. Run from command line:

```cmd
ScreenCapture.exe --help
```

### Basic Usage

```powershell
# Capture full screen
ScreenCapture.exe

# Capture active window
ScreenCapture.exe -m active

# Capture window by title
ScreenCapture.exe -m window -w "Google Chrome"

# Capture to specific location
ScreenCapture.exe -o "C:\Screenshots\screenshot.png"

# Capture with custom prefix
ScreenCapture.exe -d "C:\Tickets" -p "ticket_12345"
```

## 📖 Usage Guide

### Capture Modes

| Mode | Description | Example |
|------|-------------|---------|
| `full` | Full screen (primary monitor) | `-m full` |
| `active` | Currently active/focused window | `-m active` |
| `window` | Window by title (partial match) | `-m window -w "Chrome"` |
| `region` | Specific rectangular area | `-m region -r 100 100 800 600` |
| `multi` | All monitors combined | `-m multi` |
| `monitor` | Specific monitor by index | `-m monitor --monitor 1` |

### Command-Line Options

#### Capture Modes
```
-m, --mode <mode>          Capture mode (default: fullscreen)
                           Options: full, active, window, region, multi, monitor
```

#### Output Options
```
-o, --output <path>        Output file path (auto-generated if not specified)
-d, --directory <path>     Output directory (default: current directory)
-p, --prefix <name>        Filename prefix (default: 'screenshot')
--quality <0-100>          Image quality for JPEG (default: 95)
```

#### Window Capture
```
-w, --window <title>       Window title to capture (partial match supported)
```

#### Region Capture
```
-r, --region <x y w h>     Capture specific region (X Y Width Height in pixels)
```

#### Monitor Options
```
--monitor <index>          Capture specific monitor (0-based index)
```

#### Advanced Options
```
--delay <ms>               Delay before capture in milliseconds
--cursor                   Include mouse cursor in capture
--no-winrt                 Disable WinRT capture (use GDI+ fallback)
-v, --verbose              Enable verbose logging
```

#### Utility Commands
```
--list-windows             List all available windows
-h, --help                 Show help message
```

### Exit Codes

| Code | Meaning | Description |
|------|---------|-------------|
| 0 | Success | Screenshot captured successfully |
| 1 | General Error | Unexpected error occurred |
| 2 | Invalid Arguments | Command-line arguments are invalid |
| 3 | Capture Failed | Screenshot capture operation failed |

## 💼 Integration Examples

### PowerShell - Bug Tracking Integration

```powershell
# Automatic screenshot for bug reports
function Submit-BugReport {
    param(
        [string]$TicketId,
        [string]$Description
    )
    
    $screenshotDir = "C:\Tickets\$TicketId"
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    
    # Capture active window
    $result = & ScreenCapture.exe `
        -m active `
        -d $screenshotDir `
        -p "bug_${TicketId}_${timestamp}" `
        -v
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Screenshot captured successfully" -ForegroundColor Green
        
        # Get the screenshot path from output
        $screenshotPath = Get-ChildItem $screenshotDir -Filter "bug_${TicketId}*.png" | 
            Select-Object -First 1 -ExpandProperty FullName
        
        # Attach to ticket system (example)
        Add-JiraAttachment -TicketId $TicketId -FilePath $screenshotPath
        
        return $screenshotPath
    }
    else {
        Write-Error "Screenshot capture failed with exit code $LASTEXITCODE"
        return $null
    }
}

# Usage
Submit-BugReport -TicketId "PROJ-1234" -Description "UI rendering issue"
```

### C# - Application Integration

```csharp
using System.Diagnostics;

public class ScreenshotService
{
    private readonly string _screenshotExePath;
    private readonly string _outputDirectory;
    
    public ScreenshotService(string exePath, string outputDir)
    {
        _screenshotExePath = exePath;
        _outputDirectory = outputDir;
    }
    
    public async Task<string?> CaptureActiveWindowAsync(string ticketId)
    {
        var prefix = $"ticket_{ticketId}";
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _screenshotExePath,
                Arguments = $"-m active -d \"{_outputDirectory}\" -p \"{prefix}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        
        process.Start();
        await process.WaitForExitAsync();
        
        if (process.ExitCode == 0)
        {
            // Find the generated screenshot file
            var files = Directory.GetFiles(_outputDirectory, $"{prefix}*.png");
            return files.OrderByDescending(File.GetCreationTime).FirstOrDefault();
        }
        
        return null;
    }
}

// Usage
var service = new ScreenshotService(@"C:\Tools\ScreenCapture.exe", @"C:\Screenshots");
var screenshotPath = await service.CaptureActiveWindowAsync("TICKET-5678");
```

### Batch Script - Simple Integration

```batch
@echo off
setlocal

set TICKET_ID=%1
set SCREENSHOT_DIR=C:\Tickets\%TICKET_ID%

if not exist "%SCREENSHOT_DIR%" mkdir "%SCREENSHOT_DIR%"

echo Capturing screenshot for ticket %TICKET_ID%...

ScreenCapture.exe -m active -d "%SCREENSHOT_DIR%" -p "ticket_%TICKET_ID%"

if %ERRORLEVEL% EQU 0 (
    echo Screenshot captured successfully
    echo Location: %SCREENSHOT_DIR%
) else (
    echo Failed to capture screenshot
    exit /b 1
)

endlocal
```

### Python Integration

```python
import subprocess
import os
from pathlib import Path

def capture_screenshot(ticket_id: str, mode: str = "active") -> str:
    """
    Capture screenshot for ticket
    
    Args:
        ticket_id: Ticket identifier
        mode: Capture mode (active, full, window, etc.)
    
    Returns:
        Path to captured screenshot or None if failed
    """
    output_dir = f"C:\\Tickets\\{ticket_id}"
    Path(output_dir).mkdir(parents=True, exist_ok=True)
    
    prefix = f"ticket_{ticket_id}"
    
    cmd = [
        "ScreenCapture.exe",
        "-m", mode,
        "-d", output_dir,
        "-p", prefix
    ]
    
    result = subprocess.run(cmd, capture_output=True, text=True)
    
    if result.returncode == 0:
        # Find the generated file
        screenshots = list(Path(output_dir).glob(f"{prefix}*.png"))
        if screenshots:
            return str(max(screenshots, key=os.path.getctime))
    
    return None

# Usage
screenshot_path = capture_screenshot("PROJ-9012", mode="active")
if screenshot_path:
    print(f"Screenshot saved: {screenshot_path}")
```

## 🔧 Building from Source

### Prerequisites

- .NET 8 SDK or later
- Visual Studio 2022 (optional, for IDE experience)
- Windows 10/11

### Build Steps

#### Using PowerShell

```powershell
# Clone the repository
git clone https://github.com/your-org/screen-capture-agent.git
cd screen-capture-agent

# Run build script
.\build.ps1 -Configuration Release

# Output will be in: publish\Release\ScreenCapture.exe
```

#### Using Command Prompt

```cmd
REM Clone the repository
git clone https://github.com/your-org/screen-capture-agent.git
cd screen-capture-agent

REM Run build script
build.bat

REM Output will be in: publish\Release\ScreenCapture.exe
```

#### Manual Build

```powershell
# Restore dependencies
dotnet restore

# Build solution
dotnet build --configuration Release

# Publish as single-file executable
dotnet publish ScreenCaptureAgent\ScreenCaptureAgent.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output publish\Release `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true
```

## 🏗️ Architecture

### Project Structure

```
ScreenCaptureAgent/
├── ScreenCaptureAgent.sln              # Visual Studio solution
├── ScreenCaptureAgent/                 # Console application
│   ├── Program.cs                      # Entry point
│   ├── CLI/
│   │   └── CommandLineParser.cs        # Command-line parsing
│   └── ScreenCaptureAgent.csproj
├── ScreenCaptureAgent.Core/            # Core library
│   ├── Models/
│   │   ├── CaptureMode.cs              # Capture mode enumeration
│   │   ├── CaptureOptions.cs           # Configuration options
│   │   ├── CaptureResult.cs            # Result model
│   │   └── WindowInfo.cs               # Window information
│   ├── Services/
│   │   ├── ScreenCaptureService.cs     # Main orchestration service
│   │   ├── WinRtCaptureService.cs      # WinRT capture implementation
│   │   └── GdiCaptureService.cs        # GDI+ fallback
│   ├── Native/
│   │   └── NativeMethods.cs            # Windows API interop
│   └── ScreenCaptureAgent.Core.csproj
├── build.ps1                           # PowerShell build script
├── build.bat                           # Batch build script
└── README.md                           # This file
```

### Technology Stack

- **.NET 8.0** - Latest .NET runtime
- **Windows.Graphics.Capture** - Modern WinRT capture API
- **SharpDX** - Direct3D11 interop for WinRT
- **System.Drawing.Common** - GDI+ fallback and image processing
- **Windows Forms** - Monitor and screen information

### Key Design Decisions

1. **WinRT GraphicsCapture API**: Modern API that properly captures browser content without black screens
2. **GDI+ Fallback**: Ensures compatibility when WinRT is unavailable
3. **Self-Contained Deployment**: No runtime dependencies for easy distribution
4. **Single File Executable**: Simplified deployment and usage
5. **Exit Codes**: Proper integration with automation tools
6. **Verbose Logging**: Detailed diagnostics for troubleshooting

## 🐛 Troubleshooting

### Common Issues

#### Black Screen in Chrome/Edge
**Solution**: Ensure WinRT capture is enabled (it's the default):
```cmd
ScreenCapture.exe -m window -w "Chrome"
```

If still having issues, check Windows version (need 1903+).

#### "Failed to capture screenshot"
**Troubleshooting steps**:
1. Run with verbose logging:
   ```cmd
   ScreenCapture.exe -m active -v
   ```
2. Check if window is visible:
   ```cmd
   ScreenCapture.exe --list-windows
   ```
3. Try GDI+ fallback:
   ```cmd
   ScreenCapture.exe -m active --no-winrt
   ```

#### Permission Denied
**Solution**: Run as Administrator for certain protected windows:
```cmd
Right-click > Run as Administrator
```

#### Window Not Found
**Solution**: Use partial title matching or list windows:
```cmd
# List all windows
ScreenCapture.exe --list-windows

# Use partial match
ScreenCapture.exe -m window -w "Chrome"  # Matches "Google Chrome - Page Title"
```

## 📊 Performance

### Benchmarks

| Capture Mode | Typical Duration | Memory Usage |
|--------------|------------------|--------------|
| Full Screen (1920x1080) | 50-100ms | ~30MB |
| Active Window | 40-80ms | ~25MB |
| Region | 30-60ms | ~20MB |
| Multi-Monitor (2x 1920x1080) | 100-150ms | ~50MB |

*Benchmarks performed on Windows 11, Intel i7-11th gen, 16GB RAM*

## 🔒 Security Considerations

- **No Network Activity**: Tool operates entirely offline
- **No Data Collection**: No telemetry or analytics
- **Local File System Only**: Screenshots saved locally
- **Protected Windows**: Some system windows may require elevation
- **Screen Recording Permission**: May require "Graphics Capture" permission on Windows 11

## 📝 Best Practices

### For Ticketing Integration

1. **Consistent Naming**: Use ticket ID in filename prefix
   ```cmd
   ScreenCapture.exe -p "ticket_${TICKET_ID}"
   ```

2. **Organized Storage**: Create ticket-specific directories
   ```cmd
   ScreenCapture.exe -d "C:\Tickets\${TICKET_ID}"
   ```

3. **Error Handling**: Always check exit codes
   ```powershell
   if ($LASTEXITCODE -ne 0) { 
       # Handle error 
   }
   ```

4. **Logging**: Use verbose mode for diagnostics
   ```cmd
   ScreenCapture.exe -v >> capture.log 2>&1
   ```

### For Automation

1. **Delays**: Add delay for menu/popup captures
   ```cmd
   ScreenCapture.exe --delay 2000
   ```

2. **Window Verification**: List windows before capture
   ```cmd
   ScreenCapture.exe --list-windows
   ```

3. **Fallback Strategy**: Implement multiple capture strategies
   ```powershell
   # Try specific window first
   ScreenCapture.exe -m window -w "MyApp"
   if ($LASTEXITCODE -ne 0) {
       # Fallback to active window
       ScreenCapture.exe -m active
   }
   ```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or create issues for bugs and feature requests.

### Development Setup

1. Clone the repository
2. Open `ScreenCaptureAgent.sln` in Visual Studio 2022
3. Restore NuGet packages
4. Build and run

### Code Style

- Follow C# coding conventions
- Use meaningful variable names
- Add XML documentation comments for public APIs
- Include unit tests for new features

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Built with .NET 8 and Windows App SDK
- Uses SharpDX for Direct3D11 interop
- Inspired by enterprise ticketing workflows

## 📞 Support

For issues and questions:
- Create an issue on GitHub
- Check the troubleshooting section
- Review examples for common scenarios

---

**Built with ❤️ for enterprise automation and ticketing workflows**
