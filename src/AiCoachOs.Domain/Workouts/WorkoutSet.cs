using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Workouts;

public class WorkoutSet : Entity<Guid>
{
    public Guid WorkoutExerciseId { get; private set; }
    public WorkoutExercise WorkoutExercise { get; private set; } = null!;

    public int SetNumber { get; private set; }
    public int Repetitions { get; private set; }
    public decimal LoadKg { get; private set; }
    public decimal? Rir { get; private set; }
    public bool IsCompleted { get; private set; }
    public string? Notes { get; private set; }

    private WorkoutSet() { } // EF Core

    public WorkoutSet(
        Guid id,
        Guid workoutExerciseId,
        int setNumber,
        int repetitions,
        decimal loadKg,
        decimal? rir = null,
        bool isCompleted = true,
        string? notes = null) : base(id)
    {
        if (workoutExerciseId == Guid.Empty)
            throw new ArgumentException("WorkoutExerciseId cannot be empty.", nameof(workoutExerciseId));
        if (setNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(setNumber), "Set number must be at least 1.");
        if (repetitions < 0)
            throw new ArgumentOutOfRangeException(nameof(repetitions), "Repetitions cannot be negative.");
        if (loadKg < 0)
            throw new ArgumentOutOfRangeException(nameof(loadKg), "Load cannot be negative.");
        if (rir.HasValue && (rir.Value < 0 || rir.Value > 10))
            throw new ArgumentOutOfRangeException(nameof(rir), "RIR must be between 0 and 10.");

        WorkoutExerciseId = workoutExerciseId;
        SetNumber = setNumber;
        Repetitions = repetitions;
        LoadKg = loadKg;
        Rir = rir;
        IsCompleted = isCompleted;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void UpdatePerformance(int repetitions, decimal loadKg, decimal? rir, bool isCompleted, string? notes)
    {
        if (repetitions < 0)
            throw new ArgumentOutOfRangeException(nameof(repetitions), "Repetitions cannot be negative.");
        if (loadKg < 0)
            throw new ArgumentOutOfRangeException(nameof(loadKg), "Load cannot be negative.");
        if (rir.HasValue && (rir.Value < 0 || rir.Value > 10))
            throw new ArgumentOutOfRangeException(nameof(rir), "RIR must be between 0 and 10.");

        Repetitions = repetitions;
        LoadKg = loadKg;
        Rir = rir;
        IsCompleted = isCompleted;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        MarkUpdated();
    }
}
