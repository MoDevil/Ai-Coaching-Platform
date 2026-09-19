using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Workouts.DTOs;
using AiCoachOs.Application.Workouts.Engine;
using AiCoachOs.Application.Workouts.Services;
using AiCoachOs.Domain.Workouts;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class WorkoutService : IWorkoutService
{
    private readonly IApplicationDbContext _context;
    private readonly IProgressionEvaluator _progressionEvaluator;

    public WorkoutService(IApplicationDbContext context, IProgressionEvaluator progressionEvaluator)
    {
        _context = context;
        _progressionEvaluator = progressionEvaluator;
    }

    public async Task<WorkoutSessionDto> StartWorkoutAsync(Guid coachId, StartWorkoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(request.ClientId, cancellationToken);
        if (client == null)
            throw new NotFoundException($"Client with ID '{request.ClientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this client.");

        Guid? resolvedProgramVersionId = null;
        if (request.TrainingSessionId.HasValue)
        {
            var trainingSession = await _context.TrainingSessions
                .Include(ts => ts.TrainingWeek)
                .FirstOrDefaultAsync(ts => ts.Id == request.TrainingSessionId.Value, cancellationToken);

            if (trainingSession != null)
            {
                resolvedProgramVersionId = trainingSession.TrainingWeek.ProgramVersionId;
            }
        }

        var workout = new WorkoutSession(
            id: Guid.NewGuid(),
            clientId: request.ClientId,
            coachId: coachId,
            startedAtUtc: DateTime.UtcNow,
            programVersionId: resolvedProgramVersionId,
            trainingSessionId: request.TrainingSessionId,
            notes: request.Notes);

        // If trainingSessionId is provided, populate planned exercise slots into WorkoutExercises
        if (request.TrainingSessionId.HasValue)
        {
            var trainingSession = await _context.TrainingSessions
                .Include(ts => ts.Slots)
                    .ThenInclude(s => s.Exercise)
                .FirstOrDefaultAsync(ts => ts.Id == request.TrainingSessionId.Value, cancellationToken);

            if (trainingSession != null)
            {
                foreach (var slot in trainingSession.Slots.OrderBy(s => s.Order))
                {
                    var workoutExercise = new WorkoutExercise(
                        id: Guid.NewGuid(),
                        workoutSessionId: workout.Id,
                        exerciseId: slot.ExerciseId,
                        orderInSession: slot.Order,
                        exerciseSlotId: slot.Id);

                    workout.AddExercise(workoutExercise);
                }
            }
        }

        await _context.AddWorkoutSessionAsync(workout, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with full graph to map cleanly
        var loaded = await _context.FindWorkoutSessionByIdAsync(workout.Id, cancellationToken);
        return MapToDto(loaded ?? workout);
    }

    public async Task<WorkoutSessionDto?> GetWorkoutByIdAsync(Guid coachId, Guid workoutId, CancellationToken cancellationToken = default)
    {
        var workout = await _context.FindWorkoutSessionByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            return null;

        if (workout.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this workout.");

        return MapToDto(workout);
    }

    public async Task<IReadOnlyList<WorkoutSummaryDto>> GetWorkoutsByClientIdAsync(Guid coachId, Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null)
            throw new NotFoundException($"Client with ID '{clientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this client.");

        var workouts = await _context.WorkoutSessions
            .Where(w => w.ClientId == clientId && w.CoachId == coachId)
            .Include(w => w.Client)
            .Include(w => w.TrainingSession)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Sets)
            .OrderByDescending(w => w.StartedAtUtc)
            .ToListAsync(cancellationToken);

        return workouts.Select(w => new WorkoutSummaryDto(
            Id: w.Id,
            ClientId: w.ClientId,
            ClientName: $"{w.Client.FirstName} {w.Client.LastName}",
            TrainingSessionId: w.TrainingSessionId,
            PlannedSessionName: w.TrainingSession?.Name,
            StartedAtUtc: w.StartedAtUtc,
            CompletedAtUtc: w.CompletedAtUtc,
            Status: w.Status,
            TotalExercises: w.Exercises.Count,
            TotalSetsCompleted: w.Exercises.SelectMany(e => e.Sets).Count(s => s.IsCompleted),
            Notes: w.Notes
        )).ToList().AsReadOnly();
    }

    public async Task<WorkoutSessionDto> AddExerciseAsync(Guid coachId, Guid workoutId, AddWorkoutExerciseRequestDto request, CancellationToken cancellationToken = default)
    {
        var workout = await _context.FindWorkoutSessionByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new NotFoundException($"Workout session with ID '{workoutId}' was not found.");

        if (workout.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this workout.");

        var exercise = await _context.FindExerciseByIdAsync(request.ExerciseId, cancellationToken);
        if (exercise == null)
            throw new NotFoundException($"Exercise with ID '{request.ExerciseId}' was not found.");

        var order = workout.Exercises.Count + 1;

        var workoutExercise = new WorkoutExercise(
            id: Guid.NewGuid(),
            workoutSessionId: workout.Id,
            exerciseId: request.ExerciseId,
            orderInSession: order,
            exerciseSlotId: request.ExerciseSlotId,
            notes: request.Notes);

        workout.AddExercise(workoutExercise);
        await _context.AddWorkoutExerciseAsync(workoutExercise, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var reloaded = await _context.FindWorkoutSessionByIdAsync(workoutId, cancellationToken);
        return MapToDto(reloaded ?? workout);
    }

    public async Task<WorkoutSessionDto> RecordSetAsync(Guid coachId, Guid workoutId, Guid workoutExerciseId, RecordWorkoutSetRequestDto request, CancellationToken cancellationToken = default)
    {
        var workout = await _context.FindWorkoutSessionByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new NotFoundException($"Workout session with ID '{workoutId}' was not found.");

        if (workout.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this workout.");

        var workoutExercise = workout.Exercises.FirstOrDefault(e => e.Id == workoutExerciseId);
        if (workoutExercise == null)
            throw new NotFoundException($"Workout exercise with ID '{workoutExerciseId}' was not found in this session.");

        var set = new WorkoutSet(
            id: Guid.NewGuid(),
            workoutExerciseId: workoutExercise.Id,
            setNumber: request.SetNumber,
            repetitions: request.Repetitions,
            loadKg: request.LoadKg,
            rir: request.Rir,
            isCompleted: request.IsCompleted,
            notes: request.Notes);

        workoutExercise.AddSet(set);
        await _context.AddWorkoutSetAsync(set, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var reloaded = await _context.FindWorkoutSessionByIdAsync(workoutId, cancellationToken);
        return MapToDto(reloaded ?? workout);
    }

    public async Task<WorkoutSessionDto> UpdateSetAsync(Guid coachId, Guid workoutId, Guid setId, UpdateWorkoutSetRequestDto request, CancellationToken cancellationToken = default)
    {
        var workout = await _context.FindWorkoutSessionByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new NotFoundException($"Workout session with ID '{workoutId}' was not found.");

        if (workout.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this workout.");

        var targetSet = workout.Exercises.SelectMany(e => e.Sets).FirstOrDefault(s => s.Id == setId);
        if (targetSet == null)
            throw new NotFoundException($"Workout set with ID '{setId}' was not found in this session.");

        targetSet.UpdatePerformance(
            repetitions: request.Repetitions,
            loadKg: request.LoadKg,
            rir: request.Rir,
            isCompleted: request.IsCompleted,
            notes: request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        var reloaded = await _context.FindWorkoutSessionByIdAsync(workoutId, cancellationToken);
        return MapToDto(reloaded ?? workout);
    }

    public async Task<WorkoutSessionDto> CompleteWorkoutAsync(Guid coachId, Guid workoutId, CompleteWorkoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var workout = await _context.FindWorkoutSessionByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new NotFoundException($"Workout session with ID '{workoutId}' was not found.");

        if (workout.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this workout.");

        workout.Complete(DateTime.UtcNow, request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        var reloaded = await _context.FindWorkoutSessionByIdAsync(workoutId, cancellationToken);
        return MapToDto(reloaded ?? workout);
    }

    public async Task<WorkoutSessionDto> AbandonWorkoutAsync(Guid coachId, Guid workoutId, CancellationToken cancellationToken = default)
    {
        var workout = await _context.FindWorkoutSessionByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new NotFoundException($"Workout session with ID '{workoutId}' was not found.");

        if (workout.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this workout.");

        workout.Abandon();
        await _context.SaveChangesAsync(cancellationToken);

        var reloaded = await _context.FindWorkoutSessionByIdAsync(workoutId, cancellationToken);
        return MapToDto(reloaded ?? workout);
    }

    private WorkoutSessionDto MapToDto(WorkoutSession session)
    {
        var clientName = session.Client != null
            ? $"{session.Client.FirstName} {session.Client.LastName}"
            : string.Empty;

        var exercisesDto = new List<WorkoutExerciseDto>();

        foreach (var we in session.Exercises)
        {
            var setsDto = we.Sets.Select(s => new WorkoutSetDto(
                Id: s.Id,
                WorkoutExerciseId: s.WorkoutExerciseId,
                SetNumber: s.SetNumber,
                Repetitions: s.Repetitions,
                LoadKg: s.LoadKg,
                Rir: s.Rir,
                IsCompleted: s.IsCompleted,
                Notes: s.Notes
            )).ToList();

            var exerciseName = we.Exercise?.Name ?? string.Empty;

            ProgressionEvaluationResultDto? progResult = null;
            if (we.ExerciseSlot != null)
            {
                progResult = _progressionEvaluator.Evaluate(we.ExerciseSlot, we.Sets);
            }

            exercisesDto.Add(new WorkoutExerciseDto(
                Id: we.Id,
                WorkoutSessionId: we.WorkoutSessionId,
                ExerciseId: we.ExerciseId,
                ExerciseName: exerciseName,
                ExerciseSlotId: we.ExerciseSlotId,
                OrderInSession: we.OrderInSession,
                PlannedTargetRepRange: we.ExerciseSlot?.TargetRepRange,
                PlannedEffortGuideline: we.ExerciseSlot?.EffortGuideline,
                PlannedTargetSets: we.ExerciseSlot?.TargetSets,
                PlannedProgressionRule: we.ExerciseSlot?.ProgressionRule?.Type.ToString(),
                Notes: we.Notes,
                Sets: setsDto,
                ProgressionResult: progResult
            ));
        }

        return new WorkoutSessionDto(
            Id: session.Id,
            ClientId: session.ClientId,
            ClientName: clientName,
            CoachId: session.CoachId,
            ProgramVersionId: session.ProgramVersionId,
            TrainingSessionId: session.TrainingSessionId,
            PlannedSessionName: session.TrainingSession?.Name,
            StartedAtUtc: session.StartedAtUtc,
            CompletedAtUtc: session.CompletedAtUtc,
            Status: session.Status,
            Notes: session.Notes,
            Exercises: exercisesDto
        );
    }
}
