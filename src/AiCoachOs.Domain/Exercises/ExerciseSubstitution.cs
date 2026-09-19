using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Exercises;

public class ExerciseSubstitution : Entity<Guid>
{
    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;

    public Guid SubstituteExerciseId { get; private set; }
    public Exercise SubstituteExercise { get; private set; } = null!;

    public string? IntentPreservationNotes { get; private set; }

    private ExerciseSubstitution() { } // EF Core

    public ExerciseSubstitution(
        Guid id,
        Guid exerciseId,
        Guid substituteExerciseId,
        string? intentPreservationNotes = null) : base(id)
    {
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (substituteExerciseId == Guid.Empty)
            throw new ArgumentException("SubstituteExerciseId cannot be empty.", nameof(substituteExerciseId));
        if (exerciseId == substituteExerciseId)
            throw new ArgumentException("An exercise cannot be a substitute for itself.");

        ExerciseId = exerciseId;
        SubstituteExerciseId = substituteExerciseId;
        IntentPreservationNotes = intentPreservationNotes?.Trim();
    }
}
