# Getting Started with Screen Capture Agent

Welcome! This guide will help you get up and running with Screen Capture Agent in minutes.

## 📥 Installation

### Option 1: Download Pre-Built Release (Recommended)

1. Go to the [Releases](../../releases) page
2. Download `ScreenCapture.exe` from the latest release
3. Save it to a folder (e.g., `C:\Tools\ScreenCapture\`)
4. No installation needed - it's ready to run!

**File Size**: ~70 MB (self-contained, no .NET runtime needed)

### Option 2: Build from Source
```cmd
git clone https://github.com/your-org/screen-capture-agent.git
cd screen-capture-agent
dotnet publish ScreenCaptureAgent\ScreenCaptureAgent.csproj ^
    --configuration Release ^
    --output bin ^
    --self-contained true ^
    --runtime win-x64
```

The executable will be at `bin\ScreenCapture.exe`

---

## ✅ Quick System Check

**Requirements:**
- ✅ Windows 10 (version 1903+) or Windows 11
- ✅ 64-bit Windows
- ❌ No .NET runtime needed (self-contained)

**Verify it works:**
```cmd
# Open Command Prompt
cd C:\Tools\ScreenCapture

# Run help command
ScreenCapture.exe --help
```

If you see the help text, you're ready to go! 🎉

---

## 🎯 Your First Captures

### 1️⃣ Capture Full Screen

The simplest command - captures your entire screen:
```cmd
ScreenCapture.exe
```

**Result**: Screenshot saved to current directory as `screenshot_YYYYMMDD_HHMMSS.png`

**Example output:**
```
✓ Screenshot captured successfully!

File: screenshot_20260202_143022.png
Size: 1920x1080 pixels
File Size: 1.2 MB
```

---

### 2️⃣ Capture to Specific Folder

Save screenshots to a dedicated folder:
```cmd
# Create output folder first
mkdir C:\Screenshots

# Capture to that folder
ScreenCapture.exe -d C:\Screenshots
```

**Result**: `C:\Screenshots\screenshot_20260202_143022.png`

---

### 3️⃣ Capture Active Window

Capture just the window you're currently using:
```cmd
# Click on any window (Word, Excel, Chrome, etc.)
# Then run:
ScreenCapture.exe -m active -d C:\Screenshots
```

**Tip**: The window you clicked last will be captured.

---

### 4️⃣ Capture Specific Application

Capture a window by its title (even if minimized):
```cmd
# First, see what windows are available
ScreenCapture.exe --list-windows

# Example output:
# Available Windows:
# ----------------
#   Document1.docx - Word
#   Book1.xlsx - Excel
#   Presentation1.pptx - PowerPoint
#   Google - Google Chrome
# ----------------

# Then capture the one you want (partial title match works!)
ScreenCapture.exe -m window -w "Word" -d C:\Screenshots
```

**Result**: Captures the Word window, even if it was minimized!

---

## 🚀 Common Use Cases

### 📝 Documenting Software Issues
```cmd
# Capture the error window
ScreenCapture.exe -m active -d C:\BugReports -p "error_"

# Result: C:\BugReports\error_20260202_143022.png
```

### 📊 Capturing Reports
```cmd
# Capture Excel report
ScreenCapture.exe -m window -w "Excel" -d C:\Reports -p "monthly_report_"

# Capture PowerPoint presentation
ScreenCapture.exe -m window -w "PowerPoint" -d C:\Presentations
```

### 🌐 Capturing Browser Content
```cmd
# Single Chrome window
ScreenCapture.exe -m window -w "Chrome" -d C:\WebCaptures

# ALL Chrome windows at once
ScreenCapture.exe -m chrome -d C:\WebCaptures

# ALL Edge windows at once
ScreenCapture.exe -m edge -d C:\WebCaptures

# ALL browser windows (Chrome + Edge)
ScreenCapture.exe -m browsers -d C:\WebCaptures
```

### 🎥 Capturing with Delay (for Menus, Dropdowns)
```cmd
# Start the command
ScreenCapture.exe -m active --delay 3000 -d C:\Screenshots

# You have 3 seconds to:
# 1. Click on the window
# 2. Open a dropdown menu
# 3. The screenshot captures after 3 seconds
```

### 🖼️ Capturing Specific Region
```cmd
# Format: -r X Y Width Height
# Capture region starting at (100, 100) with size 800x600
ScreenCapture.exe -m region -r 100 100 800 600 -d C:\Screenshots
```

### 🖥️ Multi-Monitor Setups
```cmd
# Capture all monitors as one large image
ScreenCapture.exe -m multi -d C:\Screenshots

# Capture just monitor 1 (primary)
ScreenCapture.exe -m monitor --monitor 0 -d C:\Screenshots

# Capture just monitor 2 (secondary)
ScreenCapture.exe -m monitor --monitor 1 -d C:\Screenshots
```

---

## 🔧 Useful Options

### Verbose Mode (See What's Happening)
```cmd
# Add -v to any command
ScreenCapture.exe -m active -d C:\Screenshots -v
```

**Output:**
```
[14:30:45.123] WinRT capture initialized successfully
[14:30:45.124] Capture mode: ActiveWindow
[14:30:45.125] Capturing active window: Document1.docx - Word
[14:30:45.126] Window was minimized, restoring...
[14:30:45.127] Waiting 500ms for window to render...
[14:30:45.635] Captured using GDI
[14:30:45.702] Screenshot saved: C:\Screenshots\screenshot_20260202_143045.png (1.2 MB)

✓ Screenshot captured successfully!
```

### Custom Filename Prefix
```cmd
# Instead of "screenshot_...", use your own prefix
ScreenCapture.exe -m active -d C:\Tickets -p "TICKET-12345_"

# Result: C:\Tickets\TICKET-12345_20260202_143022.png
```

### Specific Filename
```cmd
# Specify exact output path and filename
ScreenCapture.exe -m active -o "C:\Reports\Q4_Sales_Report.png"

# Result: C:\Reports\Q4_Sales_Report.png
```

### JPEG Format with Quality Control
```cmd
# Save as JPEG with custom quality
ScreenCapture.exe -m active -o "C:\Screenshots\photo.jpg" --quality 85

# Quality: 0-100 (higher = better quality, larger file)
# Default: 95
```

---

## 📋 Quick Reference Card

| What to Capture | Command |
|-----------------|---------|
| Full screen | `ScreenCapture.exe` |
| Active window | `ScreenCapture.exe -m active` |
| Word document | `ScreenCapture.exe -m window -w "Word"` |
| Excel spreadsheet | `ScreenCapture.exe -m window -w "Excel"` |
| Chrome browser | `ScreenCapture.exe -m window -w "Chrome"` |
| All Chrome tabs | `ScreenCapture.exe -m chrome` |
| All browsers | `ScreenCapture.exe -m browsers` |
| Specific region | `ScreenCapture.exe -m region -r 0 0 800 600` |
| All monitors | `ScreenCapture.exe -m multi` |

**Common Options:**
- `-d C:\Screenshots` - Output folder
- `-p "prefix_"` - Filename prefix
- `-v` - Verbose output
- `--delay 2000` - Wait 2 seconds before capture

---

## 🐛 Troubleshooting Common Issues

### ❌ "Window not found"

**Problem**: Can't find the window to capture

**Solution**:
```cmd
# Step 1: List all windows
ScreenCapture.exe --list-windows

# Step 2: Use the exact title or partial match
ScreenCapture.exe -m window -w "Word" -v
```

**Tip**: Partial matching works! Use "Word" instead of the full title.

---

### ❌ Blank or Black Screenshots

**Problem**: Screenshot is blank or shows wrong content

**Causes & Solutions**:

**1. Window is minimized:**
```cmd
# Solution: Tool auto-restores minimized windows (built-in feature)
ScreenCapture.exe -m window -w "Excel" -v

# You'll see: "Window was minimized, restoring..."
```

**2. Window needs time to render:**
```cmd
# Solution: Add custom delay
ScreenCapture.exe -m window -w "Chrome" --delay 1000 -v
```

**3. Browser rendering issue (rare):**
```cmd
# Solution: Try GDI+ fallback mode
ScreenCapture.exe -m window -w "Chrome" --no-winrt -v
```

---

### ❌ "Access Violation" or Crashes

**Problem**: Application crashes with error

**Solution**:
```cmd
# Run as Administrator (right-click → Run as Administrator)
ScreenCapture.exe -m active -v
```

**Note**: Usually only needed for capturing elevated windows.

---

### ❌ "A generic error occurred in GDI+"

**Problem**: Error saving file

**Causes & Solutions**:

**1. Directory doesn't exist:**
```cmd
# Create directory first
mkdir C:\Screenshots

# Then capture
ScreenCapture.exe -d C:\Screenshots
```

**2. No write permissions:**
```cmd
# Use a different folder where you have permissions
ScreenCapture.exe -d C:\Users\YourName\Pictures
```

**3. Used -o with directory path (incorrect):**
```cmd
# ❌ Wrong: -o with directory
ScreenCapture.exe -o C:\Screenshots\

# ✅ Correct: Use -d for directory
ScreenCapture.exe -d C:\Screenshots

# ✅ Or: Use -o with full filename
ScreenCapture.exe -o C:\Screenshots\myfile.png
```

---

### ❌ Screenshot Size is 199x34 pixels (Too Small)

**Problem**: Very small screenshot (usually blank)

**Cause**: Window was minimized and didn't restore properly

**Solution**:
```cmd
# Increase the delay to let window restore
ScreenCapture.exe -m window -w "Word" --delay 2000 -v

# Manually restore window first, then capture
# (Click on window in taskbar, then run command)
```

---

## 🔄 Automation Examples

### Windows Batch Script

**capture-reports.bat:**
```batch
@echo off
echo Capturing daily reports...

REM Set output folder
set OUTPUT=C:\DailyReports\%DATE%
mkdir "%OUTPUT%"

REM Capture Excel report
ScreenCapture.exe -m window -w "Excel" -d "%OUTPUT%" -p "excel_"

REM Capture PowerPoint
ScreenCapture.exe -m window -w "PowerPoint" -d "%OUTPUT%" -p "ppt_"

echo Done! Reports saved to %OUTPUT%
pause
```

### PowerShell Script

**capture-browsers.ps1:**
```powershell
# Capture all open browser windows
$outputDir = "C:\BrowserCaptures\$(Get-Date -Format 'yyyy-MM-dd')"
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

Write-Host "Capturing all browser windows..."
& ScreenCapture.exe -m browsers -d $outputDir -v

if ($LASTEXITCODE -eq 0) {
    Write-Host "Success! Screenshots saved to: $outputDir" -ForegroundColor Green
    Start-Process $outputDir
} else {
    Write-Host "Capture failed with exit code: $LASTEXITCODE" -ForegroundColor Red
}
```

### Task Scheduler (Automated)

**Schedule automatic captures:**

1. Open Task Scheduler
2. Create Basic Task
3. Name: "Daily Screenshot"
4. Trigger: Daily at 5:00 PM
5. Action: Start a program
   - Program: `C:\Tools\ScreenCapture\ScreenCapture.exe`
   - Arguments: `-m active -d C:\AutoCaptures`
6. Finish

---

## 📚 Next Steps

### Learn More Commands
```cmd
# See all available options
ScreenCapture.exe --help

# List available windows
ScreenCapture.exe --list-windows
```

### Integration with Ticketing Systems

See [README.md](README.md#-enterprise-integration) for:
- JIRA integration examples
- ServiceNow integration
- Zendesk integration
- Custom API integration

### Advanced Features

- **Batch captures**: Capture multiple windows at once
- **Custom delays**: Perfect for capturing menus and dropdowns
- **Quality control**: Optimize file size vs quality
- **Silent mode**: Run from scripts without UI

---

## 💡 Pro Tips

### Tip 1: Add to PATH for Easy Access

Add ScreenCapture to your PATH to run from anywhere:
```cmd
# Add to System PATH
setx PATH "%PATH%;C:\Tools\ScreenCapture"

# Now run from any folder
cd C:\Documents
ScreenCapture.exe -m active
```

### Tip 2: Create Shortcuts

**Desktop shortcut:**
- Right-click desktop → New → Shortcut
- Target: `C:\Tools\ScreenCapture\ScreenCapture.exe -m active -d C:\Screenshots`
- Name: "Capture Active Window"

### Tip 3: Keyboard Shortcut via AutoHotkey
```autohotkey
; Press Ctrl+Shift+S to capture active window
^+s::
Run, C:\Tools\ScreenCapture\ScreenCapture.exe -m active -d C:\Screenshots
return
```

### Tip 4: Always Use Verbose Mode When Testing
```cmd
# Verbose mode shows exactly what's happening
ScreenCapture.exe -m window -w "Excel" -v

# Helps debug issues quickly
```

---

## 🎓 Training Resources

### Video Tutorials (Coming Soon)
- [ ] Basic screen capture
- [ ] Window capturing techniques
- [ ] Browser batch capture
- [ ] Integration with JIRA

### Sample Scripts
Check the `examples/` folder for:
- Batch automation scripts
- PowerShell integration
- Python wrappers
- C# integration code

---

## ❓ Getting Help

### Self-Help Resources

1. **Check the logs** (use `-v` flag)
2. **Read error messages** (they're descriptive!)
3. **Try with `--no-winrt`** (fallback mode)
4. **List windows first** (`--list-windows`)

### Community Support

- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)
- **Documentation**: [README.md](README.md)

### Common Questions

**Q: Do I need admin rights?**  
A: Only for capturing elevated windows. Regular windows work with standard user rights.

**Q: Can I capture minimized windows?**  
A: Yes! The tool automatically restores them before capturing.

**Q: Does it work on Windows 7?**  
A: No, requires Windows 10 (version 1903+) or Windows 11.

**Q: Can I capture multiple monitors?**  
A: Yes! Use `-m multi` to capture all monitors as one image.

**Q: How do I capture all my Chrome tabs?**  
A: Screen capture captures windows, not individual tabs. Use `-m chrome` to capture all Chrome windows.

---