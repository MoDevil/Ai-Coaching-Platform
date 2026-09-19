using AiCoachOs.Domain.Exercises;

namespace AiCoachOs.Domain.AnatomyAndBiomechanics;

/// <summary>
/// Explicit association linking an anatomical muscle to a joint action it performs.
/// </summary>
public class MuscleJointAction
{
    public Guid MuscleId { get; private set; }
    public Muscle Muscle { get; private set; } = null!;

    public Guid JointActionId { get; private set; }
    public JointAction JointAction { get; private set; } = null!;

    public bool IsPrimaryAction { get; private set; }

    private MuscleJointAction() { } // EF Core

    public MuscleJointAction(Guid muscleId, Guid jointActionId, bool isPrimaryAction = true)
    {
        if (muscleId == Guid.Empty)
            throw new ArgumentException("MuscleId cannot be empty.", nameof(muscleId));
        if (jointActionId == Guid.Empty)
            throw new ArgumentException("JointActionId cannot be empty.", nameof(jointActionId));

        MuscleId = muscleId;
        JointActionId = jointActionId;
        IsPrimaryAction = isPrimaryAction;
    }
}
