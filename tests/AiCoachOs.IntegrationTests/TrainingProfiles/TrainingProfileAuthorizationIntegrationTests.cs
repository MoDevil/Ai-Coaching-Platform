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

public class TrainingProfileAuthorizationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TrainingProfileAuthorizationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string name)
    {
        var email = $"{name.ToLowerInvariant()}_{Guid.NewGuid():N}@gym.eg";
        var request = new RegisterCoachRequestDto(name, email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    [Fact]
    public async Task CoachCannotAccessAnotherCoachClientTrainingProfile_ReturnsNotFound()
    {
        // 1. Coach A registers and creates a client
        var tokenA = await RegisterAndLoginCoachAsync("CoachA_TP");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var createClientResp = await _client.PostAsJsonAsync("/api/clients", new CreateClientRequestDto("Private", "Client"));
        createClientResp.EnsureSuccessStatusCode();
        var clientA = await createClientResp.Content.ReadFromJsonAsync<ClientDto>();

        // 2. Coach B registers
        var tokenB = await RegisterAndLoginCoachAsync("CoachB_TP");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        // Act & Assert 1: Coach B tries to GET Coach A's client training profile
        var getResponse = await _client.GetAsync($"/api/clients/{clientA!.Id}/training-profile");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Act & Assert 2: Coach B tries to PUT Coach A's client training profile
        var updateRequest = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Novice,
            SessionDurationMinMinutes: 30,
            SessionDurationTargetMinutes: 45,
            SessionDurationMaxMinutes: 60,
            WeeklyAvailability: new TrainingAvailabilityDto(3, Array.Empty<DayOfWeek>(), Array.Empty<DayOfWeek>()),
            AvailableEquipmentIds: null,
            ExercisePreferences: null,
            ExerciseConstraints: null,
            Priorities: null
        );
        var updateResponse = await _client.PutAsJsonAsync($"/api/clients/{clientA.Id}/training-profile", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Act & Assert 3: Coach B tries to update availability
        var availResponse = await _client.PutAsJsonAsync(
            $"/api/clients/{clientA.Id}/training-profile/availability",
            new TrainingAvailabilityDto(4, Array.Empty<DayOfWeek>(), Array.Empty<DayOfWeek>())
        );
        availResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Act & Assert 4: Coach B tries to update priorities
        var prioResponse = await _client.PutAsJsonAsync(
            $"/api/clients/{clientA.Id}/training-profile/priorities",
            new List<ClientTrainingPriorityDto>()
        );
        prioResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotAccessM2Endpoints()
    {
        // Unauthenticated
        _client.DefaultRequestHeaders.Authorization = null;

        var exResponse = await _client.GetAsync("/api/exercises");
        exResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var tpResponse = await _client.GetAsync($"/api/clients/{Guid.NewGuid()}/training-profile");
        tpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
