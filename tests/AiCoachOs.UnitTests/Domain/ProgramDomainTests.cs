using AiCoachOs.Domain.Programs;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain;

public class ProgramDomainTests
{
    [Fact]
    public void CreateProgram_WithValidData_ShouldInitializeCorrectly()
    {
        // Arrange
        var programId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var goalSnapshot = new GoalSnapshot(PrimaryGoalType.Hypertrophy, PrimaryGoalType.Strength, "Upper body focus", 12);

        // Act
        var program = new Program(
            id: programId,
            clientId: clientId,
            coachId: coachId,
            name: "Hypertrophy Phase 1",
            goalSnapshot: goalSnapshot,
            rationaleSummary: "Built for upper body hypertrophy with 4 sessions per week.",
            status: ProgramStatus.Draft);

        // Assert
        program.Id.Should().Be(programId);
        program.ClientId.Should().Be(clientId);
        program.CoachId.Should().Be(coachId);
        program.Name.Should().Be("Hypertrophy Phase 1");
        program.GoalSnapshot.PrimaryGoal.Should().Be(PrimaryGoalType.Hypertrophy);
        program.GoalSnapshot.SecondaryGoal.Should().Be(PrimaryGoalType.Strength);
        program.Status.Should().Be(ProgramStatus.Draft);
        program.RationaleSummary.Should().Contain("upper body hypertrophy");
        program.Versions.Should().BeEmpty();
    }

    [Fact]
    public void Program_AddVersion_ShouldSetPreviousVersionsInactive()
    {
        // Arrange
        var program = new Program(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Progression Program",
            new GoalSnapshot(PrimaryGoalType.Strength),
            "Strength cycle",
            ProgramStatus.Active);

        var v1 = new ProgramVersion(Guid.NewGuid(), program.Id, 1, RecoveryCapacity.Moderate, "Initial", true);
        var v2 = new ProgramVersion(Guid.NewGuid(), program.Id, 2, RecoveryCapacity.Moderate, "Volume increment", true);

        // Act
        program.AddVersion(v1);
        program.AddVersion(v2);

        // Assert
        v1.IsActive.Should().BeFalse();
        v2.IsActive.Should().BeTrue();
        program.GetActiveVersion()?.Id.Should().Be(v2.Id);
        program.Versions.Should().HaveCount(2);
    }

    [Fact]
    public void TrainingSession_AddSlot_ShouldMaintainSlots()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new TrainingSession(
            id: sessionId,
            trainingWeekId: Guid.NewGuid(),
            dayNumber: 1,
            name: "Lower / Push Day",
            sessionIntent: "Knee dominant and chest focus",
            estimatedDurationMinutes: 60,
            dayOfWeek: DayOfWeek.Monday);

        var slot = new ExerciseSlot(
            id: Guid.NewGuid(),
            trainingSessionId: sessionId,
            exerciseId: Guid.NewGuid(),
            order: 1,
            targetSets: 3,
            targetRepRange: "6-10",
            effortGuideline: "1-2 RIR",
            restSeconds: 120,
            selectionRationale: "Compound quad driver",
            progressionRule: new ProgressionRule(ProgressionRuleType.LinearLoad, "6-10 reps @ 1-2 RIR", "+2.5 kg"));

        // Act
        session.AddSlot(slot);

        // Assert
        session.Slots.Should().HaveCount(1);
        session.Slots.First().Order.Should().Be(1);
        session.Slots.First().ProgressionRule?.Type.Should().Be(ProgressionRuleType.LinearLoad);
    }
}
