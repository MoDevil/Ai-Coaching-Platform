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

    public void ApplyReview(
        AIRecommendationReviewStatus targetStatus,
        string? coachDecision = null,
        string? finalImplementedPlan = null)
    {
        if (ReviewStatus == AIRecommendationReviewStatus.Archived)
        {
            throw new InvalidOperationException("Archived recommendations cannot undergo state transitions.");
        }

        if (ReviewStatus == AIRecommendationReviewStatus.Accepted)
        {
            if (targetStatus != AIRecommendationReviewStatus.Accepted && targetStatus != AIRecommendationReviewStatus.Archived)
            {
                throw new InvalidOperationException($"Cannot transition from Accepted to {targetStatus}. Only Archived or updating Accepted details is allowed.");
            }
        }
        else if (ReviewStatus == AIRecommendationReviewStatus.Rejected)
        {
            if (targetStatus != AIRecommendationReviewStatus.Archived)
            {
                throw new InvalidOperationException($"Cannot transition from Rejected to {targetStatus}. Only Archived is allowed.");
            }
        }
        else if (ReviewStatus == AIRecommendationReviewStatus.PendingReview || ReviewStatus == AIRecommendationReviewStatus.UnderReview)
        {
            if (targetStatus != AIRecommendationReviewStatus.UnderReview &&
                targetStatus != AIRecommendationReviewStatus.Accepted &&
                targetStatus != AIRecommendationReviewStatus.Rejected &&
                targetStatus != AIRecommendationReviewStatus.Archived)
            {
                throw new InvalidOperationException($"Invalid target status transition from {ReviewStatus} to {targetStatus}.");
            }
        }

        if (targetStatus == AIRecommendationReviewStatus.Accepted)
        {
            var effectiveDecision = coachDecision ?? CoachDecisionNote;
            if (string.IsNullOrWhiteSpace(effectiveDecision))
            {
                throw new ArgumentException("Coach decision is required when accepting a recommendation.", nameof(coachDecision));
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(finalImplementedPlan))
            {
                throw new ArgumentException("Final implemented plan can only be supplied when the recommendation is accepted.", nameof(finalImplementedPlan));
            }
        }

        ReviewStatus = targetStatus;

        if (!string.IsNullOrWhiteSpace(coachDecision))
        {
            CoachDecisionNote = coachDecision.Trim();
        }

        if (targetStatus == AIRecommendationReviewStatus.Accepted)
        {
            CoachDecision = CoachDecisionOutcome.Accepted;
            CoachDecisionAt ??= DateTime.UtcNow;
            if (finalImplementedPlan != null)
            {
                FinalImplementedPlan = string.IsNullOrWhiteSpace(finalImplementedPlan) ? null : finalImplementedPlan.Trim();
            }
        }
        else if (targetStatus == AIRecommendationReviewStatus.Rejected)
        {
            CoachDecision = CoachDecisionOutcome.Rejected;
            CoachDecisionAt ??= DateTime.UtcNow;
        }

        MarkUpdated();
    }

    public void RecordDecision(
        CoachDecisionOutcome decision,
        string? decisionNote = null,
        string? finalImplementedPlan = null)
    {
        var targetStatus = decision switch
        {
            CoachDecisionOutcome.Accepted => AIRecommendationReviewStatus.Accepted,
            CoachDecisionOutcome.AcceptedWithModifications => AIRecommendationReviewStatus.Accepted,
            CoachDecisionOutcome.Rejected => AIRecommendationReviewStatus.Rejected,
            CoachDecisionOutcome.Deferred => AIRecommendationReviewStatus.UnderReview,
            _ => AIRecommendationReviewStatus.PendingReview
        };

        ApplyReview(targetStatus, string.IsNullOrWhiteSpace(decisionNote) ? decision.ToString() : decisionNote, finalImplementedPlan);
    }
}
