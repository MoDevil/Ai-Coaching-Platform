using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Substances.Dtos;
using AiCoachOs.Domain.Substances;
using AiCoachOs.Infrastructure.Persistence;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AiCoachOs.IntegrationTests.Substances;

public class SubstanceIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SubstanceIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string prefix = "coach_substance")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto("SubstanceCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    [Fact]
    public async Task GetSupplements_WhenAuthenticated_Returns200WithSupplements()
    {
        var token = await RegisterAndLoginCoachAsync("supp_list");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/supplements");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await response.Content.ReadFromJsonAsync<List<SupplementKnowledgeSummaryDto>>();
        list.Should().NotBeNull();
        list!.Count.Should().BeGreaterThanOrEqualTo(3);
        list.Should().Contain(s => s.Name == "Creatine Monohydrate");
        list.Should().Contain(s => s.Name == "Caffeine");
        list.Should().Contain(s => s.Name == "Whey Protein");
    }

    [Fact]
    public async Task GetSupplementById_Returns200WithUncertaintyAndEvidenceDetails()
    {
        var token = await RegisterAndLoginCoachAsync("supp_detail");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/supplements/{SubstanceSeedData.CreatineId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await response.Content.ReadFromJsonAsync<SupplementKnowledgeDto>();
        detail.Should().NotBeNull();
        detail!.Name.Should().Be("Creatine Monohydrate");
        detail.UncertaintyStatement.Should().NotBeNullOrWhiteSpace();
        detail.EvidenceSummary.Should().NotBeNullOrWhiteSpace();
        detail.TypicalDoseRange.Should().NotBeNullOrWhiteSpace();
        detail.EvidenceClaim.Should().NotBeNull();
        detail.EvidenceClaim!.SourceCitations.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetHormones_WhenAuthenticated_Returns200WithHormoneList()
    {
        var token = await RegisterAndLoginCoachAsync("hormone_list");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/hormones");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await response.Content.ReadFromJsonAsync<List<HormoneKnowledgeSummaryDto>>();
        list.Should().NotBeNull();
        list!.Count.Should().BeGreaterThanOrEqualTo(3);
        list.Should().Contain(h => h.Name.Contains("Testosterone"));
        list.Should().Contain(h => h.Name.Contains("Cortisol"));
    }

    [Fact]
    public async Task GetHormoneById_Returns200WithPhysiologicalRoleAndUncertainty()
    {
        var token = await RegisterAndLoginCoachAsync("hormone_detail");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/hormones/{SubstanceSeedData.TestosteroneId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await response.Content.ReadFromJsonAsync<HormoneKnowledgeDto>();
        detail.Should().NotBeNull();
        detail!.PhysiologicalRole.Should().NotBeNullOrWhiteSpace();
        detail.TrainingImpactSummary.Should().NotBeNullOrWhiteSpace();
        detail.UncertaintyStatement.Should().NotBeNullOrWhiteSpace();
        detail.BiomarkerReferenceNotes.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetPEDSafetyRecords_WhenAuthenticated_Returns200WithPEDSummaries()
    {
        var token = await RegisterAndLoginCoachAsync("ped_list");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/ped-safety");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await response.Content.ReadFromJsonAsync<List<PEDSafetyRecordSummaryDto>>();
        list.Should().NotBeNull();
        list!.Count.Should().BeGreaterThanOrEqualTo(2);
        list.Should().Contain(p => p.Name.Contains("Anabolic-Androgenic Steroids"));
    }

    [Fact]
    public async Task GetPEDSafetyRecordById_Returns200_WithDisclaimerAndOrganRisks_AndNoProhibitedFields()
    {
        var token = await RegisterAndLoginCoachAsync("ped_detail");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/ped-safety/{SubstanceSeedData.AasOverviewId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var rawJson = await response.Content.ReadAsStringAsync();
        rawJson.ToLowerInvariant().Should().NotContain("cyclelength");
        rawJson.ToLowerInvariant().Should().NotContain("dosagemg");
        rawJson.ToLowerInvariant().Should().NotContain("stackingprotocol");
        rawJson.ToLowerInvariant().Should().NotContain("pctprotocol");
        rawJson.ToLowerInvariant().Should().NotContain("sourcevendor");

        var detail = await response.Content.ReadFromJsonAsync<PEDSafetyRecordDto>();
        detail.Should().NotBeNull();
        detail!.SafetyDisclaimer.Should().Contain("HARM REDUCTION ONLY");
        detail.Risks.Should().NotBeEmpty();
        detail.Risks.Should().Contain(r => r.OrganSystem == OrganSystem.Cardiovascular);
    }

    [Fact]
    public async Task EvaluateSubstanceSafety_WhenCardiovascularEmergencyReported_ReturnsEmergencyEscalation()
    {
        var token = await RegisterAndLoginCoachAsync("coach_eval_emerg");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new EvaluateSubstanceSafetyRequestDto
        {
            SubstanceRecordId = SubstanceSeedData.AasOverviewId,
            ReportedSignals = new List<string> { "Client reported acute chest pressure and sudden shortness of breath after heavy sets" }
        };

        var response = await _client.PostAsJsonAsync("/api/substance-safety/evaluate", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SubstanceSafetyEvaluationResultDto>();
        result.Should().NotBeNull();
        result!.EscalationLevel.Should().Be(SubstanceEscalationLevel.EmergencyMedicalAttention);
        result.MatchedRedFlags.Should().Contain("PED Cardiovascular Emergency Symptoms");
        result.EscalationRecordId.Should().NotBeNull();
    }

    [Fact]
    public async Task EvaluateSubstanceSafety_WhenJaundiceReported_ReturnsUrgentMedicalReferral()
    {
        var token = await RegisterAndLoginCoachAsync("coach_eval_jaundice");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new EvaluateSubstanceSafetyRequestDto
        {
            SubstanceRecordId = SubstanceSeedData.SarmOverviewId,
            ReportedSignals = new List<string> { "Yellow eyes, dark tea-colored urine, and right upper quadrant discomfort" }
        };

        var response = await _client.PostAsJsonAsync("/api/substance-safety/evaluate", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SubstanceSafetyEvaluationResultDto>();
        result.Should().NotBeNull();
        result!.EscalationLevel.Should().Be(SubstanceEscalationLevel.UrgentMedicalReferral);
        result.MatchedRedFlags.Should().Contain("Hepatic Toxicity & Cholestatic Jaundice");
    }

    [Fact]
    public async Task CoachIsolation_CoachBCannotAccessCoachAEscalationRecords()
    {
        // 1. Coach A logs an evaluation
        var tokenA = await RegisterAndLoginCoachAsync("coach_a");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var requestA = new EvaluateSubstanceSafetyRequestDto
        {
            SubstanceRecordId = SubstanceSeedData.AasOverviewId,
            ReportedSignals = new List<string> { "Chest pain and palpitations" }
        };
        var evalRespA = await _client.PostAsJsonAsync("/api/substance-safety/evaluate", requestA);
        evalRespA.StatusCode.Should().Be(HttpStatusCode.OK);
        var evalA = await evalRespA.Content.ReadFromJsonAsync<SubstanceSafetyEvaluationResultDto>();

        // Coach A gets their escalations
        var listRespA = await _client.GetAsync("/api/substance-safety/escalations");
        listRespA.StatusCode.Should().Be(HttpStatusCode.OK);
        var listA = await listRespA.Content.ReadFromJsonAsync<List<SubstanceEscalationRecordDto>>();
        listA.Should().Contain(e => e.Id == evalA!.EscalationRecordId);

        // 2. Coach B registers and checks their escalations
        var tokenB = await RegisterAndLoginCoachAsync("coach_b");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        var listRespB = await _client.GetAsync("/api/substance-safety/escalations");
        listRespB.StatusCode.Should().Be(HttpStatusCode.OK);
        var listB = await listRespB.Content.ReadFromJsonAsync<List<SubstanceEscalationRecordDto>>();

        // Coach B MUST NOT see Coach A's records
        listB.Should().NotContain(e => e.Id == evalA!.EscalationRecordId);
    }

    [Fact]
    public async Task UnauthenticatedRequests_ToAllM12Endpoints_Return401Unauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null; // No token

        var resp1 = await _client.GetAsync("/api/supplements");
        resp1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var resp2 = await _client.GetAsync($"/api/supplements/{Guid.NewGuid()}");
        resp2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var resp3 = await _client.GetAsync("/api/hormones");
        resp3.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var resp4 = await _client.GetAsync($"/api/hormones/{Guid.NewGuid()}");
        resp4.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var resp5 = await _client.GetAsync("/api/ped-safety");
        resp5.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var resp6 = await _client.GetAsync($"/api/ped-safety/{Guid.NewGuid()}");
        resp6.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var resp7 = await _client.PostAsJsonAsync("/api/substance-safety/evaluate", new EvaluateSubstanceSafetyRequestDto());
        resp7.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var resp8 = await _client.GetAsync("/api/substance-safety/escalations");
        resp8.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var resp9 = await _client.GetAsync("/api/substance-safety/rules");
        resp9.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
