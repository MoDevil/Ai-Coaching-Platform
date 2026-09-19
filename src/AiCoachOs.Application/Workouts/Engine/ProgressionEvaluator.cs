using System.Text.RegularExpressions;
using AiCoachOs.Application.Workouts.DTOs;
using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.Workouts;

namespace AiCoachOs.Application.Workouts.Engine;

public interface IProgressionEvaluator
{
    ProgressionEvaluationResultDto Evaluate(
        ExerciseSlot? plannedSlot,
        IReadOnlyCollection<WorkoutSet> completedSets);
}

public class ProgressionEvaluator : IProgressionEvaluator
{
    public ProgressionEvaluationResultDto Evaluate(
        ExerciseSlot? plannedSlot,
        IReadOnlyCollection<WorkoutSet> completedSets)
    {
        if (plannedSlot == null || plannedSlot.ProgressionRule == null)
        {
            return new ProgressionEvaluationResultDto(
                Status: ProgressionEvaluationStatus.NotEvaluable,
                Reason: "No planned progression rule defined for this exercise.",
                SuggestedNextTarget: null);
        }

        var validSets = completedSets.Where(s => s.IsCompleted).OrderBy(s => s.SetNumber).ToList();
        if (validSets.Count == 0)
        {
            return new ProgressionEvaluationResultDto(
                Status: ProgressionEvaluationStatus.Incomplete,
                Reason: "No completed sets recorded for this exercise.",
                SuggestedNextTarget: null);
        }

        if (validSets.Count < plannedSlot.TargetSets)
        {
            return new ProgressionEvaluationResultDto(
                Status: ProgressionEvaluationStatus.Incomplete,
                Reason: $"Completed {validSets.Count} of {plannedSlot.TargetSets} planned sets. Incomplete volume to trigger progression.",
                SuggestedNextTarget: $"{validSets.Count}/{plannedSlot.TargetSets} sets completed — repeat same target load/reps to consolidate volume.");
        }

        var rule = plannedSlot.ProgressionRule;
        var (minReps, maxReps) = ParseRepRange(plannedSlot.TargetRepRange);

        return rule.Type switch
        {
            ProgressionRuleType.LinearLoad => EvaluateLinearLoad(plannedSlot, validSets, minReps, maxReps),
            ProgressionRuleType.RepTarget => EvaluateRepTarget(plannedSlot, validSets, maxReps),
            ProgressionRuleType.RirTarget => EvaluateRirTarget(plannedSlot, validSets),
            _ => new ProgressionEvaluationResultDto(
                Status: ProgressionEvaluationStatus.NotEvaluable,
                Reason: $"Progression rule type '{rule.Type}' is not evaluable.",
                SuggestedNextTarget: null)
        };
    }

    private static ProgressionEvaluationResultDto EvaluateLinearLoad(
        ExerciseSlot plannedSlot,
        List<WorkoutSet> sets,
        int minReps,
        int maxReps)
    {
        var rule = plannedSlot.ProgressionRule!;
        // Linear load progression: Target is met when all planned sets hit or exceed the upper rep threshold
        // with valid effort (RIR >= 1 if recorded)
        bool allSetsReachedTopReps = sets.All(s => s.Repetitions >= maxReps);
        bool hasFailedRir = sets.Any(s => s.Rir.HasValue && s.Rir.Value < 0.5m); // Reached complete failure prematurely

        if (allSetsReachedTopReps && !hasFailedRir)
        {
            var currentLoad = sets.Max(s => s.LoadKg);
            return new ProgressionEvaluationResultDto(
                Status: ProgressionEvaluationStatus.Met,
                Reason: $"All {sets.Count} sets reached upper target boundary of {maxReps} reps with technical control. Progression condition satisfied.",
                SuggestedNextTarget: $"Advance load by {rule.IncrementValue} (Target: {currentLoad} kg + {rule.IncrementValue})");
        }

        if (!allSetsReachedTopReps)
        {
            var lowestRepSet = sets.OrderBy(s => s.Repetitions).First();
            return new ProgressionEvaluationResultDto(
                Status: ProgressionEvaluationStatus.NotMet,
                Reason: $"Set {lowestRepSet.SetNumber} achieved {lowestRepSet.Repetitions} reps, falling below top threshold of {maxReps} reps. Maintain load until all sets hit {maxReps} reps.",
                SuggestedNextTarget: $"Maintain current load ({sets.Max(s => s.LoadKg)} kg) for {plannedSlot.TargetRepRange} reps @ {plannedSlot.EffortGuideline}");
        }

        return new ProgressionEvaluationResultDto(
            Status: ProgressionEvaluationStatus.NotMet,
            Reason: "Top reps achieved but technical effort was pushed to true failure (< 1 RIR). Consolidate same load with higher reserve before increasing.",
            SuggestedNextTarget: $"Repeat {sets.Max(s => s.LoadKg)} kg with consistent execution @ {plannedSlot.EffortGuideline}");
    }

