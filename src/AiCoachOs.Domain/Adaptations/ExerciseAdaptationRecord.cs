using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Programs;

namespace AiCoachOs.Domain.Adaptations;

public class ExerciseAdaptationRecord : Entity<Guid>
{
    public Guid AdaptationAssessmentId { get; private set; }
    public AdaptationAssessment AdaptationAssessment { get; private set; } = null!;

    public Guid ExerciseSlotId { get; private set; }
    public ExerciseSlot ExerciseSlot { get; private set; } = null!;

    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;

    public int ExposureCount { get; private set; }
    public int ProgressionMetCount { get; private set; }
    public EffortAlignmentStatus EffortAlignmentStatus { get; private set; }
    public PerformanceTrend PerformanceTrend { get; private set; }
    public bool PlateauConfirmed { get; private set; }
    public decimal AdherenceToExercise { get; private set; }

    private ExerciseAdaptationRecord() { } // EF Core

    public ExerciseAdaptationRecord(
        Guid id,
        Guid adaptationAssessmentId,
        Guid exerciseSlotId,
        Guid exerciseId,
        int exposureCount,
        int progressionMetCount,
        EffortAlignmentStatus effortAlignmentStatus,
        PerformanceTrend performanceTrend,
        bool plateauConfirmed,
        decimal adherenceToExercise) : base(id)
    {
        if (adaptationAssessmentId == Guid.Empty)
            throw new ArgumentException("AdaptationAssessmentId cannot be empty.", nameof(adaptationAssessmentId));
        if (exerciseSlotId == Guid.Empty)
            throw new ArgumentException("ExerciseSlotId cannot be empty.", nameof(exerciseSlotId));
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (exposureCount < 0)
            throw new ArgumentOutOfRangeException(nameof(exposureCount), "Exposure count cannot be negative.");
        if (progressionMetCount < 0)
            throw new ArgumentOutOfRangeException(nameof(progressionMetCount), "Progression met count cannot be negative.");
        if (adherenceToExercise < 0 || adherenceToExercise > 100)
            throw new ArgumentOutOfRangeException(nameof(adherenceToExercise), "Adherence must be between 0 and 100 percent.");

        AdaptationAssessmentId = adaptationAssessmentId;
        ExerciseSlotId = exerciseSlotId;
        ExerciseId = exerciseId;
        ExposureCount = exposureCount;
        ProgressionMetCount = progressionMetCount;
        EffortAlignmentStatus = effortAlignmentStatus;
        PerformanceTrend = performanceTrend;
        PlateauConfirmed = plateauConfirmed;
        AdherenceToExercise = adherenceToExercise;
    }
}
