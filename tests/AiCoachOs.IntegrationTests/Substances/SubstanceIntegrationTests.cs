using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Substances.Dtos;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Substances;
using AiCoachOs.Infrastructure.Persistence;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AiCoachOs.IntegrationTests.Substances;

public class SubstanceIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private static readonly object _fixtureLock = new();
    private static bool _fixturesSeeded = false;

    public SubstanceIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        EnsureSyntheticFixtures();
    }

    private void EnsureSyntheticFixtures()
    {
        lock (_fixtureLock)
        {
            if (_fixturesSeeded) return;

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Clean old substance records in test db directly in SQL
            context.Database.ExecuteSqlRaw("DELETE FROM \"SubstanceEscalationRecords\"; DELETE FROM \"PEDRiskRecords\"; DELETE FROM \"PEDRedFlagRules\"; DELETE FROM \"Substances\";");

            // 1. Synthetic Sources
            foreach (var s in SubstanceSeedData.GetKnowledgeSources())
            {
                if (!context.KnowledgeSourcesDbSet.Any(x => x.Id == s.Id))
                    context.KnowledgeSourcesDbSet.Add(s);
            }
            context.SaveChanges();

            // 2. Synthetic Claims
            foreach (var c in SubstanceSeedData.GetKnowledgeClaims())
            {
                if (!context.KnowledgeClaimsDbSet.Any(x => x.Id == c.Id))
                    context.KnowledgeClaimsDbSet.Add(c);
            }
            context.SaveChanges();

            // 3. Synthetic Supplements
            foreach (var sup in SubstanceSeedData.GetSupplements())
            {
                if (!context.SubstancesDbSet.Any(x => x.Id == sup.Id))
                    context.SubstancesDbSet.Add(sup);
            }
            context.SaveChanges();

            // 4. Synthetic Hormones
            foreach (var h in SubstanceSeedData.GetHormones())
            {
                if (!context.SubstancesDbSet.Any(x => x.Id == h.Id))
                    context.SubstancesDbSet.Add(h);
            }
            context.SaveChanges();

            // 5. Synthetic PEDs
            foreach (var ped in SubstanceSeedData.GetPEDSafetyRecords())
            {
                if (!context.SubstancesDbSet.Any(x => x.Id == ped.Id))
                    context.SubstancesDbSet.Add(ped);
            }
            context.SaveChanges();

            // 6. Synthetic Rules
            foreach (var r in SubstanceSeedData.GetPEDRedFlagRules())
            {
                if (!context.PEDRedFlagRulesDbSet.Any(x => x.Id == r.Id))
                    context.PEDRedFlagRulesDbSet.Add(r);
            }
            context.SaveChanges();

            _fixturesSeeded = true;
        }
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
    public async Task GetSupplements_WithFiltering_ReturnsFilteredResults()
    {
        var token = await RegisterAndLoginCoachAsync("supp_filter");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/supplements?name=creatine");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await response.Content.ReadFromJsonAsync<List<SupplementKnowledgeSummaryDto>>();
        list.Should().NotBeNull();
        list!.Should().ContainSingle(s => s.Name == "Creatine Monohydrate");
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
        detail.PrimaryClaimedBenefit.Should().NotBeNullOrWhiteSpace();
        detail.TypicalDoseRangeMin.Should().Be(3m);
        detail.TypicalDoseRangeMax.Should().Be(5m);
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
        detail.TrainingRelevance.Should().NotBeNullOrWhiteSpace();
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
        detail.Risks.Should().Contain(r => r.RiskCategory == RiskCategory.Cardiovascular);
    }

    [Fact]
    public async Task EvaluateSubstanceSafety_WhenCardiovascularEmergencyReported_ReturnsUrgentMedicalAttention()
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
        result!.EscalationLevel.Should().Be(EscalationLevel.UrgentMedicalAttention);
        result.MatchedRedFlags.Should().Contain("PED Cardiovascular Emergency Symptoms");
        result.EscalationRecordId.Should().NotBeNull();
    }

    [Fact]
    public async Task EvaluateSubstanceSafety_WhenJaundiceReported_ReturnsHealthcareProfessionalReferral()
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
        result!.EscalationLevel.Should().Be(EscalationLevel.HealthcareProfessionalReferral);
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

        var resp9 = await _client.GetAsync("/api/substance-safety/red-flags");
        resp9.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
