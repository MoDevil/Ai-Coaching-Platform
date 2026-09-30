using System.Text.Json.Serialization;
using AiCoachOs.Domain.Memory;

namespace AiCoachOs.Application.Ai.Dtos;

public class GenerateReasoningRequestDto
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("reasoningCategory")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReasoningCategory ReasoningCategory { get; set; }

    [JsonPropertyName("additionalContext")]
    public string? AdditionalContext { get; set; }
}

public class AiCompletionRequest
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
}

public class StructuredRecommendation
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("observations")]
    public List<string> Observations { get; set; } = new();

    [JsonPropertyName("recommendations")]
    public List<string> Recommendations { get; set; } = new();

    [JsonPropertyName("rationale")]
    public string Rationale { get; set; } = string.Empty;

    [JsonPropertyName("confidence_statement")]
    public string ConfidenceStatement { get; set; } = string.Empty;

    [JsonPropertyName("assumptions")]
    public List<string> Assumptions { get; set; } = new();

    [JsonPropertyName("missing_high_value_data")]
    public List<string> MissingHighValueData { get; set; } = new();

    [JsonPropertyName("evidence_refs")]
    public List<Guid> EvidenceRefs { get; set; } = new();

    [JsonPropertyName("safety_summary")]
    public string? SafetySummary { get; set; }

    [JsonPropertyName("coach_action_required")]
    public bool CoachActionRequired { get; set; } = true;
}

public class AiCompletionResponse
{
    public bool IsSuccess { get; set; }
    public StructuredRecommendation? StructuredRecommendation { get; set; }
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
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("observations")]
    public List<string>? Observations { get; set; }

    [JsonPropertyName("recommendations")]
    public List<string>? Recommendations { get; set; }

    [JsonPropertyName("recommendation")]
    public string? LegacyRecommendation { get; set; }

    [JsonPropertyName("rationale")]
    public string? Rationale { get; set; }

    [JsonPropertyName("confidence_statement")]
    public string? ConfidenceStatement { get; set; }

    [JsonPropertyName("assumptions")]
    public List<string>? Assumptions { get; set; }

    [JsonPropertyName("missing_high_value_data")]
    public List<string>? MissingHighValueData { get; set; }

    [JsonPropertyName("evidence_refs")]
    public List<string>? EvidenceRefs { get; set; }

    [JsonPropertyName("evidence_claim_ids")]
    public List<string>? EvidenceClaimIds { get; set; }

    [JsonPropertyName("safety_summary")]
    public string? SafetySummary { get; set; }

    [JsonPropertyName("coach_action_required")]
    public bool? CoachActionRequired { get; set; }
}

public class ReviewAIRecommendationRequestDto
{
    [JsonPropertyName("reviewStatus")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AIRecommendationReviewStatus ReviewStatus { get; set; }

    [JsonPropertyName("coachDecision")]
    public string? CoachDecision { get; set; }

    [JsonPropertyName("finalImplementedPlan")]
    public string? FinalImplementedPlan { get; set; }
}

public class ResolvedKnowledgeClaimDto
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string ClaimText { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Domain.Knowledge.EvidenceLevel EvidenceLevel { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Domain.Knowledge.ClaimStatus Status { get; set; }
}

public class AIRecommendationSummaryDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid CoachId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AIRecommendationCategory Category { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AIRecommendationReviewStatus Status { get; set; }
    public bool CoachActionRequired { get; set; }
    public string? SafetySummary { get; set; }
    public int KnowledgeClaimCount { get; set; }
}

public class AIRecommendationDetailDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid CoachId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AIRecommendationCategory RecommendationCategory { get; set; }
    public string Summary { get; set; } = string.Empty;
    public IReadOnlyList<string> Observations { get; set; } = new List<string>();
    public IReadOnlyList<string> Recommendations { get; set; } = new List<string>();
    public string Rationale { get; set; } = string.Empty;
    public string ConfidenceStatement { get; set; } = string.Empty;
    public IReadOnlyList<string> Assumptions { get; set; } = new List<string>();
    public IReadOnlyList<string> MissingHighValueData { get; set; } = new List<string>();
    public string? SafetySummary { get; set; }
    public bool CoachActionRequired { get; set; } = true;
    public IReadOnlyList<ResolvedKnowledgeClaimDto> ResolvedKnowledgeClaims { get; set; } = new List<ResolvedKnowledgeClaimDto>();
    public string AIProvider { get; set; } = string.Empty;
    public string AIModel { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AIRecommendationReviewStatus ReviewStatus { get; set; }
    public string? CoachDecision { get; set; }
    public DateTime? CoachDecisionAt { get; set; }
    public string? FinalImplementedPlan { get; set; }
}

