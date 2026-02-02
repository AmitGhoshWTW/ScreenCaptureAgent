using ScreenCaptureAgent.Core.Models;

namespace ScreenCaptureAgent.CLI;

/// <summary>
/// Command-line argument parser
/// </summary>
public class CommandLineParser
{
    public static CaptureOptions Parse(string[] args)
    {
        var options = new CaptureOptions();
        
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLowerInvariant();
            
            switch (arg)
            {
                case "-h":
                case "--help":
                case "/?":
                    ShowHelp();
                    Environment.Exit(0);
                    break;

                case "-m":
                case "--mode":
                    if (i + 1 < args.Length)
                    {
                        options.Mode = ParseCaptureMode(args[++i]);
                    }
                    break;

                case "-o":
                case "--output":
                    if (i + 1 < args.Length)
                    {
                        options.OutputPath = args[++i];
                    }
                    break;

                case "-d":
                case "--directory":
                    if (i + 1 < args.Length)
                    {
                        options.OutputDirectory = args[++i];
                    }
                    break;

                case "-p":
                case "--prefix":
                    if (i + 1 < args.Length)
                    {
                        options.FileNamePrefix = args[++i];
                    }
                    break;

                case "-w":
                case "--window":
                    if (i + 1 < args.Length)
                    {
                        options.WindowTitle = args[++i];
                    }
                    break;

                case "-r":
                case "--region":
                    if (i + 4 < args.Length)
                    {
                        options.Region = new CaptureRegion
                        {
                            X = int.Parse(args[++i]),
                            Y = int.Parse(args[++i]),
                            Width = int.Parse(args[++i]),
                            Height = int.Parse(args[++i])
                        };
                    }
                    break;

                case "--monitor":
                    if (i + 1 < args.Length)
                    {
                        options.MonitorIndex = int.Parse(args[++i]);
                    }
                    break;

                case "--delay":
                    if (i + 1 < args.Length)
                    {
                        options.DelayMs = int.Parse(args[++i]);
                    }
                    break;

                case "--quality":
                    if (i + 1 < args.Length)
                    {
                        options.Quality = int.Parse(args[++i]);
                    }
                    break;

                case "--cursor":
                    options.IncludeCursor = true;
                    break;

                case "--no-winrt":
                    options.UseWinRtCapture = false;
                    break;

                case "-v":
                case "--verbose":
                    options.Verbose = true;
                    break;

                case "--list-windows":
                    ListWindows();
                    Environment.Exit(0);
                    break;
            }
        }

