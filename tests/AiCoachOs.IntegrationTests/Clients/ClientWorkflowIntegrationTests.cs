using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Domain.Clients;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Clients;

public class ClientWorkflowIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ClientWorkflowIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string name)
    {
        var email = $"{name.ToLowerInvariant()}_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto(name, email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    [Fact]
    public async Task CreateClient_WithOnlyRequiredFields_ShouldSucceed()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync("CoachTarek");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateClientRequestDto(
            FirstName: "Ziad",
            LastName: "Mahmoud"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ClientDto>();
        created.Should().NotBeNull();
        created!.FirstName.Should().Be("Ziad");
        created.LastName.Should().Be("Mahmoud");
        created.Email.Should().BeNull();
        created.Phone.Should().BeNull();
        created.DateOfBirth.Should().BeNull();
        created.Gender.Should().BeNull();
        created.Goal.Should().BeNull();
        created.IntakeNotes.Should().BeNull();
        created.Status.Should().Be(ClientStatus.Active);
    }

    [Fact]
    public async Task CreateClient_WithAllOptionalFields_ShouldSucceed()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync("CoachYoussef");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateClientRequestDto(
            FirstName: "Hassan",
            LastName: "Nour",
            Email: "hassan.nour@gmail.com",
            Phone: "+201234567890",
            DateOfBirth: new DateTime(1998, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender: Gender.Male,
            Goal: new ClientGoalDto("Hypertrophy", 16, "Chest and back focus"),
            IntakeNotes: "Trains 4 days per week in Alexandria"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ClientDto>();
        created.Should().NotBeNull();
        created!.FirstName.Should().Be("Hassan");
        created.LastName.Should().Be("Nour");
        created.Email.Should().Be("hassan.nour@gmail.com");
        created.Goal.Should().NotBeNull();
        created.Goal!.PrimaryGoal.Should().Be("Hypertrophy");
        created.Goal.TargetTimelineWeeks.Should().Be(16);
    }

    [Fact]
    public async Task GetCoachClients_ShouldReturnList()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync("CoachNader");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/clients", new CreateClientRequestDto("ClientOne", "Test"));
        await _client.PostAsJsonAsync("/api/clients", new CreateClientRequestDto("ClientTwo", "Test"));

        // Act
        var response = await _client.GetAsync("/api/clients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<IReadOnlyList<ClientSummaryDto>>();
        list.Should().NotBeNull();
        list!.Count.Should().BeGreaterThanOrEqualTo(2);
        list.Should().Contain(c => c.FirstName == "ClientOne");
        list.Should().Contain(c => c.FirstName == "ClientTwo");
    }

    [Fact]
    public async Task UpdateClient_ShouldModifyProfileAndGoal()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync("CoachSamir");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/clients", new CreateClientRequestDto("Maged", "Adly"));
        var created = await createResponse.Content.ReadFromJsonAsync<ClientDto>();

        var updateRequest = new UpdateClientRequestDto(
            FirstName: "Maged",
            LastName: "Adly",
            Email: "maged.updated@gym.eg",
            Phone: "+201011122233",
            DateOfBirth: null,
            Gender: Gender.Male,
            Goal: new ClientGoalDto("FatLoss", 12, "Gradual calorie deficit"),
            IntakeNotes: "Updated notes"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/clients/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ClientDto>();
        updated.Should().NotBeNull();
        updated!.Email.Should().Be("maged.updated@gym.eg");
        updated.Goal.Should().NotBeNull();
        updated.Goal!.PrimaryGoal.Should().Be("FatLoss");
        updated.IntakeNotes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task ArchiveClient_ShouldSetStatusToArchived()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync("CoachFady");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/clients", new CreateClientRequestDto("Rami", "Soliman"));
        var created = await createResponse.Content.ReadFromJsonAsync<ClientDto>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/clients/{created!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify client status is now archived
        var getResponse = await _client.GetAsync($"/api/clients/{created.Id}");
        var client = await getResponse.Content.ReadFromJsonAsync<ClientDto>();
        client!.Status.Should().Be(ClientStatus.Archived);
    }

    [Fact]
    public async Task ConsentWorkflow_ShouldRecordAndRetrieveConsent()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync("CoachWael");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/clients", new CreateClientRequestDto("Amr", "Diab"));
        var created = await createResponse.Content.ReadFromJsonAsync<ClientDto>();

        var consentRequest = new { ConsentType = "DataProcessing", IsGranted = true, Notes = "Signed in intake" };

        // Act
        var postConsent = await _client.PostAsJsonAsync($"/api/clients/{created!.Id}/consent", consentRequest);
        postConsent.StatusCode.Should().Be(HttpStatusCode.OK);

        var getConsent = await _client.GetAsync($"/api/clients/{created.Id}/consent");
        getConsent.StatusCode.Should().Be(HttpStatusCode.OK);

        var consents = await getConsent.Content.ReadFromJsonAsync<IReadOnlyList<ConsentRecordDto>>();
        consents.Should().NotBeNull();
        consents!.Should().Contain(c => c.ConsentType == "DataProcessing" && c.IsGranted == true);
    }
}
