using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Knowledge.DTOs;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Infrastructure.Persistence;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Knowledge;

public class KnowledgeIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public KnowledgeIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync()
    {
        var email = $"coach_knowledge_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto("KnowledgeCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    [Fact]
    public async Task CreateAndRetrieveKnowledgeSource_ShouldPersistSuccessfully()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateKnowledgeSourceDto(
            KnowledgeSourceType.ScientificPaper,
            $"Dose-Response Volume Meta-Analysis {Guid.NewGuid():N}",
            "Schoenfeld et al.",
            2019,
            EvidenceLevel.MetaAnalysis,
            "10.1249/MSS.0000000000001764",
            "https://pubmed.ncbi.nlm.nih.gov/30153194/",
            "Key graded volume findings"
        );

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/knowledge/sources", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Title.Should().Be(request.Title);
        created.EvidenceLevel.Should().Be(EvidenceLevel.MetaAnalysis);

        // Act - Get By Id
        var getResponse = await _client.GetAsync($"/api/knowledge/sources/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<KnowledgeSourceDto>();
        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be(request.Title);

        // Act - List
        var listResponse = await _client.GetAsync("/api/knowledge/sources");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<KnowledgeSourceSummaryDto>>();
        list.Should().Contain(s => s.Id == created.Id);
    }

    [Fact]
    public async Task CreateGeneralClaim_WithNullExerciseId_ShouldPersistSuccessfully()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateKnowledgeClaimDto(
            "RestIntervals",
            "What is the effect of rest interval length on muscle growth?",
            "Rest intervals of 2+ minutes between compound sets yield greater hypertrophy than short (<1 min) rest intervals.",
            EvidenceLevel.MetaAnalysis,
            ClaimStatus.Active,
            ExerciseId: null,
            Population: "Trained resistance lifters",
            Limitations: "Metabolic conditioning or density training protocols were excluded.",
            PracticalApplication: "Rest at least 2-3 minutes on heavy compound movements to maintain volume load."
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/knowledge/claims", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var claim = await response.Content.ReadFromJsonAsync<KnowledgeClaimDto>();
        claim.Should().NotBeNull();
        claim!.Id.Should().NotBeEmpty();
        claim.ExerciseId.Should().BeNull();
        claim.Topic.Should().Be("RestIntervals");
        claim.Status.Should().Be(ClaimStatus.Active);
    }

    [Fact]
    public async Task CreateExerciseLinkedClaim_ShouldAssociateWithExercise()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var squatId = new Guid("44444444-4444-4444-4444-444444444401"); // Barbell Back Squat

        var request = new CreateKnowledgeClaimDto(
            "SquatDepth",
            "How does squat depth affect lower body hypertrophy?",
            "Full depth squats produce superior adductor and glute hypertrophy with equal quad hypertrophy compared to partials.",
            EvidenceLevel.RandomizedControlledTrial,
            ClaimStatus.Active,
            ExerciseId: squatId,
            Population: "Healthy young males",
            PracticalApplication: "Standardize depth below parallel unless limited by active joint pain."
        );

        // Act - Create claim
        var response = await _client.PostAsJsonAsync("/api/knowledge/claims", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var claim = await response.Content.ReadFromJsonAsync<KnowledgeClaimDto>();
        claim!.ExerciseId.Should().Be(squatId);
        claim.ExerciseName.Should().Contain("Squat");

        // Act - Query claims via exercise endpoint
        var exerciseClaimsResponse = await _client.GetAsync($"/api/exercises/{squatId}/claims");
        exerciseClaimsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var claimsList = await exerciseClaimsResponse.Content.ReadFromJsonAsync<IReadOnlyList<KnowledgeClaimSummaryDto>>();
        claimsList.Should().Contain(c => c.Id == claim.Id);
    }

    [Fact]
    public async Task AssociateSourceWithClaim_ShouldPersistAndReturnSupportingSources()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create Source
        var sourceRes = await _client.PostAsJsonAsync("/api/knowledge/sources", new CreateKnowledgeSourceDto(
            KnowledgeSourceType.ScientificPaper, "Muscle Protein Synthesis and Volume", "Phillips et al.", 2021, EvidenceLevel.RandomizedControlledTrial));
        var source = await sourceRes.Content.ReadFromJsonAsync<KnowledgeSourceDto>();

        // 2. Create Claim
        var claimRes = await _client.PostAsJsonAsync("/api/knowledge/claims", new CreateKnowledgeClaimDto(
            "Frequency", "Does training frequency impact hypertrophy when volume is matched?",
            "Volume-equated frequency shows similar hypertrophy between 1x, 2x, and 3x weekly per muscle group.",
            EvidenceLevel.MetaAnalysis));
        var claim = await claimRes.Content.ReadFromJsonAsync<KnowledgeClaimDto>();

        // Act - Link source to claim
        var linkRes = await _client.PostAsJsonAsync($"/api/knowledge/claims/{claim!.Id}/sources", new AddClaimSourceDto(
            source!.Id, "Direct RCT evidence matching frequency volume"));

        // Assert
        linkRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedClaim = await linkRes.Content.ReadFromJsonAsync<KnowledgeClaimDto>();
        updatedClaim!.Sources.Should().HaveCount(1);
        updatedClaim.Sources.First().SourceId.Should().Be(source.Id);
        updatedClaim.Sources.First().RelevanceNote.Should().Contain("Direct RCT evidence");
    }

    [Fact]
    public async Task SupersedeClaim_WithReplacement_ShouldUpdateStatusAndVersioningChain()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create Old Claim
        var oldClaimRes = await _client.PostAsJsonAsync("/api/knowledge/claims", new CreateKnowledgeClaimDto(
            "RepRanges", "Are heavy loads strictly necessary for hypertrophy?",
            "Loads must exceed 70% 1RM to trigger optimal mechanical tension and muscle hypertrophy.",
            EvidenceLevel.Mechanistic, ClaimStatus.Active));
        var oldClaim = await oldClaimRes.Content.ReadFromJsonAsync<KnowledgeClaimDto>();

        // 2. Create Replacement Claim
        var newClaimRes = await _client.PostAsJsonAsync("/api/knowledge/claims", new CreateKnowledgeClaimDto(
            "RepRanges", "Are heavy loads strictly necessary for hypertrophy?",
            "Loads between 30% and 85% 1RM elicit comparable hypertrophy when sets are performed close to failure (0-3 RIR).",
            EvidenceLevel.MetaAnalysis, ClaimStatus.Active));
        var newClaim = await newClaimRes.Content.ReadFromJsonAsync<KnowledgeClaimDto>();

        // Act - Supersede old claim
        var supersedeRes = await _client.PostAsJsonAsync($"/api/knowledge/claims/{oldClaim!.Id}/supersede", new SupersedeClaimDto(
            newClaim!.Id, "Updated by comprehensive meta-analysis evaluating load ranges from 30% to 85% 1RM"));

        // Assert
        supersedeRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var supersededClaim = await supersedeRes.Content.ReadFromJsonAsync<KnowledgeClaimDto>();
        supersededClaim!.Status.Should().Be(ClaimStatus.Superseded);
        supersededClaim.SupersededByClaimId.Should().Be(newClaim.Id);
        supersededClaim.SupersededAtUtc.Should().NotBeNull();
        supersededClaim.SupersessionReason.Should().Contain("meta-analysis");

        // Verify replacement claim remains active
        var replacementFetch = await _client.GetAsync($"/api/knowledge/claims/{newClaim.Id}");
        var replacement = await replacementFetch.Content.ReadFromJsonAsync<KnowledgeClaimDto>();
        replacement!.Status.Should().Be(ClaimStatus.Active);
    }

    [Fact]
    public async Task UnauthenticatedAccess_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var responseSources = await _client.GetAsync("/api/knowledge/sources");
        var responseClaims = await _client.GetAsync("/api/knowledge/claims");

        // Assert
        responseSources.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        responseClaims.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
