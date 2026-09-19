using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Domain.TrainingProfiles;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.TrainingProfiles;

public class TrainingProfileIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TrainingProfileIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(string Token, Guid ClientId)> SetupCoachAndClientAsync(string coachName, string clientFirst)
    {
        var email = $"{coachName.ToLowerInvariant()}_{Guid.NewGuid():N}@gym.eg";
        var regResp = await _client.PostAsJsonAsync("/api/auth/register", new RegisterCoachRequestDto(coachName, email, "Password123!"));
        regResp.EnsureSuccessStatusCode();
        var auth = await regResp.Content.ReadFromJsonAsync<AuthResponseDto>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        var createClientResp = await _client.PostAsJsonAsync("/api/clients", new CreateClientRequestDto(clientFirst, "Athlete"));
        createClientResp.EnsureSuccessStatusCode();
        var client = await createClientResp.Content.ReadFromJsonAsync<ClientDto>();

        return (auth.Token, client!.Id);
    }

    [Fact]
    public async Task GetTrainingProfile_ReturnsDefaultProfile_WhenFirstRequested()
    {
        // Arrange
        var (token, clientId) = await SetupCoachAndClientAsync("CoachTarek", "Ahmed");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/clients/{clientId}/training-profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<TrainingProfileDto>();
        profile.Should().NotBeNull();
        profile!.ClientId.Should().Be(clientId);
        profile.ExperienceLevel.Should().Be(TrainingExperienceLevel.Intermediate);
        profile.WeeklyAvailability.SessionsPerWeek.Should().Be(3);
        profile.SessionDurationTargetMinutes.Should().Be(60);
    }

    [Fact]
    public async Task UpdateTrainingProfile_UpdatesExperienceAndDurations()
    {
        // Arrange
        var (token, clientId) = await SetupCoachAndClientAsync("CoachKareem", "Youssef");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Advanced,
            SessionDurationMinMinutes: 45,
            SessionDurationTargetMinutes: 75,
            SessionDurationMaxMinutes: 90,
            WeeklyAvailability: new TrainingAvailabilityDto(4, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday }, new[] { DayOfWeek.Monday, DayOfWeek.Thursday }),
            AvailableEquipmentIds: new List<Guid> { Guid.NewGuid() },
            ExercisePreferences: "Loves Romanian deadlifts and cable work",
            ExerciseConstraints: "Mild right knee tendon discomfort with deep knee flexion",
            Priorities: new List<ClientTrainingPriorityDto>
            {
                new(null, 1, "Hamstrings & Glute hypertrophy", "Focus on lengthened tension"),
                new(null, 2, "Upper Chest volume", null)
            }
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/clients/{clientId}/training-profile", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TrainingProfileDto>();
        updated.Should().NotBeNull();
        updated!.ExperienceLevel.Should().Be(TrainingExperienceLevel.Advanced);
        updated.SessionDurationMinMinutes.Should().Be(45);
        updated.SessionDurationTargetMinutes.Should().Be(75);
        updated.SessionDurationMaxMinutes.Should().Be(90);
        updated.ExercisePreferences.Should().Be("Loves Romanian deadlifts and cable work");
        updated.ExerciseConstraints.Should().Be("Mild right knee tendon discomfort with deep knee flexion");
        updated.Priorities.Should().HaveCount(2);
        updated.Priorities[0].Order.Should().Be(1);
        updated.Priorities[0].FocusArea.Should().Be("Hamstrings & Glute hypertrophy");
        updated.Priorities[1].Order.Should().Be(2);
        updated.Priorities[1].FocusArea.Should().Be("Upper Chest volume");
    }

    [Fact]
    public async Task UpdateAvailability_UpdatesSessionsAndDays()
    {
        // Arrange
        var (token, clientId) = await SetupCoachAndClientAsync("CoachHossam", "Nader");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var availability = new TrainingAvailabilityDto(
            SessionsPerWeek: 5,
            AvailableDays: new[] { DayOfWeek.Sunday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Saturday },
            PreferredDays: new[] { DayOfWeek.Sunday, DayOfWeek.Tuesday, DayOfWeek.Thursday }
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/clients/{clientId}/training-profile/availability", availability);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TrainingProfileDto>();
        updated.Should().NotBeNull();
        updated!.WeeklyAvailability.SessionsPerWeek.Should().Be(5);
        updated.WeeklyAvailability.AvailableDays.Should().HaveCount(5);
        updated.WeeklyAvailability.PreferredDays.Should().HaveCount(3);
    }

    [Fact]
    public async Task UpdatePriorities_SetsAndOrdersPriorities()
    {
        // Arrange
        var (token, clientId) = await SetupCoachAndClientAsync("CoachSamir", "Mahmoud");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var priorities = new List<ClientTrainingPriorityDto>
        {
            new(null, 1, "Posterior Chain Strength", "Deadlift technique"),
            new(null, 2, "Triceps Overload", null)
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/clients/{clientId}/training-profile/priorities", priorities);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TrainingProfileDto>();
        updated.Should().NotBeNull();
        updated!.Priorities.Should().HaveCount(2);
        updated.Priorities[0].FocusArea.Should().Be("Posterior Chain Strength");
        updated.Priorities[1].FocusArea.Should().Be("Triceps Overload");
    }
}
