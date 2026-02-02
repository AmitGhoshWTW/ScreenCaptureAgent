using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ScreenCaptureAgent.Core.Native
{
    /// <summary>
    /// Windows API interop for screen capture and window management.
    /// Provides P/Invoke signatures and structs for common window operations.
    /// </summary>
    public static class NativeMethods
    {
        /// <summary>
        /// Retrieves a handle to the foreground (active) window.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Retrieves a handle to the desktop window.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window.
        /// </summary>
        /// <param name="hWnd">Window handle.</param>
        /// <param name="lpRect">Output rectangle in screen coordinates.</param>
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// Enumerates all top-level windows on the screen by passing handles to a callback function.
        /// </summary>
        /// <param name="enumProc">Callback that receives window handles.</param>
        /// <param name="lParam">Application-defined value passed to callback.</param>
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        /// <summary>
        /// Copies the text of the specified window's title bar into a buffer.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        /// <summary>
        /// Retrieves the length, in characters, of the specified window's title bar text.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        /// <summary>
        /// Determines the visibility state of the specified window.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// Retrieves a handle to the Shell's desktop window.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetShellWindow();

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and optionally the process ID.
        /// </summary>
        /// <param name="hWnd">Window handle.</param>
        /// <param name="processId">Output process ID.</param>
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        /// <summary>
        /// Brings the specified window to the foreground and activates it.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Retrieves an attribute of a window with Desktop Window Manager (DWM).
        /// </summary>
        /// <param name="hwnd">Window handle.</param>
        /// <param name="dwAttribute">Attribute ID.</param>
        /// <param name="pvAttribute">Output attribute value.</param>
        /// <param name="cbAttribute">Size of the attribute structure.</param>
        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        /// <summary>
        /// DWM attribute: extended frame bounds (actual visual bounds including drop shadows).
        /// </summary>
        public const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        /// <summary>
        /// Callback signature for window enumeration.
        /// </summary>
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // === FIX: Must be PUBLIC because WindowInfo uses it ===
        /// <summary>
        /// Rectangle struct used by Win32 APIs. Must be public for downstream usage.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            /// <summary>
            /// Computed width from Right-Left.
            /// </summary>
            public int Width => Right - Left;

            /// <summary>
            /// Computed height from Bottom-Top.
            /// </summary>
            public int Height => Bottom - Top;
        }

        // (Optional but recommended) POINT should also be public
        /// <summary>
        /// Point struct used by Win32 APIs.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
    }
}
