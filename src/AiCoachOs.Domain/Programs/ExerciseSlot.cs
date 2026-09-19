using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Exercises;

namespace AiCoachOs.Domain.Programs;

public class ExerciseSlot : Entity<Guid>
{
    public Guid TrainingSessionId { get; private set; }
    public TrainingSession TrainingSession { get; private set; } = null!;

    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;

    public int Order { get; private set; }
    public int TargetSets { get; private set; }
    public string TargetRepRange { get; private set; } = string.Empty;
    public string EffortGuideline { get; private set; } = string.Empty; // e.g. "2-3 RIR", "1-2 RIR"
    public int RestSeconds { get; private set; }
    public string SelectionRationale { get; private set; } = string.Empty;
    public ProgressionRule? ProgressionRule { get; private set; }
    public string? CoachingNote { get; private set; }

    private ExerciseSlot() { } // EF Core

    public ExerciseSlot(
        Guid id,
        Guid trainingSessionId,
        Guid exerciseId,
        int order,
        int targetSets,
        string targetRepRange,
        string effortGuideline,
        int restSeconds,
        string selectionRationale,
        ProgressionRule? progressionRule = null,
        string? coachingNote = null) : base(id)
    {
        if (trainingSessionId == Guid.Empty)
            throw new ArgumentException("TrainingSessionId cannot be empty.", nameof(trainingSessionId));
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be at least 1.");
        if (targetSets < 1)
            throw new ArgumentOutOfRangeException(nameof(targetSets), "Target sets must be at least 1.");
        if (string.IsNullOrWhiteSpace(targetRepRange))
            throw new ArgumentException("Target rep range cannot be empty.", nameof(targetRepRange));
        if (string.IsNullOrWhiteSpace(effortGuideline))
            throw new ArgumentException("Effort guideline cannot be empty.", nameof(effortGuideline));
        if (restSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(restSeconds), "Rest seconds cannot be negative.");
        if (string.IsNullOrWhiteSpace(selectionRationale))
            throw new ArgumentException("Selection rationale cannot be empty.", nameof(selectionRationale));

        TrainingSessionId = trainingSessionId;
        ExerciseId = exerciseId;
        Order = order;
        TargetSets = targetSets;
        TargetRepRange = targetRepRange.Trim();
        EffortGuideline = effortGuideline.Trim();
        RestSeconds = restSeconds;
        SelectionRationale = selectionRationale.Trim();
        ProgressionRule = progressionRule;
        CoachingNote = string.IsNullOrWhiteSpace(coachingNote) ? null : coachingNote.Trim();
    }
}
