namespace ScreenCaptureAgent.Core.Models;

/// <summary>
/// Defines the available screen capture modes
/// </summary>
public enum CaptureMode
{
    /// <summary>
    /// Capture the entire primary screen
    /// </summary>
    FullScreen,
    
    /// <summary>
    /// Capture the currently active window
    /// </summary>
    ActiveWindow,
    
    /// <summary>
    /// Capture a window by its title (partial match supported)
    /// </summary>
    WindowByTitle,
    
    /// <summary>
    /// Capture a specific region of the screen
    /// </summary>
    Region,
    
    /// <summary>
    /// Capture all monitors
    /// </summary>
    MultiMonitor,
    
    /// <summary>
    /// Capture a specific monitor by index
    /// </summary>
    SpecificMonitor,

    AllChromeWindows,    // NEW
    AllEdgeWindows,      // NEW
    AllBrowsers          // NEW - Both Chrome and Edge
}
