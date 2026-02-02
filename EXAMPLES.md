# Usage Examples

## Basic Examples

### 1. Capture Full Screen
```cmd
ScreenCapture.exe
```
Output: `screenshot_20250131_143025_123.png` in current directory

### 2. Capture Active Window
```cmd
ScreenCapture.exe -m active
```
Captures the currently focused window

### 3. Capture Window by Title
```cmd
ScreenCapture.exe -m window -w "Google Chrome"
```
Finds and captures Chrome window (partial match works)

### 4. Capture to Specific Location
```cmd
ScreenCapture.exe -o "C:\Screenshots\error_report.png"
```
Saves to exact path specified

### 5. Capture with Custom Directory and Prefix
```cmd
ScreenCapture.exe -d "C:\Tickets" -p "ticket_12345"
```
Output: `C:\Tickets\ticket_12345_20250131_143025_123.png`

## Advanced Examples

### 6. Capture Specific Region
```cmd
ScreenCapture.exe -m region -r 100 100 800 600
```
Captures region starting at (100,100) with size 800x600

### 7. Capture Second Monitor
```cmd
ScreenCapture.exe -m monitor --monitor 1
```
Captures monitor at index 1 (0-based)

### 8. Capture All Monitors
```cmd
ScreenCapture.exe -m multi
```
Creates one image containing all monitors

### 9. Capture with Delay
```cmd
ScreenCapture.exe -m active --delay 3000
```
Waits 3 seconds before capturing (useful for menus/dialogs)

### 10. Verbose Output
```cmd
ScreenCapture.exe -m active -v
```
Shows detailed logging of the capture process

## Integration Examples

### PowerShell - Loop Through Windows
```powershell
# Capture all Chrome windows
$windows = @("Chrome - Page 1", "Chrome - Page 2", "Chrome - Page 3")

foreach ($window in $windows) {
    $prefix = $window -replace '[^\w]', '_'
    ScreenCapture.exe -m window -w $window -d "C:\Captures" -p $prefix
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Captured: $window" -ForegroundColor Green
    }
}
```

### PowerShell - Error Reporting Workflow
```powershell
function New-ErrorReport {
    param(
        [string]$ApplicationName,
        [string]$ErrorMessage
    )
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $reportDir = "C:\ErrorReports\$timestamp"
    
    New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
    
    # Capture active window
    Write-Host "Capturing screenshot..." -ForegroundColor Yellow
    ScreenCapture.exe -m active -d $reportDir -p "error_screenshot"
    
    if ($LASTEXITCODE -eq 0) {
        # Create error report
        $report = @{
            Timestamp = $timestamp
            Application = $ApplicationName
            Error = $ErrorMessage
            Screenshot = (Get-ChildItem $reportDir -Filter "*.png" | Select-Object -First 1).FullName
        } | ConvertTo-Json
        
        $report | Out-File "$reportDir\error_report.json"
        
        Write-Host "Error report created: $reportDir" -ForegroundColor Green
        return $reportDir
    }
}

# Usage
New-ErrorReport -ApplicationName "CRM System" -ErrorMessage "Failed to save customer record"
```

### Batch Script - Scheduled Screenshot
```batch
@echo off
setlocal

:: Configuration
set OUTPUT_DIR=C:\ScheduledScreenshots
set DATE_STR=%date:~-4%%date:~-10,2%%date:~-7,2%
set TIME_STR=%time:~0,2%%time:~3,2%%time:~6,2%
set TIMESTAMP=%DATE_STR%_%TIME_STR: =0%

:: Create directory
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

:: Capture full screen
ScreenCapture.exe -m full -d "%OUTPUT_DIR%" -p "scheduled_%TIMESTAMP%"

if %ERRORLEVEL% EQU 0 (
    echo Screenshot captured successfully
) else (
    echo Failed to capture screenshot
    exit /b 1
)

endlocal
```

### Python - Multi-Application Monitoring
```python
import subprocess
import time
from datetime import datetime
from pathlib import Path

class ScreenshotMonitor:
    def __init__(self, exe_path, output_dir):
        self.exe_path = exe_path
        self.output_dir = Path(output_dir)
        self.output_dir.mkdir(parents=True, exist_ok=True)
    
    def monitor_applications(self, app_titles, interval_seconds=300):
        """Monitor and capture specified applications at intervals"""
        
        while True:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            
            for app_title in app_titles:
                prefix = f"{app_title.replace(' ', '_')}_{timestamp}"
                
                result = subprocess.run([
                    self.exe_path,
                    '-m', 'window',
                    '-w', app_title,
                    '-d', str(self.output_dir),
                    '-p', prefix
                ], capture_output=True)
                
                if result.returncode == 0:
                    print(f"[{timestamp}] Captured: {app_title}")
                else:
                    print(f"[{timestamp}] Failed: {app_title}")
            
            time.sleep(interval_seconds)

# Usage
monitor = ScreenshotMonitor(
    exe_path=r"C:\Tools\ScreenCapture.exe",
    output_dir=r"C:\Monitoring"
)

# Monitor these apps every 5 minutes
monitor.monitor_applications(
    app_titles=["Trading Platform", "Market Data", "Risk Monitor"],
    interval_seconds=300
)
```

