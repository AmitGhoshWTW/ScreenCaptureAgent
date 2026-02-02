using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using ScreenCaptureAgent.Core.Models;
using static ScreenCaptureAgent.Core.Native.NativeMethods;

namespace ScreenCaptureAgent.Core.Services;

/// <summary>
/// Main screen capture service that orchestrates capture operations
/// </summary>
public class ScreenCaptureService : IDisposable
{
    private readonly WinRtCaptureService? _winRtCapture;
    private readonly GdiCaptureService _gdiCapture;
    private readonly Action<string>? _logger;

    public ScreenCaptureService(bool useWinRt = true, Action<string>? logger = null)
    {
        _logger = logger;
        _gdiCapture = new GdiCaptureService();

        if (useWinRt)
        {
            try
            {
                _winRtCapture = new WinRtCaptureService();
                Log("WinRT capture initialized successfully");
            }
            catch (Exception ex)
            {
                Log($"WinRT capture initialization failed: {ex.Message}, falling back to GDI");
            }
        }
    }

    private async Task<Bitmap?> CaptureFullScreenAsync(CaptureOptions options)
    {
        if (options.UseWinRtCapture && _winRtCapture != null)
        {
            try
            {
                var primaryMonitor = GetPrimaryMonitorHandle();
                var bitmap = await _winRtCapture.CaptureMonitorAsync(primaryMonitor);
                if (bitmap != null)
                {
                    Log("Captured using WinRT (primary monitor)");
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Log($"WinRT capture failed: {ex.Message}, falling back to GDI");
            }
        }

        Log("Capturing using GDI (primary monitor)");
        return _gdiCapture.CaptureMonitor(0);
    }

    private async Task<Bitmap?> CaptureActiveWindowAsync(CaptureOptions options)
    {
        var activeWindow = GetForegroundWindow();
        if (activeWindow == IntPtr.Zero)
        {
            Log("No active window found");
            return null;
        }

        var windowInfo = WindowInfo.FromHandle(activeWindow);
        if (windowInfo != null)
        {
            Log($"Capturing active window: {windowInfo.Title}");

            // Ensure window is restored if minimized
            BringWindowToForeground(activeWindow);

            // Small delay to let window render
            var delayMs = options.DelayMs > 0 ? options.DelayMs : 200;
            await Task.Delay(delayMs);
        }

        if (options.UseWinRtCapture && _winRtCapture != null)
        {
            try
            {
                var bitmap = await _winRtCapture.CaptureWindowAsync(activeWindow);
                if (bitmap != null)
                {
                    Log("Captured using WinRT");
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Log($"WinRT capture failed: {ex.Message}, falling back to GDI");
            }
        }

        Log("Capturing using GDI");
        return _gdiCapture.CaptureWindow(activeWindow);
    }

    private async Task<Bitmap?> CaptureWindowByTitleAsync(CaptureOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.WindowTitle))
        {
            Log("Window title not specified");
            return null;
        }

        Log($"Searching for window: {options.WindowTitle}");
        var window = WindowInfo.FindByTitle(options.WindowTitle);

        if (window == null)
        {
            Log($"Window not found: {options.WindowTitle}");
            Log("Available windows:");
            foreach (var w in WindowInfo.GetAllWindows().Take(10))
            {
                Log($"  - {w.Title}");
            }
            return null;
        }

        Log($"Found window: {window.Title}");

        // ========== ADD THIS SECTION ==========
        // CRITICAL: Restore and bring window to foreground
        BringWindowToForeground(window.Handle);

        // Wait for window to render
        var delayMs = options.DelayMs > 0 ? options.DelayMs : 500;
        Log($"Waiting {delayMs}ms for window to render...");
        await Task.Delay(delayMs);
        // ========== END NEW SECTION ==========

        if (options.UseWinRtCapture && _winRtCapture != null)
        {
            try
            {
                var bitmap = await _winRtCapture.CaptureWindowAsync(window.Handle);
                if (bitmap != null)
                {
                    Log("Captured using WinRT");
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Log($"WinRT capture failed: {ex.Message}, falling back to GDI");
            }
        }

        Log("Capturing using GDI");
        return _gdiCapture.CaptureWindow(window.Handle);
    }

    private Bitmap? CaptureRegion(CaptureOptions options)
    {
        if (options.Region == null)
        {
            Log("Region not specified");
            return null;
        }

        Log($"Capturing region: X={options.Region.X}, Y={options.Region.Y}, W={options.Region.Width}, H={options.Region.Height}");

        var rectangle = new Rectangle(
            options.Region.X,
            options.Region.Y,
            options.Region.Width,
            options.Region.Height
        );

        return _gdiCapture.CaptureRegion(rectangle);
    }

    private Bitmap? CaptureMultiMonitor(CaptureOptions options)
    {
        Log("Capturing all monitors (virtual screen)");
        return _gdiCapture.CaptureFullScreen();
    }

    private Bitmap? CaptureSpecificMonitor(CaptureOptions options)
    {
        Log($"Capturing monitor #{options.MonitorIndex}");
        return _gdiCapture.CaptureMonitor(options.MonitorIndex);
    }

    private string GenerateOutputPath(CaptureOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.OutputPath))
        {
            // Check if it's a directory path
            if (options.OutputPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ||
                options.OutputPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()) ||
                Directory.Exists(options.OutputPath))
            {
                // It's a directory, generate filename
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var filename = $"{options.FileNamePrefix}_{timestamp}.png";
                return Path.Combine(options.OutputPath, filename);
            }

