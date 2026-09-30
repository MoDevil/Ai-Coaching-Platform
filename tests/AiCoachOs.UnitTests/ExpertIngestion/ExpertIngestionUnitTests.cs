using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.ExpertIngestion.Dtos;
using AiCoachOs.Domain.ExpertIngestion;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Infrastructure.Ai;
using AiCoachOs.Infrastructure.ExpertIngestion;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AiCoachOs.UnitTests.ExpertIngestion;

public class ExpertIngestionUnitTests
{
    [Fact]
    public void MedicalContentDetector_DetectsClinicalPhrases_Successfully()
    {
        // Arrange & Act
        var isMedical1 = MedicalContentDetector.ScanForMedicalContent("This protocol will treat diabetes and lower fasting blood glucose.", out var reason1);
        var isMedical2 = MedicalContentDetector.ScanForMedicalContent("We will prescribe clinical pharmacology doses.", out var reason2);
        var isMedical3 = MedicalContentDetector.ScanForMedicalContent("Here is the anabolic steroid cycle for physique athletes.", out var reason3);
        var isMedical4 = MedicalContentDetector.ScanForMedicalContent("This is the injury rehab protocol for a torn meniscus.", out var reason4);

        // Assert
        Assert.True(isMedical1);
        Assert.NotNull(reason1);
        Assert.True(isMedical2);
        Assert.NotNull(reason2);
        Assert.True(isMedical3);
        Assert.NotNull(reason3);
        Assert.True(isMedical4);
        Assert.NotNull(reason4);
    }

    [Fact]
    public void MedicalContentDetector_ReturnsFalse_ForSafeCoachingClaims()
    {
        // Arrange & Act
        var isMedical = MedicalContentDetector.ScanForMedicalContent("Performing 12 sets per week with 2 RIR optimizes hypertrophy.", out var reason);

        // Assert
        Assert.False(isMedical);
        Assert.Null(reason);
    }

    [Fact]
    public void MedicalContentDetector_HasExactLockedWarningConstant()
    {
        Assert.Equal("This content may contain medical claims. Review carefully. AI Coach OS does not validate medical advice.", MedicalContentDetector.MedicalWarningNotice);
    }

    [Fact]
    public void ExpertContentIngestion_DomainInvariants_AndStatusRecalculation()
    {
        // Arrange
        var coachId = Guid.NewGuid();
        var ingestion = new ExpertContentIngestion(
            Guid.NewGuid(),
            coachId,
            "https://youtube.com/watch?v=test1234",
            "Hypertrophy Deep Dive",
            IngestionSourceType.YouTubeVideo);

        Assert.Equal(IngestionStatus.Processing, ingestion.Status);
        Assert.False(ingestion.ContainsMedicalClaims);

        // Act - Set stats & complete
        ingestion.SetExtractedStats(1250, false);
        Assert.Equal(1250, ingestion.ExtractedTextLength);
        Assert.False(ingestion.WasTruncated);

        var claim1 = new ExpertClaim(
            Guid.NewGuid(), 
            ingestion.Id, 
            "Volume is essential for hypertrophy.", 
            ClaimCategory.TrainingVolume, 
            EvidenceClassification.InterpretationOfResearch, 
            CreatorConfidence.High);

        var claim2 = new ExpertClaim(
            Guid.NewGuid(), 
            ingestion.Id, 
            "Training frequency of 2x weekly is optimal.", 
            ClaimCategory.Frequency, 
            EvidenceClassification.InterpretationOfResearch, 
            CreatorConfidence.High);

        ingestion.AddClaim(claim1);
        ingestion.AddClaim(claim2);

        ingestion.SetCompleted(containsMedicalClaims: true);
        Assert.Equal(IngestionStatus.PendingReview, ingestion.Status);
        Assert.True(ingestion.ContainsMedicalClaims);

        // Approve 1 claim -> PartiallyApproved
        claim1.Approve(Guid.NewGuid(), coachId, "Verified");
        ingestion.RecalculateOverallStatus();
        Assert.Equal(IngestionStatus.PartiallyApproved, ingestion.Status);

        // Approve 2nd claim -> Completed
        claim2.Approve(Guid.NewGuid(), coachId, "Verified");
        ingestion.RecalculateOverallStatus();
        Assert.Equal(IngestionStatus.Completed, ingestion.Status);
    }

    [Fact]
    public void ExpertClaim_ApproveRejectDefer_UpdatesReviewStatusCorrectly()
    {
        // Arrange
        var ingestionId = Guid.NewGuid();
        var coachId = Guid.NewGuid();
        var claim = new ExpertClaim(
            Guid.NewGuid(),
            ingestionId,
            "Knees traveling past toes is safe and biomechanically sound.",
            ClaimCategory.Biomechanics,
            EvidenceClassification.InterpretationOfResearch,
            CreatorConfidence.High,
            directQuote: false,
            sourceContext: "03:45");

        Assert.Equal(CoachReviewStatus.PendingReview, claim.CoachReviewStatus);
        Assert.Null(claim.ApprovedKnowledgeClaimId);

        // Act - Defer
        claim.Defer(coachId, "Need further literature review");
        Assert.Equal(CoachReviewStatus.Deferred, claim.CoachReviewStatus);
        Assert.Equal("Need further literature review", claim.CoachNote);

        // Act - Reject
        claim.Reject(coachId, "Contradicts primary safety baseline");
        Assert.Equal(CoachReviewStatus.Rejected, claim.CoachReviewStatus);

        // Act - Approve
        var knowledgeClaimId = Guid.NewGuid();
        claim.Approve(knowledgeClaimId, coachId, "Approved into M3");
        Assert.Equal(CoachReviewStatus.Approved, claim.CoachReviewStatus);
        Assert.Equal(knowledgeClaimId, claim.ApprovedKnowledgeClaimId);
        Assert.Equal(coachId, claim.ReviewedByCoachId);
        Assert.NotNull(claim.CoachReviewedAt);
    }

