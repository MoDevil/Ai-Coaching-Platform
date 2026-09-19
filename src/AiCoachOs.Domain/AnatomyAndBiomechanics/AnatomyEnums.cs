namespace AiCoachOs.Domain.AnatomyAndBiomechanics;

/// <summary>
/// Specific anatomical joint actions. Distinct from training MovementPatterns (e.g. Horizontal Push != Shoulder Horizontal Adduction).
/// </summary>
public enum JointActionType
{
    Flexion = 1,
    Extension = 2,
    Abduction = 3,
    Adduction = 4,
    HorizontalAbduction = 5,
    HorizontalAdduction = 6,
    InternalRotation = 7,
    ExternalRotation = 8,
    Elevation = 9,
    Depression = 10,
    Retraction = 11,
    Protraction = 12,
    Plantarflexion = 13,
    Dorsiflexion = 14,
    Pronation = 15,
    Supination = 16,
    LateralFlexion = 17,
    Rotation = 18
}

public enum PlaneOfMotion
{
    Sagittal = 1,
    Frontal = 2,
    Transverse = 3,
    Multiplanar = 4
}

public enum JointActionRole
{
    PrimaryMover = 1,
    SecondaryMover = 2,
    Stabilizer = 3
}

public enum BiomechanicalAspect
{
    MomentArm = 1,
    ResistanceDirection = 2,
    MuscleLength = 3,
    RangeOfMotion = 4,
    Stability = 5,
    SetupVariable = 6,
    MachineGeometry = 7
}
