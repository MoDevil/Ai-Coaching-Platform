namespace AiCoachOs.Application.Ai.Dtos;

public class VideoFrame
{
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string MimeType { get; set; } = "image/jpeg";
    public decimal TimestampSeconds { get; set; }
    public int FrameIndex { get; set; }
}

public class AiVideoRequest
{
    public List<VideoFrame> Frames { get; set; } = new();
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public int? MaxTokens { get; set; } = 1500;
    public string? Model { get; set; }
}