    [Fact]
    public void ExpertSource_CreationAndUpdate_MaintainsProperties()
    {
        // Arrange
        var source = new ExpertSource(
            Guid.NewGuid(),
            "Dr. Mike Israetel",
            ExpertSourceType.YouTubeChannel,
            "https://youtube.com/@RenaissancePeriodization");

        Assert.Equal("Dr. Mike Israetel", source.Name);
        Assert.Equal(ExpertSourceType.YouTubeChannel, source.SourceType);
        Assert.Equal("https://youtube.com/@RenaissancePeriodization", source.Url);

        // Act
        source.Update(
            "Dr. Michael Israetel",
            ExpertSourceType.YouTubeChannel,
            "https://rpstrength.com");

        // Assert
        Assert.Equal("Dr. Michael Israetel", source.Name);
        Assert.Equal("https://rpstrength.com", source.Url);
        Assert.NotNull(source.UpdatedAtUtc);
    }

    [Fact]
    public async Task ConflictDetectionService_DeterministicMatching_DetectsSupportAndConflict()
    {
        // Arrange
        var supportingClaim = new KnowledgeClaim(
            Guid.NewGuid(),
            "TrainingVolume",
            "What weekly volume maximizes hypertrophy?",
            "Performing 10 to 20 sets per week increases muscle hypertrophy in trained individuals.",
            EvidenceLevel.MetaAnalysis,
            ClaimStatus.Active);

        var conflictingClaim = new KnowledgeClaim(
            Guid.NewGuid(),
            "Biomechanics",
            "Should knees pass toes in squats?",
            "Allowing knees to travel past toes is ineffective and suboptimal for knee safety.",
            EvidenceLevel.Mechanistic,
            ClaimStatus.Active);

        var claims = new List<KnowledgeClaim> { supportingClaim, conflictingClaim };
        var asyncClaims = new AiCoachOs.UnitTests.Common.TestAsyncEnumerable<KnowledgeClaim>(claims);

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.KnowledgeClaims).Returns(asyncClaims);

        var detector = new ConflictDetectionService(mockContext.Object);

        var candidates = new List<ExtractedClaimCandidate>
        {
            new ExtractedClaimCandidate(
                ClaimText: "Higher weekly set volume increases hypertrophy effectively.",
                Category: ClaimCategory.TrainingVolume,
                EvidenceClassification: EvidenceClassification.InterpretationOfResearch,
                CreatorConfidence: CreatorConfidence.High,
                DirectQuote: false,
                SourceContext: "01:00"),
            new ExtractedClaimCandidate(
                ClaimText: "Knee travel past toes is optimal, beneficial, and superior for quad recruitment.",
                Category: ClaimCategory.Biomechanics,
                EvidenceClassification: EvidenceClassification.InterpretationOfResearch,
                CreatorConfidence: CreatorConfidence.High,
                DirectQuote: false,
                SourceContext: "05:00"),
            new ExtractedClaimCandidate(
                ClaimText: "Brand new isolated assertion with no prior literature.",
                Category: ClaimCategory.General,
                EvidenceClassification: EvidenceClassification.OpinionOnly,
                CreatorConfidence: CreatorConfidence.Low,
                DirectQuote: false,
                SourceContext: "08:00")
        };

        // Act
        var matches = await detector.DetectM3ConflictsAsync(candidates);

        // Assert
        Assert.Equal(3, matches.Count);

        // Match 1: Aligned on TrainingVolume -> Supporting
        Assert.Equal(supportingClaim.Id, matches[0].SupportingClaimId);

        // Match 2: Opposing polarity on Biomechanics -> Conflicting
        Assert.Equal(conflictingClaim.Id, matches[1].ConflictingClaimId);

        // Match 3: Novel general topic -> New territory (both null)
        Assert.Null(matches[2].SupportingClaimId);
        Assert.Null(matches[2].ConflictingClaimId);
    }

    [Fact]
    public async Task ClaimExtractionService_CapsAt20Claims_AndExcludesMedical()
    {
        // Arrange
        var mockAi = new MockAiProvider();
        var extractionService = new ClaimExtractionService(mockAi, NullLogger<ClaimExtractionService>.Instance);

        // Act
        var result = await extractionService.ExtractClaimsAsync(
            "Discussion on volume and squat progression for physique training.",
            "Hypertrophy Masterclass",
            "Expert Coach");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Claims.Count <= 20);
        Assert.All(result.Claims, c => Assert.False(MedicalContentDetector.ScanForMedicalContent(c.ClaimText, out _)));
    }
}
