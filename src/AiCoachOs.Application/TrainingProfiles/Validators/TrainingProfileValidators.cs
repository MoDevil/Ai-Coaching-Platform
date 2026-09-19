using AiCoachOs.Application.TrainingProfiles.DTOs;
using FluentValidation;

namespace AiCoachOs.Application.TrainingProfiles.Validators;

public class TrainingAvailabilityDtoValidator : AbstractValidator<TrainingAvailabilityDto>
{
    public TrainingAvailabilityDtoValidator()
    {
        RuleFor(x => x.SessionsPerWeek)
            .InclusiveBetween(1, 7)
            .WithMessage("Sessions per week must be between 1 and 7.");

        RuleFor(x => x.AvailableDays)
            .NotNull()
            .WithMessage("Available days cannot be null.");

        RuleFor(x => x.PreferredDays)
            .NotNull()
            .WithMessage("Preferred days cannot be null.");
    }
}

public class ClientTrainingPriorityDtoValidator : AbstractValidator<ClientTrainingPriorityDto>
{
    public ClientTrainingPriorityDtoValidator()
    {
        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Priority order must be at least 1.");

        RuleFor(x => x.FocusArea)
            .NotEmpty()
            .WithMessage("Priority focus area is required.")
            .MaximumLength(100)
            .WithMessage("Priority focus area cannot exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class UpdateTrainingProfileRequestValidator : AbstractValidator<UpdateTrainingProfileRequestDto>
{
    public UpdateTrainingProfileRequestValidator()
    {
        RuleFor(x => x.ExperienceLevel)
            .IsInEnum()
            .WithMessage("Valid training experience level is required.");

        RuleFor(x => x.SessionDurationMinMinutes)
            .InclusiveBetween(15, 240)
            .When(x => x.SessionDurationMinMinutes.HasValue)
            .WithMessage("Minimum session duration must be between 15 and 240 minutes.");

        RuleFor(x => x.SessionDurationTargetMinutes)
            .InclusiveBetween(15, 240)
            .When(x => x.SessionDurationTargetMinutes.HasValue)
            .WithMessage("Target session duration must be between 15 and 240 minutes.");

        RuleFor(x => x.SessionDurationMaxMinutes)
            .InclusiveBetween(15, 240)
            .When(x => x.SessionDurationMaxMinutes.HasValue)
            .WithMessage("Maximum session duration must be between 15 and 240 minutes.");

        RuleFor(x => x)
            .Must(x => !x.SessionDurationMinMinutes.HasValue || !x.SessionDurationTargetMinutes.HasValue ||
                       x.SessionDurationMinMinutes.Value <= x.SessionDurationTargetMinutes.Value)
            .WithMessage("Minimum session duration cannot exceed target duration.");

        RuleFor(x => x)
            .Must(x => !x.SessionDurationTargetMinutes.HasValue || !x.SessionDurationMaxMinutes.HasValue ||
                       x.SessionDurationTargetMinutes.Value <= x.SessionDurationMaxMinutes.Value)
            .WithMessage("Target session duration cannot exceed maximum duration.");

        RuleFor(x => x)
            .Must(x => !x.SessionDurationMinMinutes.HasValue || !x.SessionDurationMaxMinutes.HasValue ||
                       x.SessionDurationMinMinutes.Value <= x.SessionDurationMaxMinutes.Value)
            .WithMessage("Minimum session duration cannot exceed maximum duration.");

        RuleFor(x => x.WeeklyAvailability)
            .NotNull()
            .WithMessage("Weekly availability configuration is required.")
            .SetValidator(new TrainingAvailabilityDtoValidator());

        RuleForEach(x => x.Priorities)
            .SetValidator(new ClientTrainingPriorityDtoValidator())
            .When(x => x.Priorities != null);

        RuleFor(x => x.ExercisePreferences)
            .MaximumLength(2000)
            .WithMessage("Exercise preferences cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.ExercisePreferences));

        RuleFor(x => x.ExerciseConstraints)
            .MaximumLength(2000)
            .WithMessage("Exercise constraints cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.ExerciseConstraints));
    }
}
