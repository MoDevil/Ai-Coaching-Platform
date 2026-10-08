using AiCoachOs.Application.Ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Ai;

/// <summary>
/// HuggingFace Inference Providers router, which exposes an OpenAI-compatible chat-completions API.
/// </summary>
/// <remarks>
/// Model ids on this router must be namespaced, and an optional routing suffix (for example
/// <c>:cheapest</c> or <c>:cerebras</c>) can pin the upstream provider. Structured-output support
/// varies per upstream model, so a suffix should be configured if JSON mode must be reliable.
/// </remarks>
public class HuggingFaceAiProvider : OpenAiCompatibleChatProvider
{
    public HuggingFaceAiProvider(
        HttpClient httpClient,
        RotatingKeySelector keySelector,
        ILogger<HuggingFaceAiProvider> logger,
        string? model = null)
        : base(httpClient, keySelector, logger, model)
    {
    }

    public override string ProviderName => "HuggingFace";

    public override string ChatCompletionsEndpoint => "https://router.huggingface.co/v1/chat/completions";

    protected override string FallbackModelName => "openai/gpt-oss-120b";
}
