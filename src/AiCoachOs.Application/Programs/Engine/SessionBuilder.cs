using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Programs;

namespace AiCoachOs.Application.Programs.Engine;

public interface ISessionBuilder
{
    TrainingSession BuildSession(
        Guid trainingWeekId,
        int dayNumber,
        DayOfWeek? dayOfWeek,
        string sessionName,
        string sessionIntent,
        IReadOnlyList<Guid> patternSequence,
        IReadOnlyList<Exercise> allExercises,
        IReadOnlyDictionary<Guid, MovementPattern> patternLookup,
        IReadOnlySet<Guid> availableEquipmentIds,
        IReadOnlyList<string> excludedKeywords,
        IReadOnlyList<string> preferredKeywords,
        IReadOnlyDictionary<Guid, MusclePriorityLevel> musclePriorities,
        RecoveryCapacity recoveryCapacity,
        GoalTrainingParameters parameters,
        int targetDurationMinutes,
        IExerciseSelector exerciseSelector,
        IConstraintAnalyzer constraintAnalyzer,
        HashSet<Guid> sessionUsedExerciseIds);
}

public class SessionBuilder : ISessionBuilder
{
    public TrainingSession BuildSession(
        Guid trainingWeekId,
        int dayNumber,
        DayOfWeek? dayOfWeek,
        string sessionName,
        string sessionIntent,
        IReadOnlyList<Guid> patternSequence,
        IReadOnlyList<Exercise> allExercises,
        IReadOnlyDictionary<Guid, MovementPattern> patternLookup,
        IReadOnlySet<Guid> availableEquipmentIds,
        IReadOnlyList<string> excludedKeywords,
        IReadOnlyList<string> preferredKeywords,
        IReadOnlyDictionary<Guid, MusclePriorityLevel> musclePriorities,
        RecoveryCapacity recoveryCapacity,
        GoalTrainingParameters parameters,
        int targetDurationMinutes,
        IExerciseSelector exerciseSelector,
        IConstraintAnalyzer constraintAnalyzer,
        HashSet<Guid> sessionUsedExerciseIds)
    {
        var sessionId = Guid.NewGuid();
        var session = new TrainingSession(
            id: sessionId,
            trainingWeekId: trainingWeekId,
            dayNumber: dayNumber,
            name: sessionName,
            sessionIntent: sessionIntent,
            estimatedDurationMinutes: targetDurationMinutes,
            dayOfWeek: dayOfWeek);

        int currentOrder = 1;
        int accumulatedMinutes = 10; // Warmup / transition buffer

        foreach (var patternId in patternSequence)
        {
            if (accumulatedMinutes >= targetDurationMinutes)
            {
                break; // Session duration capacity reached
            }

            var candidate = exerciseSelector.SelectBestExercise(
                movementPatternId: patternId,
                allExercises: allExercises,
                availableEquipmentIds: availableEquipmentIds,
                excludedKeywords: excludedKeywords,
                preferredKeywords: preferredKeywords,
                alreadySelectedExerciseIds: sessionUsedExerciseIds,
                musclePriorities: musclePriorities,
                recoveryCapacity: recoveryCapacity,
                constraintAnalyzer: constraintAnalyzer);

            if (candidate == null)
            {
                continue;
            }

            var ex = candidate.Exercise;
            sessionUsedExerciseIds.Add(ex.Id);

            bool isCompound = ex.Category == ExerciseCategory.Compound;
            string repRange = isCompound ? parameters.CompoundRepRange : parameters.IsolationRepRange;
            int restSeconds = isCompound ? parameters.CompoundRestSeconds : parameters.IsolationRestSeconds;

            // Sets derived from baseline + muscle priority bonus
            int sets = parameters.BaselineSetsPerExercise;
            bool targetsPriorityMuscle = ex.Muscles.Any(m =>
                m.IsPrimary &&
                musclePriorities.TryGetValue(m.MuscleId, out var p) &&
                p == MusclePriorityLevel.Primary);

            if (targetsPriorityMuscle)
            {
                sets += parameters.PriorityBonusSets;
            }

            // Adjust sets for recovery capacity:
            if (recoveryCapacity == RecoveryCapacity.Low && sets > 3)
            {
                sets = 3;
            }

            // Estimated exercise slot duration:
            // ~45 sec per set work + restSeconds * (sets - 1) + 2 min transition
            int slotTimeMinutes = Math.Max(5, (sets * 45 + (sets - 1) * restSeconds + 120) / 60);

            // Progression rule based on exercise category and goal:
            ProgressionRule progressionRule = isCompound
                ? new ProgressionRule(
                    type: ProgressionRuleType.LinearLoad,
                    currentTarget: $"{repRange} reps @ {parameters.EffortGuideline}",
                    incrementValue: "+1.25 to 2.5 kg",
                    incrementCondition: "When top reps achieved for all sets with clean technical execution")
                : new ProgressionRule(
                    type: ProgressionRuleType.RepTarget,
                    currentTarget: $"{repRange} reps @ {parameters.EffortGuideline}",
                    incrementValue: "+1 rep per set",
                    incrementCondition: "When reaching upper rep boundary across all sets");

            var slot = new ExerciseSlot(
                id: Guid.NewGuid(),
                trainingSessionId: sessionId,
                exerciseId: ex.Id,
                order: currentOrder++,
                targetSets: sets,
                targetRepRange: repRange,
                effortGuideline: parameters.EffortGuideline,
                restSeconds: restSeconds,
                selectionRationale: candidate.Rationale,
                progressionRule: progressionRule,
                coachingNote: isCompound ? "Prioritize consistent bar path and stable bracing." : "Control eccentric phase through full active range of motion.");

            session.AddSlot(slot);
            accumulatedMinutes += slotTimeMinutes;
        }

        return session;
    }
}
