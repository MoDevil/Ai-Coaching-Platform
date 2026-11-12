using AiCoachOs.Domain.Programs;

namespace AiCoachOs.Application.Programs.Engine;

public record GoalTrainingParameters(
    PrimaryGoalType PrimaryGoal,
    string CompoundRepRange,
    string IsolationRepRange,
    string EffortGuideline, // RIR guideline
    int CompoundRestSeconds,
    int IsolationRestSeconds,
    int BaselineSetsPerExercise,
    int PriorityBonusSets);

public interface IGoalAnalyzer
{
    GoalSnapshot ParseGoal(string? clientGoalText, int? timelineWeeks, string? coachNotes);
    GoalTrainingParameters DeriveParameters(GoalSnapshot goalSnapshot);
}

public class GoalAnalyzer : IGoalAnalyzer
{
    public GoalSnapshot ParseGoal(string? clientGoalText, int? timelineWeeks, string? coachNotes)
    {
        var primaryGoal = PrimaryGoalType.GeneralFitness;
        PrimaryGoalType? secondaryGoal = null;
        string? emphasis = null;

        var text = (clientGoalText ?? string.Empty).ToLowerInvariant();
        var notes = (coachNotes ?? string.Empty).ToLowerInvariant();
        var combined = $"{text} {notes}";

        if (combined.Contains("fat loss") || combined.Contains("lose fat") || combined.Contains("cut") || combined.Contains("weight loss"))
        {
            primaryGoal = PrimaryGoalType.FatLoss;
            secondaryGoal = PrimaryGoalType.Hypertrophy; // Preserve muscle during deficit
        }
        else if (combined.Contains("recomp") || combined.Contains("recomposition"))
        {
            primaryGoal = PrimaryGoalType.Recomposition;
            secondaryGoal = PrimaryGoalType.Hypertrophy;
        }
        else if (combined.Contains("hypertrophy") || combined.Contains("muscle") || combined.Contains("build mass") || combined.Contains("bodybuilding"))
        {
            primaryGoal = PrimaryGoalType.Hypertrophy;
            if (combined.Contains("strength") || combined.Contains("power"))
            {
                secondaryGoal = PrimaryGoalType.Strength;
            }
        }
        else if (combined.Contains("strength") || combined.Contains("powerlifting") || combined.Contains("lift heavy"))
        {
            primaryGoal = PrimaryGoalType.Strength;
            if (combined.Contains("hypertrophy") || combined.Contains("muscle"))
            {
                secondaryGoal = PrimaryGoalType.Hypertrophy;
            }
        }

        if (combined.Contains("upper body") || combined.Contains("arms") || combined.Contains("chest") || combined.Contains("back") || combined.Contains("legs") || combined.Contains("glutes"))
        {
            emphasis = clientGoalText;
        }

        return new GoalSnapshot(primaryGoal, secondaryGoal, emphasis, timelineWeeks);
    }

    public GoalTrainingParameters DeriveParameters(GoalSnapshot goalSnapshot)
    {
        // Evidence-based rest periods: Schoenfeld et al. 2016 showed 3+ min rest
        // produces significantly greater hypertrophy and strength gains vs 1 min.
        // Belghamdi templates prescribe 3~5 min universally.
        return goalSnapshot.PrimaryGoal switch
        {
            PrimaryGoalType.Strength => new GoalTrainingParameters(
                PrimaryGoal: PrimaryGoalType.Strength,
                CompoundRepRange: "3~6",
                IsolationRepRange: "6~10",
                EffortGuideline: "2-3 RIR (compounds), 1-2 RIR (isolations)",
                CompoundRestSeconds: 300,   // 5 min — heavy strength work
                IsolationRestSeconds: 210,  // 3~5 min
                BaselineSetsPerExercise: 2,
                PriorityBonusSets: 1),

            PrimaryGoalType.Hypertrophy => new GoalTrainingParameters(
                PrimaryGoal: PrimaryGoalType.Hypertrophy,
                CompoundRepRange: "5~8",
                IsolationRepRange: "6~10",
                EffortGuideline: "1-2 RIR",
                CompoundRestSeconds: 270,   // 3~5 min
                IsolationRestSeconds: 210,  // 3~5 min
                BaselineSetsPerExercise: 2,
                PriorityBonusSets: 1),

            PrimaryGoalType.FatLoss => new GoalTrainingParameters(
                PrimaryGoal: PrimaryGoalType.FatLoss,
                CompoundRepRange: "6~10",
                IsolationRepRange: "8~12",
                EffortGuideline: "2 RIR",
                CompoundRestSeconds: 210,   // 3~5 min — preserve strength in deficit
                IsolationRestSeconds: 180,  // 3 min
                BaselineSetsPerExercise: 2,
                PriorityBonusSets: 0),

            PrimaryGoalType.Recomposition => new GoalTrainingParameters(
                PrimaryGoal: PrimaryGoalType.Recomposition,
                CompoundRepRange: "5~8",
                IsolationRepRange: "6~10",
                EffortGuideline: "1-2 RIR",
                CompoundRestSeconds: 270,   // 3~5 min
                IsolationRestSeconds: 210,  // 3~5 min
                BaselineSetsPerExercise: 2,
                PriorityBonusSets: 1),

            _ => new GoalTrainingParameters(
                PrimaryGoal: PrimaryGoalType.GeneralFitness,
                CompoundRepRange: "6~10",
                IsolationRepRange: "8~12",
                EffortGuideline: "2-3 RIR",
                CompoundRestSeconds: 210,   // 3~5 min
                IsolationRestSeconds: 180,  // 3 min
                BaselineSetsPerExercise: 2,
                PriorityBonusSets: 1)
        };
    }
}
