using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.ExpertIngestion.Dtos;
using AiCoachOs.Domain.ExpertIngestion;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Infrastructure.Ai;
using AiCoachOs.Infrastructure.ExpertIngestion;
using Microsoft.EntityFrameworkCore;
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
    public void ExpertContentIngestion_DomainInvariants_AndStatusRecalculation()
    {
        // Arrange
        var coachId = Guid.NewGuid();
        var ingestion = new ExpertContentIngestion(
            Guid.NewGuid(),
            coachId,
            "https://youtube.com/watch?v=test1234",
            IngestionContentType.YouTube,
            "Hypertrophy Deep Dive");

        Assert.Equal(IngestionStatus.Processing, ingestion.Status);
        Assert.False(ingestion.ContainsMedicalClaims);

        // Act - Set content & complete
        ingestion.SetExtractedContent("Short snippet", 500, false);
        Assert.Equal(500, ingestion.WordCount);
        Assert.False(ingestion.WasTruncated);

        var claim1 = new ExpertClaim(Guid.NewGuid(), ingestion.Id, "Hypertrophy", "Volume is essential.", ClaimNature.InterpretationOfResearch);
        var claim2 = new ExpertClaim(Guid.NewGuid(), ingestion.Id, "Hypertrophy", "Frequency matters.", ClaimNature.InterpretationOfResearch);
        ingestion.AddClaim(claim1);
        ingestion.AddClaim(claim2);

        ingestion.SetCompleted(containsMedicalClaims: true);
        Assert.Equal(IngestionStatus.PendingReview, ingestion.Status);
        Assert.True(ingestion.ContainsMedicalClaims);

        // Approve 1 claim -> PartiallyApproved
        claim1.Approve(Guid.NewGuid(), coachId, "Verified");
        ingestion.RecalculateOverallStatus();
        Assert.Equal(IngestionStatus.PartiallyApproved, ingestion.Status);

        // Approve 2nd claim -> FullyApproved
        claim2.Approve(Guid.NewGuid(), coachId, "Verified");
        ingestion.RecalculateOverallStatus();
        Assert.Equal(IngestionStatus.FullyApproved, ingestion.Status);
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
            "Squat Technique",
            "Knees traveling past toes is safe.",
            ClaimNature.InterpretationOfResearch,
            subTopic: "Knee Travel",
            contextOrTimestamp: "03:45",
            directQuote: false);

        Assert.Equal(ExpertClaimReviewStatus.Pending, claim.ReviewStatus);
        Assert.Null(claim.ApprovedKnowledgeClaimId);

        // Act - Defer
        claim.Defer(coachId, "Need further literature review");
        Assert.Equal(ExpertClaimReviewStatus.Deferred, claim.ReviewStatus);
        Assert.Equal("Need further literature review", claim.CoachNotes);

        // Act - Reject
        claim.Reject(coachId, "Contradicts primary safety baseline");
        Assert.Equal(ExpertClaimReviewStatus.Rejected, claim.ReviewStatus);

        // Act - Approve
        var knowledgeClaimId = Guid.NewGuid();
        claim.Approve(knowledgeClaimId, coachId, "Approved into M3");
        Assert.Equal(ExpertClaimReviewStatus.Approved, claim.ReviewStatus);
        Assert.Equal(knowledgeClaimId, claim.ApprovedKnowledgeClaimId);
        Assert.Equal(coachId, claim.ReviewedByCoachId);
        Assert.NotNull(claim.ReviewedAtUtc);
    }

    [Fact]
    public void ExpertSource_CreationAndUpdate_MaintainsProperties()
    {
        // Arrange
        var source = new ExpertSource(
            Guid.NewGuid(),
            "Dr. Mike Israetel",
            "Renaissance Periodization",
            ExpertPlatform.YouTube,
            "Hypertrophy & Programming",
            CredibilityTier.High,
            "PhD in Sport Physiology");

        Assert.Equal("Dr. Mike Israetel", source.Name);
        Assert.Equal(CredibilityTier.High, source.CredibilityTier);

        // Act
        source.Update(
            "Dr. Michael Israetel",
            "Renaissance Periodization YouTube",
            ExpertPlatform.YouTube,
            "Hypertrophy",
            CredibilityTier.High,
            "Updated Bio");

        // Assert
        Assert.Equal("Dr. Michael Israetel", source.Name);
        Assert.Equal("Renaissance Periodization YouTube", source.ChannelOrPublication);
        Assert.NotNull(source.UpdatedAtUtc);
    }

    [Fact]
    public async Task ConflictDetectionService_DeterministicMatching_DetectsSupportAndConflict()
    {
        // Arrange
        var supportingClaim = new KnowledgeClaim(
            Guid.NewGuid(),
            "Hypertrophy",
            "What weekly volume maximizes hypertrophy?",
            "Performing 10 to 20 sets per week increases muscle hypertrophy in trained individuals.",
            EvidenceLevel.MetaAnalysis,
            ClaimStatus.Active);

        var conflictingClaim = new KnowledgeClaim(
            Guid.NewGuid(),
            "Squat Technique",
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
                Topic: "Hypertrophy",
                SubTopic: "Volume",
                ClaimText: "Higher weekly set volume increases hypertrophy effectively.",
                ContextOrTimestamp: "01:00",
                DirectQuote: false,
                NatureOfClaim: ClaimNature.InterpretationOfResearch),
            new ExtractedClaimCandidate(
                Topic: "Squat Technique",
                SubTopic: "Knee Travel",
                ClaimText: "Knee travel past toes is optimal, beneficial, and superior for quad recruitment.",
                ContextOrTimestamp: "05:00",
                DirectQuote: false,
                NatureOfClaim: ClaimNature.InterpretationOfResearch),
            new ExtractedClaimCandidate(
                Topic: "Novel Topic XYZ",
                SubTopic: "Unexplored",
                ClaimText: "Brand new isolated assertion with no prior literature.",
                ContextOrTimestamp: "08:00",
                DirectQuote: false,
                NatureOfClaim: ClaimNature.OpinionOnly)
        };

        // Act
        var matches = await detector.DetectM3ConflictsAsync(candidates);

        // Assert
        Assert.Equal(3, matches.Count);

        // Match 1: Aligned on Hypertrophy / Volume -> Supporting
        Assert.Equal(supportingClaim.Id, matches[0].SupportingClaimId);

        // Match 2: Opposing polarity on Squat Technique -> Conflicting
        Assert.Equal(conflictingClaim.Id, matches[1].ConflictingClaimId);

        // Match 3: Novel topic -> New territory (both null)
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
