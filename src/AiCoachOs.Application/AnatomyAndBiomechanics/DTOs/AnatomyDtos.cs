using AiCoachOs.Domain.AnatomyAndBiomechanics;

namespace AiCoachOs.Application.AnatomyAndBiomechanics.DTOs;

public record AnatomicalRegionSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    int JointCount
);

public record AnatomicalRegionDto(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyList<JointSummaryDto> Joints
);

public record JointSummaryDto(
    Guid Id,
    Guid RegionId,
    string RegionName,
    string Name,
    string? CommonName,
    string? Description
);

public record JointDto(
    Guid Id,
    Guid RegionId,
    string RegionName,
    string Name,
    string? CommonName,
    string? Description,
    IReadOnlyList<JointActionSummaryDto> Actions
);

public record JointActionSummaryDto(
    Guid Id,
    Guid JointId,
    string JointName,
    JointActionType ActionType,
    string ActionTypeName,
    PlaneOfMotion PlaneOfMotion,
    string? Description
);

public record JointActionDto(
    Guid Id,
    Guid JointId,
    string JointName,
    JointActionType ActionType,
    string ActionTypeName,
    PlaneOfMotion PlaneOfMotion,
    string? Description,
    IReadOnlyList<MuscleSummaryDto> PrimaryMuscles
);

public record MuscleSummaryDto(
    Guid MuscleId,
    string Name,
    string? CommonName,
    string BodyPart,
    bool IsPrimaryAction
);

public record MuscleAnatomyDto(
    Guid MuscleId,
    string Name,
    string? CommonName,
    string BodyPart,
    IReadOnlyList<JointActionSummaryDto> PrimaryActions,
    IReadOnlyList<JointActionSummaryDto> SecondaryActions
);

public record ExerciseJointActionDto(
    Guid JointActionId,
    Guid JointId,
    string JointName,
    JointActionType ActionType,
    string ActionTypeName,
    PlaneOfMotion PlaneOfMotion,
    JointActionRole Role
);

public record BiomechanicalConsiderationDto(
    Guid Id,
    Guid ExerciseId,
    BiomechanicalAspect Aspect,
    CertaintyLevel Certainty,
    string Summary,
    string Explanation,
    string? PracticalCues,
    Guid? KnowledgeClaimId,
    string? KnowledgeClaimTopic,
    string? KnowledgeClaimText,
    DateTime CreatedAtUtc
);

public record ExerciseBiomechanicsDto(
    Guid ExerciseId,
    string ExerciseName,
    string MovementPatternName,
    string ResistanceProfileName,
    IReadOnlyList<ExerciseJointActionDto> JointActions,
    IReadOnlyList<BiomechanicalConsiderationDto> Considerations
);

public record CreateBiomechanicalConsiderationDto(
    BiomechanicalAspect Aspect,
    CertaintyLevel Certainty,
    string Summary,
    string Explanation,
    string? PracticalCues = null,
    Guid? KnowledgeClaimId = null
);

public record CreateJointActionDto(
    Guid JointId,
    JointActionType ActionType,
    PlaneOfMotion PlaneOfMotion,
    string? Description = null
);
