# Screen Capture Agent - Project Structure

## 📁 Directory Organization

```
ScreenCaptureAgent/
│
├── 📄 ScreenCaptureAgent.sln          # Visual Studio Solution File
│
├── 📂 ScreenCaptureAgent/             # Console Application Project
│   ├── Program.cs                     # Entry point & main orchestration
│   ├── CLI/
│   │   └── CommandLineParser.cs       # Command-line argument parser
│   └── ScreenCaptureAgent.csproj      # Project file (console app)
│
├── 📂 ScreenCaptureAgent.Core/        # Core Library Project
│   ├── Models/                        # Data models & DTOs
│   │   ├── CaptureMode.cs             # Enumeration of capture modes
│   │   ├── CaptureOptions.cs          # Configuration options model
│   │   ├── CaptureResult.cs           # Result/response model
│   │   └── WindowInfo.cs              # Window information model
│   │
│   ├── Services/                      # Core business logic
│   │   ├── ScreenCaptureService.cs    # Main orchestration service
│   │   ├── WinRtCaptureService.cs     # WinRT GraphicsCapture implementation
│   │   └── GdiCaptureService.cs       # GDI+ fallback implementation
│   │
│   ├── Native/                        # Windows API interop
│   │   └── NativeMethods.cs           # P/Invoke declarations
│   │
│   └── ScreenCaptureAgent.Core.csproj # Project file (class library)
│
├── 📜 Build & Deployment Scripts
│   ├── build.ps1                      # PowerShell build script
│   ├── build.bat                      # Batch build script
│   └── test.ps1                       # PowerShell test script
│
├── 📖 Documentation
│   ├── README.md                      # Main documentation (you are here!)
│   ├── GETTING_STARTED.md             # Quick start guide
│   ├── EXAMPLES.md                    # Usage examples
│   ├── INTEGRATION_GUIDE.md           # Integration patterns
│   ├── CHANGELOG.md                   # Version history
│   └── PROJECT_STRUCTURE.md           # This file
│
├── 📋 Project Files
│   ├── LICENSE                        # MIT License
│   └── .gitignore                     # Git ignore rules
│
└── 📦 Output Directories (generated)
    ├── bin/                           # Build output
    ├── obj/                           # Intermediate files
    └── publish/                       # Published executables
        └── Release/
            └── ScreenCapture.exe      # Final executable
```

## 🏗️ Architecture Overview

### Layer Separation

```
┌─────────────────────────────────────────┐
│         Console Application             │
│  (ScreenCaptureAgent)                   │
│  • Command-line parsing                 │
│  • User interaction                     │
│  • Exit code management                 │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│         Core Library                    │
│  (ScreenCaptureAgent.Core)              │
│  • Screen capture logic                 │
│  • WinRT & GDI+ implementations         │
│  • Window management                    │
│  • Image processing                     │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│      Windows APIs & .NET Libraries      │
│  • Windows.Graphics.Capture             │
│  • System.Drawing.Common                │
│  • SharpDX (Direct3D11)                 │
│  • Windows Forms (screen info)          │
└─────────────────────────────────────────┘
```

## 📦 Key Components

### 1. ScreenCaptureAgent (Console App)

**Purpose**: User-facing command-line interface

**Key Files**:
- `Program.cs`: Entry point, error handling, result display
- `CommandLineParser.cs`: Parse and validate command-line arguments

**Responsibilities**:
- Parse command-line arguments
- Display help and usage information
- Invoke core capture service
- Format and display results
- Return appropriate exit codes

### 2. ScreenCaptureAgent.Core (Library)

**Purpose**: Core screenshot capture functionality

**Key Components**:

#### Models
- **CaptureMode**: Enumeration of capture types (Full, Active, Window, etc.)
- **CaptureOptions**: Configuration for capture operation
- **CaptureResult**: Response with file path, size, duration, etc.
- **WindowInfo**: Window metadata (title, handle, bounds, etc.)

#### Services
- **ScreenCaptureService**: Main orchestration, delegates to WinRT or GDI
- **WinRtCaptureService**: Modern Windows.Graphics.Capture API
- **GdiCaptureService**: Traditional GDI+ fallback

#### Native
- **NativeMethods**: P/Invoke for Windows API calls

## 🔄 Capture Flow

```
User Command
     │
     ▼
CommandLineParser
     │
     ▼
CaptureOptions (created)
     │
     ▼
ScreenCaptureService.CaptureAsync()
     │
     ├─► WinRtCaptureService (primary)
     │        │
     │        ├─► GraphicsCaptureItem
     │        ├─► Direct3D11CaptureFramePool
     │        └─► Bitmap
     │
     └─► GdiCaptureService (fallback)
              │
              ├─► Graphics.CopyFromScreen()
              └─► Bitmap
     │
     ▼
Save to PNG file
     │
     ▼
CaptureResult (returned)
     │
     ▼
Display to user
```

