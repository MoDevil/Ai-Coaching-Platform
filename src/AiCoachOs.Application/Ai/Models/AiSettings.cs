namespace AiCoachOs.Application.Ai.Models;

public class AiSettings
{
    public const string SectionName = "AiSettings";

    public string Provider { get; set; } = "Mock";
    public string AnthropicApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-6";
    public double Temperature { get; set; } = 0.2;
    public int MaxTokens { get; set; } = 2048;
    public int TimeoutSeconds { get; set; } = 30;
}
