using AiCoachOs.Application.Workouts.DTOs;

namespace AiCoachOs.Application.Workouts.Services;

public interface IWorkoutService
{
    Task<WorkoutSessionDto> StartWorkoutAsync(Guid coachId, StartWorkoutRequestDto request, CancellationToken cancellationToken = default);
    Task<WorkoutSessionDto?> GetWorkoutByIdAsync(Guid coachId, Guid workoutId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkoutSummaryDto>> GetWorkoutsByClientIdAsync(Guid coachId, Guid clientId, CancellationToken cancellationToken = default);
    Task<WorkoutSessionDto> AddExerciseAsync(Guid coachId, Guid workoutId, AddWorkoutExerciseRequestDto request, CancellationToken cancellationToken = default);
    Task<WorkoutSessionDto> RecordSetAsync(Guid coachId, Guid workoutId, Guid workoutExerciseId, RecordWorkoutSetRequestDto request, CancellationToken cancellationToken = default);
    Task<WorkoutSessionDto> UpdateSetAsync(Guid coachId, Guid workoutId, Guid setId, UpdateWorkoutSetRequestDto request, CancellationToken cancellationToken = default);
    Task<WorkoutSessionDto> CompleteWorkoutAsync(Guid coachId, Guid workoutId, CompleteWorkoutRequestDto request, CancellationToken cancellationToken = default);
    Task<WorkoutSessionDto> AbandonWorkoutAsync(Guid coachId, Guid workoutId, CancellationToken cancellationToken = default);
}
