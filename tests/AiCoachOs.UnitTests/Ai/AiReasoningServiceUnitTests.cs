using System.Text.Json;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Models;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Memory;
using AiCoachOs.Infrastructure.Ai;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AiCoachOs.UnitTests.Ai;

public class AiReasoningServiceUnitTests
{
    [Fact]
    public async Task MockAiProvider_ReturnsDeterministicStructuredResponse_WithoutFabricatingEvidenceRefs()
    {
        var provider = new MockAiProvider();
        provider.ProviderName.Should().Be("Mock");
        provider.DefaultModelName.Should().Be("mock-reasoning-v1");

        var claimId1 = Guid.NewGuid();
        var claimId2 = Guid.NewGuid();

        var request = new AiCompletionRequest
        {
            SystemPrompt = "System instruction",
            UserPrompt = $"Please reason with Claim 1: {claimId1} and Claim 2: {claimId2}"
        };

        var response = await provider.GenerateStructuredAsync(request);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.ProviderName.Should().Be("Mock");
        response.ModelName.Should().Be("mock-reasoning-v1");
        response.RecommendationText.Should().NotBeNullOrWhiteSpace();
        response.RationaleText.Should().NotBeNullOrWhiteSpace();
        response.ConfidenceStatement.Should().NotBeNullOrWhiteSpace();

        // The mock provider must not echo prompt-supplied GUIDs back as evidence citations.
        // It never reads the underlying claims, so presenting them as verified evidence would
        // put fabricated citations into the coach-facing decision package.
        response.EvidenceClaimRefs.Should().BeEmpty();
        response.StructuredRecommendation!.EvidenceRefs.Should().BeEmpty();
        response.StructuredRecommendation.ConfidenceStatement.Should().Contain("None");

        // Determinism: the same request yields the same response.
        var repeated = await provider.GenerateStructuredAsync(request);
        repeated.RecommendationText.Should().Be(response.RecommendationText);
        repeated.TokensUsed.Should().Be(0);
    }

    [Fact]
    public async Task MockAiProvider_AnalyzeVideoAsync_ReturnsSuccessfulObservation()
    {
        var provider = new MockAiProvider();
        var response = await provider.AnalyzeVideoAsync(new AiCoachOs.Application.Ai.Dtos.AiVideoRequest
        {
            Frames = new List<AiCoachOs.Application.Ai.Dtos.VideoFrame>
            {
                new() { FrameIndex = 1, TimestampSeconds = 1.0m, ImageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 }, MimeType = "image/jpeg" }
            },
            UserPrompt = "Analyze squat technique"
        });

