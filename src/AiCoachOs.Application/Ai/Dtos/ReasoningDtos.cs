using System.Text.Json.Serialization;
using AiCoachOs.Domain.Memory;

namespace AiCoachOs.Application.Ai.Dtos;

public class GenerateReasoningRequestDto
{
    public Guid ClientId { get; set; }
    public AIRecommendationCategory Category { get; set; }
    public string? GuidanceNote { get; set; }
}

public class AiCompletionRequest
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
}

public class AiCompletionResponse
{
    public bool IsSuccess { get; set; }
    public string RecommendationText { get; set; } = string.Empty;
    public string RationaleText { get; set; } = string.Empty;
    public string ConfidenceStatement { get; set; } = string.Empty;
    public List<Guid> EvidenceClaimRefs { get; set; } = new();
    public string ProviderName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public int? TokensUsed { get; set; }
    public string? ErrorMessage { get; set; }
}

public class StructuredAiRecommendationJson
{
    [JsonPropertyName("recommendation")]
    public string Recommendation { get; set; } = string.Empty;

    [JsonPropertyName("rationale")]
    public string Rationale { get; set; } = string.Empty;

    [JsonPropertyName("confidence_statement")]
    public string ConfidenceStatement { get; set; } = string.Empty;

    [JsonPropertyName("evidence_claim_ids")]
    public List<string> EvidenceClaimIds { get; set; } = new();
}
