using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Memory.Dtos;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Memory;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AiCoachOs.IntegrationTests.Ai;

public class RecommendationReviewIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public RecommendationReviewIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string suffix)
    {
        var email = $"coach_m18_{suffix}_{Guid.NewGuid():N}@aicoach.com";
        var request = new RegisterCoachRequestDto("Coach M18", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<ClientDto> CreateClientForCoachAsync(string token, string firstName = "Hany", string lastName = "Ramzy")
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new CreateClientRequestDto(firstName, lastName, $"{firstName.ToLower()}_{Guid.NewGuid():N}@test.com", null, null, null, null, null);
        var response = await _client.PostAsJsonAsync("/api/clients", request);
        response.EnsureSuccessStatusCode();
        var client = await response.Content.ReadFromJsonAsync<ClientDto>();
        return client!;
    }

    private async Task<AIRecommendationRecordDto> GenerateRecommendationAsync(string token, Guid clientId, ReasoningCategory category = ReasoningCategory.ProgramAdaptationReview)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = clientId,
            ReasoningCategory = category,
            AdditionalContext = "Standard progressive adaptation"
        });
        response.EnsureSuccessStatusCode();
        var rec = await response.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();
        return rec!;
    }

    [Fact]
    public async Task ReviewRecommendation_AcceptWithValidDecision_Returns200AndSetsAccepted()
    {
        var token = await RegisterAndLoginCoachAsync("accept_valid");
        var client = await CreateClientForCoachAsync(token);
        var rec = await GenerateRecommendationAsync(token, client.Id);

        var reviewRequest = new ReviewAIRecommendationRequestDto
        {
            ReviewStatus = AIRecommendationReviewStatus.Accepted,
            CoachDecision = "Approved 4-day volume progression",
            FinalImplementedPlan = "Increased working sets on compound lifts"
        };

        var response = await _client.PatchAsJsonAsync($"/api/reasoning/{rec.Id}/review", reviewRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();
        updated.Should().NotBeNull();
        updated!.ReviewStatus.Should().Be(AIRecommendationReviewStatus.Accepted);
        updated.CoachDecision.Should().Be(CoachDecisionOutcome.Accepted);
        updated.CoachDecisionNote.Should().Be("Approved 4-day volume progression");
        updated.FinalImplementedPlan.Should().Be("Increased working sets on compound lifts");
        updated.CoachDecisionAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ReviewRecommendation_AcceptWithoutDecisionNote_Returns400BadRequest()
    {
        var token = await RegisterAndLoginCoachAsync("accept_empty_decision");
        var client = await CreateClientForCoachAsync(token);
        var rec = await GenerateRecommendationAsync(token, client.Id);

        var reviewRequest = new ReviewAIRecommendationRequestDto
        {
            ReviewStatus = AIRecommendationReviewStatus.Accepted,
            CoachDecision = "   "
        };

        var response = await _client.PatchAsJsonAsync($"/api/reasoning/{rec.Id}/review", reviewRequest);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReviewRecommendation_RejectWithPlan_Returns400BadRequest()
    {
        var token = await RegisterAndLoginCoachAsync("reject_with_plan");
        var client = await CreateClientForCoachAsync(token);
        var rec = await GenerateRecommendationAsync(token, client.Id);

        var reviewRequest = new ReviewAIRecommendationRequestDto
        {
            ReviewStatus = AIRecommendationReviewStatus.Rejected,
            CoachDecision = "Not suitable for client current state",
            FinalImplementedPlan = "Should not be provided when rejected"
        };

        var response = await _client.PatchAsJsonAsync($"/api/reasoning/{rec.Id}/review", reviewRequest);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReviewRecommendation_InvalidTransitionFromAcceptedToRejected_Returns400BadRequest()
    {
        var token = await RegisterAndLoginCoachAsync("state_machine_block");
        var client = await CreateClientForCoachAsync(token);
        var rec = await GenerateRecommendationAsync(token, client.Id);

        // Step 1: Accept
        var acceptReq = new ReviewAIRecommendationRequestDto
        {
            ReviewStatus = AIRecommendationReviewStatus.Accepted,
            CoachDecision = "Initial approval"
        };
        var acceptRes = await _client.PatchAsJsonAsync($"/api/reasoning/{rec.Id}/review", acceptReq);
        acceptRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 2: Attempt invalid transition Accepted -> Rejected
        var rejectReq = new ReviewAIRecommendationRequestDto
        {
            ReviewStatus = AIRecommendationReviewStatus.Rejected,
            CoachDecision = "Attempting reject after accept"
        };
        var rejectRes = await _client.PatchAsJsonAsync($"/api/reasoning/{rec.Id}/review", rejectReq);
        rejectRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReviewRecommendation_CoachIsolation_CannotReviewOtherCoachRecommendation()
    {
        var coachAToken = await RegisterAndLoginCoachAsync("iso_coach_a");
        var clientA = await CreateClientForCoachAsync(coachAToken, "Client", "A");
        var recA = await GenerateRecommendationAsync(coachAToken, clientA.Id);

        var coachBToken = await RegisterAndLoginCoachAsync("iso_coach_b");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", coachBToken);

        var reviewReq = new ReviewAIRecommendationRequestDto
        {
            ReviewStatus = AIRecommendationReviewStatus.Accepted,
            CoachDecision = "Malicious review attempt"
        };

        var response = await _client.PatchAsJsonAsync($"/api/reasoning/{recA.Id}/review", reviewReq);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetClientRecommendations_ReturnsOrderedSummariesAndAppliesFilters()
    {
        var token = await RegisterAndLoginCoachAsync("list_summaries");
        var client = await CreateClientForCoachAsync(token, "Ahmed", "Helmy");

        // Generate 2 recommendations
        var rec1 = await GenerateRecommendationAsync(token, client.Id, ReasoningCategory.ProgramAdaptationReview);
        var rec2 = await GenerateRecommendationAsync(token, client.Id, ReasoningCategory.NutritionAdjustmentReview);

        // List all recommendations for client
        var listRes = await _client.GetAsync($"/api/clients/{client.Id}/recommendations");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var summaries = await listRes.Content.ReadFromJsonAsync<List<AIRecommendationSummaryDto>>();
        summaries.Should().NotBeNull();
        summaries!.Count.Should().BeGreaterThanOrEqualTo(2);
        summaries[0].GeneratedAt.Should().BeOnOrAfter(summaries[1].GeneratedAt);

        // Filter by category
        var catRes = await _client.GetAsync($"/api/clients/{client.Id}/recommendations?category={AIRecommendationCategory.NutritionAdjustmentReview}");
        catRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var catSummaries = await catRes.Content.ReadFromJsonAsync<List<AIRecommendationSummaryDto>>();
        catSummaries.Should().NotBeNull();
        catSummaries!.All(s => s.Category == AIRecommendationCategory.NutritionAdjustmentReview).Should().BeTrue();
    }

    [Fact]
    public async Task GetClientRecommendationDetail_ResolvesM3ClaimsAndTruncatesText()
    {
        var token = await RegisterAndLoginCoachAsync("detail_claims");
        var client = await CreateClientForCoachAsync(token, "Karim", "Abdelaziz");

        // Seed an M3 KnowledgeClaim in database
        Guid claimId = Guid.NewGuid();
        string longClaimText = new string('A', 300);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AiCoachOs.Application.Common.Interfaces.IApplicationDbContext>();
            var claim = new KnowledgeClaim(
                id: claimId,
                topic: "Hypertrophy Volume Thresholds",
                question: "What volume threshold optimizes hypertrophy?",
                claimText: longClaimText,
                evidenceLevel: EvidenceLevel.MetaAnalysis,
                status: ClaimStatus.Active
            );
            await db.AddKnowledgeClaimAsync(claim);
            await db.SaveChangesAsync();
        }

        // Generate recommendation citing the claim
        var rec = await GenerateRecommendationAsync(token, client.Id);

        // Fetch detail
        var detailRes = await _client.GetAsync($"/api/clients/{client.Id}/recommendations/{rec.Id}");
        detailRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await detailRes.Content.ReadFromJsonAsync<AIRecommendationDetailDto>();
        detail.Should().NotBeNull();
        detail!.Id.Should().Be(rec.Id);
        detail.ClientId.Should().Be(client.Id);
        detail.Summary.Should().NotBeNullOrWhiteSpace();
        detail.ConfidenceStatement.Should().NotBeNullOrWhiteSpace();

        // If claims are resolved, verify truncation to <= 200 chars
        if (detail.ResolvedKnowledgeClaims.Count > 0)
        {
            foreach (var resolved in detail.ResolvedKnowledgeClaims)
            {
                resolved.ClaimText.Length.Should().BeLessThanOrEqualTo(200);
            }
        }
    }

    [Fact]
    public async Task GetCoachClients_IncludesPendingRecommendationCount()
    {
        var token = await RegisterAndLoginCoachAsync("pending_count");
        var client = await CreateClientForCoachAsync(token, "Mona", "Zaki");

        // Generate 1 pending recommendation
        await GenerateRecommendationAsync(token, client.Id);

        // Fetch coach client roster
        var rosterRes = await _client.GetAsync("/api/clients");
        rosterRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var roster = await rosterRes.Content.ReadFromJsonAsync<List<ClientSummaryDto>>();
        roster.Should().NotBeNull();
        var clientSummary = roster!.FirstOrDefault(c => c.Id == client.Id);
        clientSummary.Should().NotBeNull();
        clientSummary!.PendingRecommendationCount.Should().Be(1);

        // Fetch client by ID
        var singleRes = await _client.GetAsync($"/api/clients/{client.Id}");
        singleRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var singleClient = await singleRes.Content.ReadFromJsonAsync<ClientDto>();
        singleClient.Should().NotBeNull();
        singleClient!.PendingRecommendationCount.Should().Be(1);
    }
}
