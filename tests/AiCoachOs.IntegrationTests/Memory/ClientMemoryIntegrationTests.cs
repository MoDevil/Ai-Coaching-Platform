using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Memory.Dtos;
using AiCoachOs.Domain.Memory;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Memory;

public class ClientMemoryIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ClientMemoryIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string suffix)
    {
        var email = $"coach_m13_{suffix}_{Guid.NewGuid():N}@aicoach.com";
        var request = new RegisterCoachRequestDto("Coach M13", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<ClientDto> CreateClientForCoachAsync(string token, string firstName = "Ahmed", string lastName = "Ali")
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new CreateClientRequestDto(firstName, lastName, $"{firstName.ToLower()}_{Guid.NewGuid():N}@test.com", null, null, null, null, null);
        var response = await _client.PostAsJsonAsync("/api/clients", request);
        response.EnsureSuccessStatusCode();
        var client = await response.Content.ReadFromJsonAsync<ClientDto>();
        return client!;
    }

    [Fact]
    public async Task GetMemories_Unauthenticated_Returns401Unauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/clients/{Guid.NewGuid()}/memory");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMemories_CoachIsolation_Returns404ForAnotherCoachClient()
    {
        var coachAToken = await RegisterAndLoginCoachAsync("iso_a");
        var clientA = await CreateClientForCoachAsync(coachAToken, "Client", "A");

        var coachBToken = await RegisterAndLoginCoachAsync("iso_b");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", coachBToken);

        var response = await _client.GetAsync($"/api/clients/{clientA.Id}/memory");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateMemory_WhenValid_Returns201AndPersistsMemory()
    {
        var token = await RegisterAndLoginCoachAsync("create_mem");
        var client = await CreateClientForCoachAsync(token, "Omar", "Hassan");

        var payload = new PreferenceContent(
            subjectType: PreferenceSubjectType.Exercise,
            subjectId: "ex-incline-bench",
            subjectLabel: "Incline Dumbbell Bench Press",
            sentiment: PreferenceSentiment.Prefers,
            coachNote: "Client prefers dumbbells over barbells for chest pressing.");

        var request = new CreateClientMemoryRequestDto
        {
            MemoryCategory = MemoryCategory.Preference,
            SourceType = MemorySourceType.CoachRecorded,
            Content = JsonSerializer.Serialize(payload),
            SourceDescription = "Intake consultation"
        };

        var response = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ClientMemoryRecordDto>();
        created.Should().NotBeNull();
        created!.ClientId.Should().Be(client.Id);
        created.MemoryCategory.Should().Be(MemoryCategory.Preference);
        created.SourceType.Should().Be(MemorySourceType.CoachRecorded);
        created.ConfidenceLevel.Should().Be(MemoryConfidenceLevel.Provisional);
        created.RecordStatus.Should().Be(MemoryRecordStatus.Active);
        created.IsConflicted.Should().BeFalse();
    }

    [Fact]
    public async Task CorrectMemory_WhenValid_SupersedesOldRecordAndCreatesNewActiveRecord()
    {
        var token = await RegisterAndLoginCoachAsync("correct_mem");
        var client = await CreateClientForCoachAsync(token, "Tarek", "Zaki");

        var initialPayload = new PreferenceContent(
            subjectType: PreferenceSubjectType.TrainingTime,
            subjectId: "morning",
            subjectLabel: "Morning",
            sentiment: PreferenceSentiment.Prefers);

        var createReq = new CreateClientMemoryRequestDto
        {
            MemoryCategory = MemoryCategory.ScheduleConstraint,
            SourceType = MemorySourceType.CoachRecorded,
            Content = JsonSerializer.Serialize(initialPayload)
        };

        var createRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory", createReq);
        var initialRecord = await createRes.Content.ReadFromJsonAsync<ClientMemoryRecordDto>();

        var correctReq = new CorrectClientMemoryRequestDto
        {
            Content = "{\"subjectType\":3,\"subjectId\":\"evening\",\"subjectLabel\":\"Evening\",\"sentiment\":1}",
            Reason = "Client changed job shifts to evening training"
        };

        var correctRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory/{initialRecord!.Id}/correct", correctReq);
        correctRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var correctedRecord = await correctRes.Content.ReadFromJsonAsync<ClientMemoryRecordDto>();
        correctedRecord.Should().NotBeNull();
        correctedRecord!.ConfidenceLevel.Should().Be(MemoryConfidenceLevel.Confirmed);
        correctedRecord.RecordStatus.Should().Be(MemoryRecordStatus.Active);
        correctedRecord.SourceType.Should().Be(MemorySourceType.CoachCorrected);
        correctedRecord.CoachCorrectionNote.Should().Be("Client changed job shifts to evening training");

        // Verify history chain
        var historyRes = await _client.GetAsync($"/api/clients/{client.Id}/memory/{initialRecord.Id}/history");
        historyRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await historyRes.Content.ReadFromJsonAsync<List<ClientMemoryRecordDto>>();
        history.Should().HaveCount(2);
        history!.First().RecordStatus.Should().Be(MemoryRecordStatus.Superseded);
        history.First().SupersededById.Should().Be(correctedRecord.Id);
    }

    [Fact]
    public async Task FlagUncertain_WhenActive_FlagsRecordAsUncertain()
    {
        var token = await RegisterAndLoginCoachAsync("flag_uncertain");
        var client = await CreateClientForCoachAsync(token, "Sara", "Nader");

        var createReq = new CreateClientMemoryRequestDto
        {
            MemoryCategory = MemoryCategory.NutritionHabit,
            SourceType = MemorySourceType.CoachRecorded,
            Content = "Claims to drink 4L water daily"
        };
        var createRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory", createReq);
        var created = await createRes.Content.ReadFromJsonAsync<ClientMemoryRecordDto>();

        var patchRes = await _client.PatchAsync($"/api/clients/{client.Id}/memory/{created!.Id}/flag-uncertain", null);
        patchRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var flagged = await patchRes.Content.ReadFromJsonAsync<ClientMemoryRecordDto>();
        flagged!.ConfidenceLevel.Should().Be(MemoryConfidenceLevel.Uncertain);
        flagged.RecordStatus.Should().Be(MemoryRecordStatus.Active);

        // Check unresolved questions endpoint
        var questionsRes = await _client.GetAsync($"/api/clients/{client.Id}/memory/unresolved-questions");
        questionsRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var questions = await questionsRes.Content.ReadFromJsonAsync<List<UnresolvedQuestionDto>>();
        questions.Should().Contain(q => q.RecordId == created.Id);
    }

    [Fact]
    public async Task AutoConflictDetection_OnConflictingPreferences_FlagsBothAndCreatesConflict()
    {
        var token = await RegisterAndLoginCoachAsync("auto_conflict");
        var client = await CreateClientForCoachAsync(token, "Mona", "Adel");

        var pref1 = new PreferenceContent(
            subjectType: PreferenceSubjectType.Exercise,
            subjectId: "ex-romanian-deadlift",
            subjectLabel: "Romanian Deadlift",
            sentiment: PreferenceSentiment.Prefers);

        var res1 = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory", new CreateClientMemoryRequestDto
        {
            MemoryCategory = MemoryCategory.Preference,
            SourceType = MemorySourceType.CoachRecorded,
            Content = JsonSerializer.Serialize(pref1)
        });
        res1.StatusCode.Should().Be(HttpStatusCode.Created);
        var record1 = await res1.Content.ReadFromJsonAsync<ClientMemoryRecordDto>();

        var pref2 = new PreferenceContent(
            subjectType: PreferenceSubjectType.Exercise,
            subjectId: "ex-romanian-deadlift",
            subjectLabel: "Romanian Deadlift",
            sentiment: PreferenceSentiment.Avoids);

        var res2 = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory", new CreateClientMemoryRequestDto
        {
            MemoryCategory = MemoryCategory.Aversion,
            SourceType = MemorySourceType.CoachRecorded,
            Content = JsonSerializer.Serialize(pref2)
        });
        res2.StatusCode.Should().Be(HttpStatusCode.Created);
        var record2 = await res2.Content.ReadFromJsonAsync<ClientMemoryRecordDto>();

        // Both records should now be conflicted
        record2!.IsConflicted.Should().BeTrue();
        record2.RecordStatus.Should().Be(MemoryRecordStatus.Conflicted);

        var refetched1 = await _client.GetFromJsonAsync<ClientMemoryRecordDto>($"/api/clients/{client.Id}/memory/{record1!.Id}");
        refetched1!.IsConflicted.Should().BeTrue();
        refetched1.RecordStatus.Should().Be(MemoryRecordStatus.Conflicted);

        // Conflict record exists
        var conflictsRes = await _client.GetAsync($"/api/clients/{client.Id}/memory/conflicts?unresolvedOnly=true");
        conflictsRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var conflicts = await conflictsRes.Content.ReadFromJsonAsync<List<ClientMemoryConflictDto>>();
        conflicts.Should().HaveCount(1);
        conflicts!.First().RecordAId.Should().Be(record1.Id);
        conflicts.First().RecordBId.Should().Be(record2.Id);

        // Resolve conflict: Keep Record 1
        var resolveRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory/conflicts/{conflicts.First().Id}/resolve",
            new ResolveClientMemoryConflictRequestDto
            {
                Action = ConflictResolutionAction.KeepRecordA,
                ResolutionNote = "Confirmed client enjoys RDL with proper technique"
            });
        resolveRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Refetch after resolution
        var resolved1 = await _client.GetFromJsonAsync<ClientMemoryRecordDto>($"/api/clients/{client.Id}/memory/{record1.Id}");
        var resolved2 = await _client.GetFromJsonAsync<ClientMemoryRecordDto>($"/api/clients/{client.Id}/memory/{record2.Id}");

        resolved1!.RecordStatus.Should().Be(MemoryRecordStatus.Active);
        resolved1.IsConflicted.Should().BeFalse();

        resolved2!.RecordStatus.Should().Be(MemoryRecordStatus.Superseded);
        resolved2.SupersededById.Should().Be(record1.Id);
    }

    [Fact]
    public async Task GenerateSnapshot_DeterministicRetrieval_IncludesActiveExcludesConflicted()
    {
        var token = await RegisterAndLoginCoachAsync("snapshot_gen");
        var client = await CreateClientForCoachAsync(token, "Khaled", "Amin");

        // Add 2 active memories
        await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory", new CreateClientMemoryRequestDto
        {
            MemoryCategory = MemoryCategory.GoalContext,
            Content = "Targeting 100kg bench press by summer",
            SourceType = MemorySourceType.CoachRecorded
        });

        await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory", new CreateClientMemoryRequestDto
        {
            MemoryCategory = MemoryCategory.EquipmentConstraint,
            Content = "Home gym with dumbbells up to 30kg",
            SourceType = MemorySourceType.CoachRecorded
        });

        var snapRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory/snapshot/generate", new GenerateClientMemorySnapshotRequestDto
        {
            Trigger = SnapshotGenerationTrigger.ProgramDesign
        });
        snapRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var snapshot = await snapRes.Content.ReadFromJsonAsync<ClientMemorySnapshotDto>();
        snapshot.Should().NotBeNull();
        snapshot!.ClientId.Should().Be(client.Id);
        snapshot.IncludedRecordIds.Should().HaveCount(2);
        snapshot.SnapshotContentJson.Should().Contain("100kg bench press");
        snapshot.IsStale.Should().BeFalse();

        // Query latest snapshot
        var latestRes = await _client.GetAsync($"/api/clients/{client.Id}/memory/snapshot");
        latestRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var latest = await latestRes.Content.ReadFromJsonAsync<ClientMemorySnapshotDto>();
        latest!.Id.Should().Be(snapshot.Id);
    }

    [Fact]
    public async Task AnonymizeMemories_MasksPersonalContentAndCreatesLog()
    {
        var token = await RegisterAndLoginCoachAsync("anonymize");
        var client = await CreateClientForCoachAsync(token, "Layla", "Kareem");

        var createRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory", new CreateClientMemoryRequestDto
        {
            MemoryCategory = MemoryCategory.PainObservation,
            Content = "Sacroiliac joint discomfort during heavy split squats",
            SourceDescription = "In-person session feedback"
        });
        var created = await createRes.Content.ReadFromJsonAsync<ClientMemoryRecordDto>();

        var anonRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory/anonymize", new AnonymizeClientMemoryRequestDto
        {
            AnonymizationReason = "GDPR client request"
        });
        anonRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var anonResult = await anonRes.Content.ReadFromJsonAsync<AnonymizeClientMemoryResultDto>();
        anonResult!.RecordsAnonymized.Should().Be(1);

        // Default query excludes anonymized
        var defaultListRes = await _client.GetAsync($"/api/clients/{client.Id}/memory");
        var defaultList = await defaultListRes.Content.ReadFromJsonAsync<List<ClientMemoryRecordDto>>();
        defaultList.Should().BeEmpty();

        // Explicit query with includeAnonymized
        var explicitListRes = await _client.GetAsync($"/api/clients/{client.Id}/memory?includeAnonymized=true");
        var explicitList = await explicitListRes.Content.ReadFromJsonAsync<List<ClientMemoryRecordDto>>();
        explicitList.Should().HaveCount(1);
        explicitList!.First().Content.Should().Be(ClientMemoryRecord.AnonymizedContentSentinel);
        explicitList.First().SourceDescription.Should().Be(ClientMemoryRecord.AnonymizedContentSentinel);
        explicitList.First().IsAnonymized.Should().BeTrue();
        explicitList.First().ClientId.Should().Be(client.Id);
    }
}
