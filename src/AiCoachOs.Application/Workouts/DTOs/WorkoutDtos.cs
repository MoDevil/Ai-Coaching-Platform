using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.Workouts;

namespace AiCoachOs.Application.Workouts.DTOs;

public record StartWorkoutRequestDto(
    Guid ClientId,
    Guid? TrainingSessionId = null,
    string? Notes = null);

public record AddWorkoutExerciseRequestDto(
    Guid ExerciseId,
    Guid? ExerciseSlotId = null,
    string? Notes = null);

public record RecordWorkoutSetRequestDto(
    int SetNumber,
    int Repetitions,
    decimal LoadKg,
    decimal? Rir = null,
    bool IsCompleted = true,
    string? Notes = null);

public record UpdateWorkoutSetRequestDto(
    int Repetitions,
    decimal LoadKg,
    decimal? Rir = null,
    bool IsCompleted = true,
    string? Notes = null);

public record CompleteWorkoutRequestDto(
    string? Notes = null);

public record WorkoutSetDto(
    Guid Id,
    Guid WorkoutExerciseId,
    int SetNumber,
    int Repetitions,
    decimal LoadKg,
    decimal? Rir,
    bool IsCompleted,
    string? Notes);

public record ProgressionEvaluationResultDto(
    ProgressionEvaluationStatus Status,
    string Reason,
    string? SuggestedNextTarget);

public record WorkoutExerciseDto(
    Guid Id,
    Guid WorkoutSessionId,
    Guid ExerciseId,
    string ExerciseName,
    Guid? ExerciseSlotId,
    int OrderInSession,
    string? PlannedTargetRepRange,
    string? PlannedEffortGuideline,
    int? PlannedTargetSets,
    string? PlannedProgressionRule,
    string? Notes,
    IReadOnlyList<WorkoutSetDto> Sets,
    ProgressionEvaluationResultDto? ProgressionResult);

public record WorkoutSessionDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    Guid CoachId,
    Guid? ProgramVersionId,
    Guid? TrainingSessionId,
    string? PlannedSessionName,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    WorkoutStatus Status,
    string? Notes,
    IReadOnlyList<WorkoutExerciseDto> Exercises);

public record WorkoutSummaryDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    Guid? TrainingSessionId,
    string? PlannedSessionName,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    WorkoutStatus Status,
    int TotalExercises,
    int TotalSetsCompleted,
    string? Notes);