        return options;
    }

    private static CaptureMode ParseCaptureMode(string mode)
    {
        //return mode.ToLowerInvariant() switch
        //{
        //    "full" or "fullscreen" => CaptureMode.FullScreen,
        //    "active" or "activewindow" => CaptureMode.ActiveWindow,
        //    "window" or "windowbytitle" => CaptureMode.WindowByTitle,
        //    "region" => CaptureMode.Region,
        //    "multi" or "multimonitor" => CaptureMode.MultiMonitor,
        //    "monitor" => CaptureMode.SpecificMonitor,
        //    _ => throw new ArgumentException($"Invalid capture mode: {mode}")
        //};

        return mode.ToLowerInvariant() switch
        {
            "full" or "fullscreen" => CaptureMode.FullScreen,
            "active" or "activewindow" => CaptureMode.ActiveWindow,
            "window" or "windowbytitle" => CaptureMode.WindowByTitle,
            "region" => CaptureMode.Region,
            "multi" or "multimonitor" => CaptureMode.MultiMonitor,
            "monitor" => CaptureMode.SpecificMonitor,
            "chrome" or "allchrome" => CaptureMode.AllChromeWindows,      // NEW
            "edge" or "alledge" => CaptureMode.AllEdgeWindows,            // NEW
            "browsers" or "allbrowsers" => CaptureMode.AllBrowsers,       // NEW
            _ => throw new ArgumentException($"Invalid capture mode: {mode}")
        };
    }

    private static void ListWindows()
    {
        Console.WriteLine("Available Windows:");
        Console.WriteLine(new string('-', 80));
        
        var windows = WindowInfo.GetAllWindows();
        foreach (var window in windows)
        {
            Console.WriteLine($"  {window.Title}");
        }
        
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"Total: {windows.Count} windows");
    }

    private static void ShowHelp()
    {
        Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════════════════════╗
║                         Screen Capture Agent v1.0                            ║
║                  Enterprise-Grade Screenshot Automation Tool                 ║
╚══════════════════════════════════════════════════════════════════════════════╝

USAGE:
  ScreenCapture.exe [OPTIONS]

CAPTURE MODES:
  -m, --mode <mode>          Capture mode (default: fullscreen)
                             Options: full, active, window, region, multi, monitor
                             Browser modes: chrome, edge, browsers

OUTPUT OPTIONS:
  -o, --output <path>        Output file path (auto-generated if not specified)
  -d, --directory <path>     Output directory (default: current directory)
  -p, --prefix <name>        Filename prefix (default: 'screenshot')
  --quality <0-100>          Image quality for JPEG (default: 95)

WINDOW CAPTURE:
  -w, --window <title>       Window title to capture (partial match supported)

BROWSER CAPTURE:
  chrome                     Capture all Chrome browser windows
  edge                       Capture all Edge browser windows  
  browsers                   Capture all browser windows (Chrome + Edge)

REGION CAPTURE:
  -r, --region <x y w h>     Capture specific region (X Y Width Height)

MONITOR OPTIONS:
  --monitor <index>          Capture specific monitor (0-based index)

ADVANCED OPTIONS:
  --delay <ms>               Delay before capture in milliseconds
  --cursor                   Include mouse cursor in capture
  --no-winrt                 Disable WinRT capture (use GDI+ fallback)
  -v, --verbose              Enable verbose logging

UTILITY:
  --list-windows             List all available windows
  -h, --help                 Show this help message

EXAMPLES:

  1. Capture full screen:
     ScreenCapture.exe

  2. Capture active window:
     ScreenCapture.exe -m active

  3. Capture window by title:
     ScreenCapture.exe -m window -w ""Chrome""

  4. Capture specific region:
     ScreenCapture.exe -m region -r 100 100 800 600

  5. Capture to specific file:
     ScreenCapture.exe -o ""C:\Screenshots\error.png""

  6. Capture with custom prefix and directory:
     ScreenCapture.exe -d ""C:\Tickets"" -p ""ticket_12345""

  7. Capture second monitor:
     ScreenCapture.exe -m monitor --monitor 1

  8. Capture all monitors:
     ScreenCapture.exe -m multi

  9. List available windows:
     ScreenCapture.exe --list-windows

  10. Capture with delay (useful for menus):
      ScreenCapture.exe -m active --delay 3000

  11. Capture all Chrome windows:
      ScreenCapture.exe -m chrome

  12. Capture all Edge windows:
      ScreenCapture.exe -m edge

  13. Capture all browser windows with custom directory:
      ScreenCapture.exe -m browsers -d ""C:\BrowserCaptures"" -p ""browser_""

  14. Capture all Chrome windows with verbose output:
      ScreenCapture.exe -m chrome -v

INTEGRATION WITH TICKETING SYSTEMS:
  
  PowerShell example:
    $result = & ScreenCapture.exe -m active -d ""C:\Tickets"" -p ""ticket_$ticketId""
    if ($LASTEXITCODE -eq 0) {
        # Attach screenshot to ticket
    }

  Batch example:
    ScreenCapture.exe -m window -w ""%APP_NAME%"" -o ""%TEMP%\screenshot.png""
    if %ERRORLEVEL% EQU 0 (
        echo Screenshot captured successfully
    )

EXIT CODES:
  0  - Success
  1  - General error
  2  - Invalid arguments
  3  - Capture failed

NOTES:
  • Uses WinRT GraphicsCapture API for best results (no black screens in Chrome/Edge)
  • Falls back to GDI+ if WinRT is unavailable
  • PNG format recommended for best quality
  • Self-contained executable - no dependencies required

For more information, visit: https://github.com/your-org/screen-capture-agent
");
    }
}
