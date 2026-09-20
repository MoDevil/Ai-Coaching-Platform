using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Adaptations;

public class AdaptationRecommendation : Entity<Guid>
{
    public Guid AdaptationAssessmentId { get; private set; }
    public AdaptationAssessment AdaptationAssessment { get; private set; } = null!;

    public Guid? ExerciseAdaptationRecordId { get; private set; }
    public ExerciseAdaptationRecord? ExerciseAdaptationRecord { get; private set; }

    public AdaptationActionType ActionType { get; private set; }
    public Guid? TargetSlotId { get; private set; }
    public string? SuggestedChangeDetail { get; private set; } // e.g., "SetCount: 4", "EffortGuideline: 2-3 RIR", "SubstituteExerciseId: <guid>"
    public string Rationale { get; private set; } = string.Empty;
    public RecommendationConfidence Confidence { get; private set; }
    public RecommendationStatus Status { get; private set; } = RecommendationStatus.Pending;

    public DateTime? CoachDecisionAt { get; private set; }
    public string? CoachDecisionNote { get; private set; }

    private AdaptationRecommendation() { } // EF Core

    public AdaptationRecommendation(
        Guid id,
        Guid adaptationAssessmentId,
        AdaptationActionType actionType,
        string rationale,
        RecommendationConfidence confidence,
        Guid? exerciseAdaptationRecordId = null,
        Guid? targetSlotId = null,
        string? suggestedChangeDetail = null) : base(id)
    {
        if (adaptationAssessmentId == Guid.Empty)
            throw new ArgumentException("AdaptationAssessmentId cannot be empty.", nameof(adaptationAssessmentId));
        if (string.IsNullOrWhiteSpace(rationale))
            throw new ArgumentException("Rationale cannot be empty.", nameof(rationale));

        AdaptationAssessmentId = adaptationAssessmentId;
        ActionType = actionType;
        Rationale = rationale.Trim();
        Confidence = confidence;
        Status = RecommendationStatus.Pending;
        ExerciseAdaptationRecordId = exerciseAdaptationRecordId;
        TargetSlotId = targetSlotId;
        SuggestedChangeDetail = string.IsNullOrWhiteSpace(suggestedChangeDetail) ? null : suggestedChangeDetail.Trim();
    }

    public void Approve(string? coachNote = null)
    {
        if (Status != RecommendationStatus.Pending)
            throw new InvalidOperationException($"Cannot approve recommendation that is already {Status}.");

        Status = RecommendationStatus.ApprovedByCoach;
        CoachDecisionAt = DateTime.UtcNow;
        CoachDecisionNote = string.IsNullOrWhiteSpace(coachNote) ? null : coachNote.Trim();
        MarkUpdated();
    }

    public void Reject(string coachNote)
    {
        if (Status != RecommendationStatus.Pending)
            throw new InvalidOperationException($"Cannot reject recommendation that is already {Status}.");
        if (string.IsNullOrWhiteSpace(coachNote))
            throw new ArgumentException("Coach rejection note is required.", nameof(coachNote));

        Status = RecommendationStatus.RejectedByCoach;
        CoachDecisionAt = DateTime.UtcNow;
        CoachDecisionNote = coachNote.Trim();
        MarkUpdated();
    }

    public void MarkApplied()
    {
        if (Status != RecommendationStatus.ApprovedByCoach)
            throw new InvalidOperationException($"Only coach-approved recommendations can be marked as applied. Current status is {Status}.");

        Status = RecommendationStatus.Applied;
        MarkUpdated();
    }
}
