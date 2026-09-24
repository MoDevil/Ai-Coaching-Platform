using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Programs.DTOs;
using AiCoachOs.Application.Safety.Dtos;
using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Domain.Safety;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Safety;

public class SafetyIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SafetyIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string prefix = "coach_safety")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto("SafetyCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<(Guid ClientId, string Token)> SetupClientAsync(string coachPrefix = "saf_coach")
    {
        var token = await RegisterAndLoginCoachAsync(coachPrefix);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createClientReq = new CreateClientRequestDto(
            FirstName: "Kareem",
            LastName: "Hassan",
            Email: $"kareem_{Guid.NewGuid():N}@gmail.com",
            Phone: "+201099887766",
            DateOfBirth: new DateTime(1992, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender: Domain.Clients.Gender.Male,
            Goal: new ClientGoalDto("Strength and physique", 12, "General fitness"),
            IntakeNotes: "Active client.");

        var clientResp = await _client.PostAsJsonAsync("/api/clients", createClientReq);
        clientResp.EnsureSuccessStatusCode();
        var client = await clientResp.Content.ReadFromJsonAsync<ClientDto>();

        return (client!.Id, token);
    }

    [Fact]
    public async Task ScreenReport_WhenChestPressureReported_ReturnsUrgentMedicalAttention()
    {
        var (clientId, token) = await SetupClientAsync("coach_chest");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateSafetyReportRequestDto(
            ClientId: clientId,
            TriggeredByType: TriggeredByType.WorkoutSession,
            Signals: new List<ReportedSignalDto>
            {
                new ReportedSignalDto(
                    BodyRegion: "Chest",
                    SignalType: SignalType.Pain,
                    Onset: SignalOnset.Sudden,
                    Timing: SignalTiming.DuringExercise,
                    Severity: SignalSeverity.Severe,
                    FreeText: "Felt chest pressure during heavy sets")
            });

        var response = await _client.PostAsJsonAsync("/api/safety/screen", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var screening = await response.Content.ReadFromJsonAsync<SafetyScreeningDto>();
        screening.Should().NotBeNull();
        screening!.ScreeningResult.Should().Be(SafetyCategory.UrgentMedicalAttention);
        screening.RecommendedAction.Should().Be(SafetyActionType.UrgentMedicalAttention);
        screening.RequiresCoachAcknowledgment.Should().BeTrue();
        screening.Disclaimer.Should().Contain("AI Coach OS does not diagnose medical conditions");
    }

    [Fact]
    public async Task AcknowledgeScreening_UpdatesCoachAcknowledgment()
    {
        var (clientId, token) = await SetupClientAsync("coach_ack");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var screenReq = new CreateSafetyReportRequestDto(
            ClientId: clientId,
            TriggeredByType: TriggeredByType.DirectReport,
            Signals: new List<ReportedSignalDto>
            {
                new ReportedSignalDto(
                    BodyRegion: "Right Knee",
                    SignalType: SignalType.Discomfort,
                    Onset: SignalOnset.Gradual,
                    Timing: SignalTiming.AfterExercise,
                    Severity: SignalSeverity.Mild)
            });

        var screenResp = await _client.PostAsJsonAsync("/api/safety/screen", screenReq);
        screenResp.EnsureSuccessStatusCode();
        var screening = await screenResp.Content.ReadFromJsonAsync<SafetyScreeningDto>();

        var ackReq = new AcknowledgeSafetyScreeningRequestDto(CoachNote: "Spoke to client. Advised monitoring.");
        var ackResp = await _client.PostAsJsonAsync($"/api/safety/screenings/{screening!.Id}/acknowledge", ackReq);
        ackResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await ackResp.Content.ReadFromJsonAsync<SafetyScreeningDto>();
        updated.Should().NotBeNull();
        updated!.RequiresCoachAcknowledgment.Should().BeFalse();
        updated.CoachAcknowledgedAtUtc.Should().NotBeNull();
        updated.CoachNote.Should().Be("Spoke to client. Advised monitoring.");
    }

    [Fact]
    public async Task Coach_CannotAccessAnotherCoachesClientSafetyScreenings()
    {
        var (clientId, coach1Token) = await SetupClientAsync("coach1_saf");
        var coach2Token = await RegisterAndLoginCoachAsync("coach2_saf");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", coach2Token);
        var screenReq = new CreateSafetyReportRequestDto(
            ClientId: clientId,
            TriggeredByType: TriggeredByType.WorkoutSession,
            Signals: new List<ReportedSignalDto>
            {
                new ReportedSignalDto("Shoulder", SignalType.Pain, SignalOnset.Gradual, SignalTiming.DuringExercise, SignalSeverity.Mild)
            });

        var response = await _client.PostAsJsonAsync("/api/safety/screen", screenReq);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
