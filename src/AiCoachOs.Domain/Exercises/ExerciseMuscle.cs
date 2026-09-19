namespace AiCoachOs.Domain.Exercises;

public class ExerciseMuscle
{
    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;

    public Guid MuscleId { get; private set; }
    public Muscle Muscle { get; private set; } = null!;

    public bool IsPrimary { get; private set; }

    private ExerciseMuscle() { } // EF Core

    public ExerciseMuscle(Guid exerciseId, Guid muscleId, bool isPrimary)
    {
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (muscleId == Guid.Empty)
            throw new ArgumentException("MuscleId cannot be empty.", nameof(muscleId));

        ExerciseId = exerciseId;
        MuscleId = muscleId;
        IsPrimary = isPrimary;
    }
}
