using AiCoachOs.Application.AnatomyAndBiomechanics.DTOs;
using FluentValidation;

namespace AiCoachOs.Application.AnatomyAndBiomechanics.Validators;

public class CreateBiomechanicalConsiderationValidator : AbstractValidator<CreateBiomechanicalConsiderationDto>
{
    public CreateBiomechanicalConsiderationValidator()
    {
        RuleFor(x => x.Aspect)
            .IsInEnum().WithMessage("Valid biomechanical aspect is required.");

        RuleFor(x => x.Certainty)
            .IsInEnum().WithMessage("Valid certainty level (Established, Inferred, Hypothesis) is required.");

        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Biomechanical consideration summary cannot be empty.")
            .MaximumLength(300).WithMessage("Summary cannot exceed 300 characters.");

        RuleFor(x => x.Explanation)
            .NotEmpty().WithMessage("Biomechanical consideration explanation cannot be empty.")
            .MaximumLength(2000).WithMessage("Explanation cannot exceed 2000 characters.");

        RuleFor(x => x.PracticalCues)
            .MaximumLength(1000).WithMessage("Practical cues cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.PracticalCues));
    }
}
