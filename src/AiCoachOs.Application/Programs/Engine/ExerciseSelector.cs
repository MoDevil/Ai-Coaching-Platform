using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Programs;

namespace AiCoachOs.Application.Programs.Engine;

public record CandidateExercise(
    Exercise Exercise,
    string Rationale,
    bool IsSubstituted,
    string? OriginalExerciseName);

public interface IExerciseSelector
{
    CandidateExercise? SelectBestExercise(
        Guid movementPatternId,
        IReadOnlyList<Exercise> allExercises,
        IReadOnlySet<Guid> availableEquipmentIds,
        IReadOnlyList<string> excludedKeywords,
        IReadOnlyList<string> preferredKeywords,
        IReadOnlySet<Guid> alreadySelectedExerciseIds,
        IReadOnlyDictionary<Guid, MusclePriorityLevel> musclePriorities,
        RecoveryCapacity recoveryCapacity,
        IConstraintAnalyzer constraintAnalyzer);
}

public class ExerciseSelector : IExerciseSelector
{
    public CandidateExercise? SelectBestExercise(
        Guid movementPatternId,
        IReadOnlyList<Exercise> allExercises,
        IReadOnlySet<Guid> availableEquipmentIds,
        IReadOnlyList<string> excludedKeywords,
        IReadOnlyList<string> preferredKeywords,
        IReadOnlySet<Guid> alreadySelectedExerciseIds,
        IReadOnlyDictionary<Guid, MusclePriorityLevel> musclePriorities,
        RecoveryCapacity recoveryCapacity,
        IConstraintAnalyzer constraintAnalyzer)
    {
        // 1. Filter candidates for the requested movement pattern
        var patternCandidates = allExercises
            .Where(e => e.MovementPatternId == movementPatternId && !alreadySelectedExerciseIds.Contains(e.Id))
            .ToList();

        if (patternCandidates.Count == 0)
        {
            // Fallback: any exercise not already selected that targets priority muscles
            patternCandidates = allExercises
                .Where(e => !alreadySelectedExerciseIds.Contains(e.Id))
                .ToList();
        }

        // Rank candidates
        // Criteria (Deterministic Qualitative Ranking):
        // 1. Can perform with available equipment and not excluded (Hard Constraint)
        // 2. Client preference match (Soft Constraint)
        // 3. Muscle priority targeting (Priority Constraint)
        // 4. Recovery Capacity alignment:
        //    - If Low recovery capacity, penalize High Systemic Fatigue exercises
        //    - If High recovery capacity, favor High Stimulus Potential exercises
        // 5. Progression potential

        var validCandidates = patternCandidates
            .Where(e => constraintAnalyzer.CanPerformExercise(e, availableEquipmentIds, excludedKeywords))
            .ToList();

        if (validCandidates.Count > 0)
        {
            var best = PickTopCandidate(validCandidates, preferredKeywords, musclePriorities, recoveryCapacity);
            var rationale = BuildSelectionRationale(best, isSubstituted: false, originalName: null, recoveryCapacity);
            return new CandidateExercise(best, rationale, IsSubstituted: false, OriginalExerciseName: null);
        }

        // Substitution Trigger: If no candidate directly matched due to equipment/exclusion,
        // search for explicit substitutions in M2 ExerciseSubstitution among the requested pattern
        foreach (var original in patternCandidates)
        {
            foreach (var sub in original.Substitutions)
            {
                var substituteExercise = allExercises.FirstOrDefault(e => e.Id == sub.SubstituteExerciseId);
                if (substituteExercise != null &&
                    !alreadySelectedExerciseIds.Contains(substituteExercise.Id) &&
                    constraintAnalyzer.CanPerformExercise(substituteExercise, availableEquipmentIds, excludedKeywords))
                {
                    var rationale = $"Substituted for {original.Name} due to equipment/exclusion constraint. " +
                                    $"{sub.IntentPreservationNotes ?? "Preserves primary movement pattern and target muscle stimuli."}";
                    return new CandidateExercise(substituteExercise, rationale, IsSubstituted: true, OriginalExerciseName: original.Name);
                }
            }
        }

        // If no explicit substitution, search other exercises in the library that share primary muscles
        var patternPrimaryMuscleIds = patternCandidates
            .SelectMany(e => e.Muscles.Where(m => m.IsPrimary).Select(m => m.MuscleId))
            .Distinct()
            .ToHashSet();

        var fallbackCandidates = allExercises
            .Where(e => !alreadySelectedExerciseIds.Contains(e.Id))
            .Where(e => e.Muscles.Any(m => m.IsPrimary && patternPrimaryMuscleIds.Contains(m.MuscleId)))
            .Where(e => constraintAnalyzer.CanPerformExercise(e, availableEquipmentIds, excludedKeywords))
            .ToList();

        if (fallbackCandidates.Count > 0)
        {
            var bestFallback = PickTopCandidate(fallbackCandidates, preferredKeywords, musclePriorities, recoveryCapacity);
            var rationale = $"Selected as biomechanical alternative targeting primary muscles while adhering to equipment and tolerance constraints.";
            return new CandidateExercise(bestFallback, rationale, IsSubstituted: true, OriginalExerciseName: patternCandidates.FirstOrDefault()?.Name);
        }

        return null;
    }

