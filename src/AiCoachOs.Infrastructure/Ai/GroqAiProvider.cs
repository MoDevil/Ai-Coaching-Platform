using AiCoachOs.Application.Ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Ai;

public class GroqAiProvider : OpenAiCompatibleChatProvider
{
    public GroqAiProvider(
        HttpClient httpClient,
        RotatingKeySelector keySelector,
        ILogger<GroqAiProvider> logger,
        string? model = null)
        : base(httpClient, keySelector, logger, model)
    {
    }

    public override string ProviderName => "Groq";

    public override string ChatCompletionsEndpoint => "https://api.groq.com/openai/v1/chat/completions";

    protected override string FallbackModelName => "llama-3.3-70b-versatile";
}
