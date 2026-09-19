using AiCoachOs.Application.Knowledge.DTOs;
using FluentValidation;

namespace AiCoachOs.Application.Knowledge.Validators;

public class CreateKnowledgeSourceDtoValidator : AbstractValidator<CreateKnowledgeSourceDto>
{
    public CreateKnowledgeSourceDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Source title is required.")
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters.");

        RuleFor(x => x.Authors)
            .NotEmpty().WithMessage("Source authors are required.")
            .MaximumLength(500).WithMessage("Authors cannot exceed 500 characters.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1800, DateTime.UtcNow.Year + 1)
            .WithMessage($"Publication year must be between 1800 and {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x.SourceType)
            .IsInEnum().WithMessage("Invalid source type.");

        RuleFor(x => x.EvidenceLevel)
            .IsInEnum().WithMessage("Invalid evidence level.");

        RuleFor(x => x.Doi)
            .MaximumLength(150).WithMessage("DOI cannot exceed 150 characters.");

        RuleFor(x => x.Url)
            .MaximumLength(1000).WithMessage("URL cannot exceed 1000 characters.");
    }
}

public class CreateKnowledgeClaimDtoValidator : AbstractValidator<CreateKnowledgeClaimDto>
{
    public CreateKnowledgeClaimDtoValidator()
    {
        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required.")
            .MaximumLength(100).WithMessage("Topic cannot exceed 100 characters.");

        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Research question is required.")
            .MaximumLength(500).WithMessage("Question cannot exceed 500 characters.");

        RuleFor(x => x.ClaimText)
            .NotEmpty().WithMessage("Claim text is required.")
            .MaximumLength(3000).WithMessage("Claim text cannot exceed 3000 characters.");

        RuleFor(x => x.EvidenceLevel)
            .IsInEnum().WithMessage("Invalid evidence level.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid claim status.");

        RuleFor(x => x.Population)
            .MaximumLength(500).WithMessage("Population context cannot exceed 500 characters.");

        RuleFor(x => x.Limitations)
            .MaximumLength(2000).WithMessage("Limitations cannot exceed 2000 characters.");

        RuleFor(x => x.PracticalApplication)
            .MaximumLength(2000).WithMessage("Practical application cannot exceed 2000 characters.");
    }
}

public class AddClaimSourceDtoValidator : AbstractValidator<AddClaimSourceDto>
{
    public AddClaimSourceDtoValidator()
    {
        RuleFor(x => x.SourceId)
            .NotEmpty().WithMessage("Source ID is required.");

        RuleFor(x => x.RelevanceNote)
            .MaximumLength(1000).WithMessage("Relevance note cannot exceed 1000 characters.");
    }
}

public class SupersedeClaimDtoValidator : AbstractValidator<SupersedeClaimDto>
{
    public SupersedeClaimDtoValidator()
    {
        RuleFor(x => x.ReplacementClaimId)
            .NotEmpty().WithMessage("Replacement claim ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Supersession reason is required.")
            .MaximumLength(1000).WithMessage("Supersession reason cannot exceed 1000 characters.");
    }
}
