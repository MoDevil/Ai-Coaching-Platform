using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Programs.DTOs;
using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.TrainingProfiles;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Programs;

public class ProgramsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProgramsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync()
    {
        var email = $"coach_prog_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto("ProgramCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<(Guid ClientId, string Token)> SetupClientWithProfileAsync()
    {
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create Client
        var createClientReq = new CreateClientRequestDto(
            FirstName: "Omar",
            LastName: "Farouk",
            Email: $"omar_{Guid.NewGuid():N}@gmail.com",
            Phone: "+201012345678",
            DateOfBirth: new DateTime(1995, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender: Domain.Clients.Gender.Male,
            Goal: new ClientGoalDto("Build muscle mass and upper body strength", 12, "Priority on chest and back"),
            IntakeNotes: "Great sleep and recovery capacity. Ready for progressive overload.");

        var clientResp = await _client.PostAsJsonAsync("/api/clients", createClientReq);
        if (!clientResp.IsSuccessStatusCode)
        {
            var err = await clientResp.Content.ReadAsStringAsync();
            throw new Exception($"Create client failed: {clientResp.StatusCode} - {err}");
        }
        clientResp.EnsureSuccessStatusCode();
        var client = await clientResp.Content.ReadFromJsonAsync<ClientDto>();

        // Update Profile
        var profileReq = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Intermediate,
            WeeklyAvailability: new TrainingAvailabilityDto(
                SessionsPerWeek: 4,
                AvailableDays: new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                PreferredDays: new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday }),
            SessionDurationTargetMinutes: 60,
            SessionDurationMinMinutes: 45,
            SessionDurationMaxMinutes: 75,
            AvailableEquipmentIds: null,
            ExercisePreferences: "Dumbbell Press, Pull-ups",
            ExerciseConstraints: "No overhead barbell behind neck",
            Priorities: new List<ClientTrainingPriorityDto>
            {
                new(null, 1, "Chest", "Primary focus area"),
                new(null, 2, "Back", "Secondary focus area")
            });

        var profileResp = await _client.PutAsJsonAsync($"/api/clients/{client!.Id}/training-profile", profileReq);
        profileResp.EnsureSuccessStatusCode();

        return (client.Id, token);
    }

    [Fact]
    public async Task GenerateProgram_WithValidClientAndProfile_CreatesCompleteProgram()
    {
        // Arrange
        var (clientId, token) = await SetupClientWithProfileAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var generateReq = new GenerateProgramRequestDto(
            ClientId: clientId,
            ProgramName: "Omar Hypertrophy Block 1",
            CoachNotes: "Focus on hypertrophy rep ranges and clean bar paths.",
            NumberOfWeeks: 4);

        // Act
        var response = await _client.PostAsJsonAsync("/api/programs/generate", generateReq);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Generate program failed with status {response.StatusCode}: {err}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var program = await response.Content.ReadFromJsonAsync<ProgramDto>();
        program.Should().NotBeNull();
        program!.ClientId.Should().Be(clientId);
        program.Status.Should().Be(ProgramStatus.Draft);
        program.GoalSnapshot.PrimaryGoal.Should().Be(PrimaryGoalType.Hypertrophy);
        program.RationaleSummary.Should().Contain("Hypertrophy");
        program.RationaleSummary.Should().Contain("4 sessions/week");

        // Verify version, weeks and sessions
        program.ActiveVersion.Should().NotBeNull();
        program.ActiveVersion!.Weeks.Should().HaveCount(4);
        var week1 = program.ActiveVersion.Weeks.First();
        week1.Sessions.Should().HaveCount(4);

        // Verify slots and progression rules
        var session1 = week1.Sessions.First();
        session1.Slots.Should().NotBeEmpty();
        session1.Slots.First().ProgressionRule.Should().NotBeNull();
        session1.Slots.First().SelectionRationale.Should().NotBeNullOrWhiteSpace();

        // Verify output volume summary (Invariant: Volume is an output)
        program.VolumeSummary.Should().NotBeNull();
        program.VolumeSummary!.TotalWeeklySessions.Should().Be(4);
        program.VolumeSummary.TotalWeeklySets.Should().BeGreaterThan(0);
        program.VolumeSummary.MuscleVolumes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetActiveProgramForClient_ReturnsActiveOrDraftProgram()
    {
        // Arrange
        var (clientId, token) = await SetupClientWithProfileAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var generateReq = new GenerateProgramRequestDto(
            ClientId: clientId,
            ProgramName: "Active Program Test",
            NumberOfWeeks: 4);

        var genResp = await _client.PostAsJsonAsync("/api/programs/generate", generateReq);
        genResp.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync($"/api/programs/client/{clientId}/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var activeProgram = await response.Content.ReadFromJsonAsync<ProgramDto>();
        activeProgram.Should().NotBeNull();
        activeProgram!.Name.Should().Be("Active Program Test");
    }

    [Fact]
    public async Task UpdateProgramStatus_TransitionsStatusToActive()
    {
        // Arrange
        var (clientId, token) = await SetupClientWithProfileAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var generateReq = new GenerateProgramRequestDto(ClientId: clientId, NumberOfWeeks: 4);
        var genResp = await _client.PostAsJsonAsync("/api/programs/generate", generateReq);
        genResp.EnsureSuccessStatusCode();
        var program = await genResp.Content.ReadFromJsonAsync<ProgramDto>();

        // Act
        var updateReq = new UpdateProgramStatusDto(ProgramStatus.Active);
        var updateResp = await _client.PutAsJsonAsync($"/api/programs/{program!.Id}/status", updateReq);

        // Assert
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResp.Content.ReadFromJsonAsync<ProgramDto>();
        updated!.Status.Should().Be(ProgramStatus.Active);
    }
}
