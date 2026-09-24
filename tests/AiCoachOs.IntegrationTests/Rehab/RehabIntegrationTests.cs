using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Programs.DTOs;
using AiCoachOs.Application.Rehab.Dtos;
using AiCoachOs.Application.Safety.Dtos;
using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Domain.Rehab;
using AiCoachOs.Domain.Safety;
using AiCoachOs.Domain.TrainingProfiles;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Rehab;

public class RehabIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RehabIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string prefix = "coach_rehab")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto("RehabCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<(Guid ClientId, string Token)> SetupClientAsync(string coachPrefix = "reh_coach")
    {
        var token = await RegisterAndLoginCoachAsync(coachPrefix);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createClientReq = new CreateClientRequestDto(
            FirstName: "Ahmed",
            LastName: "Mahmoud",
            Email: $"ahmed_{Guid.NewGuid():N}@gmail.com",
            Phone: "+201011223344",
            DateOfBirth: new DateTime(1995, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender: Domain.Clients.Gender.Male,
            Goal: new ClientGoalDto("Hypertrophy", 12, "Muscle building"),
            IntakeNotes: "Active client.");

        var clientResp = await _client.PostAsJsonAsync("/api/clients", createClientReq);
        clientResp.EnsureSuccessStatusCode();
        var client = await clientResp.Content.ReadFromJsonAsync<ClientDto>();

        return (client!.Id, token);
    }

    [Fact]
    public async Task CreateLimitation_ValidRequest_CreatesSuccessfully()
    {
        var (clientId, _) = await SetupClientAsync("coach_create_lim");

        var createReq = new CreateTrainingLimitationRequestDto(
            ClientId: clientId,
            AffectedBodyRegion: "Left Knee",
            LimitationSource: LimitationSource.ReportedByClient,
            Description: "Mild discomfort during deep flexion");

        var resp = await _client.PostAsJsonAsync("/api/rehab/limitations", createReq);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var lim = await resp.Content.ReadFromJsonAsync<TrainingLimitationDto>();
        lim.Should().NotBeNull();
        lim!.AffectedBodyRegion.Should().Be("Left Knee");
        lim.Status.Should().Be(LimitationStatus.Active);
    }

    [Fact]
    public async Task GenerateConsiderations_AndRecordDecisions_WorksEndToEnd()
    {
        var (clientId, _) = await SetupClientAsync("coach_flow");

        // 1. Create limitation
        var createReq = new CreateTrainingLimitationRequestDto(
            ClientId: clientId,
            AffectedBodyRegion: "Right Shoulder",
            LimitationSource: LimitationSource.ReportedByClient,
            Description: "Aching sensation during overhead pressing");

        var limResp = await _client.PostAsJsonAsync("/api/rehab/limitations", createReq);
        limResp.EnsureSuccessStatusCode();
        var lim = await limResp.Content.ReadFromJsonAsync<TrainingLimitationDto>();

        // 2. Generate considerations
        var genReq = new GenerateConsiderationsRequestDto();
        var genResp = await _client.PostAsJsonAsync($"/api/rehab/limitations/{lim!.Id}/generate", genReq);
        genResp.EnsureSuccessStatusCode();
        var considerations = await genResp.Content.ReadFromJsonAsync<List<RehabAwarenessConsiderationDto>>();
        considerations.Should().NotBeEmpty();

        var first = considerations![0];
        first.Status.Should().Be(ConsiderationStatus.Pending);

        // 3. Approve consideration
        var decisionReq = new RecordConsiderationDecisionRequestDto(
            Decision: ConsiderationStatus.ApprovedByCoach,
            Note: "Approved conservative load adjustment for next 2 weeks.");

        var decResp = await _client.PostAsJsonAsync($"/api/rehab/considerations/{first.Id}/decision", decisionReq);
        decResp.EnsureSuccessStatusCode();
        var updated = await decResp.Content.ReadFromJsonAsync<RehabAwarenessConsiderationDto>();
        updated!.Status.Should().Be(ConsiderationStatus.ApprovedByCoach);
        updated.CoachDecisionNote.Should().Be("Approved conservative load adjustment for next 2 weeks.");
    }

    [Fact]
    public async Task PostReferralLimitation_RequiresExplicitActivation_BeforeGeneratingConsiderations()
    {
        var (clientId, _) = await SetupClientAsync("coach_post_ref");

        // 1. Submit a referral-level safety screening
        var screenReq = new CreateSafetyReportRequestDto(
            ClientId: clientId,
            TriggeredByType: TriggeredByType.DirectReport,
            Signals: new[]
            {
                new ReportedSignalDto(
                    BodyRegion: "Lumbar Spine",
                    SignalType: SignalType.Numbness,
                    Onset: SignalOnset.Gradual,
                    Timing: SignalTiming.Persistent,
                    Severity: SignalSeverity.Moderate,
                    Duration: SignalDuration.Chronic,
                    FreeText: "Radiating numbness down right leg")
            });

        var screenResp = await _client.PostAsJsonAsync("/api/safety/screen", screenReq);
        screenResp.EnsureSuccessStatusCode();
        var screening = await screenResp.Content.ReadFromJsonAsync<SafetyScreeningDto>();

        // 2. Create post-referral limitation
        var createLimReq = new CreateTrainingLimitationRequestDto(
            ClientId: clientId,
            AffectedBodyRegion: "Lumbar Spine",
            LimitationSource: LimitationSource.PostReferral,
            SafetyScreeningId: screening!.Id,
            Description: "Referred to physio; pending clearance");

        var limResp = await _client.PostAsJsonAsync("/api/rehab/limitations", createLimReq);
        limResp.EnsureSuccessStatusCode();
        var lim = await limResp.Content.ReadFromJsonAsync<TrainingLimitationDto>();

        // 3. Trying to generate considerations without activation should fail
        var genResp = await _client.PostAsJsonAsync($"/api/rehab/limitations/{lim!.Id}/generate", new GenerateConsiderationsRequestDto());
        genResp.StatusCode.Should().Be(HttpStatusCode.BadRequest); // or internal error converted

        // 4. Activate limitation with coach note
        var actReq = new ActivateLimitationRequestDto("Physiotherapist cleared client for pain-free trunk stability drills on 2026-09-22.");
        var actResp = await _client.PostAsJsonAsync($"/api/rehab/limitations/{lim.Id}/activate", actReq);
        actResp.EnsureSuccessStatusCode();

        // 5. Now generation succeeds
        var genRespAfter = await _client.PostAsJsonAsync($"/api/rehab/limitations/{lim.Id}/generate", new GenerateConsiderationsRequestDto());
        genRespAfter.EnsureSuccessStatusCode();
        var considerations = await genRespAfter.Content.ReadFromJsonAsync<List<RehabAwarenessConsiderationDto>>();
        considerations.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ConsiderationApproval_DoesNotMutate_ProgramOrExerciseSlots()
    {
        var (clientId, _) = await SetupClientAsync("coach_prog_check");

        // 1. Setup profile and generate a program
        var profileReq = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Intermediate,
            WeeklyAvailability: new TrainingAvailabilityDto(
                SessionsPerWeek: 3,
                AvailableDays: new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                PreferredDays: new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }),
            SessionDurationTargetMinutes: 60,
            SessionDurationMinMinutes: 45,
            SessionDurationMaxMinutes: 75,
            AvailableEquipmentIds: null,
            ExercisePreferences: null,
            ExerciseConstraints: null,
            Priorities: new List<ClientTrainingPriorityDto>
            {
                new(null, 1, "Shoulders", "Primary focus area")
            });

        var profResp = await _client.PutAsJsonAsync($"/api/clients/{clientId}/training-profile", profileReq);
        profResp.EnsureSuccessStatusCode();

        var genProgReq = new GenerateProgramRequestDto(clientId, "Hypertrophy Block", "Initial build");
        var genProgResp = await _client.PostAsJsonAsync("/api/programs/generate", genProgReq);
        genProgResp.EnsureSuccessStatusCode();
        var prog = await genProgResp.Content.ReadFromJsonAsync<ProgramDto>();
        prog.Should().NotBeNull();
        var initialVersionCount = prog!.Versions.Count;

        // 2. Create limitation and generate considerations
        var createLimReq = new CreateTrainingLimitationRequestDto(
            ClientId: clientId,
            AffectedBodyRegion: "Elbow",
            LimitationSource: LimitationSource.ReportedByClient,
            Description: "Medial elbow discomfort");

        var limResp = await _client.PostAsJsonAsync("/api/rehab/limitations", createLimReq);
        limResp.EnsureSuccessStatusCode();
        var lim = await limResp.Content.ReadFromJsonAsync<TrainingLimitationDto>();

        var genResp = await _client.PostAsJsonAsync($"/api/rehab/limitations/{lim!.Id}/generate", new GenerateConsiderationsRequestDto());
        genResp.EnsureSuccessStatusCode();
        var considerations = await genResp.Content.ReadFromJsonAsync<List<RehabAwarenessConsiderationDto>>();
        considerations.Should().NotBeEmpty();

        // 3. Approve all considerations
        foreach (var c in considerations!)
        {
            var decResp = await _client.PostAsJsonAsync($"/api/rehab/considerations/{c.Id}/decision", new RecordConsiderationDecisionRequestDto(
                Decision: ConsiderationStatus.ApprovedByCoach,
                Note: "Approved"));
            decResp.EnsureSuccessStatusCode();
        }

        // 4. Retrieve program again and assert nothing was mutated automatically
        var progAfterResp = await _client.GetAsync($"/api/programs/{prog.Id}");
        progAfterResp.EnsureSuccessStatusCode();
        var progAfter = await progAfterResp.Content.ReadFromJsonAsync<ProgramDto>();
        progAfter!.Versions.Count.Should().Be(initialVersionCount);
        progAfter.Status.Should().Be(prog.Status);
    }

    [Fact]
    public async Task CoachIsolation_UnauthorizedCoachCannotAccessClientLimitations()
    {
        var (clientId, _) = await SetupClientAsync("coach_owner");

        var otherCoachToken = await RegisterAndLoginCoachAsync("coach_intruder");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherCoachToken);

        var resp = await _client.GetAsync($"/api/rehab/clients/{clientId}/limitations");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
