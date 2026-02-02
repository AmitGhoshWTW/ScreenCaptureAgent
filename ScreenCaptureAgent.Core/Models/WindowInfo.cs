using System.Runtime.InteropServices;
using System.Text;
using static ScreenCaptureAgent.Core.Native.NativeMethods;

namespace ScreenCaptureAgent.Core.Models;

/// <summary>
/// Represents a native window with handle, title, bounds, process information and visibility.
/// Provides helpers to construct instances from HWND and query/filter windows.
/// </summary>
public class WindowInfo
{
    /// <summary>
    /// Native window handle (HWND).
    /// </summary>
    public IntPtr Handle { get; set; }

    /// <summary>
    /// Window title text.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Window bounding rectangle in screen coordinates.
    /// </summary>
    public RECT Bounds { get; set; }

    /// <summary>
    /// Owning process ID for the window.
    /// </summary>
    public uint ProcessId { get; set; }

    /// <summary>
    /// True if the window is visible.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Builds a <see cref="WindowInfo"/> from a native window handle.
    /// Returns null if the handle is invalid or the window has no title.
    /// </summary>
    /// <param name="hWnd">A native window handle (HWND).</param>
    /// <returns>A populated <see cref="WindowInfo"/> or null.</returns>
    public static WindowInfo? FromHandle(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
            return null;

        // Query title length first; zero-length typically indicates no user-visible title.
        var length = GetWindowTextLength(hWnd);
        if (length == 0)
            return null;

        // Retrieve window title into a buffer sized for the exact length (+1 for terminator).
        var builder = new StringBuilder(length + 1);
        GetWindowText(hWnd, builder, builder.Capacity);
        var title = builder.ToString();

        // Skip windows with empty titles.
        if (string.IsNullOrEmpty(title))
            return null;

        // Obtain bounds and owning process ID from Win32 APIs.
        GetWindowRect(hWnd, out var rect);
        GetWindowThreadProcessId(hWnd, out var processId);

        // Construct the info object including visibility state.
        return new WindowInfo
        {
            Handle = hWnd,
            Title = title,
            Bounds = rect,
            ProcessId = processId,
            IsVisible = IsWindowVisible(hWnd)
        };
    }

    /// <summary>
    /// Enumerates all top-level visible windows and returns them as a list of <see cref="WindowInfo"/>.
    /// </summary>
    /// <returns>List of visible windows with titles.</returns>
    public static List<WindowInfo> GetAllWindows()
    {
        var windows = new List<WindowInfo>();
        var shellWindow = GetShellWindow();

        // Enumerate all windows and filter out shell/invisible ones.
        EnumWindows((hWnd, lParam) =>
        {
            if (hWnd == shellWindow) return true;            // Skip the shell window
            if (!IsWindowVisible(hWnd)) return true;         // Skip invisible windows

            var windowInfo = FromHandle(hWnd);
            if (windowInfo != null && !string.IsNullOrWhiteSpace(windowInfo.Title))
            {
                windows.Add(windowInfo);
            }
            return true;
        }, IntPtr.Zero);

        return windows;
    }

    /// <summary>
    /// Finds a window by title using exact match first, then partial match.
    /// </summary>
    /// <param name="titlePattern">Title to match (case-insensitive).</param>
    /// <returns>The first matching window or null.</returns>
    public static WindowInfo? FindByTitle(string titlePattern)
    {
        var windows = GetAllWindows();

        // Exact match first
        var exactMatch = windows.FirstOrDefault(w =>
            w.Title.Equals(titlePattern, StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null)
            return exactMatch;

        // Partial match
        return windows.FirstOrDefault(w =>
            w.Title.Contains(titlePattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds all Chrome windows by process name.
    /// </summary>
    /// <returns>List of Chrome windows.</returns>
    // NEW: Find all Chrome windows
    public static List<WindowInfo> FindAllChromeWindows()
    {
        return FindAllWindowsByProcessName("chrome");
    }

    /// <summary>
    /// Finds all Edge windows by process name.
    /// </summary>
    /// <returns>List of Edge windows.</returns>
    // NEW: Find all Edge windows
    public static List<WindowInfo> FindAllEdgeWindows()
    {
        return FindAllWindowsByProcessName("msedge");
    }

    /// <summary>
    /// Finds all browser windows (Chrome + Edge) and returns a concatenated list.
    /// </summary>
    /// <returns>Combined list of Chrome and Edge windows.</returns>
    // NEW: Find all browser windows (Chrome + Edge)
    public static List<WindowInfo> FindAllBrowserWindows()
    {
        var chromeWindows = FindAllChromeWindows();
        var edgeWindows = FindAllEdgeWindows();
        // NOTE: Concatenates lists; duplicates by handle are possible if both sets overlap.
        return chromeWindows.Concat(edgeWindows).ToList();
    }

    /// <summary>
    /// Finds all windows for a given process name (case-insensitive).
    /// </summary>
    /// <param name="processName">Process name (e.g., "chrome").</param>
    /// <returns>List of matching windows.</returns>
    // NEW: Find all windows by process name
    public static List<WindowInfo> FindAllWindowsByProcessName(string processName)
    {
        var allWindows = GetAllWindows();
        var matchingWindows = new List<WindowInfo>();

        foreach (var window in allWindows)
        {
            try
            {
                // Resolve process name from PID; may throw if the process has exited.
                var process = System.Diagnostics.Process.GetProcessById((int)window.ProcessId);
                if (process.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    matchingWindows.Add(window);
                }
            }
            catch
            {
                // Process may have exited or be inaccessible; skip it safely.
            }
        }

        return matchingWindows;
    }

    /// <summary>
    /// Returns a human-readable representation including title and PID.
    /// </summary>
    public override string ToString() => $"{Title} (PID: {ProcessId})";
}
