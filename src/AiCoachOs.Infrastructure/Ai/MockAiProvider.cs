using System.Text.RegularExpressions;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;

namespace AiCoachOs.Infrastructure.Ai;

public class MockAiProvider : IAiProvider
{
    public string ProviderName => "Mock";
    public string DefaultModelName => "mock-reasoning-v1";

    public Task<AiCompletionResponse> GenerateCompletionAsync(
        AiCompletionRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Extract any GUIDs present in the user prompt (representing eligible knowledge claim IDs)
        var guidRegex = new Regex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
        var matches = guidRegex.Matches(request.UserPrompt);
        var citedGuids = matches.Take(2).Select(m => Guid.Parse(m.Value)).ToList();

        var response = new AiCompletionResponse
        {
            IsSuccess = true,
            ProviderName = ProviderName,
            ModelName = DefaultModelName,
            TokensUsed = 350,
            RecommendationText = "Recommended program stimulus adjusted based on client recovery capacity and active constraints.",
            RationaleText = "Evidence demonstrates that matching weekly stimulus to individual recovery capacity optimizes progressive overload without inducing excessive systemic fatigue.",
            ConfidenceStatement = "High confidence based on structured scientific knowledge base and verified client memory snapshot.",
            EvidenceClaimRefs = citedGuids
        };

        return Task.FromResult(response);
    }
}
