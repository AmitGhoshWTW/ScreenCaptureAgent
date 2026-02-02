namespace ScreenCaptureAgent.Core.Models;

/// <summary>
/// Configuration options for screen capture.
/// Controls mode, destination, quality, region selection, and capture engine.
/// </summary>
public class CaptureOptions
{
    /// <summary>
    /// The capture mode to use.
    /// </summary>
    public CaptureMode Mode { get; set; } = CaptureMode.FullScreen;
    
    /// <summary>
    /// Output file path (if not specified, auto-generated with timestamp).
    /// </summary>
    public string? OutputPath { get; set; }
    
    /// <summary>
    /// Window title to capture (for WindowByTitle mode).
    /// </summary>
    public string? WindowTitle { get; set; }
    
    /// <summary>
    /// Monitor index (for SpecificMonitor mode, 0-based).
    /// </summary>
    public int MonitorIndex { get; set; } = 0;
    
    /// <summary>
    /// Region to capture (X, Y, Width, Height).
    /// </summary>
    public CaptureRegion? Region { get; set; }
    
    /// <summary>
    /// Output directory for screenshots.
    /// </summary>
    public string OutputDirectory { get; set; } = Environment.CurrentDirectory;
    
    /// <summary>
    /// File name prefix.
    /// </summary>
    public string FileNamePrefix { get; set; } = "screenshot";
    
    /// <summary>
    /// Image quality (0-100, for compression formats).
    /// </summary>
    public int Quality { get; set; } = 95;
    
    /// <summary>
    /// Whether to include cursor in capture.
    /// </summary>
    public bool IncludeCursor { get; set; } = false;
    
    /// <summary>
    /// Delay before capture (in milliseconds).
    /// </summary>
    public int DelayMs { get; set; } = 0;
    
    /// <summary>
    /// Whether to use WinRT GraphicsCapture API (recommended for modern Windows).
    /// </summary>
    public bool UseWinRtCapture { get; set; } = true;
    
    /// <summary>
    /// Whether to show verbose logging.
    /// </summary>
    public bool Verbose { get; set; } = false;
}

/// <summary>
/// Represents a rectangular region for capture.
/// </summary>
public class CaptureRegion
{
    /// <summary>
    /// Left X coordinate of the region.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Top Y coordinate of the region.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Region width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Region height in pixels.
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// Initializes a new instance with default values.
    /// </summary>
    public CaptureRegion() { }
    
    /// <summary>
    /// Initializes a new instance with coordinates and size.
    /// </summary>
    /// <param name="x">Left X coordinate.</param>
    /// <param name="y">Top Y coordinate.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public CaptureRegion(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    /// <summary>
    /// Returns a readable representation of the region.
    /// </summary>
    public override string ToString() => $"[{X},{Y},{Width},{Height}]";
}
