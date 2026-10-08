using AiCoachOs.Application.Ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Ai;

/// <summary>
/// Cerebras shared inference. Cerebras requires <c>stream: false</c> when
/// <c>response_format: json_object</c> is used; this client never sets <c>stream</c>, so the
/// request satisfies that constraint.
/// </summary>
public class CerebrasAiProvider : OpenAiCompatibleChatProvider
{
    public CerebrasAiProvider(
        HttpClient httpClient,
        RotatingKeySelector keySelector,
        ILogger<CerebrasAiProvider> logger,
        string? model = null)
        : base(httpClient, keySelector, logger, model)
    {
    }

    public override string ProviderName => "Cerebras";

    public override string ChatCompletionsEndpoint => "https://api.cerebras.ai/v1/chat/completions";

    protected override string FallbackModelName => "qwen-3.8-27b";
}
