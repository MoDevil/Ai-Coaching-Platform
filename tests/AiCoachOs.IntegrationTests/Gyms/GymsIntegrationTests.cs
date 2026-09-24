using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Gyms.Dtos;
using AiCoachOs.Application.Nutrition.Engine;
using AiCoachOs.Domain.Gyms;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Gyms;

public class GymsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GymsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string prefix = "coach_gym")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto("GymCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<(Guid ClientId, string Token)> SetupClientAsync(string coachPrefix = "gym_coach")
    {
        var token = await RegisterAndLoginCoachAsync(coachPrefix);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createClientReq = new CreateClientRequestDto(
            FirstName: "Ahmed",
            LastName: "Saeed",
            Email: $"ahmed_{Guid.NewGuid():N}@gmail.com",
            Phone: "+201011223366",
            DateOfBirth: new DateTime(1995, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender: Domain.Clients.Gender.Male,
            Goal: new ClientGoalDto("Hypertrophy", 12, "Muscle building"),
            IntakeNotes: "Healthy client.");

        var clientResp = await _client.PostAsJsonAsync("/api/clients", createClientReq);
        clientResp.EnsureSuccessStatusCode();
        var client = await clientResp.Content.ReadFromJsonAsync<ClientDto>();

        return (client!.Id, token);
    }

    [Fact]
    public async Task Gyms_FullLifecycle_CreateListAssignAndDelete()
    {
        // Arrange
        var (clientId, token) = await SetupClientAsync("lifecycle_coach");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Get available equipment options
        var equipOptionsResp = await _client.GetAsync("/api/gyms/equipment-options");
        equipOptionsResp.EnsureSuccessStatusCode();
        var equipOptions = await equipOptionsResp.Content.ReadFromJsonAsync<List<EquipmentOptionDto>>();
        equipOptions.Should().NotBeNull();
        equipOptions.Should().NotBeEmpty();

        // 2. Create Gym Profile with authoritative inventory
        var createReq = new CreateGymProfileRequestDto(
            Name: "Golds Gym Maadi",
            Tier: EquipmentTier.Commercial,
            Location: "Maadi, Cairo",
            ExplicitEquipmentIds: new List<Guid> { equipOptions!.First().Id },
            IsInventoryAuthoritative: true);

        var createResp = await _client.PostAsJsonAsync("/api/gyms", createReq);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdGym = await createResp.Content.ReadFromJsonAsync<GymProfileDto>();
        createdGym.Should().NotBeNull();
        createdGym!.Name.Should().Be("Golds Gym Maadi");
        createdGym.Tier.Should().Be(EquipmentTier.Commercial);
        createdGym.IsInventoryAuthoritative.Should().BeTrue();
        createdGym.ExplicitEquipmentIds.Should().Contain(equipOptions.First().Id);

        // 3. List Gyms (Coach Isolation check)
        var listResp = await _client.GetAsync("/api/gyms");
        listResp.EnsureSuccessStatusCode();
        var gyms = await listResp.Content.ReadFromJsonAsync<List<GymProfileDto>>();
        gyms.Should().Contain(g => g.Id == createdGym.Id);

        // 4. Assign client to gym
        var assignReq = new AssignClientGymRequestDto(clientId, createdGym.Id);
        var assignResp = await _client.PostAsJsonAsync($"/api/gyms/assign-client/{clientId}", assignReq);
        assignResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 5. Delete Gym
        var deleteResp = await _client.DeleteAsync($"/api/gyms/{createdGym.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it is removed
        var getAfterDelete = await _client.GetAsync($"/api/gyms/{createdGym.Id}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FoodSuggestions_Endpoint_ReturnsSafeStatusAndScientificFraming()
    {
        // Arrange
        var (clientId, token) = await SetupClientAsync("sugg_coach");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var resp = await _client.GetAsync($"/api/nutrition/clients/{clientId}/suggestions");

        // Assert
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<FoodSuggestionResult>();
        result.Should().NotBeNull();
        result!.ScientificFramingNote.Should().Contain("24 hours");
        result.Status.Should().BeOneOf(FoodSuggestionStatus.Success, FoodSuggestionStatus.NoDataAvailable, FoodSuggestionStatus.InsufficientData);
    }
}
