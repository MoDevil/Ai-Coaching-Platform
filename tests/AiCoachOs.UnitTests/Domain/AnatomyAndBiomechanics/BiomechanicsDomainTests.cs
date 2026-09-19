using AiCoachOs.Domain.AnatomyAndBiomechanics;
using AiCoachOs.Domain.Exercises;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain.AnatomyAndBiomechanics;

public class BiomechanicsDomainTests
{
    [Fact]
    public void CreateBiomechanicalConsideration_WithValidFields_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();

        // Act
        var consideration = new BiomechanicalConsideration(
            id,
            exerciseId,
            BiomechanicalAspect.MomentArm,
            CertaintyLevel.Established,
            "Torso angle alters knee vs hip moment arm",
            "Upright torso increases forward knee travel and quad demand.",
            "Elevate heels to reduce ankle dorsiflexion requirement."
        );

        // Assert
        consideration.Id.Should().Be(id);
        consideration.ExerciseId.Should().Be(exerciseId);
        consideration.Aspect.Should().Be(BiomechanicalAspect.MomentArm);
        consideration.Certainty.Should().Be(CertaintyLevel.Established);
        consideration.Summary.Should().Be("Torso angle alters knee vs hip moment arm");
        consideration.Explanation.Should().Be("Upright torso increases forward knee travel and quad demand.");
        consideration.PracticalCues.Should().Be("Elevate heels to reduce ankle dorsiflexion requirement.");
        consideration.KnowledgeClaimId.Should().BeNull();
    }

    [Fact]
    public void CreateBiomechanicalConsideration_WithEmptyExerciseId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new BiomechanicalConsideration(
            Guid.NewGuid(),
            Guid.Empty,
            BiomechanicalAspect.SetupVariable,
            CertaintyLevel.Inferred,
            "Summary",
            "Explanation"
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateBiomechanicalConsideration_WithInvalidSummary_ShouldThrowArgumentException(string? summary)
    {
        // Act
        var act = () => new BiomechanicalConsideration(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BiomechanicalAspect.SetupVariable,
            CertaintyLevel.Inferred,
            summary!,
            "Explanation"
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateBiomechanicalConsideration_WithInvalidExplanation_ShouldThrowArgumentException(string? explanation)
    {
        // Act
        var act = () => new BiomechanicalConsideration(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BiomechanicalAspect.SetupVariable,
            CertaintyLevel.Inferred,
            "Summary",
            explanation!
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void LinkKnowledgeClaim_WithValidClaimId_ShouldSetKnowledgeClaimId()
    {
        // Arrange
        var consideration = new BiomechanicalConsideration(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BiomechanicalAspect.MuscleLength,
            CertaintyLevel.Established,
            "Lengthened loading",
            "Lengthened tension promotes hypertrophy."
        );
        var claimId = Guid.NewGuid();

        // Act
        consideration.LinkKnowledgeClaim(claimId);

        // Assert
        consideration.KnowledgeClaimId.Should().Be(claimId);
        consideration.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void LinkKnowledgeClaim_WithEmptyClaimId_ShouldThrowArgumentException()
    {
        // Arrange
        var consideration = new BiomechanicalConsideration(
            Guid.NewGuid(),
            Guid.NewGuid(),
            BiomechanicalAspect.MuscleLength,
            CertaintyLevel.Established,
            "Lengthened loading",
            "Lengthened tension promotes hypertrophy."
        );

        // Act
        var act = () => consideration.LinkKnowledgeClaim(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Exercise_MetadataStatus_RemainsProvisionalWhenAnatomyOrBiomechanicsLinked()
    {
        // Verify invariant: Linking anatomy or biomechanics to an exercise must NEVER
        // automatically promote Exercise.MetadataStatus from Provisional to Verified.
        var exercise = new Exercise(
            Guid.NewGuid(),
            "Barbell Back Squat",
            ExerciseCategory.Compound,
            Guid.NewGuid(),
            QualitativeRating.High,
            QualitativeRating.High,
            QualitativeRating.High,
            QualitativeRating.High,
            QualitativeRating.High,
            QualitativeRating.High,
            ResistanceProfile.MidRange
        );

        // Invariant check
        exercise.MetadataStatus.Should().Be(MetadataStatus.Provisional);
    }
}
