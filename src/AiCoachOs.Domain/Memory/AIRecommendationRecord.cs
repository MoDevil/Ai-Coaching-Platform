using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Memory;

/// <summary>
/// Schema and persistence entity for AI recommendations.
/// M13 owns schema/persistence only. Recommendation generation occurs in downstream milestones.
/// </summary>
public class AIRecommendationRecord : Entity<Guid>
{
    private readonly List<Guid> _knowledgeClaimRefs = new();

    public Guid ClientId { get; private set; }
    public Guid CoachId { get; private set; }
    public AIRecommendationCategory RecommendationCategory { get; private set; }
    public string RecommendationText { get; private set; } = string.Empty;
    public string RationaleText { get; private set; } = string.Empty;
    public string ConfidenceStatement { get; private set; } = string.Empty;
    public DateTime GeneratedAt { get; private set; }
    public string AIProvider { get; private set; } = string.Empty;
    public string AIModel { get; private set; } = string.Empty;

    public AIRecommendationReviewStatus ReviewStatus { get; private set; }
    public CoachDecisionOutcome? CoachDecision { get; private set; }
    public string? CoachDecisionNote { get; private set; }
    public DateTime? CoachDecisionAt { get; private set; }
    public string? FinalImplementedPlan { get; private set; }
    public Guid? LinkedMemoryRecordId { get; private set; }

    public IReadOnlyCollection<Guid> KnowledgeClaimRefs => _knowledgeClaimRefs;

    private AIRecommendationRecord() { } // EF Core

    public AIRecommendationRecord(
        Guid id,
        Guid clientId,
        Guid coachId,
        AIRecommendationCategory recommendationCategory,
        string recommendationText,
        string rationaleText,
        string confidenceStatement,
        string aiProvider,
        string aiModel,
        IEnumerable<Guid>? knowledgeClaimRefs = null,
        DateTime? generatedAt = null,
        Guid? linkedMemoryRecordId = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (string.IsNullOrWhiteSpace(recommendationText))
            throw new ArgumentException("Recommendation text cannot be empty.", nameof(recommendationText));
        if (string.IsNullOrWhiteSpace(rationaleText))
            throw new ArgumentException("Rationale text cannot be empty.", nameof(rationaleText));

        ClientId = clientId;
        CoachId = coachId;
        RecommendationCategory = recommendationCategory;
        RecommendationText = recommendationText.Trim();
        RationaleText = rationaleText.Trim();
        ConfidenceStatement = confidenceStatement?.Trim() ?? string.Empty;
        AIProvider = string.IsNullOrWhiteSpace(aiProvider) ? "None" : aiProvider.Trim();
        AIModel = string.IsNullOrWhiteSpace(aiModel) ? "None" : aiModel.Trim();
        GeneratedAt = generatedAt ?? DateTime.UtcNow;
        ReviewStatus = AIRecommendationReviewStatus.PendingReview;
        LinkedMemoryRecordId = linkedMemoryRecordId;

        if (knowledgeClaimRefs != null)
        {
            _knowledgeClaimRefs.AddRange(knowledgeClaimRefs);
        }
    }

    public void RecordDecision(
        CoachDecisionOutcome decision,
        string? decisionNote = null,
        string? finalImplementedPlan = null)
    {
        CoachDecision = decision;
        CoachDecisionNote = string.IsNullOrWhiteSpace(decisionNote) ? null : decisionNote.Trim();
        FinalImplementedPlan = string.IsNullOrWhiteSpace(finalImplementedPlan) ? null : finalImplementedPlan.Trim();
        CoachDecisionAt = DateTime.UtcNow;

        ReviewStatus = decision switch
        {
            CoachDecisionOutcome.Accepted => AIRecommendationReviewStatus.Approved,
            CoachDecisionOutcome.AcceptedWithModifications => AIRecommendationReviewStatus.Modified,
            CoachDecisionOutcome.Rejected => AIRecommendationReviewStatus.Rejected,
            CoachDecisionOutcome.Deferred => AIRecommendationReviewStatus.PendingReview,
            _ => AIRecommendationReviewStatus.PendingReview
        };

        MarkUpdated();
    }
}
