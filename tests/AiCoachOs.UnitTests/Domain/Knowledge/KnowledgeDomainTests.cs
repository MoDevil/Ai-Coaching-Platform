using AiCoachOs.Domain.Knowledge;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain.Knowledge;

public class KnowledgeDomainTests
{
    [Fact]
    public void CreateKnowledgeSource_WithValidFields_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var source = new KnowledgeSource(
            id: id,
            sourceType: KnowledgeSourceType.ScientificPaper,
            title: "Resistance Training Volume Enhances Muscle Hypertrophy but Not Strength",
            authors: "Schoenfeld, B. J., et al.",
            year: 2019,
            evidenceLevel: EvidenceLevel.MetaAnalysis,
            doi: "10.1249/MSS.0000000000001764",
            url: "https://pubmed.ncbi.nlm.nih.gov/30153194/",
            notes: "Graded dose-response between weekly sets and hypertrophy"
        );

        // Assert
        source.Id.Should().Be(id);
        source.SourceType.Should().Be(KnowledgeSourceType.ScientificPaper);
        source.Title.Should().Be("Resistance Training Volume Enhances Muscle Hypertrophy but Not Strength");
        source.Authors.Should().Be("Schoenfeld, B. J., et al.");
        source.Year.Should().Be(2019);
        source.EvidenceLevel.Should().Be(EvidenceLevel.MetaAnalysis);
        source.Doi.Should().Be("10.1249/MSS.0000000000001764");
        source.ClaimSources.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateKnowledgeSource_WithInvalidTitle_ShouldThrowArgumentException(string? title)
    {
        // Act
        var act = () => new KnowledgeSource(
            Guid.NewGuid(), KnowledgeSourceType.ScientificPaper, title!, "Authors", 2020, EvidenceLevel.MetaAnalysis);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateKnowledgeSource_WithInvalidAuthors_ShouldThrowArgumentException(string? authors)
    {
        // Act
        var act = () => new KnowledgeSource(
            Guid.NewGuid(), KnowledgeSourceType.ScientificPaper, "Title", authors!, 2020, EvidenceLevel.MetaAnalysis);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(1799)]
    [InlineData(2099)]
    public void CreateKnowledgeSource_WithInvalidYear_ShouldThrowArgumentException(int year)
    {
        // Act
        var act = () => new KnowledgeSource(
            Guid.NewGuid(), KnowledgeSourceType.ScientificPaper, "Title", "Authors", year, EvidenceLevel.MetaAnalysis);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateKnowledgeClaim_GeneralClaim_ShouldSucceedWithNullExerciseId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var claim = new KnowledgeClaim(
            id: id,
            topic: "Volume",
            question: "What is the optimal weekly volume threshold for trained lifters?",
            claimText: "10-20 weekly sets per muscle group generally maximizes hypertrophic adaptation.",
            evidenceLevel: EvidenceLevel.MetaAnalysis,
            status: ClaimStatus.Active,
            exerciseId: null,
            population: "Trained adults",
            limitations: "Individual recovery capacity varies widely based on sleep and nutrition.",
            practicalApplication: "Start at minimum effective volume and scale upward based on recovery markers."
        );

        // Assert
        claim.Id.Should().Be(id);
        claim.ExerciseId.Should().BeNull();
        claim.Topic.Should().Be("Volume");
        claim.Status.Should().Be(ClaimStatus.Active);
        claim.EvidenceLevel.Should().Be(EvidenceLevel.MetaAnalysis);
        claim.Sources.Should().BeEmpty();
        claim.SupersededByClaimId.Should().BeNull();
    }

    [Fact]
    public void CreateKnowledgeClaim_ExerciseSpecific_ShouldRetainExerciseIdWithoutMutatingExercise()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();

        // Act
        var claim = new KnowledgeClaim(
            Guid.NewGuid(),
            "RangeOfMotion",
            "Does full ROM squatting produce superior quad hypertrophy?",
            "Deep squats produce greater adductor and gluteus maximus hypertrophy compared to partial squats.",
            EvidenceLevel.RandomizedControlledTrial,
            ClaimStatus.Active,
            exerciseId: exerciseId
        );

        // Assert
        claim.ExerciseId.Should().Be(exerciseId);
        claim.Status.Should().Be(ClaimStatus.Active);
    }

