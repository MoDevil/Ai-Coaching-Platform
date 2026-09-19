using FluentValidation;
using AiCoachOs.Application.Workouts.DTOs;

namespace AiCoachOs.Application.Workouts.Validators;

public class StartWorkoutRequestValidator : AbstractValidator<StartWorkoutRequestDto>
{
    public StartWorkoutRequestValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public class AddWorkoutExerciseRequestValidator : AbstractValidator<AddWorkoutExerciseRequestDto>
{
    public AddWorkoutExerciseRequestValidator()
    {
        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("ExerciseId is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
    }
}

public class RecordWorkoutSetRequestValidator : AbstractValidator<RecordWorkoutSetRequestDto>
{
    public RecordWorkoutSetRequestValidator()
    {
        RuleFor(x => x.SetNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Set number must be at least 1.");

        RuleFor(x => x.Repetitions)
            .GreaterThanOrEqualTo(0).WithMessage("Repetitions cannot be negative.");

        RuleFor(x => x.LoadKg)
            .GreaterThanOrEqualTo(0).WithMessage("Load cannot be negative.");

        RuleFor(x => x.Rir)
            .InclusiveBetween(0, 10)
            .When(x => x.Rir.HasValue)
            .WithMessage("RIR must be between 0 and 10.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
    }
}

public class UpdateWorkoutSetRequestValidator : AbstractValidator<UpdateWorkoutSetRequestDto>
{
    public UpdateWorkoutSetRequestValidator()
    {
        RuleFor(x => x.Repetitions)
            .GreaterThanOrEqualTo(0).WithMessage("Repetitions cannot be negative.");

        RuleFor(x => x.LoadKg)
            .GreaterThanOrEqualTo(0).WithMessage("Load cannot be negative.");

        RuleFor(x => x.Rir)
            .InclusiveBetween(0, 10)
            .When(x => x.Rir.HasValue)
            .WithMessage("RIR must be between 0 and 10.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
    }
}

public class CompleteWorkoutRequestValidator : AbstractValidator<CompleteWorkoutRequestDto>
{
    public CompleteWorkoutRequestValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}
