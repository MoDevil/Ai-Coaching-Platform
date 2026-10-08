using AiCoachOs.Application.Ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Ai;

/// <summary>
/// SambaNova SambaCloud developer endpoint. No vendor-specific headers are required; the security
/// scheme is plain bearer authentication.
/// </summary>
public class SambaNovaAiProvider : OpenAiCompatibleChatProvider
{
    public SambaNovaAiProvider(
        HttpClient httpClient,
        RotatingKeySelector keySelector,
        ILogger<SambaNovaAiProvider> logger,
        string? model = null)
        : base(httpClient, keySelector, logger, model)
    {
    }

    public override string ProviderName => "SambaNova";

    public override string ChatCompletionsEndpoint => "https://api.sambanova.ai/v1/chat/completions";

    protected override string FallbackModelName => "gpt-oss-120b";
}
