using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Programs.DTOs;
using AiCoachOs.Application.Programs.Engine;
using AiCoachOs.Application.Programs.Services;
using AiCoachOs.Domain.Programs;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class ProgramService : IProgramService
{
    private readonly IApplicationDbContext _context;
    private readonly IProgramBuilder _programBuilder;

    public ProgramService(IApplicationDbContext context, IProgramBuilder programBuilder)
    {
        _context = context;
        _programBuilder = programBuilder;
    }

    public async Task<ProgramDto> GenerateProgramAsync(Guid coachId, GenerateProgramRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(request.ClientId, cancellationToken);
        if (client == null)
        {
            throw new NotFoundException($"Client with ID '{request.ClientId}' was not found.");
        }

        if (client.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach does not have access to this client.");
        }

        var profile = await _context.FindTrainingProfileByClientIdAsync(request.ClientId, cancellationToken);
        if (profile == null)
        {
            throw new InvalidOperationException("Client must have a completed training profile before generating a program.");
        }

        // Load exercises with relationships (muscles, equipment, substitutions)
        var exercises = await _context.Exercises
            .Include(e => e.MovementPattern)
            .Include(e => e.Muscles)
                .ThenInclude(em => em.Muscle)
            .Include(e => e.Equipment)
            .Include(e => e.Substitutions)
            .ToListAsync(cancellationToken);

        var patterns = await _context.MovementPatterns.ToListAsync(cancellationToken);
        var muscles = await _context.Muscles.ToListAsync(cancellationToken);

        AiCoachOs.Domain.Gyms.GymProfile? gym = null;
        if (profile.GymProfileId.HasValue)
        {
            gym = await _context.FindGymProfileByIdAsync(profile.GymProfileId.Value, cancellationToken);
        }

        var (program, version, volumeSummary) = _programBuilder.BuildProgram(
            client: client,
            profile: profile,
            allExercises: exercises,
            allPatterns: patterns,
            allMuscles: muscles,
            customProgramName: request.ProgramName,
            coachNotes: request.CoachNotes,
            numberOfWeeks: request.NumberOfWeeks,
            gym: gym);

        await _context.AddProgramAsync(program, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(program, client.FirstName + " " + client.LastName, exercises, muscles, volumeSummary);
    }

    public async Task<ProgramDto?> GetProgramByIdAsync(Guid coachId, Guid programId, CancellationToken cancellationToken = default)
    {
        var program = await _context.FindProgramByIdAsync(programId, cancellationToken);
        if (program == null)
        {
            return null;
        }

        if (program.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach does not have access to this program.");
        }

        var exercises = await _context.Exercises
            .Include(e => e.MovementPattern)
            .Include(e => e.Muscles)
            .ToListAsync(cancellationToken);

        var muscles = await _context.Muscles.ToListAsync(cancellationToken);

        var activeVersion = program.GetActiveVersion();
        VolumeSummaryDto? volumeSummary = null;
        if (activeVersion != null)
        {
            volumeSummary = _programBuilder.CalculateVolumeSummary(activeVersion, exercises, muscles);
        }

        var clientName = $"{program.Client.FirstName} {program.Client.LastName}".Trim();
        return MapToDto(program, clientName, exercises, muscles, volumeSummary);
    }

    public async Task<IReadOnlyList<ProgramSummaryDto>> GetProgramsByClientIdAsync(Guid coachId, Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null)
        {
            throw new NotFoundException($"Client with ID '{clientId}' was not found.");
        }

        if (client.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach does not have access to this client.");
        }

        var programs = await _context.Programs
            .Include(p => p.Versions)
                .ThenInclude(v => v.Weeks)
                    .ThenInclude(w => w.Sessions)
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var clientName = $"{client.FirstName} {client.LastName}".Trim();

        return programs.Select(p =>
        {
            var active = p.GetActiveVersion();
            var sessionsPerWeek = active?.Weeks.FirstOrDefault()?.Sessions.Count ?? 0;

            return new ProgramSummaryDto(
                Id: p.Id,
                ClientId: p.ClientId,
                ClientName: clientName,
                CoachId: p.CoachId,
                Name: p.Name,
                Status: p.Status,
                PrimaryGoal: p.GoalSnapshot.PrimaryGoal,
                SecondaryGoal: p.GoalSnapshot.SecondaryGoal,
                ActiveVersionNumber: active?.VersionNumber ?? 0,
                TotalSessionsPerWeek: sessionsPerWeek,
                RationaleSummary: p.RationaleSummary,
                CreatedAtUtc: p.CreatedAtUtc);
        }).ToList().AsReadOnly();
    }

    public async Task<ProgramDto?> GetActiveProgramForClientAsync(Guid coachId, Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null)
        {
            throw new NotFoundException($"Client with ID '{clientId}' was not found.");
        }

        if (client.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach does not have access to this client.");
        }

        var program = await _context.Programs
            .Where(p => p.ClientId == clientId && (p.Status == ProgramStatus.Active || p.Status == ProgramStatus.Draft))
            .OrderByDescending(p => p.Status == ProgramStatus.Active)
            .ThenByDescending(p => p.CreatedAtUtc)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (program == Guid.Empty)
        {
            return null;
        }

        return await GetProgramByIdAsync(coachId, program, cancellationToken);
    }

    public async Task<ProgramDto> UpdateProgramStatusAsync(Guid coachId, Guid programId, UpdateProgramStatusDto request, CancellationToken cancellationToken = default)
    {
        var program = await _context.FindProgramByIdAsync(programId, cancellationToken);
        if (program == null)
        {
            throw new NotFoundException($"Program with ID '{programId}' was not found.");
        }

        if (program.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach does not have access to this program.");
        }

        program.UpdateStatus(request.Status);
        await _context.SaveChangesAsync(cancellationToken);

        var exercises = await _context.Exercises.Include(e => e.MovementPattern).Include(e => e.Muscles).ToListAsync(cancellationToken);
        var muscles = await _context.Muscles.ToListAsync(cancellationToken);
        var activeVersion = program.GetActiveVersion();
        VolumeSummaryDto? volumeSummary = activeVersion != null
            ? _programBuilder.CalculateVolumeSummary(activeVersion, exercises, muscles)
            : null;

        return MapToDto(program, $"{program.Client.FirstName} {program.Client.LastName}".Trim(), exercises, muscles, volumeSummary);
    }

    private static ProgramDto MapToDto(
        Program program,
        string clientName,
        IReadOnlyList<Domain.Exercises.Exercise> exercises,
        IReadOnlyList<Domain.Exercises.Muscle> muscles,
        VolumeSummaryDto? volumeSummary)
    {
        var exerciseLookup = exercises.ToDictionary(e => e.Id);
        var muscleLookup = muscles.ToDictionary(m => m.Id);

        var versionDtos = program.Versions.Select(v =>
        {
            var priorityDtos = v.MusclePriorities.Select(mp => new ProgramMusclePriorityDto(
                Id: mp.Id,
                MuscleId: mp.MuscleId,
                MuscleName: muscleLookup.TryGetValue(mp.MuscleId, out var m) ? m.Name : "Unknown Muscle",
                PriorityLevel: mp.PriorityLevel,
                Justification: mp.Justification)).ToList().AsReadOnly();

            var weekDtos = v.Weeks.Select(w =>
            {
                var sessionDtos = w.Sessions.Select(s =>
                {
                    var slotDtos = s.Slots.Select(sl =>
                    {
                        var ex = exerciseLookup.TryGetValue(sl.ExerciseId, out var e) ? e : null;
                        var ruleDto = sl.ProgressionRule != null
                            ? new ProgressionRuleDto(
                                Type: sl.ProgressionRule.Type,
                                CurrentTarget: sl.ProgressionRule.CurrentTarget,
                                IncrementValue: sl.ProgressionRule.IncrementValue,
                                IncrementCondition: sl.ProgressionRule.IncrementCondition)
                            : null;

                        return new ExerciseSlotDto(
                            Id: sl.Id,
                            TrainingSessionId: sl.TrainingSessionId,
                            ExerciseId: sl.ExerciseId,
                            ExerciseName: ex?.Name ?? "Unknown Exercise",
                            MovementPatternName: ex?.MovementPattern?.Name ?? string.Empty,
                            Order: sl.Order,
                            TargetSets: sl.TargetSets,
                            TargetRepRange: sl.TargetRepRange,
                            EffortGuideline: sl.EffortGuideline,
                            RestSeconds: sl.RestSeconds,
                            SelectionRationale: sl.SelectionRationale,
                            ProgressionRule: ruleDto,
                            CoachingNote: sl.CoachingNote);
                    }).ToList().AsReadOnly();

                    return new TrainingSessionDto(
                        Id: s.Id,
                        TrainingWeekId: s.TrainingWeekId,
                        DayNumber: s.DayNumber,
                        DayOfWeek: s.DayOfWeek,
                        Name: s.Name,
                        SessionIntent: s.SessionIntent,
                        EstimatedDurationMinutes: s.EstimatedDurationMinutes,
                        Slots: slotDtos);
                }).ToList().AsReadOnly();

                return new TrainingWeekDto(
                    Id: w.Id,
                    ProgramVersionId: w.ProgramVersionId,
                    WeekNumber: w.WeekNumber,
                    Sessions: sessionDtos);
            }).ToList().AsReadOnly();

            return new ProgramVersionDto(
                Id: v.Id,
                ProgramId: v.ProgramId,
                VersionNumber: v.VersionNumber,
                RecoveryCapacity: v.RecoveryCapacity,
                ChangeReason: v.ChangeReason,
                IsActive: v.IsActive,
                MusclePriorities: priorityDtos,
                Weeks: weekDtos,
                CreatedAtUtc: v.CreatedAtUtc);
        }).ToList().AsReadOnly();

        var activeVersionDto = versionDtos.FirstOrDefault(v => v.IsActive) ?? versionDtos.FirstOrDefault();

        var goalDto = new GoalSnapshotDto(
            PrimaryGoal: program.GoalSnapshot.PrimaryGoal,
            SecondaryGoal: program.GoalSnapshot.SecondaryGoal,
            GoalEmphasis: program.GoalSnapshot.GoalEmphasis,
            TargetTimelineWeeks: program.GoalSnapshot.TargetTimelineWeeks);

        return new ProgramDto(
            Id: program.Id,
            ClientId: program.ClientId,
            ClientName: clientName,
            CoachId: program.CoachId,
            Name: program.Name,
            Status: program.Status,
            GoalSnapshot: goalDto,
            RationaleSummary: program.RationaleSummary,
            ActiveVersion: activeVersionDto,
            Versions: versionDtos,
            VolumeSummary: volumeSummary,
            CreatedAtUtc: program.CreatedAtUtc,
            UpdatedAtUtc: program.UpdatedAtUtc);
    }
}
