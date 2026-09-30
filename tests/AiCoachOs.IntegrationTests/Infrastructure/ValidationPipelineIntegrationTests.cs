using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Programs.DTOs;
using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Application.Workouts.DTOs;
using AiCoachOs.Domain.TrainingProfiles;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Infrastructure;

/// <summary>
/// Proves that the registered FluentValidation validators are actually invoked on the HTTP pipeline.
/// Before this was wired, validators were resolved in DI but never executed, so rules such as the
/// RIR bounds and the email format were unenforced at runtime. These tests assert request-level
/// behaviour rather than the validator classes in isolation, because the defect was that the
/// validators passed their own unit tests while having no effect on the API.
/// </summary>
public class ValidationPipelineIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ValidationPipelineIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterCoachAsync(string prefix)
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@egyptgym.com";
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterCoachRequestDto("Validation Coach", email, "Password123!"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>())!.Token;
    }

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(body).RootElement.Clone();
    }

    private static void ShouldHaveFieldError(JsonElement problem, string field)
    {
        problem.GetProperty("errors").TryGetProperty(field, out _).Should().BeTrue(
            $"expected a validation error for '{field}' but the response was: {problem}");
    }

    private async Task<Guid> CreateClientAsync()
    {
        var request = new CreateClientRequestDto(
            FirstName: "Tarek",
            LastName: "Nabil",
            Email: $"tarek_{Guid.NewGuid():N}@gmail.com",
            Phone: "+201098765432",
            DateOfBirth: new DateTime(1994, 3, 11, 0, 0, 0, DateTimeKind.Utc),
            Gender: Domain.Clients.Gender.Male,
            Goal: new ClientGoalDto("Build chest and delts hypertrophy", 12, "Focus on upper chest"),
            IntakeNotes: "Good recovery capacity.");

        var response = await _client.PostAsJsonAsync("/api/clients", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ClientDto>())!.Id;
    }

    private async Task<Guid> CreateClientWithProfileAsync()
    {
        var clientId = await CreateClientAsync();

        var profile = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Intermediate,
            SessionDurationMinMinutes: 45,
            SessionDurationTargetMinutes: 60,
            SessionDurationMaxMinutes: 75,
            WeeklyAvailability: new TrainingAvailabilityDto(
                SessionsPerWeek: 3,
                AvailableDays: new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                PreferredDays: new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }),
            AvailableEquipmentIds: null,
            ExercisePreferences: "Dumbbell Bench Press, Lat Pulldown",
            ExerciseConstraints: null,
            Priorities: new List<ClientTrainingPriorityDto>
            {
                new(null, 1, "Chest", "Primary hypertrophy target")
            });

        var response = await _client.PutAsJsonAsync($"/api/clients/{clientId}/training-profile", profile);
        response.EnsureSuccessStatusCode();

        return clientId;
    }

    private async Task<WorkoutSessionDto> StartPlannedWorkoutAsync(Guid clientId)
    {
        var generate = await _client.PostAsJsonAsync("/api/programs/generate", new GenerateProgramRequestDto(
            ClientId: clientId,
            ProgramName: "Validation Program",
            CoachNotes: "Controlled eccentrics",
            NumberOfWeeks: 4));
        generate.EnsureSuccessStatusCode();

        var program = await generate.Content.ReadFromJsonAsync<ProgramDto>();
        var session = program!.ActiveVersion!.Weeks.First().Sessions.First();

        var start = await _client.PostAsJsonAsync("/api/workouts/start",
            new StartWorkoutRequestDto(ClientId: clientId, TrainingSessionId: session.Id));
        start.EnsureSuccessStatusCode();

        return (await start.Content.ReadFromJsonAsync<WorkoutSessionDto>())!;
    }

    [Fact]
    public async Task Register_WithMalformedEmail_Returns400ValidationProblem()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterCoachRequestDto("Coach", "not-an-email", "Password123!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problem = await ReadProblemAsync(response);
        problem.GetProperty("title").GetString().Should().Be("Validation Error");
        ShouldHaveFieldError(problem, "Email");
    }

    [Fact]
    public async Task Register_WithEmptyFullName_Returns400ValidationProblem()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterCoachRequestDto(string.Empty, $"noname_{Guid.NewGuid():N}@egyptgym.com", "Password123!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ShouldHaveFieldError(await ReadProblemAsync(response), "FullName");
    }

    [Theory]
    [InlineData("Sh0rt!")]         // below the 8 character minimum
    [InlineData("nouppercase1!")]  // no uppercase letter
    [InlineData("NOLOWERCASE1!")]  // no lowercase letter
    [InlineData("NoDigitsHere!")]  // no digit
    [InlineData("NoSpecial123")]   // no non-alphanumeric character
    public async Task Register_WithWeakPassword_Returns400ValidationProblem(string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterCoachRequestDto("Weak Password", $"weakpw_{Guid.NewGuid():N}@egyptgym.com", password));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ShouldHaveFieldError(await ReadProblemAsync(response), "Password");
    }

    [Fact]
    public async Task UpdateTrainingProfile_WithInvalidAvailability_Returns400ValidationProblem()
    {
        // This endpoint previously validated by hand inside the controller. The manual check was
        // removed in favour of the pipeline, so this test guards against enforcement regressing.
        var token = await RegisterCoachAsync("coach_profile");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var clientId = await CreateClientAsync();

        var request = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Intermediate,
            SessionDurationMinMinutes: 30,
            SessionDurationTargetMinutes: 60,
            SessionDurationMaxMinutes: 75,
            WeeklyAvailability: new TrainingAvailabilityDto(
                SessionsPerWeek: 0, // below the 1..7 bound
                AvailableDays: new[] { DayOfWeek.Monday },
                PreferredDays: new[] { DayOfWeek.Monday }),
            AvailableEquipmentIds: null,
            ExercisePreferences: null,
            ExerciseConstraints: null,
            Priorities: new List<ClientTrainingPriorityDto>());

        var response = await _client.PutAsJsonAsync($"/api/clients/{clientId}/training-profile", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ShouldHaveFieldError(await ReadProblemAsync(response), "WeeklyAvailability.SessionsPerWeek");
    }

    [Fact]
    public async Task UpdateTrainingProfile_WithMinDurationAboveTarget_Returns400ValidationProblem()
    {
        var token = await RegisterCoachAsync("coach_durations");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var clientId = await CreateClientAsync();

        var request = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Intermediate,
            SessionDurationMinMinutes: 90, // greater than the target below
            SessionDurationTargetMinutes: 45,
            SessionDurationMaxMinutes: 75,
            WeeklyAvailability: new TrainingAvailabilityDto(
                SessionsPerWeek: 3,
                AvailableDays: new[] { DayOfWeek.Monday, DayOfWeek.Wednesday },
                PreferredDays: new[] { DayOfWeek.Monday }),
            AvailableEquipmentIds: null,
            ExercisePreferences: null,
            ExerciseConstraints: null,
            Priorities: new List<ClientTrainingPriorityDto>());

        var response = await _client.PutAsJsonAsync($"/api/clients/{clientId}/training-profile", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await ReadProblemAsync(response);
        problem.GetProperty("errors").EnumerateObject().Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateAvailability_WithOutOfRangeSessionsPerWeek_Returns400ValidationProblem()
    {
        var token = await RegisterCoachAsync("coach_avail");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var clientId = await CreateClientAsync();

        var valid = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Intermediate,
            SessionDurationMinMinutes: 45,
            SessionDurationTargetMinutes: 60,
            SessionDurationMaxMinutes: 75,
            WeeklyAvailability: new TrainingAvailabilityDto(
                SessionsPerWeek: 3,
                AvailableDays: new[] { DayOfWeek.Monday },
                PreferredDays: new[] { DayOfWeek.Monday }),
            AvailableEquipmentIds: null,
            ExercisePreferences: null,
            ExerciseConstraints: null,
            Priorities: new List<ClientTrainingPriorityDto>());

        var setup = await _client.PutAsJsonAsync($"/api/clients/{clientId}/training-profile", valid);
        setup.EnsureSuccessStatusCode();

        var invalid = new TrainingAvailabilityDto(
            SessionsPerWeek: 12, // above the 1..7 bound
            AvailableDays: new[] { DayOfWeek.Monday },
            PreferredDays: new[] { DayOfWeek.Monday });

        var response = await _client.PutAsJsonAsync($"/api/clients/{clientId}/training-profile/availability", invalid);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ShouldHaveFieldError(await ReadProblemAsync(response), "SessionsPerWeek");
    }

    [Theory]
    [InlineData(11)]  // above the 0..10 bound
    [InlineData(-1)]  // below the 0..10 bound
    public async Task RecordWorkoutSet_WithOutOfRangeRir_Returns400ValidationProblem(int rir)
    {
        // RIR drives every progression and plateau verdict in the deterministic engines, so an
        // out-of-range value has to be rejected at the boundary rather than silently distorting
        // the coaching decision that follows.
        var token = await RegisterCoachAsync("coach_rir");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var clientId = await CreateClientWithProfileAsync();
        var workout = await StartPlannedWorkoutAsync(clientId);
        var workoutExerciseId = workout.Exercises.First().Id;

        var response = await _client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/exercises/{workoutExerciseId}/sets",
            new RecordWorkoutSetRequestDto(
                SetNumber: 1,
                Repetitions: 8,
                LoadKg: 60m,
                Rir: rir));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ShouldHaveFieldError(await ReadProblemAsync(response), "Rir");
    }

    [Fact]
    public async Task RecordWorkoutSet_WithNegativeLoad_Returns400ValidationProblem()
    {
        var token = await RegisterCoachAsync("coach_negload");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var clientId = await CreateClientWithProfileAsync();
        var workout = await StartPlannedWorkoutAsync(clientId);
        var workoutExerciseId = workout.Exercises.First().Id;

        var response = await _client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/exercises/{workoutExerciseId}/sets",
            new RecordWorkoutSetRequestDto(
                SetNumber: 1,
                Repetitions: 8,
                LoadKg: -50m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ShouldHaveFieldError(await ReadProblemAsync(response), "LoadKg");
    }
}
