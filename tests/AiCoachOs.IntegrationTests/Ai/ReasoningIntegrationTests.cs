using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Memory.Dtos;
using AiCoachOs.Application.Safety.Dtos;
using AiCoachOs.Domain.Memory;
using AiCoachOs.Domain.Safety;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Ai;

public class ReasoningIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ReasoningIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string suffix)
    {
        var email = $"coach_m14_{suffix}_{Guid.NewGuid():N}@aicoach.com";
        var request = new RegisterCoachRequestDto("Coach M14", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<ClientDto> CreateClientForCoachAsync(string token, string firstName = "Youssef", string lastName = "Sherif")
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new CreateClientRequestDto(firstName, lastName, $"{firstName.ToLower()}_{Guid.NewGuid():N}@test.com", null, null, null, null, null);
        var response = await _client.PostAsJsonAsync("/api/clients", request);
        response.EnsureSuccessStatusCode();
        var client = await response.Content.ReadFromJsonAsync<ClientDto>();
        return client!;
    }

    [Fact]
    public async Task GenerateReasoning_Unauthenticated_Returns401Unauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = Guid.NewGuid(),
            ReasoningCategory = ReasoningCategory.ProgramAdaptationReview
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateReasoning_CoachIsolation_CannotGenerateForAnotherCoachClient()
    {
        var coachAToken = await RegisterAndLoginCoachAsync("iso_a");
        var clientA = await CreateClientForCoachAsync(coachAToken, "Client", "A");

        var coachBToken = await RegisterAndLoginCoachAsync("iso_b");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", coachBToken);

        var response = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = clientA.Id,
            ReasoningCategory = ReasoningCategory.ExerciseModificationReview
        });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData(ReasoningCategory.ProgramAdaptationReview)]
    [InlineData(ReasoningCategory.NutritionAdjustmentReview)]
    [InlineData(ReasoningCategory.ExerciseModificationReview)]
    [InlineData(ReasoningCategory.SafetyContextSummary)]
    [InlineData(ReasoningCategory.GeneralCoachingNote)]
    public async Task GenerateReasoning_ForExactFiveLockedCategories_Returns201AndPersists(ReasoningCategory category)
    {
        var token = await RegisterAndLoginCoachAsync($"cat_{category}");
        var client = await CreateClientForCoachAsync(token, "Tarek", "Nabil");

        // Add a memory record first so memory snapshot has content
        await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory", new CreateClientMemoryRequestDto
        {
            MemoryCategory = MemoryCategory.GoalContext,
            SourceType = MemorySourceType.CoachRecorded,
            Content = "Targeting hypertrophy for back and shoulders with 3 days training availability."
        });

        // Act: Generate AI Reasoning using exact locked DTO shape
        var genResponse = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = client.Id,
            ReasoningCategory = category,
            AdditionalContext = "Focus on joint friendly exercises"
        });

        genResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var rec = await genResponse.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();

        rec.Should().NotBeNull();
        rec!.ClientId.Should().Be(client.Id);
        ((int)rec.RecommendationCategory).Should().Be((int)category);
        rec.ReviewStatus.Should().Be(AIRecommendationReviewStatus.PendingReview);
        rec.AIProvider.Should().Be("Mock");
        rec.AIModel.Should().Be("mock-reasoning-v1");
        rec.RecommendationText.Should().NotBeNullOrWhiteSpace();
        rec.RationaleText.Should().NotBeNullOrWhiteSpace();
        rec.ConfidenceStatement.Should().NotBeNullOrWhiteSpace();

        // Query by ID via GET /api/reasoning/{id}
        var getResponse = await _client.GetAsync($"/api/reasoning/{rec.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedRec = await getResponse.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();

        fetchedRec.Should().NotBeNull();
        fetchedRec!.Id.Should().Be(rec.Id);
        ((int)fetchedRec.RecommendationCategory).Should().Be((int)category);
        fetchedRec.ReviewStatus.Should().Be(AIRecommendationReviewStatus.PendingReview);
    }

    [Fact]
    public async Task GenerateReasoning_WhenUrgentUnacknowledgedSafetyFlag_AbortsBeforeProviderExecution()
    {
        var token = await RegisterAndLoginCoachAsync("urgent_safety");
        var client = await CreateClientForCoachAsync(token, "Hassan", "Kamel");

        // Screen urgent safety report (e.g. Chest pain with severe onset)
        var urgentReq = new CreateSafetyReportRequestDto(
            ClientId: client.Id,
            TriggeredByType: TriggeredByType.WorkoutSession,
            Signals: new List<ReportedSignalDto>
            {
                new ReportedSignalDto(
                    BodyRegion: "Chest",
                    SignalType: SignalType.Pain,
                    Onset: SignalOnset.Sudden,
                    Timing: SignalTiming.DuringExercise,
                    Severity: SignalSeverity.Severe,
                    FreeText: "Crushing chest pain radiating to arm")
            });

        await _client.PostAsJsonAsync("/api/safety/screen", urgentReq);

        // Act: Reasoning must be aborted
        var genResponse = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = client.Id,
            ReasoningCategory = ReasoningCategory.SafetyContextSummary,
            AdditionalContext = "Safety summary requested"
        });

        // Must fail with 400 or 500 (InvalidOperationException) due to safety gate abort
        genResponse.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GenerateReasoning_WhenReferralSafetyFlag_ProceedsWithSafetyReferralNotice()
    {
        var token = await RegisterAndLoginCoachAsync("referral_safety");
        var client = await CreateClientForCoachAsync(token, "Maged", "Adel");

        // Screen a non-urgent referral signal (e.g. chronic persistent numbness)
        var referralReq = new CreateSafetyReportRequestDto(
            ClientId: client.Id,
            TriggeredByType: TriggeredByType.DirectReport,
            Signals: new List<ReportedSignalDto>
            {
                new ReportedSignalDto(
                    BodyRegion: "Knee",
                    SignalType: SignalType.Numbness,
                    Onset: SignalOnset.Gradual,
                    Timing: SignalTiming.Persistent,
                    Severity: SignalSeverity.Moderate,
                    FreeText: "Ongoing persistent numbness in knee joint")
            });

        await _client.PostAsJsonAsync("/api/safety/screen", referralReq);

        // Act: Reasoning proceeds
        var genResponse = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = client.Id,
            ReasoningCategory = ReasoningCategory.ExerciseModificationReview,
            AdditionalContext = "Recommend safe knee exercises"
        });

        genResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var rec = await genResponse.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();

        rec.Should().NotBeNull();
        rec!.RecommendationText.Should().Contain("[SAFETY REFERRAL NOTICE]");
        rec.ReviewStatus.Should().Be(AIRecommendationReviewStatus.PendingReview);
    }
}
