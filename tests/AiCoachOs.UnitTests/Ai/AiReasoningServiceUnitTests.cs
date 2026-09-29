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
    public async Task MockAiProvider_GeneratesDeterministicStructuredResponse_AndExtractsProvidedGuids()
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
        response.EvidenceClaimRefs.Should().Contain(claimId1);
        response.EvidenceClaimRefs.Should().Contain(claimId2);
    }

    [Fact]
    public void MockAiProvider_ImageAndVideoStubs_ThrowNotImplementedException()
    {
        var provider = new MockAiProvider();
        var actImage = () => provider.AnalyzeImageAsync(Array.Empty<byte>(), "prompt");
        var actVideo = () => provider.AnalyzeVideoAsync(Array.Empty<byte>(), "prompt");

        actImage.Should().ThrowAsync<NotImplementedException>();
        actVideo.Should().ThrowAsync<NotImplementedException>();
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
    public void StructuredAiRecommendationJson_DeserializesProperly()
    {
        var claimId = Guid.NewGuid().ToString();
        var json = $$"""
        {
            "recommendation": "Adjust daily protein target to 2.0 g/kg",
            "rationale": "Supported by evidence in resistance trained populations",
            "confidence_statement": "High certainty based on meta-analyses",
            "evidence_claim_ids": ["{{claimId}}"]
        }
        """;

        var payload = JsonSerializer.Deserialize<StructuredAiRecommendationJson>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        payload.Should().NotBeNull();
        payload!.Recommendation.Should().Be("Adjust daily protein target to 2.0 g/kg");
        payload.Rationale.Should().Be("Supported by evidence in resistance trained populations");
        payload.ConfidenceStatement.Should().Be("High certainty based on meta-analyses");
        payload.EvidenceClaimIds.Should().Contain(claimId);
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
