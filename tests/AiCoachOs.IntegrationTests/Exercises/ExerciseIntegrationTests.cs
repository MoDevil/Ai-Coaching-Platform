using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Exercises.DTOs;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Infrastructure.Persistence;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Exercises;

public class ExerciseIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExerciseIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync()
    {
        var email = $"coach_ex_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto("ExerciseCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    [Fact]
    public async Task GetExercises_ReturnsSeededExerciseCatalog()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/exercises");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exercises = await response.Content.ReadFromJsonAsync<IReadOnlyList<ExerciseSummaryDto>>();
        exercises.Should().NotBeNull();
        exercises!.Count.Should().BeGreaterThanOrEqualTo(10);
        exercises.Should().OnlyContain(e => e.MetadataStatus == MetadataStatus.Provisional);
        exercises.Should().Contain(e => e.Name.Contains("Squat"));
        exercises.Should().Contain(e => e.Name.Contains("Bench"));
    }

    [Fact]
    public async Task GetExercises_WithPatternFilter_ReturnsFilteredResults()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - filter by Squat pattern
        var response = await _client.GetAsync($"/api/exercises?movementPatternId={ExerciseLibrarySeeder.Patterns.Squat}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exercises = await response.Content.ReadFromJsonAsync<IReadOnlyList<ExerciseSummaryDto>>();
        exercises.Should().NotBeNull();
        exercises!.Should().NotBeEmpty();
        exercises!.All(e => e.MovementPatternId == ExerciseLibrarySeeder.Patterns.Squat).Should().BeTrue();
    }

    [Fact]
    public async Task GetExercises_WithSearchQuery_ReturnsMatchingExercises()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/exercises?search=bench");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exercises = await response.Content.ReadFromJsonAsync<IReadOnlyList<ExerciseSummaryDto>>();
        exercises.Should().NotBeNull();
        exercises!.Should().NotBeEmpty();
        exercises!.All(e => e.Name.ToLower().Contains("bench") || (e.Aliases != null && e.Aliases.ToLower().Contains("bench"))).Should().BeTrue();
    }

    [Fact]
    public async Task GetExerciseById_ReturnsFullDetailsWithMusclesAndEquipment()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var listResponse = await _client.GetAsync("/api/exercises");
        var exercises = await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<ExerciseSummaryDto>>();
        var firstId = exercises!.First().Id;

        // Act
        var response = await _client.GetAsync($"/api/exercises/{firstId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<ExerciseDetailDto>();
        detail.Should().NotBeNull();
        detail!.Id.Should().Be(firstId);
        detail.MetadataStatus.Should().Be(MetadataStatus.Provisional);
        detail.Muscles.Should().NotBeEmpty();
        detail.Equipment.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetExerciseSubstitutions_ReturnsValidSubstitutionsWithIntentNotes()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var squatId = new Guid("44444444-4444-4444-4444-444444444401"); // Barbell Back Squat

        // Act
        var response = await _client.GetAsync($"/api/exercises/{squatId}/substitutions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var substitutions = await response.Content.ReadFromJsonAsync<IReadOnlyList<ExerciseSubstitutionDto>>();
        substitutions.Should().NotBeNull();
        substitutions!.Should().NotBeEmpty();
        substitutions!.Should().Contain(s => s.SubstituteExerciseName.Contains("Hack Squat") && s.IntentPreservationNotes != null);
    }

    [Fact]
    public async Task GetMetadataEndpoints_ReturnLists()
    {
        // Arrange
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act & Assert Patterns
        var patResp = await _client.GetAsync("/api/exercises/meta/movement-patterns");
        patResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var patterns = await patResp.Content.ReadFromJsonAsync<IReadOnlyList<MovementPatternDto>>();
        patterns.Should().NotBeEmpty();

        // Act & Assert Muscles
        var muscResp = await _client.GetAsync("/api/exercises/meta/muscles");
        muscResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var muscles = await muscResp.Content.ReadFromJsonAsync<IReadOnlyList<MuscleDto>>();
        muscles.Should().NotBeEmpty();

        // Act & Assert Equipment
        var eqResp = await _client.GetAsync("/api/exercises/meta/equipment");
        eqResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var equipment = await eqResp.Content.ReadFromJsonAsync<IReadOnlyList<EquipmentDto>>();
        equipment.Should().NotBeEmpty();
    }
}
