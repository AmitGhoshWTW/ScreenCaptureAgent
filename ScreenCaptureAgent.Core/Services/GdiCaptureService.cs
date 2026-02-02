using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using ScreenCaptureAgent.Core.Models;

namespace ScreenCaptureAgent.Core.Services;

/// <summary>
/// Screen capture service using GDI+ (fallback method)
/// </summary>
public class GdiCaptureService
{
    public Bitmap? CaptureFullScreen()
    {
        try
        {
            // Get the bounds of all screens combined
            var bounds = GetVirtualScreenBounds();

            // Create bitmap with absolute dimensions
            var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Copy from the virtual screen origin
                graphics.CopyFromScreen(
                    bounds.Left,
                    bounds.Top,
                    0,
                    0,
                    bounds.Size,
                    CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GDI full screen capture failed: {ex.Message}");
            return null;
        }
    }

    public Bitmap? CaptureActiveWindow()
    {
        try
        {
            var foregroundWindow = Native.NativeMethods.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
                return null;

            return CaptureWindow(foregroundWindow);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GDI active window capture failed: {ex.Message}");
            return null;
        }
    }

    public Bitmap? CaptureWindow(IntPtr windowHandle)
    {
        try
        {
            if (windowHandle == IntPtr.Zero)
                return null;

            // Get window rectangle
            if (!Native.NativeMethods.GetWindowRect(windowHandle, out var rect))
                return null;

            // Calculate dimensions
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            // Validate dimensions
            if (width <= 0 || height <= 0)
                return null;

            // Limit maximum dimensions to prevent memory issues
            const int maxDimension = 16384; // 16K pixels
            if (width > maxDimension || height > maxDimension)
            {
                System.Diagnostics.Debug.WriteLine($"Window too large: {width}x{height}");
                return null;
            }

            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(
                    rect.Left,
                    rect.Top,
                    0,
                    0,
                    new Size(width, height),
                    CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GDI window capture failed: {ex.Message}");
            return null;
        }
    }

    public Bitmap? CaptureRegion(Rectangle region)
    {
        try
        {
            // Validate region
            if (region.Width <= 0 || region.Height <= 0)
                return null;

            // Limit maximum dimensions
            const int maxDimension = 16384;
            if (region.Width > maxDimension || region.Height > maxDimension)
            {
                System.Diagnostics.Debug.WriteLine($"Region too large: {region.Width}x{region.Height}");
                return null;
            }

            var bitmap = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(
                    region.Left,
                    region.Top,
                    0,
                    0,
                    region.Size,
                    CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GDI region capture failed: {ex.Message}");
            return null;
        }
    }

    public Bitmap? CaptureMonitor(int monitorIndex)
    {
        try
        {
            var screens = Screen.AllScreens;

            if (monitorIndex < 0 || monitorIndex >= screens.Length)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid monitor index: {monitorIndex}");
                return null;
            }

            var screen = screens[monitorIndex];
            var bounds = screen.Bounds;

            // Validate dimensions
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return null;

            var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(
                    bounds.Left,
                    bounds.Top,
                    0,
                    0,
                    bounds.Size,
                    CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GDI monitor capture failed: {ex.Message}");
            return null;
        }
    }

    private Rectangle GetVirtualScreenBounds()
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (var screen in Screen.AllScreens)
        {
            minX = Math.Min(minX, screen.Bounds.Left);
            minY = Math.Min(minY, screen.Bounds.Top);
            maxX = Math.Max(maxX, screen.Bounds.Right);
            maxY = Math.Max(maxY, screen.Bounds.Bottom);
        }

        return new Rectangle(
            minX,
            minY,
            maxX - minX,
            maxY - minY
        );
    }
}
