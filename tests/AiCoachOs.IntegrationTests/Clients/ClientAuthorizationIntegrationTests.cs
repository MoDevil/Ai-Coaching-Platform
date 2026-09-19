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

public class ClientAuthorizationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ClientAuthorizationIntegrationTests(CustomWebApplicationFactory factory)
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
    public async Task CoachCannotAccessAnotherCoachClient_ReturnsNotFound()
    {
        // 1. Coach A registers and creates a client
        var tokenA = await RegisterAndLoginCoachAsync("CoachA");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var createResponse = await _client.PostAsJsonAsync("/api/clients", new CreateClientRequestDto(
            FirstName: "PrivateClient",
            LastName: "OfCoachA",
            Email: "private@aicoach.eg"
        ));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var clientA = await createResponse.Content.ReadFromJsonAsync<ClientDto>();
        clientA.Should().NotBeNull();

        // 2. Coach B registers
        var tokenB = await RegisterAndLoginCoachAsync("CoachB");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        // Act & Assert 1: Coach B tries to GET Coach A's client
        var getResponse = await _client.GetAsync($"/api/clients/{clientA!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Act & Assert 2: Coach B tries to PUT Coach A's client
        var updateResponse = await _client.PutAsJsonAsync($"/api/clients/{clientA.Id}", new UpdateClientRequestDto(
            FirstName: "HackedName",
            LastName: "HackedLast"
        ));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Act & Assert 3: Coach B tries to DELETE Coach A's client
        var deleteResponse = await _client.DeleteAsync($"/api/clients/{clientA.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Act & Assert 4: Coach B lists clients - must not see Coach A's client
        var listResponse = await _client.GetAsync("/api/clients");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var coachBClients = await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<ClientSummaryDto>>();
        coachBClients.Should().NotBeNull();
        coachBClients!.Should().NotContain(c => c.Id == clientA.Id);

        // Act & Assert 5: Coach B tries to post consent to Coach A's client
        var consentResponse = await _client.PostAsJsonAsync($"/api/clients/{clientA.Id}/consent", new
        {
            ConsentType = "Intake",
            IsGranted = true,
            Notes = "Intruder consent"
        });
        consentResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotAccessClientsEndpoints()
    {
        // Remove auth header
        _client.DefaultRequestHeaders.Authorization = null;

        var listResponse = await _client.GetAsync("/api/clients");
        listResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var getResponse = await _client.GetAsync($"/api/clients/{Guid.NewGuid()}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var createResponse = await _client.PostAsJsonAsync("/api/clients", new CreateClientRequestDto("Anon", "User"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
