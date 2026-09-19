using AiCoachOs.Application.Clients.DTOs;
using FluentValidation;

namespace AiCoachOs.Application.Clients.Validators;

public class CreateClientRequestValidator : AbstractValidator<CreateClientRequestDto>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone cannot exceed 30 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Goal!.PrimaryGoal)
            .NotEmpty().WithMessage("Primary goal cannot be empty if goal is specified.")
            .When(x => x.Goal != null);

        RuleFor(x => x.Goal!.TargetTimelineWeeks)
            .GreaterThan(0).WithMessage("Target timeline weeks must be greater than 0.")
            .When(x => x.Goal != null && x.Goal.TargetTimelineWeeks.HasValue);
    }
}

public class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequestDto>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone cannot exceed 30 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Goal!.PrimaryGoal)
            .NotEmpty().WithMessage("Primary goal cannot be empty if goal is specified.")
            .When(x => x.Goal != null);

        RuleFor(x => x.Goal!.TargetTimelineWeeks)
            .GreaterThan(0).WithMessage("Target timeline weeks must be greater than 0.")
            .When(x => x.Goal != null && x.Goal.TargetTimelineWeeks.HasValue);
    }
}
