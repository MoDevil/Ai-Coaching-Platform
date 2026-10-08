using AiCoachOs.Application.Ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Ai;

/// <summary>
/// xAI Grok. The chat-completions surface is documented as legacy in favour of the responses API,
/// but it remains supported and is the only one that speaks the OpenAI chat dialect this client
/// implements, so the provider is registered against it deliberately.
/// </summary>
public class XAiProvider : OpenAiCompatibleChatProvider
{
    public XAiProvider(
        HttpClient httpClient,
        RotatingKeySelector keySelector,
        ILogger<XAiProvider> logger,
        string? model = null)
        : base(httpClient, keySelector, logger, model)
    {
    }

    public override string ProviderName => "xAI";

    public override string ChatCompletionsEndpoint => "https://api.x.ai/v1/chat/completions";

    protected override string FallbackModelName => "grok-4.6";
}