### C# - ASP.NET Core Integration
```csharp
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

[ApiController]
[Route("api/[controller]")]
public class ScreenshotController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ScreenshotController> _logger;
    
    public ScreenshotController(IConfiguration configuration, ILogger<ScreenshotController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    [HttpPost("capture")]
    public async Task<IActionResult> CaptureScreenshot([FromBody] CaptureRequest request)
    {
        var exePath = _configuration["ScreenCapture:ExePath"];
        var outputDir = _configuration["ScreenCapture:OutputDir"];
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"-m {request.Mode} -d \"{outputDir}\" -p \"{request.Prefix}\"",
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
            // Find the captured file
            var files = Directory.GetFiles(outputDir, $"{request.Prefix}*.png");
            if (files.Length > 0)
            {
                var file = files.OrderByDescending(File.GetCreationTime).First();
                _logger.LogInformation("Screenshot captured: {File}", file);
                
                return Ok(new { success = true, filePath = file });
            }
        }
        
        _logger.LogError("Screenshot capture failed");
        return BadRequest(new { success = false, error = "Capture failed" });
    }
}

public class CaptureRequest
{
    public string Mode { get; set; } = "full";
    public string Prefix { get; set; } = "screenshot";
}
```

## Troubleshooting Examples

### Check Available Windows
```cmd
ScreenCapture.exe --list-windows
```

### Test with Verbose Logging
```cmd
ScreenCapture.exe -m active -v > capture.log 2>&1
type capture.log
```

### Try GDI Fallback
```cmd
ScreenCapture.exe -m active --no-winrt
```

### Test Region Capture
```cmd
:: Capture top-left 800x600 area
ScreenCapture.exe -m region -r 0 0 800 600 -v
```

## Automation Examples

### Task Scheduler (Windows)
```powershell
# Create scheduled task to capture every hour
$action = New-ScheduledTaskAction -Execute "C:\Tools\ScreenCapture.exe" `
    -Argument "-m full -d C:\HourlyScreenshots -p hourly"

$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Hours 1)

Register-ScheduledTask -TaskName "Hourly Screenshot" `
    -Action $action `
    -Trigger $trigger `
    -Description "Captures screenshot every hour"
```

### Jenkins Pipeline
```groovy
pipeline {
    agent any
    
    stages {
        stage('Test') {
            steps {
                script {
                    bat '''
                        ScreenCapture.exe -m active -d "%WORKSPACE%\\screenshots" -p "test_run"
                    '''
                }
            }
        }
        
        stage('Archive') {
            steps {
                archiveArtifacts artifacts: 'screenshots/*.png', fingerprint: true
            }
        }
    }
}
```

### GitHub Actions
```yaml
name: UI Test with Screenshots

on: [push]

jobs:
  test:
    runs-on: windows-latest
    
    steps:
      - uses: actions/checkout@v2
      
      - name: Setup Screenshot Tool
        run: |
          Invoke-WebRequest -Uri "https://your-releases/ScreenCapture.exe" -OutFile "ScreenCapture.exe"
      
      - name: Run UI Tests
        run: |
          # Your UI tests here
      
      - name: Capture Screenshots on Failure
        if: failure()
        run: |
          .\ScreenCapture.exe -m active -d screenshots -p "failure"
      
      - name: Upload Screenshots
        if: failure()
        uses: actions/upload-artifact@v2
        with:
          name: test-screenshots
          path: screenshots/*.png
```

## Best Practices

### 1. Always Check Exit Codes
```powershell
ScreenCapture.exe -m active
if ($LASTEXITCODE -ne 0) {
    Write-Error "Capture failed!"
}
```

### 2. Use Try-Catch for Error Handling
```powershell
try {
    & ScreenCapture.exe -m active -d $outputDir
    if ($LASTEXITCODE -ne 0) {
        throw "Screenshot capture failed"
    }
} catch {
    Write-Error "Error: $_"
    # Fallback or notify
}
```

### 3. Implement Retry Logic
```powershell
$maxRetries = 3
$retryCount = 0

while ($retryCount -lt $maxRetries) {
    ScreenCapture.exe -m active
    
    if ($LASTEXITCODE -eq 0) {
        break
    }
    
    $retryCount++
    Start-Sleep -Seconds 2
}
```

### 4. Clean Up Old Files
```powershell
# Remove screenshots older than 7 days
$cutoffDate = (Get-Date).AddDays(-7)
Get-ChildItem $screenshotDir -Filter "*.png" |
    Where-Object { $_.CreationTime -lt $cutoffDate } |
    Remove-Item -Force
```

For more examples and integration patterns, see INTEGRATION_GUIDE.md
