using System.Net.Http.Headers;
using AiCoachOs.Application.Ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Ai;

public class OpenRouterAiProvider : OpenAiCompatibleChatProvider
{
    public OpenRouterAiProvider(
        HttpClient httpClient,
        RotatingKeySelector keySelector,
        ILogger<OpenRouterAiProvider> logger,
        string? model = null)
        : base(httpClient, keySelector, logger, model)
    {
    }

    public override string ProviderName => "OpenRouter";

    public override string ChatCompletionsEndpoint => "https://openrouter.ai/api/v1/chat/completions";

    protected override string FallbackModelName => "openai/gpt-4o-mini";

    /// <summary>
    /// OpenRouter attributes traffic using the optional X-Title header. No referer is sent because
    /// this application is served from a deployment-specific origin that is not known at build time.
    /// </summary>
    protected override void ConfigureRequest(HttpRequestMessage httpRequest)
    {
        httpRequest.Headers.TryAddWithoutValidation("X-Title", "AI Coach OS");
    }
}
