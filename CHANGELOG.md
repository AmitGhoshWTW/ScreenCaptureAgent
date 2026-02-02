# Changelog

All notable changes to Screen Capture Agent will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-31

### Added
- Initial release of Screen Capture Agent
- Full screen capture mode
- Active window capture mode
- Window by title capture mode (with partial matching)
- Region capture mode
- Multi-monitor support
- Specific monitor capture
- WinRT GraphicsCapture API implementation
- GDI+ fallback for compatibility
- PNG output format support
- Command-line interface with comprehensive options
- Verbose logging mode
- Exit code reporting for automation
- Window enumeration and listing
- Delay before capture option
- Custom output directory and file prefix
- Auto-generated timestamps in filenames
- Self-contained single-file executable
- No black screens in Chrome/Edge browsers
- Browser tab capture support
- Comprehensive error handling
- Detailed help documentation
- Build scripts (PowerShell and Batch)
- Test suite (PowerShell)
- Integration examples for major ticketing systems
- Complete API documentation

### Technical Details
- Built with .NET 8.0
- Uses Windows.Graphics.Capture API (WinRT)
- SharpDX for Direct3D11 interop
- System.Drawing.Common for image processing
- Self-contained deployment with ReadyToRun compilation
- Single-file executable with embedded dependencies
- Optimized for Windows 10 (1903+) and Windows 11

### Documentation
- Comprehensive README with usage examples
- Integration guide for JIRA, ServiceNow, Azure DevOps, Zendesk
- Usage examples for PowerShell, C#, Python, Node.js, Batch
- Troubleshooting guide
- Architecture documentation
- Best practices guide

### Known Limitations
- Windows-only (requires Windows 10 1903+ or Windows 11)
- Some protected system windows may require elevation
- WinRT capture requires Windows.Graphics.Capture API support

## [Unreleased]

### Planned Features
- JPEG output format with quality control
- Clipboard integration
- Hotkey support for interactive capture
- OCR text extraction from screenshots
- Annotation capabilities
- GIF animation capture
- Video recording support
- Automatic upload to cloud storage
- REST API mode for remote capture
- Configuration file support
- Plugin architecture
- macOS and Linux support (future)

---

For detailed migration guides and upgrade instructions, see MIGRATION.md
For contributing guidelines, see CONTRIBUTING.md
