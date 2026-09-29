using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Memory.Dtos;
using AiCoachOs.Application.Videos.Dtos;
using AiCoachOs.Domain.Memory;
using AiCoachOs.Domain.Videos;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Videos;

public class VideoVisionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private static byte[]? _cachedTestVideoBytes;
    private static readonly object _lock = new();

    public VideoVisionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static byte[] GetOrCreateTestVideoBytes()
    {
        lock (_lock)
        {
            if (_cachedTestVideoBytes != null)
                return _cachedTestVideoBytes;

            var tempOutput = Path.Combine(Path.GetTempPath(), $"test_video_{Guid.NewGuid():N}.mp4");
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-y -v error -nostats -f lavfi -i testsrc=duration=3:size=320x240:rate=10 -c:v libx264 -pix_fmt yuv420p \"{tempOutput}\"",
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                process?.WaitForExit(5000);

                if (File.Exists(tempOutput))
                {
                    _cachedTestVideoBytes = File.ReadAllBytes(tempOutput);
                }
                else
                {
                    _cachedTestVideoBytes = new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D };
                }
            }
            catch
            {
                _cachedTestVideoBytes = new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D };
            }
            finally
            {
                if (File.Exists(tempOutput))
                {
                    try { File.Delete(tempOutput); } catch { }
                }
            }

            return _cachedTestVideoBytes;
        }
    }

    private async Task<string> RegisterAndLoginCoachAsync(string suffix)
    {
        var email = $"coach_m16_{suffix}_{Guid.NewGuid():N}@aicoach.com";
        var request = new RegisterCoachRequestDto("Coach M16", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<ClientDto> CreateClientForCoachAsync(string token, string firstName = "Hany", string lastName = "Ramses")
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new CreateClientRequestDto(firstName, lastName, $"{firstName.ToLower()}_{Guid.NewGuid():N}@test.com", null, null, null, null, null);
        var response = await _client.PostAsJsonAsync("/api/clients", request);
        response.EnsureSuccessStatusCode();
        var client = await response.Content.ReadFromJsonAsync<ClientDto>();
        return client!;
    }

    private async Task<ClientVideoDto> UploadTestVideoAsync(Guid clientId, string exerciseName = "Barbell Back Squat", string? notes = "Heavy double")
    {
        var videoBytes = GetOrCreateTestVideoBytes();
        var uploadReq = new UploadVideoRequestDto
        {
            FileBytes = videoBytes,
            MimeType = "video/mp4",
            ExerciseName = exerciseName,
            Notes = notes
        };

        var response = await _client.PostAsJsonAsync($"/api/clients/{clientId}/videos", uploadReq);
        response.EnsureSuccessStatusCode();
        var video = await response.Content.ReadFromJsonAsync<ClientVideoDto>();
        return video!;
    }

    [Fact]
    public async Task VideosEndpoints_Unauthenticated_Returns401Unauthorized()
    {
        var unauthedClient = _factory.CreateClient();
        var randomClientId = Guid.NewGuid();
        var randomVideoId = Guid.NewGuid();

        var res1 = await unauthedClient.GetAsync($"/api/clients/{randomClientId}/videos");
        res1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res2 = await unauthedClient.PostAsJsonAsync($"/api/clients/{randomClientId}/videos", new UploadVideoRequestDto());
        res2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res3 = await unauthedClient.GetAsync($"/api/clients/{randomClientId}/videos/{randomVideoId}/url");
        res3.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res4 = await unauthedClient.GetAsync($"/api/clients/{randomClientId}/videos/{randomVideoId}/frames");
        res4.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res5 = await unauthedClient.DeleteAsync($"/api/clients/{randomClientId}/videos/{randomVideoId}");
        res5.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res6 = await unauthedClient.PostAsync($"/api/clients/{randomClientId}/videos/{randomVideoId}/analyze", null);
        res6.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res7 = await unauthedClient.GetAsync($"/api/clients/{randomClientId}/videos/{randomVideoId}/analysis-status");
        res7.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res8 = await unauthedClient.GetAsync($"/api/clients/{randomClientId}/videos/{randomVideoId}/observation");
        res8.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res9 = await unauthedClient.GetAsync($"/api/clients/{randomClientId}/videos/{randomVideoId}/observations");
        res9.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadVideo_ValidVideo_NormalizesExtractsFramesAndReturns201()
    {
        var token = await RegisterAndLoginCoachAsync("upload_valid");
        var client = await CreateClientForCoachAsync(token, "Tarek", "Nabil");

        var video = await UploadTestVideoAsync(client.Id, "Conventional Deadlift", "Top set 200kg");

        video.Should().NotBeNull();
        video.ClientId.Should().Be(client.Id);
        video.ExerciseName.Should().Be("Conventional Deadlift");
        video.DurationSeconds.Should().BeInRange(2, 180);
        video.FrameCount.Should().BeInRange(1, 8);
        video.IsAnonymized.Should().BeFalse();
        video.CoachNotes.Should().Be("Top set 200kg");
        video.ObservationRecordId.Should().BeNull();

        // Verify listing endpoint
        var listRes = await _client.GetAsync($"/api/clients/{client.Id}/videos");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listRes.Content.ReadFromJsonAsync<List<ClientVideoSummaryDto>>();
        list.Should().Contain(v => v.Id == video.Id);
    }

    [Fact]
    public async Task GetSignedUrl_AndFrames_ReturnValidTokens_AndEnforceCoachIsolation()
    {
        var coachAToken = await RegisterAndLoginCoachAsync("signed_url_coach_a");
        var clientA = await CreateClientForCoachAsync(coachAToken, "Sherif", "Mansour");

        var videoA = await UploadTestVideoAsync(clientA.Id, "Barbell Bench Press");

        // Coach A gets video signed URL
        var urlRes = await _client.GetAsync($"/api/clients/{clientA.Id}/videos/{videoA.Id}/url");
        urlRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var signedMedia = await urlRes.Content.ReadFromJsonAsync<SignedMediaUrlDto>();
        signedMedia.Should().NotBeNull();
        signedMedia!.Url.Should().Contain("/api/photos/view?token=");

        // Coach A gets frame signed URLs
        var framesRes = await _client.GetAsync($"/api/clients/{clientA.Id}/videos/{videoA.Id}/frames");
        framesRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var frames = await framesRes.Content.ReadFromJsonAsync<List<VideoFrameDto>>();
        frames.Should().NotBeNull();
        frames!.Count.Should().Be(videoA.FrameCount);
        frames.Should().OnlyContain(f => f.Url.Contains("/api/photos/view?token="));

        // Coach B attempt access -> Unauthorized / Forbidden / 404
        var coachBToken = await RegisterAndLoginCoachAsync("signed_url_coach_b");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", coachBToken);

        var deniedUrlRes = await _client.GetAsync($"/api/clients/{clientA.Id}/videos/{videoA.Id}/url");
        deniedUrlRes.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);

        var deniedFramesRes = await _client.GetAsync($"/api/clients/{clientA.Id}/videos/{videoA.Id}/frames");
        deniedFramesRes.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task AnalyzeVideo_EnqueuesJob_CompletesAsync_AndPersistsObservationToClientMemory()
    {
        var token = await RegisterAndLoginCoachAsync("async_analysis");
        var client = await CreateClientForCoachAsync(token, "Mazen", "Ali");

        var video = await UploadTestVideoAsync(client.Id, "Overhead Press");

        // Act: Enqueue analysis
        var enqueueRes = await _client.PostAsync($"/api/clients/{client.Id}/videos/{video.Id}/analyze", null);
        enqueueRes.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var enqueueDto = await enqueueRes.Content.ReadFromJsonAsync<EnqueueVideoAnalysisResponseDto>();
        enqueueDto.Should().NotBeNull();
        enqueueDto!.JobId.Should().NotBeEmpty();

        // Poll job status until Completed or timeout (15s)
        VideoAnalysisJobStatusDto? jobStatus = null;
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(300);
            var statusRes = await _client.GetAsync($"/api/clients/{client.Id}/videos/{video.Id}/analysis-status");
            statusRes.StatusCode.Should().Be(HttpStatusCode.OK);
            jobStatus = await statusRes.Content.ReadFromJsonAsync<VideoAnalysisJobStatusDto>();
            if (jobStatus?.Status == "Completed" || jobStatus?.Status == "Failed")
            {
                break;
            }
        }

        jobStatus.Should().NotBeNull();
        jobStatus!.Status.Should().Be("Completed");
        jobStatus.ObservationRecordId.Should().NotBeNull();

        // Check single observation endpoint
        var obsRes = await _client.GetAsync($"/api/clients/{client.Id}/videos/{video.Id}/observation");
        obsRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var obs = await obsRes.Content.ReadFromJsonAsync<ExerciseTechniqueObservationResultDto>();
        obs.Should().NotBeNull();
        obs!.MovementExecutionNotes.Should().NotBeNullOrWhiteSpace();
        obs.LimitationsStatement.Should().NotBeNullOrWhiteSpace();
        obs.CoachActionRequired.Should().BeTrue();

        // Check Client Memory Record was created with M13 provenance
        var memRes = await _client.GetAsync($"/api/clients/{client.Id}/memory");
        memRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var memories = await memRes.Content.ReadFromJsonAsync<List<ClientMemoryRecordDto>>();
        var videoMem = memories!.FirstOrDefault(m => m.Id == jobStatus.ObservationRecordId);
        videoMem.Should().NotBeNull();
        videoMem!.MemoryCategory.Should().Be(MemoryCategory.ExerciseTechniqueObservation);
        videoMem.SourceType.Should().Be(MemorySourceType.SystemGenerated);
        videoMem.ConfidenceLevel.Should().Be(MemoryConfidenceLevel.Provisional);
        videoMem.RecordStatus.Should().Be(MemoryRecordStatus.Active);
        videoMem.SourceReference.Should().Be(video.Id.ToString());
    }

    [Fact]
    public async Task ObservationHistory_M16C1_PreservesMultipleRuns_OrderedNewestFirst()
    {
        var token = await RegisterAndLoginCoachAsync("m16_c1_history");
        var client = await CreateClientForCoachAsync(token, "Yasser", "Gamal");

        var video = await UploadTestVideoAsync(client.Id, "Front Squat");

        // First Analysis Run
        var run1Res = await _client.PostAsync($"/api/clients/{client.Id}/videos/{video.Id}/analyze", null);
        run1Res.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // Poll 1st run
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(200);
            var st = await _client.GetFromJsonAsync<VideoAnalysisJobStatusDto>($"/api/clients/{client.Id}/videos/{video.Id}/analysis-status");
            if (st?.Status == "Completed") break;
        }

        var obs1Res = await _client.GetAsync($"/api/clients/{client.Id}/videos/{video.Id}/observation");
        var obs1 = await obs1Res.Content.ReadFromJsonAsync<ExerciseTechniqueObservationResultDto>();

        // Second Analysis Run (Re-analysis)
        await Task.Delay(100);
        var run2Res = await _client.PostAsync($"/api/clients/{client.Id}/videos/{video.Id}/analyze", null);
        run2Res.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // Poll 2nd run
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(200);
            var st = await _client.GetFromJsonAsync<VideoAnalysisJobStatusDto>($"/api/clients/{client.Id}/videos/{video.Id}/analysis-status");
            if (st?.Status == "Completed") break;
        }

        var obs2Res = await _client.GetAsync($"/api/clients/{client.Id}/videos/{video.Id}/observation");
        var obs2 = await obs2Res.Content.ReadFromJsonAsync<ExerciseTechniqueObservationResultDto>();

        // Latest observation endpoint returns run 2
        obs2.Should().NotBeNull();
        obs2!.AnalyzedAtUtc.Should().BeOnOrAfter(obs1!.AnalyzedAtUtc);

        // Historical observations endpoint (M16-C1) returns both runs ordered newest first
        var historyRes = await _client.GetAsync($"/api/clients/{client.Id}/videos/{video.Id}/observations");
        historyRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await historyRes.Content.ReadFromJsonAsync<List<ExerciseTechniqueObservationResultDto>>();

        history.Should().NotBeNull();
        history!.Count.Should().BeGreaterThanOrEqualTo(2);
        history[0].AnalyzedAtUtc.Should().BeOnOrAfter(history[1].AnalyzedAtUtc);
    }

    [Fact]
    public async Task DeleteVideo_ExecutesAnonymizationLifecycle_AndPostAnonymizationAnalysisReturns409()
    {
        var token = await RegisterAndLoginCoachAsync("delete_anonymize");
        var client = await CreateClientForCoachAsync(token, "Ziad", "Mahmoud");

        var video = await UploadTestVideoAsync(client.Id, "Romanian Deadlift", "Testing hamstring flexibility");

        // Run analysis first
        await _client.PostAsync($"/api/clients/{client.Id}/videos/{video.Id}/analyze", null);
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(200);
            var st = await _client.GetFromJsonAsync<VideoAnalysisJobStatusDto>($"/api/clients/{client.Id}/videos/{video.Id}/analysis-status");
            if (st?.Status == "Completed") break;
        }

        // Act: Delete video
        var deleteRes = await _client.DeleteAsync($"/api/clients/{client.Id}/videos/{video.Id}");
        deleteRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Active listing excludes video
        var listRes = await _client.GetAsync($"/api/clients/{client.Id}/videos");
        var list = await listRes.Content.ReadFromJsonAsync<List<ClientVideoSummaryDto>>();
        list.Should().NotContain(v => v.Id == video.Id);

        // Re-analyzing anonymized video returns 409 Conflict
        var analyzeAgainRes = await _client.PostAsync($"/api/clients/{client.Id}/videos/{video.Id}/analyze", null);
        analyzeAgainRes.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Requesting signed URLs on anonymized video fails
        var urlRes = await _client.GetAsync($"/api/clients/{client.Id}/videos/{video.Id}/url");
        urlRes.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);

        var framesRes = await _client.GetAsync($"/api/clients/{client.Id}/videos/{video.Id}/frames");
        framesRes.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task AnonymizeClientMemories_PurgesVideosAndFramesStorage_AndMarksAnonymized()
    {
        var token = await RegisterAndLoginCoachAsync("client_anonymize_full");
        var client = await CreateClientForCoachAsync(token, "Waleed", "Hassan");

        var video = await UploadTestVideoAsync(client.Id, "Incline Dumbbell Press");

        // Run analysis
        await _client.PostAsync($"/api/clients/{client.Id}/videos/{video.Id}/analyze", null);
        VideoAnalysisJobStatusDto? jobStatus = null;
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(200);
            jobStatus = await _client.GetFromJsonAsync<VideoAnalysisJobStatusDto>($"/api/clients/{client.Id}/videos/{video.Id}/analysis-status");
            if (jobStatus?.Status == "Completed") break;
        }

        jobStatus.Should().NotBeNull();
        jobStatus!.ObservationRecordId.Should().NotBeNull();
        var obsRecordId = jobStatus.ObservationRecordId.Value;

        // Anonymize client
        var anonRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory/anonymize", new AnonymizeClientMemoryRequestDto
        {
            AnonymizationReason = "GDPR client erasure request"
        });
        anonRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Listing active videos returns empty
        var listRes = await _client.GetAsync($"/api/clients/{client.Id}/videos");
        var list = await listRes.Content.ReadFromJsonAsync<List<ClientVideoSummaryDto>>();
        list.Should().NotContain(v => v.Id == video.Id);

        // Memory record for client technique is anonymized
        var memRes = await _client.GetAsync($"/api/clients/{client.Id}/memory?includeAnonymized=true");
        var memories = await memRes.Content.ReadFromJsonAsync<List<ClientMemoryRecordDto>>();
        var videoMem = memories!.FirstOrDefault(m => m.Id == obsRecordId);
        videoMem.Should().NotBeNull();
        videoMem!.IsAnonymized.Should().BeTrue();
        videoMem.Content.Should().Be(ClientMemoryRecord.AnonymizedContentSentinel);
        videoMem.SourceReference.Should().BeNull();
    }
}
