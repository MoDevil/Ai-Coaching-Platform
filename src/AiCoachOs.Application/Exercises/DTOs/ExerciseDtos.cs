using AiCoachOs.Domain.Exercises;

namespace AiCoachOs.Application.Exercises.DTOs;

public record ExerciseSummaryDto(
    Guid Id,
    string Name,
    string? Aliases,
    ExerciseCategory Category,
    Guid MovementPatternId,
    string MovementPatternName,
    QualitativeRating StabilityRequirement,
    QualitativeRating TechnicalDemand,
    QualitativeRating LocalFatigueCost,
    QualitativeRating SystemicFatigueCost,
    QualitativeRating StimulusPotential,
    QualitativeRating ProgressionPotential,
    ResistanceProfile ResistanceProfile,
    MetadataStatus MetadataStatus,
    IReadOnlyList<string> PrimaryMuscles,
    IReadOnlyList<string> SecondaryMuscles,
    IReadOnlyList<string> EquipmentNames,
    Guid? SubstitutionGroupId
);

public record ExerciseMuscleDto(
    Guid MuscleId,
    string Name,
    string? CommonName,
    string BodyPart,
    bool IsPrimary
);

public record ExerciseEquipmentDto(
    Guid EquipmentId,
    string Name,
    string? Category,
    bool IsRequired
);

public record ExerciseSubstitutionDto(
    Guid SubstituteExerciseId,
    string SubstituteExerciseName,
    ExerciseCategory Category,
    string MovementPatternName,
    ResistanceProfile ResistanceProfile,
    QualitativeRating StabilityRequirement,
    string? IntentPreservationNotes
);

public record ExerciseDetailDto(
    Guid Id,
    string Name,
    string? Aliases,
    ExerciseCategory Category,
    Guid MovementPatternId,
    string MovementPatternName,
    string? JointActions,
    QualitativeRating StabilityRequirement,
    QualitativeRating TechnicalDemand,
    QualitativeRating LocalFatigueCost,
    QualitativeRating SystemicFatigueCost,
    QualitativeRating StimulusPotential,
    QualitativeRating ProgressionPotential,
    ResistanceProfile ResistanceProfile,
    MetadataStatus MetadataStatus,
    Guid? SubstitutionGroupId,
    IReadOnlyList<ExerciseMuscleDto> Muscles,
    IReadOnlyList<ExerciseEquipmentDto> Equipment,
    IReadOnlyList<ExerciseSubstitutionDto> Substitutions,
    DateTime CreatedAtUtc
);

public record MuscleDto(
    Guid Id,
    string Name,
    string? CommonName,
    string BodyPart
);

public record MovementPatternDto(
    Guid Id,
    string Name,
    string? Description
);

public record EquipmentDto(
    Guid Id,
    string Name,
    string? Category
);

public record ExerciseFilterDto(
    string? Search = null,
    Guid? MovementPatternId = null,
    Guid? MuscleId = null,
    Guid? EquipmentId = null,
    ExerciseCategory? Category = null
);
