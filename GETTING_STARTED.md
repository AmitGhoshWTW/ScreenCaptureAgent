# Getting Started with Screen Capture Agent

## 🚀 Quick Start (5 Minutes)

### Step 1: Build the Executable

#### Option A: Using PowerShell (Recommended)
```powershell
# Navigate to the project directory
cd ScreenCaptureAgent

# Run the build script
.\build.ps1 -Configuration Release
```

#### Option B: Using Command Prompt
```cmd
cd ScreenCaptureAgent
build.bat
```

#### Option C: Using Visual Studio
1. Open `ScreenCaptureAgent.sln` in Visual Studio 2022
2. Select `Release` configuration
3. Right-click on `ScreenCaptureAgent` project → Publish
4. Choose `Folder` → Select output folder → Publish

### Step 2: Locate the Executable

After building, you'll find the executable at:
```
ScreenCaptureAgent\publish\Release\ScreenCapture.exe
```

The file is approximately **60-80 MB** (self-contained, includes .NET runtime).

### Step 3: Test It

#### Basic Test
```cmd
# Navigate to the publish folder
cd publish\Release

# Run help
.\ScreenCapture.exe --help

# Capture full screen
.\ScreenCapture.exe

# Check the output
dir *.png
```

#### Interactive Test
```powershell
# Run the test suite
.\test.ps1
```

## 📦 Deployment

### Option 1: Copy Single EXE
Simply copy `ScreenCapture.exe` to your target location:
```cmd
copy ScreenCapture.exe C:\Tools\
```

No installation required! The exe is self-contained.

### Option 2: Add to PATH
Add the directory to your system PATH for global access:

**PowerShell (Admin)**:
```powershell
$path = [Environment]::GetEnvironmentVariable("Path", "Machine")
$newPath = "$path;C:\Tools"
[Environment]::SetEnvironmentVariable("Path", $newPath, "Machine")
```

**Manually**:
1. Right-click `This PC` → Properties
2. Advanced system settings → Environment Variables
3. Edit `Path` → Add `C:\Tools`

### Option 3: Create Batch Wrapper
Create `screenshot.bat`:
```batch
@echo off
C:\Tools\ScreenCapture.exe %*
```

Now you can run: `screenshot -m active`

## 🎯 Common Use Cases

### Use Case 1: Bug Reporting
```powershell
# Capture current window for bug report
ScreenCapture.exe -m active -d "C:\BugReports" -p "bug_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
```

### Use Case 2: Documentation
```cmd
:: Capture specific window
ScreenCapture.exe -m window -w "Application Name" -o "docs\screenshot.png"
```

### Use Case 3: Monitoring
```powershell
# Scheduled screenshot every hour
$action = New-ScheduledTaskAction -Execute "C:\Tools\ScreenCapture.exe" `
    -Argument "-m full -d C:\Monitoring"

$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) `
    -RepetitionInterval (New-TimeSpan -Hours 1)

Register-ScheduledTask -TaskName "Hourly Screenshot" `
    -Action $action -Trigger $trigger
```

### Use Case 4: Automated Testing
```csharp
// In your test teardown
if (TestContext.CurrentTestOutcome == UnitTestOutcome.Failed)
{
    var process = Process.Start(new ProcessStartInfo
    {
        FileName = "ScreenCapture.exe",
        Arguments = $"-m active -d \"{TestContext.TestRunDirectory}\" -p \"failure_{TestContext.TestName}\"",
        CreateNoWindow = true
    });
    process.WaitForExit();
}
```

## 🔍 Troubleshooting

### "ScreenCapture.exe is not recognized"
**Solution**: The exe is not in PATH. Either:
1. Use full path: `C:\Tools\ScreenCapture.exe`
2. Navigate to directory first: `cd C:\Tools`
3. Add to PATH (see deployment options above)

### "Failed to capture screenshot"
**Solution**: Run with verbose mode to see details:
```cmd
ScreenCapture.exe -m active -v
```

Common causes:
- Window is minimized (restore it first)
- Window is on another virtual desktop
- Need administrator privileges

### "No active window found"
**Solution**: Make sure a window is focused before running:
```cmd
:: List available windows
ScreenCapture.exe --list-windows

:: Capture specific window by name
ScreenCapture.exe -m window -w "Window Title"
```

### Black screen in Chrome/Edge
**Good News**: This is already handled! The tool uses WinRT which captures browsers correctly.

If you still see issues:
```cmd
:: Make sure WinRT is enabled (it's default)
ScreenCapture.exe -m window -w "Chrome"

:: Check your Windows version (need 1903+)
winver
```

## 📖 Next Steps

### Learn More
1. **Read the full README**: `README.md` for complete feature list
2. **Check examples**: `EXAMPLES.md` for code samples
3. **Integration guide**: `INTEGRATION_GUIDE.md` for ticketing systems
4. **API documentation**: Explore the source code

### Integration
1. Choose your ticketing system integration from `INTEGRATION_GUIDE.md`
2. Implement error handling using exit codes
3. Set up logging and monitoring
4. Create automated workflows

### Customization
1. Modify `CommandLineParser.cs` for custom arguments
2. Extend `ScreenCaptureService.cs` for new capture modes
3. Add custom image processing in `GdiCaptureService.cs`
4. Implement plugins for your specific needs

## 🆘 Getting Help

### Resources
- **README.md**: Complete documentation
- **EXAMPLES.md**: Code samples and patterns
- **INTEGRATION_GUIDE.md**: Ticketing system integration
- **CHANGELOG.md**: Version history and updates

### Support
- Create an issue on GitHub
- Check existing issues for solutions
- Review troubleshooting section in README

## ✅ Verification Checklist

After installation, verify:

- [ ] `ScreenCapture.exe --help` displays help text
- [ ] `ScreenCapture.exe --list-windows` shows available windows
- [ ] `ScreenCapture.exe -m active` captures a screenshot
- [ ] Screenshot file is created in current directory
- [ ] File is valid PNG and can be opened
- [ ] Exit code is 0 for successful capture

## 🎓 Learning Path

### Beginner
1. Run basic captures: `ScreenCapture.exe`
2. Try different modes: `-m active`, `-m window`
3. Use custom output: `-o` and `-d` options
4. List windows: `--list-windows`

### Intermediate
1. Write simple automation scripts
2. Integrate with your application
3. Handle exit codes properly
4. Implement error handling

### Advanced
1. Build ticketing system integration
2. Create monitoring solutions
3. Implement retry logic
4. Custom preprocessing/postprocessing

## 🔐 Security Notes

- **No network activity**: Tool operates entirely offline
- **No telemetry**: No data collection or analytics
- **Local only**: Screenshots saved to local file system
- **Source available**: Full source code for security review

## 📋 Requirements Summary

✅ **Operating System**: Windows 10 (1903+) or Windows 11  
✅ **Architecture**: x64  
✅ **Runtime**: None (self-contained)  
✅ **Disk Space**: ~100 MB for exe  
✅ **Permissions**: User-level (admin for some system windows)  

---

**You're all set! Start capturing screenshots! 🎉**

For questions or issues, refer to README.md or create a GitHub issue.
