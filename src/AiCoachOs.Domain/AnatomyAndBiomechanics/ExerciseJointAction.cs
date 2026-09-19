using AiCoachOs.Domain.Exercises;

namespace AiCoachOs.Domain.AnatomyAndBiomechanics;

/// <summary>
/// Explicit association linking an Exercise to an anatomical JointAction performed during execution.
/// </summary>
public class ExerciseJointAction
{
    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;

    public Guid JointActionId { get; private set; }
    public JointAction JointAction { get; private set; } = null!;

    public JointActionRole Role { get; private set; }

    private ExerciseJointAction() { } // EF Core

    public ExerciseJointAction(Guid exerciseId, Guid jointActionId, JointActionRole role = JointActionRole.PrimaryMover)
    {
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (jointActionId == Guid.Empty)
            throw new ArgumentException("JointActionId cannot be empty.", nameof(jointActionId));

        ExerciseId = exerciseId;
        JointActionId = jointActionId;
        Role = role;
    }
}
