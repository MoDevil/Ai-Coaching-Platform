using AiCoachOs.Domain.Programs;

namespace AiCoachOs.Application.Programs.DTOs;

public record GenerateProgramRequestDto(
    Guid ClientId,
    string? ProgramName = null,
    string? CoachNotes = null,
    int NumberOfWeeks = 4);

public record UpdateProgramStatusDto(
    ProgramStatus Status);

public record ProgramSummaryDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    Guid CoachId,
    string Name,
    ProgramStatus Status,
    PrimaryGoalType PrimaryGoal,
    PrimaryGoalType? SecondaryGoal,
    int ActiveVersionNumber,
    int TotalSessionsPerWeek,
    string RationaleSummary,
    DateTime CreatedAtUtc);

public record ProgramDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    Guid CoachId,
    string Name,
    ProgramStatus Status,
    GoalSnapshotDto GoalSnapshot,
    string RationaleSummary,
    ProgramVersionDto? ActiveVersion,
    IReadOnlyList<ProgramVersionDto> Versions,
    VolumeSummaryDto? VolumeSummary,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public record GoalSnapshotDto(
    PrimaryGoalType PrimaryGoal,
    PrimaryGoalType? SecondaryGoal,
    string? GoalEmphasis,
    int? TargetTimelineWeeks);

public record ProgramVersionDto(
    Guid Id,
    Guid ProgramId,
    int VersionNumber,
    RecoveryCapacity RecoveryCapacity,
    string? ChangeReason,
    bool IsActive,
    IReadOnlyList<ProgramMusclePriorityDto> MusclePriorities,
    IReadOnlyList<TrainingWeekDto> Weeks,
    DateTime CreatedAtUtc);

public record ProgramMusclePriorityDto(
    Guid Id,
    Guid MuscleId,
    string MuscleName,
    MusclePriorityLevel PriorityLevel,
    string? Justification);

public record TrainingWeekDto(
    Guid Id,
    Guid ProgramVersionId,
    int WeekNumber,
    IReadOnlyList<TrainingSessionDto> Sessions);

public record TrainingSessionDto(
    Guid Id,
    Guid TrainingWeekId,
    int DayNumber,
    DayOfWeek? DayOfWeek,
    string Name,
    string SessionIntent,
    int EstimatedDurationMinutes,
    IReadOnlyList<ExerciseSlotDto> Slots);

public record ExerciseSlotDto(
    Guid Id,
    Guid TrainingSessionId,
    Guid ExerciseId,
    string ExerciseName,
    string MovementPatternName,
    int Order,
    int TargetSets,
    string TargetRepRange,
    string EffortGuideline,
    int RestSeconds,
    string SelectionRationale,
    ProgressionRuleDto? ProgressionRule,
    string? CoachingNote);

public record ProgressionRuleDto(
    ProgressionRuleType Type,
    string CurrentTarget,
    string IncrementValue,
    string? IncrementCondition);

public record MuscleVolumeOutputDto(
    Guid MuscleId,
    string MuscleName,
    MusclePriorityLevel PriorityLevel,
    int DirectWeeklySets,
    int IndirectWeeklySets,
    int TotalWeeklySets,
    int FrequencyPerWeek);

public record VolumeSummaryDto(
    int TotalWeeklySessions,
    int TotalWeeklySets,
    int AverageSessionDurationMinutes,
    IReadOnlyList<MuscleVolumeOutputDto> MuscleVolumes);
