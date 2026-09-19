using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.AnatomyAndBiomechanics.DTOs;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Domain.AnatomyAndBiomechanics;
using AiCoachOs.Infrastructure.Persistence;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.AnatomyAndBiomechanics;

public class AnatomyAndBiomechanicsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AnatomyAndBiomechanicsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync()
    {
        var email = $"coach_anatomy_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto("AnatomyCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    [Fact]
    public async Task GetRegions_WithAuth_ReturnsSeededRegions()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/anatomy/regions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var regions = await response.Content.ReadFromJsonAsync<IReadOnlyList<AnatomicalRegionSummaryDto>>();
        regions.Should().NotBeNull();
        regions!.Should().Contain(r => r.Name == "Shoulder");
        regions.Should().Contain(r => r.Name == "Spine");
        regions.Should().Contain(r => r.Name == "Hip");
        regions.Should().Contain(r => r.Name == "Knee");
    }

    [Fact]
    public async Task GetJoints_WithAuth_ReturnsSeededJoints()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/anatomy/joints");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var joints = await response.Content.ReadFromJsonAsync<IReadOnlyList<JointSummaryDto>>();
        joints.Should().NotBeNull();
        joints!.Should().Contain(j => j.Name == "Glenohumeral Joint");
        joints.Should().Contain(j => j.Name == "Tibiofemoral Joint");
    }

    [Fact]
    public async Task GetJointActions_WithAuth_ReturnsActions()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/anatomy/joint-actions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actions = await response.Content.ReadFromJsonAsync<IReadOnlyList<JointActionSummaryDto>>();
        actions.Should().NotBeNull();
        actions!.Should().Contain(a => a.ActionType == JointActionType.Extension);
        actions.Should().Contain(a => a.ActionType == JointActionType.Flexion);
    }

    [Fact]
    public async Task GetMuscleAnatomy_WithAuth_ReturnsMuscleActions()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Query Quadriceps
        var response = await _client.GetAsync($"/api/anatomy/muscles/{ExerciseLibrarySeeder.Musc.Quadriceps}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var anatomy = await response.Content.ReadFromJsonAsync<MuscleAnatomyDto>();
        anatomy.Should().NotBeNull();
        anatomy!.Name.Should().Be("Quadriceps Femoris");
        anatomy.PrimaryActions.Should().Contain(a => a.ActionType == JointActionType.Extension);
    }

    [Fact]
    public async Task GetExerciseBiomechanics_WithAuth_ReturnsJointActionsAndConsiderations()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var squatId = new Guid("44444444-4444-4444-4444-444444444401");

        // Act
        var response = await _client.GetAsync($"/api/biomechanics/exercises/{squatId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var biomechanics = await response.Content.ReadFromJsonAsync<ExerciseBiomechanicsDto>();
        biomechanics.Should().NotBeNull();
        biomechanics!.ExerciseName.Should().Be("Barbell Back Squat");
        biomechanics.JointActions.Should().Contain(ja => ja.ActionType == JointActionType.Extension && ja.JointName == "Tibiofemoral Joint");
        biomechanics.Considerations.Should().Contain(c => c.Certainty == CertaintyLevel.Established);
    }

    [Fact]
    public async Task AddConsideration_WithValidData_PersistsSuccessfully()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var squatId = new Guid("44444444-4444-4444-4444-444444444401");
        var request = new CreateBiomechanicalConsiderationDto(
            BiomechanicalAspect.Stability,
            CertaintyLevel.Established,
            "External bracing elevates motor unit recruitment threshold",
            "A rigid abdominal wall creates hydraulic amplifier effect across lumbar motion segments.",
            "Take deep diaphragmatic breath into lifting belt prior to descent."
        );

        // Act
        var response = await _client.PostAsJsonAsync($"/api/biomechanics/exercises/{squatId}/considerations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<BiomechanicalConsiderationDto>();
        created.Should().NotBeNull();
        created!.ExerciseId.Should().Be(squatId);
        created.Summary.Should().Be(request.Summary);
        created.Certainty.Should().Be(CertaintyLevel.Established);
    }

    [Fact]
    public async Task AnatomyEndpoints_WithoutAuth_ReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act & Assert
        var r1 = await _client.GetAsync("/api/anatomy/regions");
        r1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var r2 = await _client.GetAsync("/api/biomechanics/exercises/44444444-4444-4444-4444-444444444401");
        r2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
