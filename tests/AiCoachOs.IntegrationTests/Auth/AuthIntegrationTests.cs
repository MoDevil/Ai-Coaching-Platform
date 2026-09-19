using System.Net;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Auth;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn200AndToken()
    {
        var email = $"coach_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto(
            FullName: "Coach Sherif",
            Email: email,
            Password: "Password123!"
        );

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        content.Should().NotBeNull();
        content!.Token.Should().NotBeNullOrWhiteSpace();
        content.FullName.Should().Be("Coach Sherif");
        content.Email.Should().Be(email);
        content.CoachId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200AndToken()
    {
        var email = $"login_{Guid.NewGuid():N}@egyptgym.com";
        var registerRequest = new RegisterCoachRequestDto(
            FullName: "Coach Karim",
            Email: email,
            Password: "SecurePassword123!"
        );
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        regResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginRequest = new LoginCoachRequestDto(email, "SecurePassword123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        content.Should().NotBeNull();
        content!.Token.Should().NotBeNullOrWhiteSpace();
        content.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401Unauthorized()
    {
        var email = $"wrongpw_{Guid.NewGuid():N}@egyptgym.com";
        var registerRequest = new RegisterCoachRequestDto("Coach Aly", email, "CorrectPassword123!");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginCoachRequestDto(email, "WrongPassword!");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturn401Unauthorized()
    {
        var response = await _client.GetAsync("/api/clients");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
