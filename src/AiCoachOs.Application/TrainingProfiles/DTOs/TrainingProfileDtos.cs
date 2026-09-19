using AiCoachOs.Domain.TrainingProfiles;

namespace AiCoachOs.Application.TrainingProfiles.DTOs;

public record TrainingAvailabilityDto(
    int SessionsPerWeek,
    IReadOnlyList<DayOfWeek> AvailableDays,
    IReadOnlyList<DayOfWeek> PreferredDays
);

public record ClientTrainingPriorityDto(
    Guid? Id,
    int Order,
    string FocusArea,
    string? Notes
);

public record TrainingProfileDto(
    Guid Id,
    Guid ClientId,
    TrainingExperienceLevel ExperienceLevel,
    int? SessionDurationMinMinutes,
    int? SessionDurationTargetMinutes,
    int? SessionDurationMaxMinutes,
    TrainingAvailabilityDto WeeklyAvailability,
    IReadOnlyList<Guid> AvailableEquipmentIds,
    string? ExercisePreferences,
    string? ExerciseConstraints,
    IReadOnlyList<ClientTrainingPriorityDto> Priorities,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record UpdateTrainingProfileRequestDto(
    TrainingExperienceLevel ExperienceLevel,
    int? SessionDurationMinMinutes,
    int? SessionDurationTargetMinutes,
    int? SessionDurationMaxMinutes,
    TrainingAvailabilityDto WeeklyAvailability,
    List<Guid>? AvailableEquipmentIds,
    string? ExercisePreferences,
    string? ExerciseConstraints,
    List<ClientTrainingPriorityDto>? Priorities
);