        response.IsSuccess.Should().BeTrue();
        response.RecommendationText.Should().Contain("movement_execution_notes");
    }

    [Theory]
    [InlineData(ReasoningCategory.ProgramAdaptationReview)]
    [InlineData(ReasoningCategory.NutritionAdjustmentReview)]
    [InlineData(ReasoningCategory.ExerciseModificationReview)]
    [InlineData(ReasoningCategory.SafetyContextSummary)]
    [InlineData(ReasoningCategory.GeneralCoachingNote)]
    public void ReasoningCategory_ContainsExactLockedFiveCategories(ReasoningCategory category)
    {
        Enum.IsDefined(typeof(ReasoningCategory), category).Should().BeTrue();
        ((int)category).Should().BeInRange(1, 5);
    }

    [Fact]
    public void GenerateReasoningRequestDto_JsonSerialization_UsesLockedPropertyNames()
    {
        var clientId = Guid.NewGuid();
        var json = $$"""
        {
            "clientId": "{{clientId}}",
            "reasoningCategory": "ProgramAdaptationReview",
            "additionalContext": "Client reports lower back fatigue"
        }
        """;

        var dto = JsonSerializer.Deserialize<GenerateReasoningRequestDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        dto.Should().NotBeNull();
        dto!.ClientId.Should().Be(clientId);
        dto.ReasoningCategory.Should().Be(ReasoningCategory.ProgramAdaptationReview);
        dto.AdditionalContext.Should().Be("Client reports lower back fatigue");
    }

    [Fact]
    public void AnthropicModel_DefaultModelIsClaudeSonnet46()
    {
        var settings = new AiSettings();
        settings.Model.Should().Be("claude-sonnet-4-6");

        var provider = new AnthropicAiProvider(
            new HttpClient(),
            Options.Create(settings),
            NullLogger<AnthropicAiProvider>.Instance);

        provider.DefaultModelName.Should().Be("claude-sonnet-4-6");
    }

    [Fact]
    public async Task AnthropicAiProvider_WhenApiKeyMissing_ReturnsErrorWithoutThrowing()
    {
        var settings = Options.Create(new AiSettings
        {
            Provider = "Anthropic",
            AnthropicApiKey = "" // Missing
        });

        var client = new HttpClient();
        var provider = new AnthropicAiProvider(client, settings, NullLogger<AnthropicAiProvider>.Instance);

        var request = new AiCompletionRequest
        {
            SystemPrompt = "System",
            UserPrompt = "User"
        };

        var response = await provider.GenerateStructuredAsync(request);

        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("API key is missing");
    }

    [Fact]
    public void StructuredRecommendation_DeserializesWithAllTenLockedFields()
    {
        var claimId = Guid.NewGuid();
        var json = $$"""
        {
            "summary": "Adjust daily protein target to 2.0 g/kg",
            "observations": ["Client is in resistance training phase", "Bodyweight trending slightly downwards"],
            "recommendations": ["Distribute protein across 4 meals", "Maintain 2.0 g/kg minimum daily intake"],
            "rationale": "Supported by evidence in resistance trained populations",
            "confidence_statement": "High certainty based on meta-analyses",
            "assumptions": ["Client is meeting caloric requirements"],
            "missing_high_value_data": ["Accurate dietary adherence logs"],
            "evidence_refs": ["{{claimId}}"],
            "safety_summary": null,
            "coach_action_required": true
        }
        """;

        var payload = JsonSerializer.Deserialize<StructuredRecommendation>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        payload.Should().NotBeNull();
        payload!.Summary.Should().Be("Adjust daily protein target to 2.0 g/kg");
        payload.Observations.Should().HaveCount(2);
        payload.Observations.Should().Contain("Client is in resistance training phase");
        payload.Recommendations.Should().HaveCount(2);
        payload.Rationale.Should().Be("Supported by evidence in resistance trained populations");
        payload.ConfidenceStatement.Should().Be("High certainty based on meta-analyses");
        payload.Assumptions.Should().Contain("Client is meeting caloric requirements");
        payload.MissingHighValueData.Should().Contain("Accurate dietary adherence logs");
        payload.EvidenceRefs.Should().Contain(claimId);
        payload.SafetySummary.Should().BeNull();
        payload.CoachActionRequired.Should().BeTrue();
    }

    [Fact]
    public void EvidenceEligibility_ActiveAndConfirmed_IsEligible_WhileActiveUnconfirmed_IsIneligible()
    {
        // Arrange
        var confirmedClaim = new KnowledgeClaim(
            Guid.NewGuid(),
            "Hypertrophy",
            "What is optimal protein intake?",
            "1.6 to 2.2 g/kg/day supports maximal muscle growth.",
            EvidenceLevel.MetaAnalysis,
            ClaimStatus.Active,
            reviewedAtUtc: DateTime.UtcNow,
            reviewedBy: "Dr. Brad Schoenfeld");

        var unconfirmedClaim = new KnowledgeClaim(
            Guid.NewGuid(),
            "Hypertrophy",
            "What is optimal set volume?",
            "10-20 weekly sets per muscle group.",
            EvidenceLevel.ExpertConsensus,
            ClaimStatus.Active,
            reviewedAtUtc: null,
            reviewedBy: null);

        var list = new List<KnowledgeClaim> { confirmedClaim, unconfirmedClaim };

        // Act: Filter by locked M14 rule (Active + Confirmed)
        var eligible = list
            .Where(k => k.Status == ClaimStatus.Active && k.ReviewedAtUtc != null && !string.IsNullOrWhiteSpace(k.ReviewedBy))
            .ToList();

        // Assert
        eligible.Should().ContainSingle();
        eligible[0].Id.Should().Be(confirmedClaim.Id);
        eligible.Should().NotContain(unconfirmedClaim);
    }

    [Fact]
    public void AIRecommendationRecord_EnforcesPendingReviewStatus_AndNonEmptyConfidence()
    {
        var recordId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var coachId = Guid.NewGuid();

        var record = new AIRecommendationRecord(
            id: recordId,
            clientId: clientId,
            coachId: coachId,
            recommendationCategory: AIRecommendationCategory.ProgramAdaptationReview,
            recommendationText: "Reduce weekly volume by 20%",
            rationaleText: "Accumulated systemic fatigue detected",
            confidenceStatement: "High confidence based on ACWR data",
            aiProvider: "claude-provider",
            aiModel: "claude-sonnet-4-6",
            knowledgeClaimRefs: new List<Guid> { Guid.NewGuid() },
            generatedAt: DateTime.UtcNow);

        record.ReviewStatus.Should().Be(AIRecommendationReviewStatus.PendingReview);
        record.ConfidenceStatement.Should().Be("High confidence based on ACWR data");
        record.RecommendationCategory.Should().Be(AIRecommendationCategory.ProgramAdaptationReview);
    }
}
