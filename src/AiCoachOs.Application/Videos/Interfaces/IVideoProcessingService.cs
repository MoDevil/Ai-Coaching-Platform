namespace AiCoachOs.Application.Videos.Interfaces;

public class ExtractedFrame
{
    public int FrameIndex { get; set; }
    public decimal TimestampSeconds { get; set; }
    public byte[] FrameBytes { get; set; } = Array.Empty<byte>();
    public string MimeType { get; set; } = "image/jpeg";
}

public class ProcessedVideoResult
{
    public byte[] NormalizedVideoBytes { get; set; } = Array.Empty<byte>();
    public string MimeType { get; set; } = "video/mp4";
    public int DurationSeconds { get; set; }
    public List<ExtractedFrame> ExtractedFrames { get; set; } = new();
}

public interface IVideoProcessingService
{
    Task<ProcessedVideoResult> ProcessAndExtractFramesAsync(
        byte[] rawBytes, 
        string originalMimeType, 
        CancellationToken cancellationToken = default);
}
