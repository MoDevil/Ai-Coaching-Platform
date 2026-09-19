using AiCoachOs.Domain.Workouts;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain;

public class WorkoutSessionDomainTests
{
    [Fact]
    public void CreateWorkoutSession_WithValidData_ShouldInitializeCorrectly()
    {
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;

        var session = new WorkoutSession(id, clientId, coachId, startedAt, notes: "Leg day focus");

        session.Id.Should().Be(id);
        session.ClientId.Should().Be(clientId);
        session.CoachId.Should().Be(coachId);
        session.StartedAtUtc.Should().Be(startedAt);
        session.Status.Should().Be(WorkoutStatus.InProgress);
        session.Notes.Should().Be("Leg day focus");
        session.Exercises.Should().BeEmpty();
    }

    [Fact]
    public void CompleteWorkout_ShouldSetStatusCompletedAndTimestamp()
    {
        var session = new WorkoutSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(-1));
        var completedAt = DateTime.UtcNow;

        session.Complete(completedAt, "Felt energetic throughout");

        session.Status.Should().Be(WorkoutStatus.Completed);
        session.CompletedAtUtc.Should().Be(completedAt);
        session.Notes.Should().Contain("Felt energetic throughout");
    }

    [Fact]
    public void CompleteWorkout_WhenAlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        var session = new WorkoutSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(-1));
        session.Complete(DateTime.UtcNow);

        var act = () => session.Complete(DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot complete a workout that is already Completed*");
    }

    [Fact]
    public void WorkoutExercise_AddSet_ShouldMaintainSetOrder()
    {
        var exerciseId = Guid.NewGuid();
        var workoutSessionId = Guid.NewGuid();
        var workoutExercise = new WorkoutExercise(Guid.NewGuid(), workoutSessionId, exerciseId, orderInSession: 1);

        var set1 = new WorkoutSet(Guid.NewGuid(), workoutExercise.Id, 1, 10, 100m, 2m);
        var set2 = new WorkoutSet(Guid.NewGuid(), workoutExercise.Id, 2, 8, 100m, 1m);

        workoutExercise.AddSet(set2);
        workoutExercise.AddSet(set1);

        workoutExercise.Sets.Should().HaveCount(2);
        workoutExercise.Sets.First().SetNumber.Should().Be(1);
        workoutExercise.Sets.Last().SetNumber.Should().Be(2);
    }

    [Fact]
    public void WorkoutSet_UpdatePerformance_ShouldValidateInvariants()
    {
        var set = new WorkoutSet(Guid.NewGuid(), Guid.NewGuid(), 1, 10, 80m, 2m);

        set.UpdatePerformance(12, 85m, 1m, true, "Form was crisp");

        set.Repetitions.Should().Be(12);
        set.LoadKg.Should().Be(85m);
        set.Rir.Should().Be(1m);
        set.Notes.Should().Be("Form was crisp");

        var actNegativeLoad = () => set.UpdatePerformance(10, -5m, 2m, true, null);
        actNegativeLoad.Should().Throw<ArgumentOutOfRangeException>();

        var actInvalidRir = () => set.UpdatePerformance(10, 50m, 12m, true, null);
        actInvalidRir.Should().Throw<ArgumentOutOfRangeException>();
    }
}
