using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Application.TrainingProfiles.Validators;
using AiCoachOs.Domain.TrainingProfiles;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class TrainingProfileValidatorTests
{
    private readonly UpdateTrainingProfileRequestValidator _profileValidator = new();
    private readonly TrainingAvailabilityDtoValidator _availabilityValidator = new();
    private readonly ClientTrainingPriorityDtoValidator _priorityValidator = new();

    [Fact]
    public void AvailabilityValidator_WithValidDto_ShouldBeValid()
    {
        var dto = new TrainingAvailabilityDto(4, new[] { DayOfWeek.Monday, DayOfWeek.Wednesday }, new[] { DayOfWeek.Monday });
        var result = _availabilityValidator.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public void AvailabilityValidator_WithInvalidSessions_ShouldFail(int sessions)
    {
        var dto = new TrainingAvailabilityDto(sessions, new[] { DayOfWeek.Monday }, Array.Empty<DayOfWeek>());
        var result = _availabilityValidator.Validate(dto);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PriorityValidator_WithValidData_ShouldBeValid()
    {
        var dto = new ClientTrainingPriorityDto(null, 1, "Hamstrings", "Lengthened position");
        var result = _priorityValidator.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PriorityValidator_WithEmptyFocusArea_ShouldFail()
    {
        var dto = new ClientTrainingPriorityDto(null, 1, "", null);
        var result = _priorityValidator.Validate(dto);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ProfileValidator_WithValidRequest_ShouldBeValid()
    {
        var request = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Intermediate,
            SessionDurationMinMinutes: 45,
            SessionDurationTargetMinutes: 60,
            SessionDurationMaxMinutes: 90,
            WeeklyAvailability: new TrainingAvailabilityDto(3, new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }, new[] { DayOfWeek.Monday }),
            AvailableEquipmentIds: new List<Guid> { Guid.NewGuid() },
            ExercisePreferences: "Squat enthusiast",
            ExerciseConstraints: null,
            Priorities: new List<ClientTrainingPriorityDto>
            {
                new(null, 1, "Quad Growth", null)
            }
        );

        var result = _profileValidator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ProfileValidator_WhenMinDurationExceedsTarget_ShouldFail()
    {
        var request = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Beginner,
            SessionDurationMinMinutes: 75,
            SessionDurationTargetMinutes: 60,
            SessionDurationMaxMinutes: 90,
            WeeklyAvailability: new TrainingAvailabilityDto(3, Array.Empty<DayOfWeek>(), Array.Empty<DayOfWeek>()),
            AvailableEquipmentIds: null,
            ExercisePreferences: null,
            ExerciseConstraints: null,
            Priorities: null
        );

        var result = _profileValidator.Validate(request);
        result.IsValid.Should().BeFalse();
    }
}
