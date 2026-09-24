using AiCoachOs.Application.Gyms.Engine;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Gyms;
using AiCoachOs.Domain.TrainingProfiles;

namespace AiCoachOs.Application.Programs.Engine;

public record ConstraintAnalysisResult(
    int SessionsPerWeek,
    IReadOnlyList<DayOfWeek> PlannedDays,
    int TargetDurationMinutes,
    int MinDurationMinutes,
    int MaxDurationMinutes,
    IReadOnlySet<Guid> AvailableEquipmentIds,
    IReadOnlyList<string> ExcludedMovementOrExerciseNames,
    IReadOnlyList<string> PreferredMovementOrExerciseNames);

public interface IConstraintAnalyzer
{
    ConstraintAnalysisResult Analyze(ClientTrainingProfile profile, GymProfile? gym = null);
    bool CanPerformExercise(Exercise exercise, IReadOnlySet<Guid> availableEquipmentIds, IReadOnlyList<string> excludedKeywords);
}

public class ConstraintAnalyzer : IConstraintAnalyzer
{
    private readonly IGymEquipmentResolver _equipmentResolver;

    public ConstraintAnalyzer(IGymEquipmentResolver? equipmentResolver = null)
    {
        _equipmentResolver = equipmentResolver ?? new GymEquipmentResolver();
    }

    public ConstraintAnalysisResult Analyze(ClientTrainingProfile profile, GymProfile? gym = null)
    {
        var availability = profile.WeeklyAvailability;
        int sessionsPerWeek = availability.SessionsPerWeek;

        // Determine planned days of week
        var plannedDays = new List<DayOfWeek>();
        if (availability.PreferredDays.Count >= sessionsPerWeek)
        {
            plannedDays.AddRange(availability.PreferredDays.Take(sessionsPerWeek));
        }
        else if (availability.AvailableDays.Count >= sessionsPerWeek)
        {
            plannedDays.AddRange(availability.AvailableDays.Take(sessionsPerWeek));
        }
        else
        {
            // Default spread across the week based on number of sessions
            plannedDays = sessionsPerWeek switch
            {
                1 => new List<DayOfWeek> { DayOfWeek.Monday },
                2 => new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Thursday },
                3 => new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                4 => new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                5 => new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Saturday },
                6 => new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                _ => new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday }
            };
        }

        int targetDuration = profile.SessionDurationTargetMinutes ?? 60;
        int minDuration = profile.SessionDurationMinMinutes ?? Math.Max(30, targetDuration - 15);
        int maxDuration = profile.SessionDurationMaxMinutes ?? (targetDuration + 15);

        var availableEquipment = _equipmentResolver.ResolveAvailableEquipment(gym, profile.AvailableEquipmentIds);

        var excluded = (profile.ExerciseConstraints ?? string.Empty)
            .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToLowerInvariant())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        var preferred = (profile.ExercisePreferences ?? string.Empty)
            .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToLowerInvariant())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        return new ConstraintAnalysisResult(
            SessionsPerWeek: sessionsPerWeek,
            PlannedDays: plannedDays.AsReadOnly(),
            TargetDurationMinutes: targetDuration,
            MinDurationMinutes: minDuration,
            MaxDurationMinutes: maxDuration,
            AvailableEquipmentIds: availableEquipment,
            ExcludedMovementOrExerciseNames: excluded.AsReadOnly(),
            PreferredMovementOrExerciseNames: preferred.AsReadOnly());
    }

    public bool CanPerformExercise(Exercise exercise, IReadOnlySet<Guid> availableEquipmentIds, IReadOnlyList<string> excludedKeywords)
    {
        // 1. Check equipment constraints
        // If client specified available equipment and exercise requires equipment not in available set
        if (availableEquipmentIds.Count > 0)
        {
            var requiredEquipment = exercise.Equipment.Where(e => e.IsRequired).Select(e => e.EquipmentId);
            if (requiredEquipment.Any(reqId => !availableEquipmentIds.Contains(reqId)))
            {
                return false;
            }
        }

        // 2. Check exclusion keywords against exercise name and aliases
        var exerciseName = exercise.Name.ToLowerInvariant();
        var aliases = (exercise.Aliases ?? string.Empty).ToLowerInvariant();

        foreach (var keyword in excludedKeywords)
        {
            if (exerciseName.Contains(keyword) || aliases.Contains(keyword))
            {
                return false;
            }
        }

        return true;
    }
}
