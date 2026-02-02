using ScreenCaptureAgent.CLI;
using ScreenCaptureAgent.Core.Services;
using SharpDX;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScreenCaptureAgent;

class Program
{
    private static int ExitCodeSuccess = 0;
    private static int ExitCodeGeneralError = 1;
    private static int ExitCodeInvalidArguments = 2;
    private static int ExitCodeCaptureFailed = 3;

    static async Task<int> Main(string[] args)
    {
        try
        {
            // Set console title
            Console.Title = "Screen Capture Agent";

            // Show banner
            ShowBanner();

            // Parse command-line arguments
            var options = CommandLineParser.Parse(args);

            // Create logger if verbose mode is enabled
            Action<string>? logger = options.Verbose ? Console.WriteLine : null;

            // Create capture service
            using var captureService = new ScreenCaptureService(options.UseWinRtCapture, logger);

            // Perform capture
            if (options.Verbose)
            {
                Console.WriteLine($"\nStarting capture...");
                Console.WriteLine($"Mode: {options.Mode}");
                Console.WriteLine($"Output Directory: {options.OutputDirectory}");
            }

            var result = await captureService.CaptureAsync(options);

            // Display result
            if (result.Success)
            {
                if (result.IsBatch)
                {
                    // Display batch results
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ Batch capture completed!");
                    Console.ResetColor();

                    Console.WriteLine($"\nResults:");
                    Console.WriteLine($"  Total: {result.BatchResults!.Count} window(s)");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  Successful: {result.SuccessCount}");
                    Console.ResetColor();

                    if (result.FailureCount > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  Failed: {result.FailureCount}");
                        Console.ResetColor();
                    }

                    Console.WriteLine($"  Duration: {result.Duration.TotalMilliseconds:F0}ms");
                    Console.WriteLine($"  Output Directory: {options.OutputDirectory}");

                    if (options.Verbose)
                    {
                        Console.WriteLine($"\nCaptured files:");
                        foreach (var batchResult in result.BatchResults.Where(r => r.Success))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write($"  ✓ ");
                            Console.ResetColor();
                            Console.WriteLine(Path.GetFileName(batchResult.FilePath));
                            Console.WriteLine($"    Size: {batchResult.Width}x{batchResult.Height} pixels ({FormatFileSize(batchResult.FileSize)})");
                        }

                        if (result.FailureCount > 0)
                        {
                            Console.WriteLine($"\nFailed captures:");
                            foreach (var batchResult in result.BatchResults.Where(r => !r.Success))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write($"  ✗ ");
                                Console.ResetColor();
                                Console.WriteLine(batchResult.ErrorMessage);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"\nUse --verbose (-v) to see individual file paths");
                    }

                    return result.FailureCount == 0 ? ExitCodeSuccess : ExitCodeCaptureFailed;
                }
                else
                {
                    // Single capture result
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ Screenshot captured successfully!");
                    Console.ResetColor();

                    Console.WriteLine($"\nFile: {result.FilePath}");
                    Console.WriteLine($"Size: {result.Width}x{result.Height} pixels");
                    Console.WriteLine($"File Size: {FormatFileSize(result.FileSize)}");
                    Console.WriteLine($"Duration: {result.Duration.TotalMilliseconds:F0}ms");

                    if (options.Verbose)
                    {
                        Console.WriteLine($"\nFull path: {Path.GetFullPath(result.FilePath!)}");
                    }

                    return ExitCodeSuccess;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n✗ Capture failed: {result.ErrorMessage}");
                Console.ResetColor();

                Console.WriteLine("\nTroubleshooting:");
                Console.WriteLine("  • Check if the target window/application is visible");
                Console.WriteLine("  • Try running as Administrator");
                Console.WriteLine("  • Use --list-windows to see available windows");
                Console.WriteLine("  • Use --verbose for detailed logging");
                Console.WriteLine("  • Use --no-winrt to try GDI+ fallback");

                return ExitCodeCaptureFailed;
            }
        }
        catch (ArgumentException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ Invalid arguments: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("\nUse --help for usage information");
            return ExitCodeInvalidArguments;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ Unexpected error: {ex.Message}");
            Console.ResetColor();

            if (args.Contains("--verbose") || args.Contains("-v"))
            {
                Console.WriteLine($"\nStack trace:");
                Console.WriteLine(ex.StackTrace);
            }

            return ExitCodeGeneralError;
        }
    }

    private static void ShowBanner()
    {
        if (Console.IsOutputRedirected)
            return;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════════════════════╗
║                         Screen Capture Agent v1.0                            ║
║                  Enterprise-Grade Screenshot Automation Tool                 ║
╚══════════════════════════════════════════════════════════════════════════════╝
");
        Console.ResetColor();
    }

    private static string FormatFileSize(long bytes)
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
}