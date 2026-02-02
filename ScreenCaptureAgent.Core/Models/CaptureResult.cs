namespace ScreenCaptureAgent.Core.Models;

/// <summary>
/// Describes the result of a capture operation, including success status, file path,
/// dimensions, size, duration, and optional batch results.
/// </summary>
public class CaptureResult
{
    /// <summary>
    /// True when the capture succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Output file path of the captured image (if any).
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Captured image width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Captured image height in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Resulting file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Duration of the capture operation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Error message when capture failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Collection of individual results for batch operations.
    /// </summary>
    public List<CaptureResult>? BatchResults { get; set; }

    /// <summary>
    /// Count of successful items in a batch or 1 if single success.
    /// </summary>
    public int SuccessCount => BatchResults?.Count(r => r.Success) ?? (Success ? 1 : 0);

    /// <summary>
    /// Count of failed items in a batch or 1 if single failure.
    /// </summary>
    public int FailureCount => BatchResults?.Count(r => !r.Success) ?? (Success ? 0 : 1);

    /// <summary>
    /// True if this result represents a batch capture.
    /// </summary>
    public bool IsBatch => BatchResults != null && BatchResults.Count > 0;

    /// <summary>
    /// Creates a successful single capture result.
    /// </summary>
    /// <param name="filePath">Path to the captured file.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="fileSize">File size in bytes.</param>
    /// <param name="duration">Capture duration.</param>
    public static CaptureResult SuccessResult(string filePath, int width, int height, long fileSize, TimeSpan duration)
    {
        // Populate a success record with metadata about the output file and timing.
        return new CaptureResult
        {
            Success = true,
            FilePath = filePath,
            Width = width,
            Height = height,
            FileSize = fileSize,
            Duration = duration
        };
    }

    /// <summary>
    /// Creates a failed capture result with an error message.
    /// </summary>
    /// <param name="errorMessage">Failure reason.</param>
    public static CaptureResult Failure(string errorMessage)
    {
        // Store the error to aid diagnostics and user feedback.
        return new CaptureResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Creates a batch result aggregating multiple capture outcomes.
    /// </summary>
    /// <param name="results">List of individual capture results.</param>
    /// <param name="totalDuration">Total time across the batch.</param>
    public static CaptureResult BatchResult(List<CaptureResult> results, TimeSpan totalDuration)
    {
        // Aggregate batch success if any child succeeded; include total duration across items.
        return new CaptureResult
        {
            Success = results.Any(r => r.Success),
            BatchResults = results,
            Duration = totalDuration
        };
    }
}