            return options.OutputPath;
        }

        var defaultTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        var defaultFilename = $"{options.FileNamePrefix}_{defaultTimestamp}.png";
        return Path.Combine(options.OutputDirectory, defaultFilename);
    }

    private void SaveBitmap(Bitmap bitmap, string path, int quality)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();

        switch (extension)
        {
            case ".png":
                bitmap.Save(path, ImageFormat.Png);
                break;
            case ".jpg":
            case ".jpeg":
                SaveAsJpeg(bitmap, path, quality);
                break;
            case ".bmp":
                bitmap.Save(path, ImageFormat.Bmp);
                break;
            default:
                bitmap.Save(path, ImageFormat.Png);
                break;
        }
    }

    private void SaveAsJpeg(Bitmap bitmap, string path, int quality)
    {
        var encoder = ImageCodecInfo.GetImageEncoders()
            .First(e => e.FormatID == ImageFormat.Jpeg.Guid);

        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);

        bitmap.Save(path, encoder, encoderParams);
    }

    private IntPtr GetPrimaryMonitorHandle()
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (screens.Length > 0)
        {
            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
            if (primaryScreen != null)
            {
                var desktopWindow = GetDesktopWindow();
                return MonitorFromWindow(desktopWindow, 1);
            }
        }
        return IntPtr.Zero;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    private void Log(string message)
    {
        _logger?.Invoke($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    private async Task<Bitmap?> CaptureByModeAsync(CaptureOptions options)
    {
        Log($"Capture mode: {options.Mode}");

        return options.Mode switch
        {
            CaptureMode.FullScreen => await CaptureFullScreenAsync(options),
            CaptureMode.ActiveWindow => await CaptureActiveWindowAsync(options),
            CaptureMode.WindowByTitle => await CaptureWindowByTitleAsync(options),
            CaptureMode.Region => CaptureRegion(options),
            CaptureMode.MultiMonitor => CaptureMultiMonitor(options),
            CaptureMode.SpecificMonitor => CaptureSpecificMonitor(options),
            _ => throw new ArgumentException($"Invalid capture mode: {options.Mode}")
        };
    }

    public async Task<CaptureResult> CaptureAsync(CaptureOptions options)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Handle batch capture modes
            if (options.Mode == CaptureMode.AllChromeWindows ||
                options.Mode == CaptureMode.AllEdgeWindows ||
                options.Mode == CaptureMode.AllBrowsers)
            {
                return await CaptureBatchAsync(options);
            }

            // Apply delay if specified (only for non-window captures, as window captures have their own delay)
            if (options.DelayMs > 0 && options.Mode != CaptureMode.ActiveWindow && options.Mode != CaptureMode.WindowByTitle)
            {
                Log($"Waiting {options.DelayMs}ms before capture...");
                await Task.Delay(options.DelayMs);
            }

            Bitmap? bitmap = null;

            try
            {
                bitmap = await CaptureByModeAsync(options);

                if (bitmap == null)
                {
                    return CaptureResult.Failure("Failed to capture screenshot");
                }

                var outputPath = GenerateOutputPath(options);
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                int width = bitmap.Width;
                int height = bitmap.Height;

                SaveBitmap(bitmap, outputPath, options.Quality);

                var fileInfo = new FileInfo(outputPath);
                stopwatch.Stop();

                Log($"Screenshot saved: {outputPath} ({FormatFileSize(fileInfo.Length)})");

                return CaptureResult.SuccessResult(
                    outputPath,
                    width,
                    height,
                    fileInfo.Length,
                    stopwatch.Elapsed);
            }
            finally
            {
                bitmap?.Dispose();
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log($"Capture error: {ex.Message}");
            return CaptureResult.Failure($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private async Task<CaptureResult> CaptureBatchAsync(CaptureOptions options)
    {
        var batchStopwatch = Stopwatch.StartNew();

        List<WindowInfo> windows = options.Mode switch
        {
            CaptureMode.AllChromeWindows => WindowInfo.FindAllChromeWindows(),
            CaptureMode.AllEdgeWindows => WindowInfo.FindAllEdgeWindows(),
            CaptureMode.AllBrowsers => WindowInfo.FindAllBrowserWindows(),
            _ => new List<WindowInfo>()
        };

        if (windows.Count == 0)
        {
            Log($"No windows found for capture mode: {options.Mode}");
            return CaptureResult.Failure($"No browser windows found");
        }

        Log($"Found {windows.Count} window(s) to capture");

        var results = new List<CaptureResult>();
        int successCount = 0;
        int failureCount = 0;

        var originalForegroundWindow = GetForegroundWindow();

        foreach (var window in windows)
        {
            try
            {
                Log($"Capturing: {window.Title}");

                BringWindowToForeground(window.Handle);

                await Task.Delay(options.DelayMs > 0 ? options.DelayMs : 500);

                Bitmap? bitmap = null;

                if (options.UseWinRtCapture && _winRtCapture != null)
                {
                    try
                    {
                        bitmap = await _winRtCapture.CaptureWindowAsync(window.Handle);
                        if (bitmap != null)
                        {
                            Log($"  Captured using WinRT");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"  WinRT failed: {ex.Message}, falling back to GDI");
                    }
                }

                if (bitmap == null)
                {
                    bitmap = _gdiCapture.CaptureWindow(window.Handle);
                    if (bitmap != null)
                    {
                        Log($"  Captured using GDI");
                    }
                }

                if (bitmap == null)
                {
                    Log($"  Failed to capture window");
                    results.Add(CaptureResult.Failure($"Failed to capture: {window.Title}"));
                    failureCount++;
                    continue;
                }

                var sanitizedTitle = SanitizeFileName(window.Title);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var filename = $"{options.FileNamePrefix}_{sanitizedTitle}_{timestamp}.png";
                var outputPath = Path.Combine(options.OutputDirectory, filename);

                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                int width = bitmap.Width;
                int height = bitmap.Height;

                SaveBitmap(bitmap, outputPath, options.Quality);
                bitmap.Dispose();

                var fileInfo = new FileInfo(outputPath);

                Log($"  Saved: {outputPath} ({FormatFileSize(fileInfo.Length)})");

                results.Add(CaptureResult.SuccessResult(
                    outputPath,
                    width,
                    height,
                    fileInfo.Length,
                    TimeSpan.Zero));

                successCount++;
            }
            catch (Exception ex)
            {
                Log($"  Error capturing {window.Title}: {ex.Message}");
                results.Add(CaptureResult.Failure($"Error: {window.Title} - {ex.Message}"));
                failureCount++;
            }
        }

        if (originalForegroundWindow != IntPtr.Zero)
        {
            SetForegroundWindow(originalForegroundWindow);
        }

        batchStopwatch.Stop();

        Log($"Batch capture complete: {successCount} succeeded, {failureCount} failed");

        return CaptureResult.BatchResult(results, batchStopwatch.Elapsed);
    }

    private void BringWindowToForeground(IntPtr windowHandle)
    {
        try
        {
            if (IsIconic(windowHandle))
            {
                ShowWindow(windowHandle, SW_RESTORE);
                Log("Window was minimized, restoring...");
            }

            SetForegroundWindow(windowHandle);

            if (GetForegroundWindow() != windowHandle)
            {
                var threadId1 = GetCurrentThreadId();
                var threadId2 = GetWindowThreadProcessId(windowHandle, IntPtr.Zero);

                if (threadId1 != threadId2)
                {
                    AttachThreadInput(threadId1, threadId2, true);
                    SetForegroundWindow(windowHandle);
                    AttachThreadInput(threadId1, threadId2, false);
                }
            }
        }
        catch (Exception ex)
        {
            Log($"Warning: Failed to bring window to foreground: {ex.Message}");
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    private const int SW_RESTORE = 9;

    private string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));

        if (sanitized.Length > 50)
        {
            sanitized = sanitized.Substring(0, 50);
        }

        return sanitized;
    }

    public void Dispose()
    {
        _winRtCapture?.Dispose();
    }
}