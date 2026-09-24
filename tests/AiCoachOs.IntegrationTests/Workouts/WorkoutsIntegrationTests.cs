using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Programs.DTOs;
using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Application.Workouts.DTOs;
using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.TrainingProfiles;
using AiCoachOs.Domain.Workouts;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Workouts;

public class WorkoutsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public WorkoutsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string prefix = "coach_wkt")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto("WorkoutCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<(Guid ClientId, ProgramDto Program, string Token)> SetupClientWithProgramAsync()
    {
        var token = await RegisterAndLoginCoachAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create Client
        var createClientReq = new CreateClientRequestDto(
            FirstName: "Kareem",
            LastName: "Hassan",
            Email: $"kareem_{Guid.NewGuid():N}@gmail.com",
            Phone: "+201098765432",
            DateOfBirth: new DateTime(1996, 8, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender: Domain.Clients.Gender.Male,
            Goal: new ClientGoalDto("Build chest and delts hypertrophy", 12, "Focus on upper chest"),
            IntakeNotes: "Great recovery capacity, ready for heavy training.");

        var clientResp = await _client.PostAsJsonAsync("/api/clients", createClientReq);
        clientResp.EnsureSuccessStatusCode();
        var client = await clientResp.Content.ReadFromJsonAsync<ClientDto>();

        // Update Profile
        var profileReq = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Intermediate,
            WeeklyAvailability: new TrainingAvailabilityDto(
                SessionsPerWeek: 3,
                AvailableDays: new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                PreferredDays: new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }),
            SessionDurationTargetMinutes: 60,
            SessionDurationMinMinutes: 45,
            SessionDurationMaxMinutes: 75,
            AvailableEquipmentIds: null,
            ExercisePreferences: "Dumbbell Bench Press, Lat Pulldown",
            ExerciseConstraints: null,
            Priorities: new List<ClientTrainingPriorityDto>
            {
                new(null, 1, "Chest", "Primary hypertrophy target"),
                new(null, 2, "Back", "Secondary target")
            });

        var profileResp = await _client.PutAsJsonAsync($"/api/clients/{client!.Id}/training-profile", profileReq);
        profileResp.EnsureSuccessStatusCode();

        // Generate Program
        var generateReq = new GenerateProgramRequestDto(
            ClientId: client.Id,
            ProgramName: "Kareem 3-Day Split",
            CoachNotes: "Focus on controlled eccentrics",
            NumberOfWeeks: 4);

        var progResp = await _client.PostAsJsonAsync("/api/programs/generate", generateReq);
        progResp.EnsureSuccessStatusCode();
        var program = await progResp.Content.ReadFromJsonAsync<ProgramDto>();

        return (client.Id, program!, token);
    }

    [Fact]
    public async Task StartWorkout_FromPlannedSession_PopulatesPlannedExercises()
    {
        // Arrange
        var (clientId, program, token) = await SetupClientWithProgramAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var firstSession = program.ActiveVersion!.Weeks.First().Sessions.First();

        var startReq = new StartWorkoutRequestDto(
            ClientId: clientId,
            TrainingSessionId: firstSession.Id,
            Notes: "Starting Day 1 Workout");

        // Act
        var response = await _client.PostAsJsonAsync("/api/workouts/start", startReq);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var workout = await response.Content.ReadFromJsonAsync<WorkoutSessionDto>();
        workout.Should().NotBeNull();
        workout!.ClientId.Should().Be(clientId);
        workout.TrainingSessionId.Should().Be(firstSession.Id);
        workout.Status.Should().Be(WorkoutStatus.InProgress);
        workout.Exercises.Should().HaveCount(firstSession.Slots.Count);
        workout.Exercises.First().ExerciseSlotId.Should().Be(firstSession.Slots.First().Id);
    }

    [Fact]
    public async Task CompleteWorkoutFlow_WithSets_EvaluatesProgressionCorrectly()
    {
        // Arrange
        var (clientId, program, token) = await SetupClientWithProgramAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var firstSession = program.ActiveVersion!.Weeks.First().Sessions.First();
        var plannedSlot = firstSession.Slots.First();

        var startReq = new StartWorkoutRequestDto(
            ClientId: clientId,
            TrainingSessionId: firstSession.Id,
            Notes: "Progressive Overload Test Session");

        var startResp = await _client.PostAsJsonAsync("/api/workouts/start", startReq);
        startResp.EnsureSuccessStatusCode();
        var workout = await startResp.Content.ReadFromJsonAsync<WorkoutSessionDto>();

        var maxReps = 12;
        if (plannedSlot.TargetRepRange.Contains("-"))
        {
            var parts = plannedSlot.TargetRepRange.Split("-");
            if (int.TryParse(parts[1].Trim(), out var parsed))
            {
                maxReps = parsed;
            }
        }

        var workoutEx = workout!.Exercises.First(e => e.ExerciseSlotId == plannedSlot.Id);

        // Act: Log planned number of sets hitting upper rep targets
        for (int i = 1; i <= plannedSlot.TargetSets; i++)
        {
            var setReq = new RecordWorkoutSetRequestDto(
                SetNumber: i,
                Repetitions: maxReps,
                LoadKg: 80m,
                Rir: 2m,
                IsCompleted: true,
                Notes: $"Set {i} strong");

            var setResp = await _client.PostAsJsonAsync($"/api/workouts/{workout!.Id}/exercises/{workoutEx.Id}/sets", setReq);
            if (!setResp.IsSuccessStatusCode)
            {
                var errContent = await setResp.Content.ReadAsStringAsync();
                throw new Exception($"RecordSet failed with {setResp.StatusCode}: {errContent}");
            }
            setResp.EnsureSuccessStatusCode();
            workout = await setResp.Content.ReadFromJsonAsync<WorkoutSessionDto>();
        }

        // Complete the workout
        var completeReq = new CompleteWorkoutRequestDto(Notes: "Felt strong and ready to progress");
        var completeResp = await _client.PostAsJsonAsync($"/api/workouts/{workout!.Id}/complete", completeReq);

        // Assert
        completeResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var completedWorkout = await completeResp.Content.ReadFromJsonAsync<WorkoutSessionDto>();
        completedWorkout.Should().NotBeNull();
        completedWorkout!.Status.Should().Be(WorkoutStatus.Completed);
        completedWorkout.CompletedAtUtc.Should().NotBeNull();

        var updatedEx = completedWorkout.Exercises.First(e => e.Id == workoutEx.Id);
        updatedEx.Sets.Should().HaveCount(plannedSlot.TargetSets);
        updatedEx.ProgressionResult.Should().NotBeNull();
        updatedEx.ProgressionResult!.Status.Should().Be(ProgressionEvaluationStatus.Met);
        updatedEx.ProgressionResult.SuggestedNextTarget.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Coach_CannotAccessAnotherCoachesWorkout()
    {
        // Arrange
        var (clientId, program, coach1Token) = await SetupClientWithProgramAsync();
        var coach2Token = await RegisterAndLoginCoachAsync("coach2");

        // Coach 1 starts workout
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", coach1Token);
        var startResp = await _client.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequestDto(clientId));
        startResp.EnsureSuccessStatusCode();
        var workout = await startResp.Content.ReadFromJsonAsync<WorkoutSessionDto>();

        // Act: Coach 2 attempts to fetch Coach 1's workout
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", coach2Token);
        var getResp = await _client.GetAsync($"/api/workouts/{workout!.Id}");

        // Assert
        getResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