    [Fact]
    public void AddSource_ShouldAddSourceAndPreventDuplicates()
    {
        // Arrange
        var claim = new KnowledgeClaim(
            Guid.NewGuid(), "Topic", "Question", "Claim text", EvidenceLevel.MetaAnalysis);
        var sourceId = Guid.NewGuid();

        // Act
        claim.AddSource(sourceId, "Primary meta-analytic evidence");
        claim.AddSource(sourceId, "Duplicate attempt");

        // Assert
        claim.Sources.Should().HaveCount(1);
        claim.Sources.First().SourceId.Should().Be(sourceId);
        claim.Sources.First().RelevanceNote.Should().Be("Primary meta-analytic evidence");
    }

    [Fact]
    public void Supersede_WithValidReplacement_ShouldUpdateStatusAndLinkReplacement()
    {
        // Arrange
        var originalClaim = new KnowledgeClaim(
            Guid.NewGuid(), "ProximityToFailure", "Is failure necessary for hypertrophy?",
            "Training to absolute failure on every set is required for maximal muscle growth.",
            EvidenceLevel.Mechanistic, ClaimStatus.Active);

        var replacementClaim = new KnowledgeClaim(
            Guid.NewGuid(), "ProximityToFailure", "Is failure necessary for hypertrophy?",
            "Training within 1-3 RIR produces comparable hypertrophy with substantially lower systemic fatigue.",
            EvidenceLevel.MetaAnalysis, ClaimStatus.Active);

        var supersededAt = DateTime.UtcNow;

        // Act
        originalClaim.Supersede(replacementClaim, "Updated by recent systematic review and meta-analysis on RIR vs Failure", supersededAt);

        // Assert
        originalClaim.Status.Should().Be(ClaimStatus.Superseded);
        originalClaim.SupersededByClaimId.Should().Be(replacementClaim.Id);
        originalClaim.SupersededAtUtc.Should().Be(supersededAt);
        originalClaim.SupersessionReason.Should().Contain("recent systematic review");
    }

    [Fact]
    public void Supersede_SelfSupersession_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var claim = new KnowledgeClaim(
            Guid.NewGuid(), "Topic", "Question", "Claim text", EvidenceLevel.MetaAnalysis);

        // Act
        var act = () => claim.Supersede(claim, "Attempt self-supersession");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot supersede itself*");
    }

    [Fact]
    public void Supersede_AlreadySupersededClaim_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var claim = new KnowledgeClaim(
            Guid.NewGuid(), "Topic", "Question", "Claim text", EvidenceLevel.Mechanistic);
        var replacement1 = new KnowledgeClaim(
            Guid.NewGuid(), "Topic", "Question", "Replacement 1", EvidenceLevel.ExpertConsensus);
        var replacement2 = new KnowledgeClaim(
            Guid.NewGuid(), "Topic", "Question", "Replacement 2", EvidenceLevel.MetaAnalysis);

        claim.Supersede(replacement1, "First supersession");

        // Act
        var act = () => claim.Supersede(replacement2, "Second supersession");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already superseded*");
    }

    [Fact]
    public void MarkReviewed_ShouldUpdateReviewerAndTimestamp()
    {
        // Arrange
        var claim = new KnowledgeClaim(
            Guid.NewGuid(), "Topic", "Question", "Claim text", EvidenceLevel.MetaAnalysis);
        var reviewTime = DateTime.UtcNow;

        // Act
        claim.MarkReviewed("Dr. Coach", reviewTime);

        // Assert
        claim.ReviewedBy.Should().Be("Dr. Coach");
        claim.ReviewedAtUtc.Should().Be(reviewTime);
    }
}
