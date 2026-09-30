namespace AiCoachOs.Application.Ai.Models;

public class AiProviderOptions
{
    public const string SectionName = "AiProviders";

    public List<ProviderConfig> Providers { get; set; } = new();
}

public class ProviderConfig
{
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public string ApiKeysEnvVar { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}
