using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.AnatomyAndBiomechanics;

/// <summary>
/// Specific physiological articulation available at a joint
/// (e.g., Glenohumeral Flexion, Knee Extension, Hip Abduction).
/// Strictly distinct from training movement patterns (e.g. Squat, Horizontal Push).
/// </summary>
public class JointAction : Entity<Guid>
{
    private readonly List<MuscleJointAction> _muscles = new();
    private readonly List<ExerciseJointAction> _exercises = new();

    public Guid JointId { get; private set; }
    public Joint Joint { get; private set; } = null!;

    public JointActionType ActionType { get; private set; }
    public PlaneOfMotion PlaneOfMotion { get; private set; }
    public string? Description { get; private set; }

    public IReadOnlyCollection<MuscleJointAction> Muscles => _muscles;
    public IReadOnlyCollection<ExerciseJointAction> Exercises => _exercises;

    private JointAction() { } // EF Core

    public JointAction(
        Guid id,
        Guid jointId,
        JointActionType actionType,
        PlaneOfMotion planeOfMotion,
        string? description = null) : base(id)
    {
        if (jointId == Guid.Empty)
            throw new ArgumentException("JointId cannot be empty.", nameof(jointId));

        JointId = jointId;
        ActionType = actionType;
        PlaneOfMotion = planeOfMotion;
        Description = description?.Trim();
    }
}
