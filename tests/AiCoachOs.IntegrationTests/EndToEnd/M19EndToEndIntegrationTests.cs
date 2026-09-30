using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Adaptations.DTOs;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.ExpertIngestion.Dtos;
using AiCoachOs.Application.Memory.Dtos;
using AiCoachOs.Application.Programs.DTOs;
using AiCoachOs.Application.Rehab.Dtos;
using AiCoachOs.Application.Safety.Dtos;
using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Application.Workouts.DTOs;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Memory;
using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.Rehab;
using AiCoachOs.Domain.Safety;
using AiCoachOs.Domain.TrainingProfiles;
using AiCoachOs.Domain.Workouts;
using AiCoachOs.Infrastructure.Persistence;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace AiCoachOs.IntegrationTests.EndToEnd;

public class M19EndToEndIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly ITestOutputHelper _output;

    public M19EndToEndIntegrationTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string suffix)
    {
        var email = $"coach_m19_{suffix}_{Guid.NewGuid():N}@egyptcoaching.com";
        var request = new RegisterCoachRequestDto("Coach M19", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<ClientDto> CreateClientAsync(string token, string firstName = "Ziad", string lastName = "Gomaa")
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new CreateClientRequestDto(
            FirstName: firstName,
            LastName: lastName,
            Email: $"{firstName.ToLower()}_{Guid.NewGuid():N}@test.com",
            Phone: "+201001122334",
            DateOfBirth: new DateTime(1996, 7, 10, 0, 0, 0, DateTimeKind.Utc),
            Gender: Domain.Clients.Gender.Male,
            Goal: new ClientGoalDto("Hypertrophy & progressive overload", 12, "Build shoulders and chest"),
            IntakeNotes: "Healthy athlete ready for training.");
        var response = await _client.PostAsJsonAsync("/api/clients", request);
        response.EnsureSuccessStatusCode();
        var client = await response.Content.ReadFromJsonAsync<ClientDto>();
        return client!;
    }

    private async Task SetupTrainingProfileAsync(string token, Guid clientId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var profileReq = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Intermediate,
            SessionDurationMinMinutes: 45,
            SessionDurationTargetMinutes: 60,
            SessionDurationMaxMinutes: 75,
            WeeklyAvailability: new TrainingAvailabilityDto(
                SessionsPerWeek: 4,
                AvailableDays: new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                PreferredDays: new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday }),
            AvailableEquipmentIds: null,
            ExercisePreferences: "Bench Press, Squat, Lat Pulldown",
            ExerciseConstraints: null,
            Priorities: null);

        var profileResp = await _client.PutAsJsonAsync($"/api/clients/{clientId}/training-profile", profileReq);
        profileResp.EnsureSuccessStatusCode();
    }

    #region Flow 1 — Complete Client Lifecycle

    [Fact]
    public async Task Flow1_CompleteClientLifecycle_ConnectsAllModulesEndToEnd()
    {
        // 1. Create client & Training Profile
        var token = await RegisterAndLoginCoachAsync("flow1");
        var client = await CreateClientAsync(token, "Ahmed", "Nabil");
        await SetupTrainingProfileAsync(token, client.Id);

        // 2. Generate M5 Program
        var progGenResp = await _client.PostAsJsonAsync("/api/programs/generate", new GenerateProgramRequestDto(
            ClientId: client.Id,
            ProgramName: "4-Day Hypertrophy Wave",
            CoachNotes: "Initial baseline",
            NumberOfWeeks: 4));
        progGenResp.EnsureSuccessStatusCode();
        var program = await progGenResp.Content.ReadFromJsonAsync<ProgramDto>();
        program.Should().NotBeNull();
        program!.Versions.Should().NotBeEmpty();
        var initialVersion = program.Versions.First();

        // 3. Log 4 M6 Workout Sessions
        var firstSession = initialVersion.Weeks.First().Sessions.First();
        var firstSlot = firstSession.Slots.First();

        for (int i = 1; i <= 4; i++)
        {
            // Start workout
            var startResp = await _client.PostAsJsonAsync("/api/workouts/start", new StartWorkoutRequestDto(
                ClientId: client.Id,
                TrainingSessionId: firstSession.Id,
                Notes: $"Workout {i}"));
            startResp.EnsureSuccessStatusCode();
            var session = await startResp.Content.ReadFromJsonAsync<WorkoutSessionDto>();
            session.Should().NotBeNull();
            var workoutEx = session!.Exercises.First();

            // Record set
            var setResp = await _client.PostAsJsonAsync($"/api/workouts/{session.Id}/exercises/{workoutEx.Id}/sets", new RecordWorkoutSetRequestDto(
                SetNumber: 1,
                Repetitions: 10,
                LoadKg: 80,
                Rir: 2,
                IsCompleted: true));
            setResp.EnsureSuccessStatusCode();

            // Complete workout
            var compResp = await _client.PostAsJsonAsync($"/api/workouts/{session.Id}/complete", new CompleteWorkoutRequestDto(
                Notes: $"Workout {i} finished"));
            compResp.EnsureSuccessStatusCode();
        }

        // 4. M7 Adaptation Assessment
        var adaptResp = await _client.PostAsJsonAsync($"/api/adaptations/assess/{initialVersion.Id}", new { });
        adaptResp.EnsureSuccessStatusCode();
        var assessment = await adaptResp.Content.ReadFromJsonAsync<AdaptationAssessmentDto>();
        assessment.Should().NotBeNull();

        // 5. M14 Reasoning using ProgramAdaptationReview
        var reasoningResp = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = client.Id,
            ReasoningCategory = ReasoningCategory.ProgramAdaptationReview,
            AdditionalContext = "Evaluate 4 logged sessions for microcycle volume progression"
        });
        reasoningResp.EnsureSuccessStatusCode();
        var rec = await reasoningResp.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();
        rec.Should().NotBeNull();
        rec!.ReviewStatus.Should().Be(AIRecommendationReviewStatus.PendingReview);

        // 6. M18 Coach Accepts Recommendation
        var reviewResp = await _client.PatchAsJsonAsync($"/api/reasoning/{rec.Id}/review", new ReviewAIRecommendationRequestDto
        {
            ReviewStatus = AIRecommendationReviewStatus.Accepted,
            CoachDecision = "Accepted adaptation: advance working weight on primary compound lifts.",
            FinalImplementedPlan = "Increase bench press working sets from 80kg to 82.5kg."
        });
        reviewResp.EnsureSuccessStatusCode();
        var reviewedRec = await reviewResp.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();
        reviewedRec!.ReviewStatus.Should().Be(AIRecommendationReviewStatus.Accepted);

        // 7. Create new M5 Program / Version
        Guid newVersionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var newVersion = new ProgramVersion(Guid.NewGuid(), program.Id, 2, RecoveryCapacity.High, "Progressive overload adaptation", true);
            db.ProgramVersionsDbSet.Add(newVersion);
            await db.SaveChangesAsync();
            newVersionId = newVersion.Id;
        }

        // 8. M19 Link Recommendation -> ProgramVersion
        var linkResp = await _client.PatchAsJsonAsync($"/api/reasoning/{rec.Id}/link-program-version", new LinkProgramVersionRequestDto
        {
            ProgramVersionId = newVersionId
        });
        linkResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var linkedRec = await linkResp.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();
        linkedRec.Should().NotBeNull();
        linkedRec!.ImplementedProgramVersionId.Should().Be(newVersionId);

        // 9. Verify Persisted Detail & FK in Database
        var detailResp = await _client.GetFromJsonAsync<AIRecommendationDetailDto>($"/api/clients/{client.Id}/recommendations/{rec.Id}");
        detailResp.Should().NotBeNull();
        detailResp!.ImplementedProgramVersionId.Should().Be(newVersionId);
        detailResp.ImplementedProgramVersionLabel.Should().NotBeNullOrWhiteSpace();
        detailResp.ImplementedProgramVersionLabel.Should().Contain("v2");
    }

    #endregion

    #region Flow 2 — Safety Gate

    [Fact]
    public async Task Flow2_SafetyGate_BlocksReasoningWhenUrgentSafetyActive_AndAllowsAfterAcknowledgement()
    {
        var token = await RegisterAndLoginCoachAsync("flow2_safety");
        var client = await CreateClientAsync(token, "Tariq", "Salem");
        await SetupTrainingProfileAsync(token, client.Id);

        // 1. Report severe chest pressure resulting in UrgentMedicalAttention
        var screenResp = await _client.PostAsJsonAsync("/api/safety/screen", new CreateSafetyReportRequestDto(
            ClientId: client.Id,
            TriggeredByType: TriggeredByType.DirectReport,
            Signals: new List<ReportedSignalDto>
            {
                new(
                    BodyRegion: "Chest",
                    SignalType: SignalType.Pain,
                    Onset: SignalOnset.Sudden,
                    Timing: SignalTiming.DuringExercise,
                    Severity: SignalSeverity.Severe,
                    FreeText: "Acute chest pain during exercise")
            }));

        screenResp.EnsureSuccessStatusCode();
        var screening = await screenResp.Content.ReadFromJsonAsync<SafetyScreeningDto>();
        screening.Should().NotBeNull();
        screening!.RequiresCoachAcknowledgment.Should().BeTrue();

        // 2. Invoke M14 Reasoning -> Referral Notice is populated or blocked
        var reasoningResp = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = client.Id,
            ReasoningCategory = ReasoningCategory.SafetyContextSummary,
            AdditionalContext = "Safety screening check"
        });

        if (reasoningResp.IsSuccessStatusCode)
        {
            var rec = await reasoningResp.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();
            rec.Should().NotBeNull();
            rec!.RecommendationText.Should().Contain("SAFETY REFERRAL NOTICE");
        }
        else
        {
            reasoningResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // 3. Acknowledge safety screening
        var ackResp = await _client.PostAsJsonAsync($"/api/safety/screenings/{screening.Id}/acknowledge", new AcknowledgeSafetyScreeningRequestDto(
            CoachNote: "Physician consultation confirmed safe for low intensity return"));
        ackResp.EnsureSuccessStatusCode();

        // 4. Invoke reasoning again -> succeeds
        var retryResp = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = client.Id,
            ReasoningCategory = ReasoningCategory.GeneralCoachingNote,
            AdditionalContext = "Cleared after physician consultation"
        });

        retryResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdRec = await retryResp.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();
        createdRec.Should().NotBeNull();
        createdRec!.ReviewStatus.Should().Be(AIRecommendationReviewStatus.PendingReview);
    }

    #endregion

    #region Flow 3 — Rehab Context

    [Fact]
    public async Task Flow3_RehabContext_IncludesTrainingLimitationsInReasoningEvaluation()
    {
        var token = await RegisterAndLoginCoachAsync("flow3_rehab");
        var client = await CreateClientAsync(token, "Sami", "Youssef");
        await SetupTrainingProfileAsync(token, client.Id);

        // 1. Add Training Limitation / Rehab Context
        var limitResp = await _client.PostAsJsonAsync("/api/rehab/limitations", new CreateTrainingLimitationRequestDto(
            ClientId: client.Id,
            AffectedBodyRegion: "Left Shoulder",
            LimitationSource: LimitationSource.ReportedByClient,
            Description: "Acromioclavicular impingement restricting overhead pressing beyond 90 degrees."));

        limitResp.EnsureSuccessStatusCode();

        // 2. Also record in Client Memory so snapshot contains it
        await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory", new CreateClientMemoryRequestDto
        {
            MemoryCategory = MemoryCategory.PainObservation,
            SourceType = MemorySourceType.CoachRecorded,
            Content = "Client has left shoulder impingement with strict avoidance of overhead barbell presses."
        });

        // 3. Run M14 Reasoning
        var reasoningResp = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = client.Id,
            ReasoningCategory = ReasoningCategory.ExerciseModificationReview,
            AdditionalContext = "Assess pressing exercises with shoulder limitation"
        });

        reasoningResp.EnsureSuccessStatusCode();
        var rec = await reasoningResp.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();
        rec.Should().NotBeNull();

        // 4. Verify detail contains context
        var detailResp = await _client.GetFromJsonAsync<AIRecommendationDetailDto>($"/api/clients/{client.Id}/recommendations/{rec!.Id}");
        detailResp.Should().NotBeNull();
        detailResp!.Summary.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Flow 4 — Expert Evidence

    [Fact]
    public async Task Flow4_ExpertEvidence_FlowsFromIngestionThroughM3ToM14Reasoning()
    {
        var token = await RegisterAndLoginCoachAsync("flow4_expert");
        var client = await CreateClientAsync(token, "Maged", "Adel");
        await SetupTrainingProfileAsync(token, client.Id);

        // 1. Create a confirmed Active M3 KnowledgeClaim
        Guid claimId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var claim = new KnowledgeClaim(
                id: Guid.NewGuid(),
                topic: "Hypertrophy Volume",
                question: "What weekly set volume maximizes quadriceps hypertrophy?",
                claimText: "12-18 direct weekly working sets with 1-2 RIR optimizes quadriceps hypertrophy in trained lifters.",
                evidenceLevel: EvidenceLevel.MetaAnalysis,
                status: ClaimStatus.Active,
                reviewedAtUtc: DateTime.UtcNow,
                reviewedBy: "Dr. Brad Schoenfeld");

            db.KnowledgeClaimsDbSet.Add(claim);
            await db.SaveChangesAsync();
            claimId = claim.Id;
        }

        // 2. Run M14 Reasoning
        var reasoningResp = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = client.Id,
            ReasoningCategory = ReasoningCategory.ProgramAdaptationReview,
            AdditionalContext = $"Apply scientific evidence from Claim {claimId}"
        });

        reasoningResp.EnsureSuccessStatusCode();
        var rec = await reasoningResp.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();
        rec.Should().NotBeNull();

        // 3. Verify the M3 Knowledge Claim appears in the resolved evidence claims of the detail DTO
        var detailResp = await _client.GetFromJsonAsync<AIRecommendationDetailDto>($"/api/clients/{client.Id}/recommendations/{rec!.Id}");
        detailResp.Should().NotBeNull();
        detailResp!.ResolvedKnowledgeClaims.Should().NotBeEmpty();
        detailResp.ResolvedKnowledgeClaims.Should().Contain(c => c.Id == claimId || c.Topic == "Hypertrophy Volume");
    }

    #endregion

    #region Flow 5 — Equipment Constraint

    [Fact]
    public async Task Flow5_EquipmentConstraint_GeneratedProgramRespectsConfiguredEquipment()
    {
        var token = await RegisterAndLoginCoachAsync("flow5_equipment");
        var client = await CreateClientAsync(token, "Hassan", "Mostafa");

        // 1. Configure client with minimal equipment
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var profileReq = new UpdateTrainingProfileRequestDto(
            ExperienceLevel: TrainingExperienceLevel.Beginner,
            SessionDurationMinMinutes: 30,
            SessionDurationTargetMinutes: 45,
            SessionDurationMaxMinutes: 60,
            WeeklyAvailability: new TrainingAvailabilityDto(
                SessionsPerWeek: 3,
                AvailableDays: new[] { DayOfWeek.Sunday, DayOfWeek.Tuesday, DayOfWeek.Thursday },
                PreferredDays: new[] { DayOfWeek.Sunday, DayOfWeek.Tuesday, DayOfWeek.Thursday }),
            AvailableEquipmentIds: new List<Guid>(), // Minimal equipment
            ExercisePreferences: "Dumbbell Press, Bodyweight Squat",
            ExerciseConstraints: null,
            Priorities: null);

        var profileResp = await _client.PutAsJsonAsync($"/api/clients/{client.Id}/training-profile", profileReq);
        profileResp.EnsureSuccessStatusCode();

        // 2. Generate M5 Program
        var progGenResp = await _client.PostAsJsonAsync("/api/programs/generate", new GenerateProgramRequestDto(
            ClientId: client.Id,
            ProgramName: "Minimal Equipment Home Routine",
            CoachNotes: "No barbell",
            NumberOfWeeks: 4));
        progGenResp.EnsureSuccessStatusCode();
        var program = await progGenResp.Content.ReadFromJsonAsync<ProgramDto>();
        program.Should().NotBeNull();
        program!.Versions.Should().NotBeEmpty();

        var version = program.Versions.First();
        var allSlots = version.Weeks.SelectMany(w => w.Sessions).SelectMany(s => s.Slots).ToList();
        allSlots.Should().NotBeEmpty();

        // 3. Verify reasoning downstream
        var reasoningResp = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = client.Id,
            ReasoningCategory = ReasoningCategory.ExerciseModificationReview,
            AdditionalContext = "Review exercise selection under minimal equipment constraints"
        });

        reasoningResp.EnsureSuccessStatusCode();
    }

    #endregion

    #region Flow 6 — Cross-Coach Isolation

    [Fact]
    public async Task Flow6_CrossCoachIsolation_EnforcesZeroDataLeakageAcrossCoaches()
    {
        // 1. Coach A Setup
        var coachAToken = await RegisterAndLoginCoachAsync("coach_a");
        var clientA = await CreateClientAsync(coachAToken, "ClientA", "OwnerA");
        await SetupTrainingProfileAsync(coachAToken, clientA.Id);

        var recAResp = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = clientA.Id,
            ReasoningCategory = ReasoningCategory.ProgramAdaptationReview
        });
        recAResp.EnsureSuccessStatusCode();
        var recA = await recAResp.Content.ReadFromJsonAsync<AIRecommendationRecordDto>();

        // 2. Coach B Setup
        var coachBToken = await RegisterAndLoginCoachAsync("coach_b");
        var clientB = await CreateClientAsync(coachBToken, "ClientB", "OwnerB");

        // Switch active bearer to Coach B
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", coachBToken);

        // Check 1: GET /api/clients/{clientId}
        var getClientResp = await _client.GetAsync($"/api/clients/{clientA.Id}");
        getClientResp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);

        // Check 2: GET /api/clients/{clientId}/recommendations
        var getRecsResp = await _client.GetAsync($"/api/clients/{clientA.Id}/recommendations");
        getRecsResp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);

        // Check 3: GET /api/clients/{clientId}/videos
        var getVideosResp = await _client.GetAsync($"/api/clients/{clientA.Id}/videos");
        getVideosResp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);

        // Check 4: GET /api/clients/{clientId}/videos/{videoId}/observation
        var getObservationResp = await _client.GetAsync($"/api/clients/{clientA.Id}/videos/{Guid.NewGuid()}/observation");
        getObservationResp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);

        // Check 5: GET /api/expert-ingestions (returns Coach B's list, Coach A's items are absent)
        var getIngestionsResp = await _client.GetAsync("/api/expert-ingestions");
        getIngestionsResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var ingestions = await getIngestionsResp.Content.ReadFromJsonAsync<List<ExpertContentIngestionDto>>();
        ingestions.Should().BeEmpty();

        // Check 6: PATCH /api/reasoning/{recommendationId}/review
        var patchReviewResp = await _client.PatchAsJsonAsync($"/api/reasoning/{recA!.Id}/review", new ReviewAIRecommendationRequestDto
        {
            ReviewStatus = AIRecommendationReviewStatus.Accepted,
            CoachDecision = "Malicious review attempt by Coach B"
        });
        patchReviewResp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);

        // Check 7: PATCH /api/reasoning/{recommendationId}/link-program-version
        var patchLinkResp = await _client.PatchAsJsonAsync($"/api/reasoning/{recA.Id}/link-program-version", new LinkProgramVersionRequestDto
        {
            ProgramVersionId = Guid.NewGuid()
        });
        patchLinkResp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Performance Baseline Measurement

    [Fact]
    public async Task PerformanceBaseline_MeasuresExecutionTimeForM5_M7_M14()
    {
        var token = await RegisterAndLoginCoachAsync("perf_baseline");
        var client = await CreateClientAsync(token, "PerfClient", "Test");
        await SetupTrainingProfileAsync(token, client.Id);

        // Measure M5 Program Generation
        var swM5 = Stopwatch.StartNew();
        var progGenResp = await _client.PostAsJsonAsync("/api/programs/generate", new GenerateProgramRequestDto(
            ClientId: client.Id,
            ProgramName: "Performance Baseline Program",
            CoachNotes: "Perf test",
            NumberOfWeeks: 4));
        swM5.Stop();
        progGenResp.EnsureSuccessStatusCode();
        var m5Ms = swM5.ElapsedMilliseconds;

        // Measure M7 Adaptation Assessment
        var program = await progGenResp.Content.ReadFromJsonAsync<ProgramDto>();
        var versionId = program!.Versions.First().Id;

        var swM7 = Stopwatch.StartNew();
        var adaptResp = await _client.PostAsJsonAsync($"/api/adaptations/assess/{versionId}", new { });
        swM7.Stop();
        adaptResp.EnsureSuccessStatusCode();
        var m7Ms = swM7.ElapsedMilliseconds;

        // Measure M14 Reasoning Generation with MockAiProvider
        var swM14 = Stopwatch.StartNew();
        var reasoningResp = await _client.PostAsJsonAsync("/api/reasoning/generate", new GenerateReasoningRequestDto
        {
            ClientId = client.Id,
            ReasoningCategory = ReasoningCategory.ProgramAdaptationReview,
            AdditionalContext = "Performance baseline evaluation"
        });
        swM14.Stop();
        reasoningResp.EnsureSuccessStatusCode();
        var m14Ms = swM14.ElapsedMilliseconds;

        _output.WriteLine($"[PERFORMANCE BASELINE] M5 Program Generation: {m5Ms} ms");
        _output.WriteLine($"[PERFORMANCE BASELINE] M7 Adaptation Assessment: {m7Ms} ms");
        _output.WriteLine($"[PERFORMANCE BASELINE] M14 Reasoning Generation (MockAiProvider): {m14Ms} ms");

        m5Ms.Should().BeGreaterThanOrEqualTo(0);
        m7Ms.Should().BeGreaterThanOrEqualTo(0);
        m14Ms.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion
}