## 🛠️ Build Process

```
build.ps1 / build.bat
     │
     ▼
dotnet restore
     │ (Download NuGet packages)
     ▼
dotnet build
     │ (Compile C# → IL)
     ▼
dotnet publish
     │ (IL → Native + Package)
     │
     ├─► PublishSingleFile=true
     ├─► PublishReadyToRun=true
     ├─► SelfContained=true
     └─► IncludeNativeLibraries=true
     │
     ▼
ScreenCapture.exe
(Single file, ~70MB, fully standalone)
```

## 📊 Dependencies

### NuGet Packages

**ScreenCaptureAgent.Core**:
- `Microsoft.Windows.CsWinRT` (2.0.7) - WinRT interop
- `System.Drawing.Common` (8.0.0) - GDI+ & image processing
- `SharpDX` (4.2.0) - Direct3D base
- `SharpDX.Direct3D11` (4.2.0) - Direct3D11 for WinRT
- `SharpDX.DXGI` (4.2.0) - DirectX Graphics Infrastructure

**ScreenCaptureAgent**:
- References `ScreenCaptureAgent.Core` project

### Platform Dependencies

- .NET 8.0 Windows Runtime
- Windows 10 SDK (19041) minimum
- Windows.Graphics.Capture API (Windows 10 1903+)

## 🔑 Design Patterns

### 1. **Service Pattern**
Core logic encapsulated in services (`ScreenCaptureService`, etc.)

### 2. **Strategy Pattern**
Choose between WinRT and GDI capture strategies

### 3. **Factory Pattern**
`WindowInfo.FromHandle()` creates window objects

### 4. **Options Pattern**
`CaptureOptions` for configuration

### 5. **Result Pattern**
`CaptureResult` encapsulates success/failure

## 🚀 Deployment

### Single-File Deployment

The published executable includes:
- .NET 8 runtime
- All dependencies (SharpDX, System.Drawing, etc.)
- Core library code
- Console application code

**Total Size**: ~70MB (compressed with ReadyToRun)

### Deployment Targets

- **Primary**: Windows 10 (1903+) x64
- **Secondary**: Windows 11 x64
- **Architecture**: x64 only (WinRT requirement)

## 🔧 Customization Points

### Adding New Capture Modes

1. Add enum value to `CaptureMode.cs`
2. Implement capture logic in `ScreenCaptureService.cs`
3. Update command-line parser in `CommandLineParser.cs`
4. Add documentation to README

### Adding New Output Formats

1. Extend `SaveBitmap()` in `ScreenCaptureService.cs`
2. Add format-specific encoder configuration
3. Update command-line options
4. Document new format

### Custom Image Processing

Extend `GdiCaptureService.cs` or `WinRtCaptureService.cs`:
```csharp
public Bitmap ProcessImage(Bitmap source)
{
    // Apply filters, resize, annotate, etc.
    return processed;
}
```

## 📝 Code Conventions

### Naming
- **PascalCase**: Classes, methods, properties
- **camelCase**: Parameters, local variables
- **UPPER_CASE**: Constants

### Organization
- One class per file
- Namespace matches folder structure
- Public APIs have XML documentation

### Error Handling
- Services throw exceptions
- Console app catches and converts to exit codes
- Verbose mode for detailed logging

## 🧪 Testing Strategy

### Manual Testing
Use `test.ps1` to run automated test suite

### Integration Testing
Test with real Windows applications

### Regression Testing
Verify each capture mode after changes

## 📈 Performance Considerations

### WinRT Capture
- **Fastest**: Uses GPU-accelerated capture
- **Modern**: Best for Windows 10/11
- **Reliable**: No driver dependencies

### GDI Capture
- **Compatible**: Works everywhere
- **Slower**: CPU-based
- **Fallback**: When WinRT unavailable

### Memory Management
- Bitmaps disposed properly
- Frame pools cleaned up
- No memory leaks in tight loops

## 🔐 Security

### No Network Access
- Entirely offline operation
- No telemetry or analytics

### File System Only
- Writes only to specified directories
- No registry modification
- No system file access

### Minimal Privileges
- Runs at user level
- Admin only for protected windows

## 📚 Further Reading

- **README.md**: Complete feature documentation
- **GETTING_STARTED.md**: Quick setup guide
- **EXAMPLES.md**: Code samples
- **INTEGRATION_GUIDE.md**: System integration
- **CHANGELOG.md**: Version history

---

**Questions?** Check the documentation or create an issue!
