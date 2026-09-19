using AiCoachOs.Domain.TrainingProfiles;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain;

public class ClientTrainingProfileTests
{
    [Fact]
    public void CreateProfile_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var availability = new TrainingAvailability(4, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday });

        // Act
        var profile = new ClientTrainingProfile(
            id: id,
            clientId: clientId,
            experienceLevel: TrainingExperienceLevel.Intermediate,
            weeklyAvailability: availability,
            sessionDurationMinMinutes: 45,
            sessionDurationTargetMinutes: 60,
            sessionDurationMaxMinutes: 75,
            exercisePreferences: "Prefers compound lifts",
            exerciseConstraints: "Avoid behind-the-neck presses"
        );

        // Assert
        profile.Id.Should().Be(id);
        profile.ClientId.Should().Be(clientId);
        profile.ExperienceLevel.Should().Be(TrainingExperienceLevel.Intermediate);
        profile.SessionDurationMinMinutes.Should().Be(45);
        profile.SessionDurationTargetMinutes.Should().Be(60);
        profile.SessionDurationMaxMinutes.Should().Be(75);
        profile.WeeklyAvailability.SessionsPerWeek.Should().Be(4);
        profile.ExercisePreferences.Should().Be("Prefers compound lifts");
        profile.ExerciseConstraints.Should().Be("Avoid behind-the-neck presses");
        profile.Priorities.Should().BeEmpty();
    }

    [Fact]
    public void CreateProfile_WhenMinDurationExceedsTarget_ShouldThrowArgumentException()
    {
        // Arrange
        var availability = new TrainingAvailability(3);

        // Act
        var act = () => new ClientTrainingProfile(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            experienceLevel: TrainingExperienceLevel.Beginner,
            weeklyAvailability: availability,
            sessionDurationMinMinutes: 75,
            sessionDurationTargetMinutes: 60,
            sessionDurationMaxMinutes: 90
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateProfile_WhenTargetDurationExceedsMax_ShouldThrowArgumentException()
    {
        // Arrange
        var availability = new TrainingAvailability(3);

        // Act
        var act = () => new ClientTrainingProfile(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            experienceLevel: TrainingExperienceLevel.Beginner,
            weeklyAvailability: availability,
            sessionDurationMinMinutes: 45,
            sessionDurationTargetMinutes: 90,
            sessionDurationMaxMinutes: 60
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetPriorities_ShouldSetOrderedList()
    {
        // Arrange
        var profile = new ClientTrainingProfile(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            experienceLevel: TrainingExperienceLevel.Intermediate,
            weeklyAvailability: new TrainingAvailability(3)
        );

        var priorities = new List<(int Order, string FocusArea, string? Notes)>
        {
            (2, "Upper Chest Hypertrophy", "Incline work"),
            (1, "Squat Technique & Depth", "Prioritize mobility"),
            (3, "Triceps Long Head", null)
        };

        // Act
        profile.SetPriorities(priorities);

        // Assert
        profile.Priorities.Should().HaveCount(3);
        var ordered = profile.Priorities.ToList();
        ordered[0].Order.Should().Be(1);
        ordered[0].FocusArea.Should().Be("Squat Technique & Depth");
        ordered[1].Order.Should().Be(2);
        ordered[1].FocusArea.Should().Be("Upper Chest Hypertrophy");
        ordered[2].Order.Should().Be(3);
        ordered[2].FocusArea.Should().Be("Triceps Long Head");
    }

    [Fact]
    public void Priority_WithInvalidOrder_ShouldThrowException()
    {
        // Act
        var act = () => new ClientTrainingPriority(Guid.NewGuid(), Guid.NewGuid(), 0, "Chest");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public void TrainingAvailability_WithInvalidSessionsPerWeek_ShouldThrowException(int sessions)
    {
        // Act
        var act = () => new TrainingAvailability(sessions);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