    private static ProgressionEvaluationResultDto EvaluateRepTarget(
        ExerciseSlot plannedSlot,
        List<WorkoutSet> sets,
        int maxReps)
    {
        var rule = plannedSlot.ProgressionRule!;
        // Rep target: Target is met when sets reach upper rep boundary across all sets
        bool allMet = sets.All(s => s.Repetitions >= maxReps);

        if (allMet)
        {
            return new ProgressionEvaluationResultDto(
                Status: ProgressionEvaluationStatus.Met,
                Reason: $"Upper rep threshold of {maxReps} reps achieved across all sets. Advance rep target or step load.",
                SuggestedNextTarget: $"Progression condition met: {rule.IncrementValue}");
        }

        var avgReps = sets.Average(s => s.Repetitions);
        return new ProgressionEvaluationResultDto(
            Status: ProgressionEvaluationStatus.NotMet,
            Reason: $"Average completed reps ({avgReps:F1}) has not yet saturated the upper target ({maxReps} reps).",
            SuggestedNextTarget: $"Aim for +1 rep on first set next session within {plannedSlot.TargetRepRange} rep target.");
    }

    private static ProgressionEvaluationResultDto EvaluateRirTarget(
        ExerciseSlot plannedSlot,
        List<WorkoutSet> sets)
    {
        // RIR target check: Verify effort guideline compliance
        var setsWithRir = sets.Where(s => s.Rir.HasValue).ToList();
        if (setsWithRir.Count == 0)
        {
            return new ProgressionEvaluationResultDto(
                Status: ProgressionEvaluationStatus.NotEvaluable,
                Reason: "No RIR entries recorded to evaluate effort guideline compliance.",
                SuggestedNextTarget: null);
        }

        // Parse target RIR boundary (e.g., "1-2 RIR" -> min 1, max 2)
        var (targetMinRir, targetMaxRir) = ParseRirGuideline(plannedSlot.EffortGuideline);
        bool inTargetEffortZone = setsWithRir.All(s => s.Rir!.Value >= targetMinRir - 0.5m && s.Rir!.Value <= targetMaxRir + 1.0m);

        if (inTargetEffortZone)
        {
            return new ProgressionEvaluationResultDto(
                Status: ProgressionEvaluationStatus.Met,
                Reason: $"All sets executed within target effort zone of {plannedSlot.EffortGuideline}.",
                SuggestedNextTarget: "Effort target verified. Proceed to next planned overload stimulus.");
        }

        return new ProgressionEvaluationResultDto(
            Status: ProgressionEvaluationStatus.NotMet,
            Reason: $"Effort deviated from planned guideline ({plannedSlot.EffortGuideline}).",
            SuggestedNextTarget: $"Calibrate load to maintain {plannedSlot.EffortGuideline}.");
    }

    private static (int MinReps, int MaxReps) ParseRepRange(string repRange)
    {
        var match = Regex.Match(repRange, @"(\d+)\s*-\s*(\d+)");
        if (match.Success)
        {
            int min = int.Parse(match.Groups[1].Value);
            int max = int.Parse(match.Groups[2].Value);
            return (min, max);
        }

        var singleMatch = Regex.Match(repRange, @"\d+");
        if (singleMatch.Success)
        {
            int val = int.Parse(singleMatch.Value);
            return (val, val);
        }

        return (8, 12); // Default fallback
    }

    private static (decimal MinRir, decimal MaxRir) ParseRirGuideline(string effort)
    {
        var match = Regex.Match(effort, @"(\d+)\s*-\s*(\d+)");
        if (match.Success)
        {
            decimal min = decimal.Parse(match.Groups[1].Value);
            decimal max = decimal.Parse(match.Groups[2].Value);
            return (min, max);
        }

        var single = Regex.Match(effort, @"\d+");
        if (single.Success)
        {
            decimal val = decimal.Parse(single.Value);
            return (val, val);
        }

        return (1.0m, 2.0m);
    }
}
