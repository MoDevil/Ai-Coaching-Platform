using AiCoachOs.Application.Programs.DTOs;
using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.TrainingProfiles;

namespace AiCoachOs.Application.Programs.Engine;

public interface IProgramBuilder
{
    (Program Program, ProgramVersion Version, VolumeSummaryDto VolumeSummary) BuildProgram(
        Client client,
        ClientTrainingProfile profile,
        IReadOnlyList<Exercise> allExercises,
        IReadOnlyList<MovementPattern> allPatterns,
        IReadOnlyList<Muscle> allMuscles,
        string? customProgramName,
        string? coachNotes,
        int numberOfWeeks,
        AiCoachOs.Domain.Gyms.GymProfile? gym = null);

    VolumeSummaryDto CalculateVolumeSummary(
        ProgramVersion version,
        IReadOnlyList<Exercise> allExercises,
        IReadOnlyList<Muscle> allMuscles);
}

public class ProgramBuilder : IProgramBuilder
{
    private readonly IGoalAnalyzer _goalAnalyzer;
    private readonly IConstraintAnalyzer _constraintAnalyzer;
    private readonly IRecoveryModel _recoveryModel;
    private readonly IExerciseSelector _exerciseSelector;
    private readonly ISessionBuilder _sessionBuilder;

    public ProgramBuilder(
        IGoalAnalyzer goalAnalyzer,
        IConstraintAnalyzer constraintAnalyzer,
        IRecoveryModel recoveryModel,
        IExerciseSelector exerciseSelector,
        ISessionBuilder sessionBuilder)
    {
        _goalAnalyzer = goalAnalyzer;
        _constraintAnalyzer = constraintAnalyzer;
        _recoveryModel = recoveryModel;
        _exerciseSelector = exerciseSelector;
        _sessionBuilder = sessionBuilder;
    }

