using AiCoachOs.Application.Workouts.Engine;
using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.Workouts;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class ProgressionEvaluatorTests
{
    private readonly ProgressionEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_WhenNoProgressionRule_ReturnsNotEvaluable()
    {
        var slot = new ExerciseSlot(
            id: Guid.NewGuid(),
            trainingSessionId: Guid.NewGuid(),
            exerciseId: Guid.NewGuid(),
            order: 1,
            targetSets: 3,
            targetRepRange: "8-12",
            effortGuideline: "2 RIR",
            restSeconds: 120,
            selectionRationale: "Hypertrophy",
            progressionRule: null);

        var sets = new List<WorkoutSet>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), 1, 10, 80m, 2m)
        };

        var result = _evaluator.Evaluate(slot, sets);

        result.Status.Should().Be(ProgressionEvaluationStatus.NotEvaluable);
        result.Reason.Should().Contain("No planned progression rule defined");
    }

    [Fact]
    public void Evaluate_WhenSetsIncomplete_ReturnsIncomplete()
    {
        var slot = new ExerciseSlot(
            id: Guid.NewGuid(),
            trainingSessionId: Guid.NewGuid(),
            exerciseId: Guid.NewGuid(),
            order: 1,
            targetSets: 3,
            targetRepRange: "8-12",
            effortGuideline: "2 RIR",
            restSeconds: 120,
            selectionRationale: "Hypertrophy",
            progressionRule: new ProgressionRule(ProgressionRuleType.LinearLoad, "12 reps", "2.5kg", "Add 2.5kg once 12 reps reached"));

        var sets = new List<WorkoutSet>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), 1, 12, 100m, 2m),
            new(Guid.NewGuid(), Guid.NewGuid(), 2, 12, 100m, 2m)
        }; // only 2 completed of 3 planned

        var result = _evaluator.Evaluate(slot, sets);

        result.Status.Should().Be(ProgressionEvaluationStatus.Incomplete);
        result.Reason.Should().Contain("Completed 2 of 3 planned sets");
    }

    [Fact]
    public void Evaluate_LinearLoad_WhenAllSetsHitTopReps_ReturnsMetWithNextTarget()
    {
        var slot = new ExerciseSlot(
            id: Guid.NewGuid(),
            trainingSessionId: Guid.NewGuid(),
            exerciseId: Guid.NewGuid(),
            order: 1,
            targetSets: 3,
            targetRepRange: "8-12",
            effortGuideline: "2 RIR",
            restSeconds: 120,
            selectionRationale: "Hypertrophy",
            progressionRule: new ProgressionRule(ProgressionRuleType.LinearLoad, "12 reps", "2.5kg", "Add 2.5kg once top of rep range achieved"));

        var sets = new List<WorkoutSet>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), 1, 12, 100m, 2m),
            new(Guid.NewGuid(), Guid.NewGuid(), 2, 12, 100m, 2m),
            new(Guid.NewGuid(), Guid.NewGuid(), 3, 12, 100m, 2m)
        };

        var result = _evaluator.Evaluate(slot, sets);

        result.Status.Should().Be(ProgressionEvaluationStatus.Met);
        result.Reason.Should().Contain("All 3 sets reached upper target boundary of 12 reps");
        result.SuggestedNextTarget.Should().Contain("Advance load by 2.5kg");
    }

    [Fact]
    public void Evaluate_LinearLoad_WhenNotAllSetsHitTopReps_ReturnsNotMet()
    {
        var slot = new ExerciseSlot(
            id: Guid.NewGuid(),
            trainingSessionId: Guid.NewGuid(),
            exerciseId: Guid.NewGuid(),
            order: 1,
            targetSets: 3,
            targetRepRange: "8-12",
            effortGuideline: "2 RIR",
            restSeconds: 120,
            selectionRationale: "Hypertrophy",
            progressionRule: new ProgressionRule(ProgressionRuleType.LinearLoad, "12 reps", "2.5kg", "Add 2.5kg once top of rep range achieved"));

        var sets = new List<WorkoutSet>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), 1, 12, 100m, 2m),
            new(Guid.NewGuid(), Guid.NewGuid(), 2, 11, 100m, 2m),
            new(Guid.NewGuid(), Guid.NewGuid(), 3, 10, 100m, 1m)
        };

        var result = _evaluator.Evaluate(slot, sets);

        result.Status.Should().Be(ProgressionEvaluationStatus.NotMet);
        result.Reason.Should().Contain("falling below top threshold of 12 reps");
        result.SuggestedNextTarget.Should().Contain("Maintain current load (100 kg)");
    }

    [Fact]
    public void Evaluate_RepTarget_WhenAllSetsHitTopReps_ReturnsMet()
    {
        var slot = new ExerciseSlot(
            id: Guid.NewGuid(),
            trainingSessionId: Guid.NewGuid(),
            exerciseId: Guid.NewGuid(),
            order: 1,
            targetSets: 2,
            targetRepRange: "10-15",
            effortGuideline: "1-2 RIR",
            restSeconds: 90,
            selectionRationale: "Lateral delts",
            progressionRule: new ProgressionRule(ProgressionRuleType.RepTarget, "15 reps", "1 rep", "Hit 15 reps on all sets"));

        var sets = new List<WorkoutSet>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), 1, 15, 12m, 1m),
            new(Guid.NewGuid(), Guid.NewGuid(), 2, 15, 12m, 1m)
        };

        var result = _evaluator.Evaluate(slot, sets);

        result.Status.Should().Be(ProgressionEvaluationStatus.Met);
        result.Reason.Should().Contain("Upper rep threshold of 15 reps achieved across all sets");
    }

    [Fact]
    public void Evaluate_RirTarget_WhenEffortWithinGuideline_ReturnsMet()
    {
        var slot = new ExerciseSlot(
            id: Guid.NewGuid(),
            trainingSessionId: Guid.NewGuid(),
            exerciseId: Guid.NewGuid(),
            order: 1,
            targetSets: 2,
            targetRepRange: "6-10",
            effortGuideline: "1-2 RIR",
            restSeconds: 120,
            selectionRationale: "Compound press",
            progressionRule: new ProgressionRule(ProgressionRuleType.RirTarget, "1-2 RIR", "Increase load once 2 RIR achieved easily"));

        var sets = new List<WorkoutSet>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), 1, 8, 90m, 2m),
            new(Guid.NewGuid(), Guid.NewGuid(), 2, 7, 90m, 1m)
        };

        var result = _evaluator.Evaluate(slot, sets);

        result.Status.Should().Be(ProgressionEvaluationStatus.Met);
        result.Reason.Should().Contain("All sets executed within target effort zone");
    }
}
