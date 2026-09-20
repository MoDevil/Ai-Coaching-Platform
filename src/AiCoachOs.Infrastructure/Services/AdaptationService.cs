using AiCoachOs.Application.Adaptations.DTOs;
using AiCoachOs.Application.Adaptations.Engine;
using AiCoachOs.Application.Adaptations.Services;
using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Programs.Engine;
using AiCoachOs.Domain.Adaptations;
using AiCoachOs.Domain.Programs;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class AdaptationService : IAdaptationService
{
    private readonly IApplicationDbContext _context;
    private readonly IAdaptationAnalyzer _analyzer;
    private readonly IExerciseSelector _exerciseSelector;
    private readonly IConstraintAnalyzer _constraintAnalyzer;

    public AdaptationService(
        IApplicationDbContext context,
        IAdaptationAnalyzer analyzer,
        IExerciseSelector exerciseSelector,
        IConstraintAnalyzer constraintAnalyzer)
    {
        _context = context;
        _analyzer = analyzer;
        _exerciseSelector = exerciseSelector;
        _constraintAnalyzer = constraintAnalyzer;
    }

    public async Task<AdaptationAssessmentDto> AssessProgramVersionAsync(Guid coachId, Guid programVersionId, CancellationToken cancellationToken = default)
    {
        var version = await _context.ProgramVersions
            .Include(pv => pv.Program)
                .ThenInclude(p => p.Client)
            .Include(pv => pv.Weeks)
                .ThenInclude(w => w.Sessions)
                    .ThenInclude(s => s.Slots)
                        .ThenInclude(sl => sl.Exercise)
            .FirstOrDefaultAsync(pv => pv.Id == programVersionId, cancellationToken);

        if (version == null)
            throw new NotFoundException($"ProgramVersion with ID '{programVersionId}' was not found.");

        if (version.Program.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this program.");

        var client = version.Program.Client;
        var profile = await _context.FindTrainingProfileByClientIdAsync(client.Id, cancellationToken);

        // Fetch all completed / recorded workouts for this client and program version
        var workouts = await _context.WorkoutSessions
            .Where(w => w.ClientId == client.Id && (w.ProgramVersionId == programVersionId || w.ProgramVersionId == null))
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Sets)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
            .OrderBy(w => w.StartedAtUtc)
            .ToListAsync(cancellationToken);

        var allExercises = await _context.Exercises
            .Include(e => e.MovementPattern)
            .Include(e => e.Muscles)
            .Include(e => e.Equipment)
            .ToListAsync(cancellationToken);

        var assessment = _analyzer.Analyze(
            programVersion: version,
            workouts: workouts,
            allExercises: allExercises,
            profile: profile,
            exerciseSelector: _exerciseSelector,
            constraintAnalyzer: _constraintAnalyzer);

        await _context.AddAdaptationAssessmentAsync(assessment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var loaded = await _context.FindAdaptationAssessmentByIdAsync(assessment.Id, cancellationToken);
        return MapToDto(loaded ?? assessment);
    }

    public async Task<AdaptationAssessmentDto?> GetAssessmentByIdAsync(Guid coachId, Guid assessmentId, CancellationToken cancellationToken = default)
    {
        var assessment = await _context.FindAdaptationAssessmentByIdAsync(assessmentId, cancellationToken);
        if (assessment == null)
            return null;

        if (assessment.ProgramVersion.Program.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this assessment.");

        return MapToDto(assessment);
    }

    public async Task<IReadOnlyList<AdaptationAssessmentDto>> GetAssessmentsByProgramIdAsync(Guid coachId, Guid programId, CancellationToken cancellationToken = default)
    {
        var program = await _context.FindProgramByIdAsync(programId, cancellationToken);
        if (program == null)
            throw new NotFoundException($"Program with ID '{programId}' was not found.");

        if (program.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this program.");

        var versionIds = program.Versions.Select(v => v.Id).ToList();

        var assessments = await _context.AdaptationAssessments
            .Where(a => versionIds.Contains(a.ProgramVersionId))
            .Include(a => a.ProgramVersion)
                .ThenInclude(pv => pv.Program)
                    .ThenInclude(p => p.Client)
            .Include(a => a.ExerciseRecords)
                .ThenInclude(r => r.Exercise)
            .Include(a => a.ExerciseRecords)
                .ThenInclude(r => r.ExerciseSlot)
            .Include(a => a.Recommendations)
                .ThenInclude(rec => rec.ExerciseAdaptationRecord)
                    .ThenInclude(r => r!.Exercise)
            .OrderByDescending(a => a.AssessedAt)
            .ToListAsync(cancellationToken);

        return assessments.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<AdaptationRecommendationDto> DecideRecommendationAsync(
        Guid coachId,
        Guid recommendationId,
        CoachRecommendationDecisionDto decision,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await _context.AdaptationRecommendations
            .Include(r => r.AdaptationAssessment)
                .ThenInclude(a => a.ProgramVersion)
                    .ThenInclude(pv => pv.Program)
            .Include(r => r.ExerciseAdaptationRecord)
                .ThenInclude(ear => ear!.Exercise)
            .FirstOrDefaultAsync(r => r.Id == recommendationId, cancellationToken);

        if (recommendation == null)
            throw new NotFoundException($"Adaptation recommendation with ID '{recommendationId}' was not found.");

        var program = recommendation.AdaptationAssessment.ProgramVersion.Program;
        if (program.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach does not have access to this recommendation.");

        if (decision.Approve)
        {
            recommendation.Approve(decision.CoachDecisionNote);

            // BRANCH NEW PROGRAM VERSION (Section 17: Hard Rule — only created on Coach approval)
            var currentVersion = await _context.ProgramVersions
                .Include(pv => pv.Weeks)
                    .ThenInclude(w => w.Sessions)
                        .ThenInclude(s => s.Slots)
                .Include(pv => pv.MusclePriorities)
                .FirstOrDefaultAsync(pv => pv.Id == recommendation.AdaptationAssessment.ProgramVersionId, cancellationToken);

            if (currentVersion != null)
            {
                int newVersionNumber = currentVersion.VersionNumber + 1;
                string changeReason = $"M7 Adaptive Coaching: {recommendation.ActionType} ({recommendation.Rationale}). Coach note: {decision.CoachDecisionNote}".Trim();

                var newVersion = new ProgramVersion(
                    id: Guid.NewGuid(),
                    programId: program.Id,
                    versionNumber: newVersionNumber,
                    recoveryCapacity: currentVersion.RecoveryCapacity,
                    changeReason: changeReason,
                    isActive: true);

                // Clone muscle priorities
                foreach (var mp in currentVersion.MusclePriorities)
                {
                    newVersion.AddMusclePriority(new ProgramMusclePriority(
                        id: Guid.NewGuid(),
                        programVersionId: newVersion.Id,
                        muscleId: mp.MuscleId,
                        priorityLevel: mp.PriorityLevel,
                        justification: mp.Justification));
                }

                // Parse suggested change if any
                Guid? substituteExerciseId = null;
                int? adjustedTargetSets = null;

                if (!string.IsNullOrWhiteSpace(recommendation.SuggestedChangeDetail))
                {
                    if (recommendation.SuggestedChangeDetail.Contains("SubstituteExerciseId:"))
                    {
                        var parts = recommendation.SuggestedChangeDetail.Split(';');
                        foreach (var part in parts)
                        {
                            if (part.StartsWith("SubstituteExerciseId:") &&
                                Guid.TryParse(part.Substring("SubstituteExerciseId:".Length), out var subId))
                            {
                                substituteExerciseId = subId;
                            }
                        }
                    }
                    else if (recommendation.SuggestedChangeDetail.StartsWith("TargetSets:") &&
                             int.TryParse(recommendation.SuggestedChangeDetail.Substring("TargetSets:".Length), out var setsVal))
                    {
                        adjustedTargetSets = setsVal;
                    }
                }

                // Clone weeks, sessions, and slots with applied adaptation
                foreach (var oldWeek in currentVersion.Weeks.OrderBy(w => w.WeekNumber))
                {
                    var newWeek = new TrainingWeek(
                        id: Guid.NewGuid(),
                        programVersionId: newVersion.Id,
                        weekNumber: oldWeek.WeekNumber);

                    foreach (var oldSession in oldWeek.Sessions.OrderBy(s => s.DayNumber))
                    {
                        var newSession = new TrainingSession(
                            id: Guid.NewGuid(),
                            trainingWeekId: newWeek.Id,
                            dayNumber: oldSession.DayNumber,
                            name: oldSession.Name,
                            sessionIntent: oldSession.SessionIntent,
                            estimatedDurationMinutes: oldSession.EstimatedDurationMinutes,
                            dayOfWeek: oldSession.DayOfWeek);

                        foreach (var oldSlot in oldSession.Slots.OrderBy(s => s.Order))
                        {
                            Guid slotExerciseId = oldSlot.ExerciseId;
                            int slotSets = oldSlot.TargetSets;
                            string slotRationale = oldSlot.SelectionRationale;

                            // Apply adaptation to target slot
                            if (recommendation.TargetSlotId.HasValue && oldSlot.Id == recommendation.TargetSlotId.Value)
                            {
                                if (substituteExerciseId.HasValue)
                                {
                                    slotExerciseId = substituteExerciseId.Value;
                                    slotRationale = $"Substituted via M7 Adaptive Coaching: {recommendation.Rationale}";
                                }
                                if (adjustedTargetSets.HasValue)
                                {
                                    slotSets = adjustedTargetSets.Value;
                                }
                            }

                            var newSlot = new ExerciseSlot(
                                id: Guid.NewGuid(),
                                trainingSessionId: newSession.Id,
                                exerciseId: slotExerciseId,
                                order: oldSlot.Order,
                                targetSets: slotSets,
                                targetRepRange: oldSlot.TargetRepRange,
                                effortGuideline: oldSlot.EffortGuideline,
                                restSeconds: oldSlot.RestSeconds,
                                selectionRationale: slotRationale,
                                progressionRule: oldSlot.ProgressionRule,
                                coachingNote: oldSlot.CoachingNote);

                            newSession.AddSlot(newSlot);
                        }

                        newWeek.AddSession(newSession);
                    }

                    newVersion.AddWeek(newWeek);
                }

                // Mark current version inactive and attach new version to program
                currentVersion.SetActive(false);
                program.AddVersion(newVersion);
                await _context.AddProgramVersionAsync(newVersion, cancellationToken);
                recommendation.MarkApplied();
            }
        }
        else
        {
            recommendation.Reject(decision.CoachDecisionNote ?? "Rejected by coach");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new AdaptationRecommendationDto(
            Id: recommendation.Id,
            ExerciseAdaptationRecordId: recommendation.ExerciseAdaptationRecordId,
            TargetSlotId: recommendation.TargetSlotId,
            ExerciseName: recommendation.ExerciseAdaptationRecord?.Exercise.Name,
            ActionType: recommendation.ActionType,
            SuggestedChangeDetail: recommendation.SuggestedChangeDetail,
            Rationale: recommendation.Rationale,
            Confidence: recommendation.Confidence,
            Status: recommendation.Status,
            CoachDecisionAt: recommendation.CoachDecisionAt,
            CoachDecisionNote: recommendation.CoachDecisionNote);
    }

    private static AdaptationAssessmentDto MapToDto(AdaptationAssessment a)
    {
        var program = a.ProgramVersion?.Program;
        var client = program?.Client;

        return new AdaptationAssessmentDto(
            Id: a.Id,
            ProgramVersionId: a.ProgramVersionId,
            VersionNumber: a.ProgramVersion?.VersionNumber ?? 1,
            ProgramId: program?.Id ?? Guid.Empty,
            ProgramName: program?.Name ?? "Program",
            ClientId: client?.Id ?? Guid.Empty,
            ClientName: client != null ? $"{client.FirstName} {client.LastName}".Trim() : "Client",
            AssessedAt: a.AssessedAt,
            ObservationStartDate: a.ObservationStartDate,
            ObservationEndDate: a.ObservationEndDate,
            TotalExposures: a.TotalExposures,
            CompletedExposures: a.CompletedExposures,
            AdherenceRate: a.AdherenceRate,
            OverallStatus: a.OverallStatus,
            CoachNotes: a.CoachNotes,
            ExerciseRecords: a.ExerciseRecords.Select(r => new ExerciseAdaptationRecordDto(
                Id: r.Id,
                ExerciseSlotId: r.ExerciseSlotId,
                ExerciseId: r.ExerciseId,
                ExerciseName: r.Exercise?.Name ?? "Exercise",
                ExposureCount: r.ExposureCount,
                ProgressionMetCount: r.ProgressionMetCount,
                EffortAlignmentStatus: r.EffortAlignmentStatus,
                PerformanceTrend: r.PerformanceTrend,
                PlateauConfirmed: r.PlateauConfirmed,
                AdherenceToExercise: r.AdherenceToExercise
            )).ToList().AsReadOnly(),
            Recommendations: a.Recommendations.Select(r => new AdaptationRecommendationDto(
                Id: r.Id,
                ExerciseAdaptationRecordId: r.ExerciseAdaptationRecordId,
                TargetSlotId: r.TargetSlotId,
                ExerciseName: r.ExerciseAdaptationRecord?.Exercise?.Name,
                ActionType: r.ActionType,
                SuggestedChangeDetail: r.SuggestedChangeDetail,
                Rationale: r.Rationale,
                Confidence: r.Confidence,
                Status: r.Status,
                CoachDecisionAt: r.CoachDecisionAt,
                CoachDecisionNote: r.CoachDecisionNote
            )).ToList().AsReadOnly()
        );
    }
}
