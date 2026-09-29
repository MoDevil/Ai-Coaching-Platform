using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.ExpertIngestion.Dtos;
using AiCoachOs.Domain.ExpertIngestion;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.ExpertIngestion;

public class ExpertIngestionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ExpertIngestionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string? emailPrefix = null)
    {
        var prefix = emailPrefix ?? "coach_expert";
        var email = $"{prefix}_{Guid.NewGuid():N}@gym.eg";
        var request = new RegisterCoachRequestDto("ExpertCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    [Fact]
    public async Task UnauthenticatedRequests_ShouldReturn401Unauthorized()
    {
        var client = _factory.CreateClient();

        var getIngestions = await client.GetAsync("/api/expert-ingestions");
        getIngestions.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var postIngestion = await client.PostAsJsonAsync("/api/expert-ingestions", new SubmitIngestionRequestDto("https://youtube.com/watch?v=123"));
        postIngestion.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var getSources = await client.GetAsync("/api/expert-sources");
        getSources.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var postSource = await client.PostAsJsonAsync("/api/expert-sources", new CreateExpertSourceDto("Name", "Channel", ExpertPlatform.YouTube, "Domain", CredibilityTier.High));
        postSource.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAndRetrieveExpertSource_ShouldSucceed()
    {
        var token = await RegisterAndLoginCoachAsync("source_coach");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateExpertSourceDto(
            Name: "Dr. Eric Helms",
            ChannelOrPublication: "3D Muscle Journey",
            Platform: ExpertPlatform.YouTube,
            PrimaryDomain: "Natural Bodybuilding & Nutrition",
            CredibilityTier: CredibilityTier.High,
            Bio: "PhD, CSCS, Author of Muscle & Strength Pyramids");

        var response = await _client.PostAsJsonAsync("/api/expert-sources", createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ExpertSourceDto>();
        created.Should().NotBeNull();
        created!.Name.Should().Be(createDto.Name);
        created.Platform.Should().Be(ExpertPlatform.YouTube);

        var listResponse = await _client.GetAsync("/api/expert-sources");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var sources = await listResponse.Content.ReadFromJsonAsync<List<ExpertSourceDto>>();
        sources.Should().NotBeNull();
        sources!.Should().Contain(s => s.Id == created.Id);
    }

    [Fact]
    public async Task SubmitIngestion_DuplicateUrl_ShouldReturn409Conflict()
    {
        var token = await RegisterAndLoginCoachAsync("dup_coach");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = $"https://example.com/article/dup_{Guid.NewGuid():N}";
        var request = new SubmitIngestionRequestDto(SourceUrl: url, Title: "Volume Masterclass", ContentType: IngestionContentType.Article);

        // First submission -> 202 Accepted
        var firstResponse = await _client.PostAsJsonAsync("/api/expert-ingestions", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // Second submission with exact same URL for same coach -> 409 Conflict
        var secondResponse = await _client.PostAsJsonAsync("/api/expert-ingestions", request);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task FullIngestionWorkflow_Submit_Retrieve_ReviewClaim_ShouldSucceed()
    {
        var token = await RegisterAndLoginCoachAsync("flow_coach");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create Expert Source
        var sourceDto = new CreateExpertSourceDto(
            Name: "Renaissance Periodization",
            ChannelOrPublication: "RP Strength Web",
            Platform: ExpertPlatform.Article,
            PrimaryDomain: "Hypertrophy Science",
            CredibilityTier: CredibilityTier.High);

        var sourceResp = await _client.PostAsJsonAsync("/api/expert-sources", sourceDto);
        var source = await sourceResp.Content.ReadFromJsonAsync<ExpertSourceDto>();

        // 2. Submit Ingestion -> 202 Accepted
        var url = $"https://example.com/hypertrophy-volume-{Guid.NewGuid():N}";
        var submitDto = new SubmitIngestionRequestDto(
            SourceUrl: url,
            SourceId: source!.Id,
            Title: "Hypertrophy Volume Targets",
            ContentType: IngestionContentType.Article);

        var submitResp = await _client.PostAsJsonAsync("/api/expert-ingestions", submitDto);
        submitResp.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var summary = await submitResp.Content.ReadFromJsonAsync<ExpertContentIngestionSummaryDto>();
        summary.Should().NotBeNull();
        summary!.Id.Should().NotBeEmpty();

        // 3. Poll / Wait for pipeline completion
        ExpertContentIngestionDto? ingestionDetail = null;
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(300);
            var detailResp = await _client.GetAsync($"/api/expert-ingestions/{summary.Id}");
            if (detailResp.StatusCode == HttpStatusCode.OK)
            {
                ingestionDetail = await detailResp.Content.ReadFromJsonAsync<ExpertContentIngestionDto>();
                if (ingestionDetail != null && ingestionDetail.Status != IngestionStatus.Processing)
                {
                    break;
                }
            }
        }

        ingestionDetail.Should().NotBeNull();
        ingestionDetail!.Claims.Should().NotBeEmpty();

        var targetClaim = ingestionDetail.Claims.First();

        // 4. Review Claim -> Approve
        var reviewDto = new ReviewClaimRequestDto(
            Decision: ExpertClaimReviewStatus.Approved,
            Notes: "High evidence practical guideline.",
            CreateNewKnowledgeClaim: true,
            NewClaimQuestion: "What is the optimal weekly volume threshold?",
            PractitionerNotes: "Coach observation: adjust based on individual recovery capacity.",
            EgyptSpecificNotes: "Ensure protein sources like Egyptian eggs, fava beans, cottage cheese (areesh) support recovery.");

        var reviewResp = await _client.PatchAsJsonAsync(
            $"/api/expert-ingestions/{summary.Id}/claims/{targetClaim.Id}/review",
            reviewDto);

        reviewResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviewedClaim = await reviewResp.Content.ReadFromJsonAsync<ExpertClaimDto>();
        reviewedClaim.Should().NotBeNull();
        reviewedClaim!.ReviewStatus.Should().Be(ExpertClaimReviewStatus.Approved);
        reviewedClaim.ApprovedKnowledgeClaimId.Should().NotBeNull();
        reviewedClaim.CoachNotes.Should().Be(reviewDto.Notes);
    }

    [Fact]
    public async Task CrossCoachIsolation_CoachB_CannotAccess_CoachA_Ingestion()
    {
        var tokenA = await RegisterAndLoginCoachAsync("coach_a");
        var tokenB = await RegisterAndLoginCoachAsync("coach_b");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var url = $"https://youtube.com/watch?v=isolated_{Guid.NewGuid():N}";
        var submitDto = new SubmitIngestionRequestDto(SourceUrl: url, Title: "Private Analysis");

        var submitResp = await _client.PostAsJsonAsync("/api/expert-ingestions", submitDto);
        submitResp.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var summary = await submitResp.Content.ReadFromJsonAsync<ExpertContentIngestionSummaryDto>();

        // Coach B tries to read Coach A's ingestion -> 403 Forbidden
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var getResp = await _client.GetAsync($"/api/expert-ingestions/{summary!.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
