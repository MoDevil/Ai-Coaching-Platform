using AiCoachOs.Application.Adaptations.Engine;
using AiCoachOs.Application.Programs.Engine;
using AiCoachOs.Application.Workouts.Engine;
using AiCoachOs.Domain.Adaptations;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.TrainingProfiles;
using AiCoachOs.Domain.Workouts;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class AdaptationAnalyzerTests
{
    private readonly IProgressionEvaluator _progressionEvaluator = new ProgressionEvaluator();
    private readonly IExerciseSelector _exerciseSelector = new ExerciseSelector();
    private readonly IConstraintAnalyzer _constraintAnalyzer = new ConstraintAnalyzer();

    private (ProgramVersion Version, ExerciseSlot Slot, Exercise Exercise) CreateTestSetup(int targetSets = 3, string effort = "2 RIR")
    {
        var patternId = Guid.NewGuid();
        var exercise = new Exercise(
            Guid.NewGuid(),
            "Bench Press",
            ExerciseCategory.Compound,
            patternId,
            QualitativeRating.High,
            QualitativeRating.High,
            QualitativeRating.High,
            QualitativeRating.Moderate,
            QualitativeRating.High,
            QualitativeRating.High,
            ResistanceProfile.MidRange);

        var programId = Guid.NewGuid();
        var version = new ProgramVersion(Guid.NewGuid(), programId, 1, RecoveryCapacity.Moderate, "Initial", true);
        var week = new TrainingWeek(Guid.NewGuid(), version.Id, 1);
        var session = new TrainingSession(Guid.NewGuid(), week.Id, 1, "Upper A", "Chest focus", 60);

        var slot = new ExerciseSlot(
            id: Guid.NewGuid(),
            trainingSessionId: session.Id,
            exerciseId: exercise.Id,
            order: 1,
            targetSets: targetSets,
            targetRepRange: "8-12",
            effortGuideline: effort,
            restSeconds: 120,
            selectionRationale: "Horizontal press hypertrophy",
            progressionRule: new ProgressionRule(ProgressionRuleType.LinearLoad, "12 reps", "2.5kg", "Add 2.5kg once top reps reached"));

        session.AddSlot(slot);
        week.AddSession(session);
        version.AddWeek(week);

        return (version, slot, exercise);
    }

    private WorkoutSession CreateWorkout(
        ExerciseSlot slot,
        Guid programVersionId,
        DateTime date,
        int completedSetsCount,
        int targetReps,
        decimal loadKg,
        decimal rir,
        bool completedSession = true,
        string? sessionNotes = null,
        string? exerciseNotes = null)
    {
        var sessionId = Guid.NewGuid();
        var workout = new WorkoutSession(
            id: sessionId,
            clientId: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            startedAtUtc: date,
            programVersionId: programVersionId,
            trainingSessionId: slot.TrainingSessionId,
            notes: sessionNotes);

        var workoutEx = new WorkoutExercise(
            id: Guid.NewGuid(),
            workoutSessionId: sessionId,
            exerciseId: slot.ExerciseId,
            orderInSession: 1,
            exerciseSlotId: slot.Id,
            notes: exerciseNotes);

        for (int i = 1; i <= completedSetsCount; i++)
        {
            workoutEx.AddSet(new WorkoutSet(
                id: Guid.NewGuid(),
                workoutExerciseId: workoutEx.Id,
                setNumber: i,
                repetitions: targetReps,
                loadKg: loadKg,
                rir: rir,
                isCompleted: true));
        }

        workout.AddExercise(workoutEx);
        if (completedSession)
        {
            workout.Complete(date.AddMinutes(50));
        }

        return workout;
    }

    [Fact]
    public void Analyze_WhenExposuresLessThan4_DoesNotConfirmPlateau_AndReturnsInsufficientEvidence()
    {
        var analyzer = new AdaptationAnalyzer(_progressionEvaluator);
        var (version, slot, exercise) = CreateTestSetup();

        // 3 completed exposures where progression was not met
        var workouts = new List<WorkoutSession>
        {
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-10), 3, 9, 80m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-7), 3, 9, 80m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-4), 3, 9, 80m, 2m)
        };

        var assessment = analyzer.Analyze(
            version,
            workouts,
            new[] { exercise },
            null,
            _exerciseSelector,
            _constraintAnalyzer);

        assessment.ExerciseRecords.Should().HaveCount(1);
        var record = assessment.ExerciseRecords.First();
        record.ExposureCount.Should().Be(3);
        record.PlateauConfirmed.Should().BeFalse();
        record.PerformanceTrend.Should().Be(PerformanceTrend.Insufficient);

        // Under 4 exposures with insufficient trend defaults to NoChange / ProgramWorking
        assessment.OverallStatus.Should().Be(AdaptationOverallStatus.ProgramWorking);
        assessment.Recommendations.Should().Contain(r => r.ActionType == AdaptationActionType.NoChange);
    }

    [Fact]
    public void Analyze_When4Exposures_WithHighAdherence_AndStagnantProgressionAtTargetEffort_ConfirmsPlateau()
    {
        var analyzer = new AdaptationAnalyzer(_progressionEvaluator);
        var (version, slot, exercise) = CreateTestSetup(targetSets: 3, effort: "2 RIR");

        // 4 sessions: All completed (100% adherence), target reps 9 (progression requires 12 reps, so NotMet across all 4),
        // honest effort (Actual RIR 2.0 <= Target 2.0 + 1)
        var workouts = new List<WorkoutSession>
        {
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-14), 3, 9, 80m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-10), 3, 9, 80m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-7), 3, 9, 80m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-3), 3, 9, 80m, 2m)
        };

        var assessment = analyzer.Analyze(
            version,
            workouts,
            new[] { exercise },
            null,
            _exerciseSelector,
            _constraintAnalyzer);

        var record = assessment.ExerciseRecords.First();
        record.ExposureCount.Should().Be(4);
        record.PlateauConfirmed.Should().BeTrue();
        record.ProgressionMetCount.Should().Be(0);
        record.EffortAlignmentStatus.Should().Be(EffortAlignmentStatus.Aligned);

        assessment.OverallStatus.Should().Be(AdaptationOverallStatus.ActionRequired);
        assessment.Recommendations.Should().Contain(r => r.ActionType == AdaptationActionType.ChangeExercise);
    }

    [Fact]
    public void Analyze_WhenActualRIR_IsFarAboveTarget_FlagsEffortIssue_NotPlateau()
    {
        var analyzer = new AdaptationAnalyzer(_progressionEvaluator);
        var (version, slot, exercise) = CreateTestSetup(targetSets: 3, effort: "2 RIR");

        // 4 sessions: NotMet progression, but actual RIR is 4.5 (target is 2, so 4.5 > 2 + 1)
        var workouts = new List<WorkoutSession>
        {
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-14), 3, 9, 80m, 4.5m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-10), 3, 9, 80m, 4.5m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-7), 3, 9, 80m, 4.5m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-3), 3, 9, 80m, 4.5m)
        };

        var assessment = analyzer.Analyze(
            version,
            workouts,
            new[] { exercise },
            null,
            _exerciseSelector,
            _constraintAnalyzer);

        var record = assessment.ExerciseRecords.First();
        record.PlateauConfirmed.Should().BeFalse();
        record.EffortAlignmentStatus.Should().Be(EffortAlignmentStatus.HigherThanTarget);

        assessment.Recommendations.Should().Contain(r => r.ActionType == AdaptationActionType.ModifyEffortGuideline);
        assessment.Recommendations.First(r => r.ActionType == AdaptationActionType.ModifyEffortGuideline)
            .Rationale.Should().Contain("effort proximity issue rather than physiological plateau");
    }

    [Fact]
    public void Analyze_SingleBadSession_IsTreatedAsIsolated_NoAdaptationCandidate()
    {
        var analyzer = new AdaptationAnalyzer(_progressionEvaluator);
        var (version, slot, exercise) = CreateTestSetup(targetSets: 3);

        // 3 progressing sessions (load increasing 80 -> 82.5 -> 85), 1 under-executed session
        var workouts = new List<WorkoutSession>
        {
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-14), 3, 12, 80m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-10), 3, 12, 82.5m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-7), 3, 12, 85m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-3), 0, 0, 0m, 2m) // 1 bad / zero session
        };

        var assessment = analyzer.Analyze(
            version,
            workouts,
            new[] { exercise },
            null,
            _exerciseSelector,
            _constraintAnalyzer);

        var record = assessment.ExerciseRecords.First();
        record.PlateauConfirmed.Should().BeFalse();
        // 1 bad session alone does NOT generate coach review recommendation
        assessment.Recommendations.Should().NotContain(r => r.ActionType == AdaptationActionType.CoachReview);
    }

    [Fact]
    public void Analyze_LowAdherence_Under60Percent_DoesNotTriggerPhysiologicalChanges()
    {
        var analyzer = new AdaptationAnalyzer(_progressionEvaluator);
        var (version, slot, exercise) = CreateTestSetup(targetSets: 3);

        // 4 planned sessions, but only 2 completed (50% adherence < 60%)
        var workouts = new List<WorkoutSession>
        {
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-14), 3, 9, 80m, 2m, completedSession: true),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-10), 0, 0, 0m, 0m, completedSession: false), // in progress/abandoned
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-7), 3, 9, 80m, 2m, completedSession: true),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-3), 0, 0, 0m, 0m, completedSession: false)
        };

        var assessment = analyzer.Analyze(
            version,
            workouts,
            new[] { exercise },
            null,
            _exerciseSelector,
            _constraintAnalyzer);

        assessment.AdherenceRate.Should().BeLessThan(60m);
        assessment.OverallStatus.Should().Be(AdaptationOverallStatus.ReviewRecommended);
        assessment.ExerciseRecords.Should().HaveCount(1);
        assessment.ExerciseRecords.First().PerformanceTrend.Should().Be(PerformanceTrend.Insufficient);
        assessment.ExerciseRecords.First().PlateauConfirmed.Should().BeFalse();
        assessment.Recommendations.Should().NotContain(r => r.ActionType == AdaptationActionType.ChangeExercise);
        assessment.Recommendations.Should().NotContain(r => r.ActionType == AdaptationActionType.ModifySets);
        assessment.Recommendations.Should().Contain(r => r.ActionType == AdaptationActionType.CoachReview);
    }

    [Fact]
    public void Analyze_WhenPainOrInjuryNotesPresent_FlagsReferToM8M9_WithoutPhysiologicalChanges()
    {
        var analyzer = new AdaptationAnalyzer(_progressionEvaluator);
        var (version, slot, exercise) = CreateTestSetup(targetSets: 3);

        var workouts = new List<WorkoutSession>
        {
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-10), 3, 10, 80m, 2m, sessionNotes: "Felt sharp pain in anterior shoulder on final rep"),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-7), 3, 10, 80m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-3), 3, 10, 80m, 2m)
        };

        var assessment = analyzer.Analyze(
            version,
            workouts,
            new[] { exercise },
            null,
            _exerciseSelector,
            _constraintAnalyzer);

        assessment.OverallStatus.Should().Be(AdaptationOverallStatus.ActionRequired);
        assessment.Recommendations.Should().Contain(r => r.ActionType == AdaptationActionType.ReferToM8M9);
        var painRec = assessment.Recommendations.First(r => r.ActionType == AdaptationActionType.ReferToM8M9);
        painRec.Rationale.Should().Contain("refer to M8/M9 injury & symptom review");
    }

    [Fact]
    public void Analyze_IsDeterministic_SameInputsProduceExactSameOutputs()
    {
        var analyzer = new AdaptationAnalyzer(_progressionEvaluator);
        var (version, slot, exercise) = CreateTestSetup(targetSets: 3, effort: "2 RIR");

        var workouts = new List<WorkoutSession>
        {
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-14), 3, 9, 80m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-10), 3, 9, 80m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-7), 3, 9, 80m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-3), 3, 9, 80m, 2m)
        };

        var assessment1 = analyzer.Analyze(version, workouts, new[] { exercise }, null, _exerciseSelector, _constraintAnalyzer);
        var assessment2 = analyzer.Analyze(version, workouts, new[] { exercise }, null, _exerciseSelector, _constraintAnalyzer);

        assessment1.OverallStatus.Should().Be(assessment2.OverallStatus);
        assessment1.AdherenceRate.Should().Be(assessment2.AdherenceRate);
        assessment1.ExerciseRecords.Count.Should().Be(assessment2.ExerciseRecords.Count);
        assessment1.ExerciseRecords.First().PlateauConfirmed.Should().Be(assessment2.ExerciseRecords.First().PlateauConfirmed);
        assessment1.ExerciseRecords.First().PerformanceTrend.Should().Be(assessment2.ExerciseRecords.First().PerformanceTrend);
        assessment1.Recommendations.Count.Should().Be(assessment2.Recommendations.Count);
        assessment1.Recommendations.First().ActionType.Should().Be(assessment2.Recommendations.First().ActionType);
        assessment1.Recommendations.First().Rationale.Should().Be(assessment2.Recommendations.First().Rationale);
    }

    [Fact]
    public void Analyze_VolumeIncrease_WhenSessionHasNoDurationRoom_IsBlocked()
    {
        var analyzer = new AdaptationAnalyzer(_progressionEvaluator);
        var (version, slot, exercise) = CreateTestSetup(targetSets: 3, effort: "2 RIR");

        // Set up profile with strict session duration cap equal to current duration (60 min)
        var availability = new TrainingAvailability(4, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday });
        var profile = new ClientTrainingProfile(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TrainingExperienceLevel.Intermediate,
            availability,
            sessionDurationMinMinutes: 45,
            sessionDurationTargetMinutes: 60,
            sessionDurationMaxMinutes: 60);

        // 4 sessions: all sets completed, improving load (70 -> 72.5 -> 75 -> 77.5)
        var workouts = new List<WorkoutSession>
        {
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-14), 3, 12, 70m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-10), 3, 12, 72.5m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-7), 3, 12, 75m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-3), 3, 12, 77.5m, 2m)
        };

        var assessment = analyzer.Analyze(version, workouts, new[] { exercise }, profile, _exerciseSelector, _constraintAnalyzer);

        var record = assessment.ExerciseRecords.First();
        record.PerformanceTrend.Should().Be(PerformanceTrend.Improving);
        // Because session duration cap is 60 min and current estimated duration is 60 min, +1 set exceeds room
        assessment.Recommendations.Should().NotContain(r => r.ActionType == AdaptationActionType.ModifySets && r.SuggestedChangeDetail != null && r.SuggestedChangeDetail.Contains("TargetSets:4"));
    }

    [Fact]
    public void Analyze_VolumeIncrease_WhenSessionHasRoom_RecommendsPlusOneSet()
    {
        var analyzer = new AdaptationAnalyzer(_progressionEvaluator);
        var (version, slot, exercise) = CreateTestSetup(targetSets: 3, effort: "2 RIR");

        // Profile with generous duration limit (75 min)
        var availability = new TrainingAvailability(4, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday });
        var profile = new ClientTrainingProfile(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TrainingExperienceLevel.Intermediate,
            availability,
            sessionDurationMinMinutes: 45,
            sessionDurationTargetMinutes: 60,
            sessionDurationMaxMinutes: 75);

        // 4 sessions: all sets completed, improving load (70 -> 72.5 -> 75 -> 77.5)
        var workouts = new List<WorkoutSession>
        {
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-14), 3, 12, 70m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-10), 3, 12, 72.5m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-7), 3, 12, 75m, 2m),
            CreateWorkout(slot, version.Id, DateTime.UtcNow.AddDays(-3), 3, 12, 77.5m, 2m)
        };

        var assessment = analyzer.Analyze(version, workouts, new[] { exercise }, profile, _exerciseSelector, _constraintAnalyzer);

        var record = assessment.ExerciseRecords.First();
        record.PerformanceTrend.Should().Be(PerformanceTrend.Improving);
        assessment.Recommendations.Should().Contain(r => r.ActionType == AdaptationActionType.ModifySets && r.SuggestedChangeDetail == "TargetSets:4");
    }
}