    public (Program Program, ProgramVersion Version, VolumeSummaryDto VolumeSummary) BuildProgram(
        Client client,
        ClientTrainingProfile profile,
        IReadOnlyList<Exercise> allExercises,
        IReadOnlyList<MovementPattern> allPatterns,
        IReadOnlyList<Muscle> allMuscles,
        string? customProgramName,
        string? coachNotes,
        int numberOfWeeks,
        AiCoachOs.Domain.Gyms.GymProfile? gym = null)
    {
        // 1. Goal Analysis
        var goalSnapshot = _goalAnalyzer.ParseGoal(client.Goal?.PrimaryGoal, client.Goal?.TargetTimelineWeeks, coachNotes);
        var trainingParams = _goalAnalyzer.DeriveParameters(goalSnapshot);

        // 2. Constraints Analysis
        var constraints = _constraintAnalyzer.Analyze(profile, gym);

        // 3. Recovery Evaluation
        var recoveryCapacity = _recoveryModel.EvaluateRecoveryCapacity(client, profile);

        // 4. Determine Muscle Priorities from TrainingProfile + Goals
        var musclePrioritiesDict = new Dictionary<Guid, MusclePriorityLevel>();
        var versionPrioritiesList = new List<ProgramMusclePriority>();
        var versionId = Guid.NewGuid();

        // Check explicit focus areas in ClientTrainingProfile
        var focusAreas = profile.Priorities.Select(p => p.FocusArea.ToLowerInvariant()).ToList();

        foreach (var muscle in allMuscles)
        {
            var muscleName = muscle.Name.ToLowerInvariant();
            bool isExplicitFocus = focusAreas.Any(fa => fa.Contains(muscleName) || muscleName.Contains(fa));

            var level = MusclePriorityLevel.Secondary;
            string? justification = null;

            if (isExplicitFocus)
            {
                level = MusclePriorityLevel.Primary;
                justification = "Direct client training profile priority focus area.";
            }
            else if (goalSnapshot.GoalEmphasis != null && goalSnapshot.GoalEmphasis.ToLowerInvariant().Contains(muscleName))
            {
                level = MusclePriorityLevel.Primary;
                justification = "Emphasized in client goal description.";
            }
            else
            {
                level = MusclePriorityLevel.Secondary;
                justification = "Balanced foundational stimulus.";
            }

            musclePrioritiesDict[muscle.Id] = level;
            versionPrioritiesList.Add(new ProgramMusclePriority(Guid.NewGuid(), versionId, muscle.Id, level, justification));
        }

        // 5. Build Emergent Program Structure (No fixed split templates; derived from days & non-conflicting distribution)
        // Group available movement patterns
        var patternLookup = allPatterns.ToDictionary(p => p.Id);
        var patternsByName = allPatterns.ToDictionary(p => p.Name.ToLowerInvariant());

        // Helper pattern resolver
        Guid? FindPatternId(string name)
        {
            var match = allPatterns.FirstOrDefault(p => p.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
            return match?.Id;
        }

        var squatId = FindPatternId("Squat");
        var hingeId = FindPatternId("Hinge");
        var hPushId = FindPatternId("Horizontal Push");
        var hPullId = FindPatternId("Horizontal Pull");
        var vPushId = FindPatternId("Vertical Push");
        var vPullId = FindPatternId("Vertical Pull");
        var lungeId = FindPatternId("Lunge");
        var carryId = FindPatternId("Carry");

        // Distribute patterns dynamically across the requested number of weekly sessions
        // to avoid high fatigue overlap across consecutive sessions
        var sessionConfigs = BuildDynamicSessionConfigurations(
            constraints.SessionsPerWeek,
            squatId, hingeId, hPushId, hPullId, vPushId, vPullId, lungeId, carryId,
            allPatterns.Select(p => p.Id).ToList());

        // 6. Instantiate Program & ProgramVersion
        var programId = Guid.NewGuid();
        var programName = string.IsNullOrWhiteSpace(customProgramName)
            ? $"{client.FirstName}'s {goalSnapshot.PrimaryGoal} Program"
            : customProgramName.Trim();

        var rationaleSummary =
            $"Program engineered deterministically from: {goalSnapshot.PrimaryGoal} goal, " +
            $"{constraints.SessionsPerWeek} sessions/week ({constraints.TargetDurationMinutes} min target), " +
            $"{recoveryCapacity} recovery capacity, and {profile.ExperienceLevel} experience. " +
            $"Volume emerges organically from individual session construction without hardcoded split templates.";

        var program = new Program(
            id: programId,
            clientId: client.Id,
            coachId: client.CoachId,
            name: programName,
            goalSnapshot: goalSnapshot,
            rationaleSummary: rationaleSummary,
            status: ProgramStatus.Draft);

        var version = new ProgramVersion(
            id: versionId,
            programId: programId,
            versionNumber: 1,
            recoveryCapacity: recoveryCapacity,
            changeReason: "Initial deterministic program generation.",
            isActive: true);

        foreach (var p in versionPrioritiesList)
        {
            version.AddMusclePriority(p);
        }

        // 7. Generate Weeks & Sessions
        int totalWeeks = Math.Clamp(numberOfWeeks, 1, 12);
        for (int weekNum = 1; weekNum <= totalWeeks; weekNum++)
        {
            var week = new TrainingWeek(Guid.NewGuid(), versionId, weekNum);

            for (int sessionIndex = 0; sessionIndex < sessionConfigs.Count; sessionIndex++)
            {
                var cfg = sessionConfigs[sessionIndex];
                DayOfWeek? plannedDay = sessionIndex < constraints.PlannedDays.Count
                    ? constraints.PlannedDays[sessionIndex]
                    : null;

                var sessionUsedIds = new HashSet<Guid>();

                var session = _sessionBuilder.BuildSession(
                    trainingWeekId: week.Id,
                    dayNumber: sessionIndex + 1,
                    dayOfWeek: plannedDay,
                    sessionName: cfg.Name,
                    sessionIntent: cfg.Intent,
                    patternSequence: cfg.PatternSequence,
                    allExercises: allExercises,
                    patternLookup: patternLookup,
                    availableEquipmentIds: constraints.AvailableEquipmentIds,
                    excludedKeywords: constraints.ExcludedMovementOrExerciseNames,
                    preferredKeywords: constraints.PreferredMovementOrExerciseNames,
                    musclePriorities: musclePrioritiesDict,
                    recoveryCapacity: recoveryCapacity,
                    parameters: trainingParams,
                    targetDurationMinutes: constraints.TargetDurationMinutes,
                    exerciseSelector: _exerciseSelector,
                    constraintAnalyzer: _constraintAnalyzer,
                    sessionUsedExerciseIds: sessionUsedIds);

                week.AddSession(session);
            }

            version.AddWeek(week);
        }

        program.AddVersion(version);

        // 8. Calculate Output Volume Summary
        var volumeSummary = CalculateVolumeSummary(version, allExercises, allMuscles);

        return (program, version, volumeSummary);
    }

    public VolumeSummaryDto CalculateVolumeSummary(
        ProgramVersion version,
        IReadOnlyList<Exercise> allExercises,
        IReadOnlyList<Muscle> allMuscles)
    {
        var exerciseLookup = allExercises.ToDictionary(e => e.Id);
        var firstWeek = version.Weeks.FirstOrDefault();

        if (firstWeek == null || firstWeek.Sessions.Count == 0)
        {
            return new VolumeSummaryDto(0, 0, 0, Array.Empty<MuscleVolumeOutputDto>());
        }

        var directSetsPerMuscle = new Dictionary<Guid, int>();
        var indirectSetsPerMuscle = new Dictionary<Guid, int>();
        var frequencyPerMuscle = new Dictionary<Guid, HashSet<int>>();

        foreach (var muscle in allMuscles)
        {
            directSetsPerMuscle[muscle.Id] = 0;
            indirectSetsPerMuscle[muscle.Id] = 0;
            frequencyPerMuscle[muscle.Id] = new HashSet<int>();
        }

        int totalWeeklySets = 0;
        int totalMinutes = 0;

        foreach (var session in firstWeek.Sessions)
        {
            totalMinutes += session.EstimatedDurationMinutes;

            foreach (var slot in session.Slots)
            {
                totalWeeklySets += slot.TargetSets;

                if (exerciseLookup.TryGetValue(slot.ExerciseId, out var exercise))
                {
                    foreach (var m in exercise.Muscles)
                    {
                        if (m.IsPrimary)
                        {
                            directSetsPerMuscle[m.MuscleId] = directSetsPerMuscle.GetValueOrDefault(m.MuscleId) + slot.TargetSets;
                            if (!frequencyPerMuscle.ContainsKey(m.MuscleId))
                                frequencyPerMuscle[m.MuscleId] = new HashSet<int>();
                            frequencyPerMuscle[m.MuscleId].Add(session.DayNumber);
                        }
                        else
                        {
                            indirectSetsPerMuscle[m.MuscleId] = indirectSetsPerMuscle.GetValueOrDefault(m.MuscleId) + slot.TargetSets;
                        }
                    }
                }
            }
        }

        var musclePriorityMap = version.MusclePriorities.ToDictionary(p => p.MuscleId, p => p.PriorityLevel);

        var muscleVolumeOutputs = allMuscles
            .Select(muscle =>
            {
                int direct = directSetsPerMuscle.GetValueOrDefault(muscle.Id);
                int indirect = indirectSetsPerMuscle.GetValueOrDefault(muscle.Id);
                int total = direct + indirect;
                int freq = frequencyPerMuscle.TryGetValue(muscle.Id, out var days) ? days.Count : 0;
                var priority = musclePriorityMap.GetValueOrDefault(muscle.Id, MusclePriorityLevel.Secondary);

                return new MuscleVolumeOutputDto(
                    MuscleId: muscle.Id,
                    MuscleName: muscle.Name,
                    PriorityLevel: priority,
                    DirectWeeklySets: direct,
                    IndirectWeeklySets: indirect,
                    TotalWeeklySets: total,
                    FrequencyPerWeek: freq);
            })
            .Where(m => m.TotalWeeklySets > 0 || m.PriorityLevel == MusclePriorityLevel.Primary)
            .OrderByDescending(m => m.PriorityLevel == MusclePriorityLevel.Primary)
            .ThenByDescending(m => m.DirectWeeklySets)
            .ToList();

        int avgDuration = firstWeek.Sessions.Count > 0 ? totalMinutes / firstWeek.Sessions.Count : 0;

        return new VolumeSummaryDto(
            TotalWeeklySessions: firstWeek.Sessions.Count,
            TotalWeeklySets: totalWeeklySets,
            AverageSessionDurationMinutes: avgDuration,
            MuscleVolumes: muscleVolumeOutputs.AsReadOnly());
    }

    private record SessionConfig(string Name, string Intent, List<Guid> PatternSequence);

    private static List<SessionConfig> BuildDynamicSessionConfigurations(
        int sessionsPerWeek,
        Guid? squatId, Guid? hingeId, Guid? hPushId, Guid? hPullId, Guid? vPushId, Guid? vPullId, Guid? lungeId, Guid? carryId,
        List<Guid> fallbackPatterns)
    {
        // Deterministic distribution of movement patterns according to available session count
        // Notice: This is NOT a fixed split template (like PPL or Bro Split); it dynamically balances
        // opposing planar forces and lower/upper stimulus to avoid overlapping fatigue bottlenecks.

        List<Guid> ValidList(params Guid?[] ids) => ids.Where(i => i.HasValue).Select(i => i!.Value).ToList();

        return sessionsPerWeek switch
        {
            1 => new List<SessionConfig>
            {
                new("Full Body Comprehensive", "Full body multi-planar stimulus balancing major knee, hip, pushing and pulling actions.",
                    ValidList(squatId, hPushId, hingeId, hPullId, vPushId, vPullId, lungeId))
            },

            2 => new List<SessionConfig>
            {
                new("Foundation A", "Lower knee/quad emphasis combined with upper horizontal push/pull patterns.",
                    ValidList(squatId, hPushId, hPullId, lungeId, carryId)),
                new("Foundation B", "Posterior chain hip hinge emphasis combined with upper vertical push/pull patterns.",
                    ValidList(hingeId, vPushId, vPullId, squatId, hPushId))
            },

            3 => new List<SessionConfig>
            {
                new("Session 1 (Lower / Push)", "Squat and horizontal pushing focus with complementary pulling and core stability.",
                    ValidList(squatId, hPushId, hPullId, lungeId)),
                new("Session 2 (Posterior / Pull)", "Hip hinge and vertical pulling focus with overhead pressing actions.",
                    ValidList(hingeId, vPullId, vPushId, hPullId)),
                new("Session 3 (Balanced Compound)", "Multi-planar integration combining single-leg, horizontal push, and hinge patterns.",
                    ValidList(lungeId, hPushId, squatId, vPullId, carryId))
            },

            4 => new List<SessionConfig>
            {
                new("Session 1 (Lower Quadriceps & Horizontal Push)", "Knee flexion/extension dominant paired with horizontal pushing vectors.",
                    ValidList(squatId, hPushId, lungeId, hPullId)),
                new("Session 2 (Posterior Chain & Vertical Pull)", "Hip extension dominant paired with vertical pulling vectors.",
                    ValidList(hingeId, vPullId, vPushId, carryId)),
                new("Session 3 (Lower Unilateral & Overhead Push)", "Single-leg knee stability paired with vertical pushing and horizontal pulling.",
                    ValidList(lungeId, vPushId, hPullId, squatId)),
                new("Session 4 (Posterior Hinge & Horizontal Pull)", "Hip-dominant hinge paired with horizontal chest and rowing actions.",
                    ValidList(hingeId, hPullId, hPushId, vPullId))
            },

            5 => new List<SessionConfig>
            {
                new("Session 1 (Knee Dominant & Chest)", "Squat mechanics and horizontal pressing.",
                    ValidList(squatId, hPushId, lungeId)),
                new("Session 2 (Hip Dominant & Upper Back)", "Hip hinge mechanics and horizontal rowing.",
                    ValidList(hingeId, hPullId, carryId)),
                new("Session 3 (Overhead Vectors & Vertical Pull)", "Vertical pushing and vertical pulling balance.",
                    ValidList(vPushId, vPullId, hPushId)),
                new("Session 4 (Unilateral Lower & Posterior Chain)", "Single-leg stability and hamstring/glute stimulus.",
                    ValidList(lungeId, hingeId, squatId)),
                new("Session 5 (Full Upper Hypertrophy / Volume Output)", "Upper body antagonist pairings.",
                    ValidList(hPushId, hPullId, vPushId, vPullId))
            },

            _ => new List<SessionConfig>
            {
                new("Session 1 (Knee Dominant & Push)", "Squat and horizontal pressing.",
                    ValidList(squatId, hPushId, lungeId)),
                new("Session 2 (Hip Dominant & Pull)", "Deadlift/hinge and horizontal pulling.",
                    ValidList(hingeId, hPullId, carryId)),
                new("Session 3 (Vertical Vectors)", "Vertical press and vertical pull balance.",
                    ValidList(vPushId, vPullId)),
                new("Session 4 (Lower Hypertrophy Focus)", "Combined knee and hip stimulus.",
                    ValidList(squatId, hingeId, lungeId)),
                new("Session 5 (Horizontal Vectors Focus)", "Horizontal push and row emphasis.",
                    ValidList(hPushId, hPullId)),
                new("Session 6 (Work Capacity & Multi-Planar)", "Unilateral movements and functional carries.",
                    ValidList(lungeId, carryId, vPullId))
            }
        };
    }
}
