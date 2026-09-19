using AiCoachOs.Application.Knowledge.DTOs;
using AiCoachOs.Application.Knowledge.Validators;
using AiCoachOs.Domain.Knowledge;
using FluentValidation.TestHelper;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class KnowledgeValidatorTests
{
    private readonly CreateKnowledgeSourceDtoValidator _sourceValidator = new();
    private readonly CreateKnowledgeClaimDtoValidator _claimValidator = new();
    private readonly AddClaimSourceDtoValidator _addSourceValidator = new();
    private readonly SupersedeClaimDtoValidator _supersedeValidator = new();

    [Fact]
    public void ValidateSource_WithValidData_ShouldNotHaveErrors()
    {
        var dto = new CreateKnowledgeSourceDto(
            KnowledgeSourceType.ScientificPaper,
            "Valid Study Title",
            "Researcher A",
            2023,
            EvidenceLevel.RandomizedControlledTrial,
            "10.1234/test"
        );

        var result = _sourceValidator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ValidateSource_WithEmptyTitleOrAuthors_ShouldHaveErrors(string? value)
    {
        var dtoTitle = new CreateKnowledgeSourceDto(
            KnowledgeSourceType.ScientificPaper, value!, "Authors", 2023, EvidenceLevel.RandomizedControlledTrial);
        _sourceValidator.TestValidate(dtoTitle).ShouldHaveValidationErrorFor(x => x.Title);

        var dtoAuthors = new CreateKnowledgeSourceDto(
            KnowledgeSourceType.ScientificPaper, "Title", value!, 2023, EvidenceLevel.RandomizedControlledTrial);
        _sourceValidator.TestValidate(dtoAuthors).ShouldHaveValidationErrorFor(x => x.Authors);
    }

    [Theory]
    [InlineData(1700)]
    [InlineData(2100)]
    public void ValidateSource_WithInvalidYear_ShouldHaveErrors(int year)
    {
        var dto = new CreateKnowledgeSourceDto(
            KnowledgeSourceType.ScientificPaper, "Title", "Authors", year, EvidenceLevel.RandomizedControlledTrial);
        _sourceValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Year);
    }

    [Fact]
    public void ValidateClaim_WithValidData_ShouldNotHaveErrors()
    {
        var dto = new CreateKnowledgeClaimDto(
            "Volume",
            "How many sets per muscle group?",
            "10-20 weekly sets appears optimal for hypertrophy.",
            EvidenceLevel.MetaAnalysis
        );

        var result = _claimValidator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ValidateClaim_WithMissingRequiredFields_ShouldHaveErrors(string? empty)
    {
        var dto = new CreateKnowledgeClaimDto(empty!, empty!, empty!, EvidenceLevel.MetaAnalysis);
        var result = _claimValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Topic);
        result.ShouldHaveValidationErrorFor(x => x.Question);
        result.ShouldHaveValidationErrorFor(x => x.ClaimText);
    }

    [Fact]
    public void ValidateAddClaimSource_WithEmptySourceId_ShouldHaveError()
    {
        var dto = new AddClaimSourceDto(Guid.Empty);
        _addSourceValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.SourceId);
    }

    [Fact]
    public void ValidateSupersedeClaim_WithEmptyReplacementOrReason_ShouldHaveErrors()
    {
        var dto = new SupersedeClaimDto(Guid.Empty, "");
        var result = _supersedeValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ReplacementClaimId);
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
