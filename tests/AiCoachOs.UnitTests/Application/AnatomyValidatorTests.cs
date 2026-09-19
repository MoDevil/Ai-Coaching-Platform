using AiCoachOs.Application.AnatomyAndBiomechanics.DTOs;
using AiCoachOs.Application.AnatomyAndBiomechanics.Validators;
using AiCoachOs.Domain.AnatomyAndBiomechanics;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class AnatomyValidatorTests
{
    private readonly CreateBiomechanicalConsiderationValidator _validator = new();

    [Fact]
    public void ValidDto_ShouldPassValidation()
    {
        var dto = new CreateBiomechanicalConsiderationDto(
            BiomechanicalAspect.MomentArm,
            CertaintyLevel.Established,
            "Torso angle shifts knee vs hip demand",
            "An upright torso shifts moment arm toward the knee extensor complex.",
            "Use heel wedges to achieve greater knee flexion."
        );

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptySummary_ShouldFailValidation(string? summary)
    {
        var dto = new CreateBiomechanicalConsiderationDto(
            BiomechanicalAspect.SetupVariable,
            CertaintyLevel.Inferred,
            summary!,
            "Detailed explanation"
        );

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Summary));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyExplanation_ShouldFailValidation(string? explanation)
    {
        var dto = new CreateBiomechanicalConsiderationDto(
            BiomechanicalAspect.SetupVariable,
            CertaintyLevel.Inferred,
            "Summary text",
            explanation!
        );

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Explanation));
    }

    [Fact]
    public void SummaryTooLong_ShouldFailValidation()
    {
        var dto = new CreateBiomechanicalConsiderationDto(
            BiomechanicalAspect.SetupVariable,
            CertaintyLevel.Inferred,
            new string('a', 301),
            "Detailed explanation"
        );

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Summary));
    }
}
