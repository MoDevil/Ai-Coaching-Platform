using AiCoachOs.Domain.Memory;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Ai;

public class RecommendationReviewUnitTests
{
    private static AIRecommendationRecord CreatePendingRecord()
    {
        return new AIRecommendationRecord(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            recommendationCategory: AIRecommendationCategory.ProgramAdaptationReview,
            recommendationText: "Reduce squat volume by 1 set and increase rest interval.",
            rationaleText: "Client reported elevated fatigue in lower back.",
            confidenceStatement: "High confidence based on fatigue indicators.",
            aiProvider: "Mock",
            aiModel: "mock-model"
        );
    }

    [Fact]
    public void ApplyReview_PendingToUnderReview_Succeeds()
    {
        var record = CreatePendingRecord();
        record.ReviewStatus.Should().Be(AIRecommendationReviewStatus.PendingReview);

        record.ApplyReview(AIRecommendationReviewStatus.UnderReview);

        record.ReviewStatus.Should().Be(AIRecommendationReviewStatus.UnderReview);
    }

    [Fact]
    public void ApplyReview_PendingToAccepted_WithCoachDecision_Succeeds()
    {
        var record = CreatePendingRecord();

        record.ApplyReview(AIRecommendationReviewStatus.Accepted, "Accepted squat adjustment with 3-minute rest.", "Reduced 1 set of back squats.");

        record.ReviewStatus.Should().Be(AIRecommendationReviewStatus.Accepted);
        record.CoachDecision.Should().Be(CoachDecisionOutcome.Accepted);
        record.CoachDecisionNote.Should().Be("Accepted squat adjustment with 3-minute rest.");
        record.FinalImplementedPlan.Should().Be("Reduced 1 set of back squats.");
        record.CoachDecisionAt.Should().NotBeNull();
    }

    [Fact]
    public void ApplyReview_AcceptingWithoutCoachDecision_ThrowsArgumentException()
    {
        var record = CreatePendingRecord();

        var act = () => record.ApplyReview(AIRecommendationReviewStatus.Accepted, coachDecision: "  ");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*decision is required*");
    }

    [Fact]
    public void ApplyReview_SupplyingFinalImplementedPlan_WhenNotAccepted_ThrowsArgumentException()
    {
        var record = CreatePendingRecord();

        var act = () => record.ApplyReview(AIRecommendationReviewStatus.UnderReview, coachDecision: "Reviewing", finalImplementedPlan: "Some plan");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*only be supplied when the recommendation is accepted*");
    }

    [Fact]
    public void ApplyReview_PendingToRejected_Succeeds()
    {
        var record = CreatePendingRecord();

        record.ApplyReview(AIRecommendationReviewStatus.Rejected, "Client is recovering well, volume reduction not required.");

        record.ReviewStatus.Should().Be(AIRecommendationReviewStatus.Rejected);
        record.CoachDecision.Should().Be(CoachDecisionOutcome.Rejected);
        record.CoachDecisionNote.Should().Be("Client is recovering well, volume reduction not required.");
        record.CoachDecisionAt.Should().NotBeNull();
    }

    [Fact]
    public void ApplyReview_PendingToArchived_Succeeds()
    {
        var record = CreatePendingRecord();

        record.ApplyReview(AIRecommendationReviewStatus.Archived);

        record.ReviewStatus.Should().Be(AIRecommendationReviewStatus.Archived);
    }

    [Fact]
    public void ApplyReview_AcceptedToAccepted_AllowsUpdatingPlanAndDecision()
    {
        var record = CreatePendingRecord();
        record.ApplyReview(AIRecommendationReviewStatus.Accepted, "Initial approval", "Initial plan");

        record.ApplyReview(AIRecommendationReviewStatus.Accepted, "Updated decision note", "Updated plan");

        record.ReviewStatus.Should().Be(AIRecommendationReviewStatus.Accepted);
        record.CoachDecisionNote.Should().Be("Updated decision note");
        record.FinalImplementedPlan.Should().Be("Updated plan");
    }

    [Fact]
    public void ApplyReview_AcceptedToArchived_Succeeds()
    {
        var record = CreatePendingRecord();
        record.ApplyReview(AIRecommendationReviewStatus.Accepted, "Initial approval");

        record.ApplyReview(AIRecommendationReviewStatus.Archived);

        record.ReviewStatus.Should().Be(AIRecommendationReviewStatus.Archived);
    }

    [Fact]
    public void ApplyReview_RejectedToArchived_Succeeds()
    {
        var record = CreatePendingRecord();
        record.ApplyReview(AIRecommendationReviewStatus.Rejected);

        record.ApplyReview(AIRecommendationReviewStatus.Archived);

        record.ReviewStatus.Should().Be(AIRecommendationReviewStatus.Archived);
    }

    [Theory]
    [InlineData(AIRecommendationReviewStatus.PendingReview)]
    [InlineData(AIRecommendationReviewStatus.UnderReview)]
    [InlineData(AIRecommendationReviewStatus.Rejected)]
    public void ApplyReview_AcceptedToInvalidStatus_ThrowsInvalidOperationException(AIRecommendationReviewStatus invalidTarget)
    {
        var record = CreatePendingRecord();
        record.ApplyReview(AIRecommendationReviewStatus.Accepted, "Initial approval");

        var act = () => record.ApplyReview(invalidTarget);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot transition from Accepted*");
    }

    [Theory]
    [InlineData(AIRecommendationReviewStatus.PendingReview)]
    [InlineData(AIRecommendationReviewStatus.UnderReview)]
    [InlineData(AIRecommendationReviewStatus.Accepted)]
    public void ApplyReview_RejectedToInvalidStatus_ThrowsInvalidOperationException(AIRecommendationReviewStatus invalidTarget)
    {
        var record = CreatePendingRecord();
        record.ApplyReview(AIRecommendationReviewStatus.Rejected);

        var act = () => record.ApplyReview(invalidTarget, coachDecision: "Attempting transition");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot transition from Rejected*");
    }

    [Theory]
    [InlineData(AIRecommendationReviewStatus.PendingReview)]
    [InlineData(AIRecommendationReviewStatus.UnderReview)]
    [InlineData(AIRecommendationReviewStatus.Accepted)]
    [InlineData(AIRecommendationReviewStatus.Rejected)]
    [InlineData(AIRecommendationReviewStatus.Archived)]
    public void ApplyReview_ArchivedToAnyStatus_ThrowsInvalidOperationException(AIRecommendationReviewStatus target)
    {
        var record = CreatePendingRecord();
        record.ApplyReview(AIRecommendationReviewStatus.Archived);

        var act = () => record.ApplyReview(target, coachDecision: "Any text");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Archived recommendations cannot undergo state transitions*");
    }
}
