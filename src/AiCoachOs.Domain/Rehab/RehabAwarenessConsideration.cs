using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Rehab;

public class RehabAwarenessConsideration : Entity<Guid>
{
    public Guid TrainingLimitationId { get; private set; }
    public TrainingLimitation TrainingLimitation { get; private set; } = null!;

    public Guid? ExerciseId { get; private set; }
    public Exercise? Exercise { get; private set; }

    public ConsiderationType ConsiderationType { get; private set; }
    public string ConsiderationText { get; private set; } = string.Empty;

    public Guid? KnowledgeClaimId { get; private set; }
    public KnowledgeClaim? KnowledgeClaim { get; private set; }
    public string? EvidenceBasis { get; private set; }

    public string Disclaimer { get; private set; } = string.Empty;
    public ConsiderationStatus Status { get; private set; }
    public DateTime GeneratedAtUtc { get; private set; }

    public DateTime? CoachDecisionAtUtc { get; private set; }
    public string? CoachDecisionNote { get; private set; }

    public const string FixedDisclaimer = "AI Coach OS does not diagnose medical conditions. This assessment is based on reported limitations only. Clinical evaluation is required for any health concern.";

    private RehabAwarenessConsideration() { } // EF Core

    public RehabAwarenessConsideration(
        Guid id,
        Guid trainingLimitationId,
        ConsiderationType considerationType,
        string considerationText,
        DateTime generatedAtUtc,
        Guid? exerciseId = null,
        Guid? knowledgeClaimId = null,
        string? evidenceBasis = null,
        ConsiderationStatus status = ConsiderationStatus.Pending,
        string? disclaimer = null) : base(id)
    {
        if (trainingLimitationId == Guid.Empty)
            throw new ArgumentException("TrainingLimitationId cannot be empty.", nameof(trainingLimitationId));
        if (string.IsNullOrWhiteSpace(considerationText))
            throw new ArgumentException("ConsiderationText cannot be empty.", nameof(considerationText));

        TrainingLimitationId = trainingLimitationId;
        ConsiderationType = considerationType;
        ConsiderationText = considerationText.Trim();
        GeneratedAtUtc = generatedAtUtc;
        ExerciseId = exerciseId;
        KnowledgeClaimId = knowledgeClaimId;
        EvidenceBasis = string.IsNullOrWhiteSpace(evidenceBasis) ? null : evidenceBasis.Trim();
        Status = status;
        Disclaimer = string.IsNullOrWhiteSpace(disclaimer) ? FixedDisclaimer : disclaimer.Trim();
    }

    public void Approve(DateTime decisionAtUtc, string? note = null)
    {
        Status = ConsiderationStatus.ApprovedByCoach;
        CoachDecisionAtUtc = decisionAtUtc;
        CoachDecisionNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        MarkUpdated();
    }

    public void Reject(DateTime decisionAtUtc, string? note = null)
    {
        Status = ConsiderationStatus.RejectedByCoach;
        CoachDecisionAtUtc = decisionAtUtc;
        CoachDecisionNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        MarkUpdated();
    }

    public void Apply(DateTime appliedAtUtc, string? note = null)
    {
        Status = ConsiderationStatus.Applied;
        CoachDecisionAtUtc = appliedAtUtc;
        if (!string.IsNullOrWhiteSpace(note))
        {
            CoachDecisionNote = note.Trim();
        }
        MarkUpdated();
    }
}
