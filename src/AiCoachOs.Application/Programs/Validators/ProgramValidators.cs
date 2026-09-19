using FluentValidation;
using AiCoachOs.Application.Programs.DTOs;

namespace AiCoachOs.Application.Programs.Validators;

public class GenerateProgramRequestValidator : AbstractValidator<GenerateProgramRequestDto>
{
    public GenerateProgramRequestValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId is required.");

        RuleFor(x => x.NumberOfWeeks)
            .InclusiveBetween(1, 12).WithMessage("Number of weeks must be between 1 and 12.");

        RuleFor(x => x.ProgramName)
            .MaximumLength(150).WithMessage("Program name cannot exceed 150 characters.");

        RuleFor(x => x.CoachNotes)
            .MaximumLength(1000).WithMessage("Coach notes cannot exceed 1000 characters.");
    }
}

public class UpdateProgramStatusValidator : AbstractValidator<UpdateProgramStatusDto>
{
    public UpdateProgramStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("A valid ProgramStatus must be provided.");
    }
}
