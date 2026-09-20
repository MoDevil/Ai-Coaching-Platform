using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Adaptations.DTOs;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Programs.DTOs;
using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Application.Workouts.DTOs;
using AiCoachOs.Domain.Adaptations;
using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.TrainingProfiles;
using AiCoachOs.Domain.Workouts;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Adaptations;

public class AdaptationsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdaptationsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string prefix = "coach_adp")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@egyptgym.com";
        var request = new RegisterCoachRequestDto("AdaptCoach", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<(Guid ClientId, ProgramDto Program, string Token)> SetupClientWithProgramAsync(string coachPrefix = "adp_coach")
    {
        var token = await RegisterAndLoginCoachAsync(coachPrefix);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create Client
        var createClientReq = new CreateClientRequestDto(
            FirstName: "Mahmoud",
            LastName: "Ibrahim",
            Email: $"mahmoud_{Guid.NewGuid():N}@gmail.com",
            Phone: "+201011223344",
            DateOfBirth: new DateTime(1995, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender: Domain.Clients.Gender.Male,
            Goal: new ClientGoalDto("Hypertrophy upper body focus", 12, "Build shoulders and chest"),
            IntakeNotes: "Healthy, consistent gym attender.");

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
                new(null, 1, "Chest", "Primary hypertrophy target")
            });

        var profileResp = await _client.PutAsJsonAsync($"/api/clients/{client!.Id}/training-profile", profileReq);
        profileResp.EnsureSuccessStatusCode();

        // Generate Program
        var generateReq = new GenerateProgramRequestDto(
            ClientId: client.Id,
            ProgramName: "Mahmoud 3-Day Hypertrophy",
            CoachNotes: "Controlled eccentric tempo",
            NumberOfWeeks: 4);

        var progResp = await _client.PostAsJsonAsync("/api/programs/generate", generateReq);
        progResp.EnsureSuccessStatusCode();
        var program = await progResp.Content.ReadFromJsonAsync<ProgramDto>();

        return (client.Id, program!, token);
    }

    [Fact]
    public async Task AssessProgramVersion_WithZeroWorkouts_ReturnsDefaultNoChangeAssessment()
    {
        // Arrange
        var (clientId, program, token) = await SetupClientWithProgramAsync("coach_zero");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var versionId = program.ActiveVersion!.Id;

        // Act
        var response = await _client.PostAsync($"/api/adaptations/assess/{versionId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var assessment = await response.Content.ReadFromJsonAsync<AdaptationAssessmentDto>();
        assessment.Should().NotBeNull();
        assessment!.ProgramVersionId.Should().Be(versionId);
        assessment.TotalExposures.Should().Be(0);
        assessment.AdherenceRate.Should().Be(0m);
        assessment.OverallStatus.Should().Be(AdaptationOverallStatus.ProgramWorking);
        assessment.Recommendations.Should().Contain(r => r.ActionType == AdaptationActionType.NoChange);
    }

    [Fact]
    public async Task CompleteWorkoutHistory_AssessAndApproveRecommendation_CreatesNewProgramVersion()
    {
        // Arrange
        var (clientId, program, token) = await SetupClientWithProgramAsync("coach_cycle");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var versionId = program.ActiveVersion!.Id;
        var firstSession = program.ActiveVersion.Weeks.First().Sessions.First();
        var firstSlot = firstSession.Slots.First();

        // Log 4 workouts for this session with low reps to simulate stagnation
        for (int w = 1; w <= 4; w++)
        {
            var startResp = await _client.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequestDto(
                ClientId: clientId,
                TrainingSessionId: firstSession.Id,
                Notes: $"Workout exposure {w}"));
            startResp.EnsureSuccessStatusCode();
            var workout = await startResp.Content.ReadFromJsonAsync<WorkoutSessionDto>();

            var workoutEx = workout!.Exercises.First(e => e.ExerciseSlotId == firstSlot.Id);

            for (int s = 1; s <= firstSlot.TargetSets; s++)
            {
                var setResp = await _client.PostAsJsonAsync($"/api/workouts/{workout.Id}/exercises/{workoutEx.Id}/sets", new RecordWorkoutSetRequestDto(
                    SetNumber: s,
                    Repetitions: 6, // Below target range 8-12 to fail progression rule
                    LoadKg: 70m,
                    Rir: 2.0m,
                    IsCompleted: true));
                setResp.EnsureSuccessStatusCode();
            }

            var completeResp = await _client.PostAsJsonAsync($"/api/workouts/{workout.Id}/complete", new CompleteWorkoutRequestDto());
            completeResp.EnsureSuccessStatusCode();
        }

        // Act 1: Run assessment
        var assessResp = await _client.PostAsync($"/api/adaptations/assess/{versionId}", null);
        assessResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var assessment = await assessResp.Content.ReadFromJsonAsync<AdaptationAssessmentDto>();
        assessment.Should().NotBeNull();
        assessment!.TotalExposures.Should().Be(4);
        assessment.CompletedExposures.Should().Be(4);
        assessment.AdherenceRate.Should().Be(100m);
        assessment.Recommendations.Should().NotBeEmpty();

        var rec = assessment.Recommendations.First(r => r.ActionType != AdaptationActionType.NoChange);

        // Act 2: Coach decides to approve the recommendation
        var decisionReq = new CoachRecommendationDecisionDto(
            Approve: true,
            CoachDecisionNote: "Approved adaptation based on consistent plateau data.");

        var decisionResp = await _client.PostAsJsonAsync($"/api/adaptations/recommendations/{rec.Id}/decision", decisionReq);
        decisionResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var decidedRec = await decisionResp.Content.ReadFromJsonAsync<AdaptationRecommendationDto>();
        decidedRec.Should().NotBeNull();
        decidedRec!.Status.Should().Be(RecommendationStatus.Applied);
        decidedRec.CoachDecisionNote.Should().Be("Approved adaptation based on consistent plateau data.");

        // Act 3: Verify that a new ProgramVersion v2 was created on the program
        var progResp = await _client.GetAsync($"/api/programs/{program.Id}");
        progResp.EnsureSuccessStatusCode();
        var updatedProgram = await progResp.Content.ReadFromJsonAsync<ProgramDto>();
        updatedProgram.Should().NotBeNull();
        updatedProgram!.ActiveVersion.Should().NotBeNull();
        updatedProgram.ActiveVersion!.VersionNumber.Should().Be(2);
        updatedProgram.ActiveVersion.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Coach_CannotAssessAnotherCoachesProgram()
    {
        // Arrange
        var (clientId, program, coach1Token) = await SetupClientWithProgramAsync("coach1_sec");
        var coach2Token = await RegisterAndLoginCoachAsync("coach2_sec");

        // Act: Coach 2 attempts to trigger assessment on Coach 1's program version
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", coach2Token);
        var response = await _client.PostAsync($"/api/adaptations/assess/{program.ActiveVersion!.Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
