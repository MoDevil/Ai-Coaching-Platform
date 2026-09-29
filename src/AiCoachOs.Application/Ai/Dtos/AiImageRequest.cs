namespace AiCoachOs.Application.Ai.Dtos;

public class AiImagePayload
{
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string MimeType { get; set; } = "image/jpeg";
    public string Label { get; set; } = "Current Photo";
}

public class AiImageRequest
{
    public List<AiImagePayload> Images { get; set; } = new();
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public int? MaxTokens { get; set; }
    public string? Model { get; set; }
}