    private static Exercise PickTopCandidate(
        List<Exercise> candidates,
        IReadOnlyList<string> preferredKeywords,
        IReadOnlyDictionary<Guid, MusclePriorityLevel> musclePriorities,
        RecoveryCapacity recoveryCapacity)
    {
        // Deterministic multi-factor scoring (strictly ordinal priority ranking)
        return candidates
            .OrderByDescending(e =>
            {
                int score = 0;

                // Preference bonus
                var name = e.Name.ToLowerInvariant();
                if (preferredKeywords.Any(k => name.Contains(k)))
                {
                    score += 100;
                }

                // Muscle priority bonus
                foreach (var muscle in e.Muscles)
                {
                    if (musclePriorities.TryGetValue(muscle.MuscleId, out var priority))
                    {
                        if (priority == MusclePriorityLevel.Primary)
                            score += muscle.IsPrimary ? 50 : 25;
                        else if (priority == MusclePriorityLevel.Secondary)
                            score += muscle.IsPrimary ? 20 : 10;
                    }
                }

                // Recovery capacity alignment
                if (recoveryCapacity == RecoveryCapacity.Low)
                {
                    // Favor low/moderate systemic fatigue, penalize high fatigue
                    if (e.SystemicFatigueCost == QualitativeRating.Low) score += 30;
                    else if (e.SystemicFatigueCost == QualitativeRating.High) score -= 40;

                    // Favor stable movements
                    if (e.StabilityRequirement == QualitativeRating.Low) score += 10;
                }
                else if (recoveryCapacity == RecoveryCapacity.High)
                {
                    // High capacity can tolerate and benefits from high stimulus
                    if (e.StimulusPotential == QualitativeRating.High) score += 30;
                    if (e.ProgressionPotential == QualitativeRating.High) score += 20;
                }
                else // Moderate
                {
                    if (e.StimulusPotential == QualitativeRating.High) score += 20;
                    if (e.SystemicFatigueCost == QualitativeRating.Low) score += 10;
                }

                return score;
            })
            .ThenBy(e => e.Name) // Deterministic tie-breaker
            .First();
    }

    private static string BuildSelectionRationale(
        Exercise exercise,
        bool isSubstituted,
        string? originalName,
        RecoveryCapacity recoveryCapacity)
    {
        if (isSubstituted && !string.IsNullOrEmpty(originalName))
        {
            return $"Selected as biomechanical alternative for {originalName} matching equipment availability and joint movement parameters.";
        }

        var fatigueDesc = exercise.SystemicFatigueCost switch
        {
            QualitativeRating.Low => "low systemic fatigue impact",
            QualitativeRating.Moderate => "moderate systemic fatigue footprint",
            _ => "high systemic fatigue load"
        };

        var stimulusDesc = exercise.StimulusPotential switch
        {
            QualitativeRating.High => "high stimulus potential",
            QualitativeRating.Moderate => "moderate stimulus potential",
            _ => "targeted stimulus"
        };

        return $"Selected for movement pattern balance offering {stimulusDesc} with {fatigueDesc}, aligned with client {recoveryCapacity} recovery capacity.";
    }
}
