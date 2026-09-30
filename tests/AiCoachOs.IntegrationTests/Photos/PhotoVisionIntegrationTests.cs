using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Memory.Dtos;
using AiCoachOs.Application.Photos.Dtos;
using AiCoachOs.Domain.Memory;
using AiCoachOs.Domain.Photos;
using AiCoachOs.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.IntegrationTests.Photos;

public class PhotoVisionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PhotoVisionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginCoachAsync(string suffix)
    {
        var email = $"coach_m15_{suffix}_{Guid.NewGuid():N}@aicoach.com";
        var request = new RegisterCoachRequestDto("Coach M15", email, "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return content!.Token;
    }

    private async Task<ClientDto> CreateClientForCoachAsync(string token, string firstName = "Kareem", string lastName = "Fahmy")
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new CreateClientRequestDto(firstName, lastName, $"{firstName.ToLower()}_{Guid.NewGuid():N}@test.com", null, null, null, null, null);
        var response = await _client.PostAsJsonAsync("/api/clients", request);
        response.EnsureSuccessStatusCode();
        var client = await response.Content.ReadFromJsonAsync<ClientDto>();
        return client!;
    }

    private static byte[] CreateSampleJpegBytes()
    {
        // Minimal valid JPEG stream with SOI (FF D8) and EOI (FF D9)
        return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x60, 0x00, 0x60, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43, 0x00, 0xFF, 0xD9 };
    }

    [Fact]
    public async Task PhotosEndpoints_Unauthenticated_Returns401Unauthorized()
    {
        var unauthedClient = _factory.CreateClient();
        var randomClientId = Guid.NewGuid();
        var randomPhotoId = Guid.NewGuid();

        var res1 = await unauthedClient.GetAsync($"/api/clients/{randomClientId}/photos");
        res1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res2 = await unauthedClient.PostAsJsonAsync($"/api/clients/{randomClientId}/photos", new UploadPhotoRequestDto());
        res2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res3 = await unauthedClient.GetAsync($"/api/clients/{randomClientId}/photos/{randomPhotoId}/url");
        res3.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res4 = await unauthedClient.DeleteAsync($"/api/clients/{randomClientId}/photos/{randomPhotoId}");
        res4.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res5 = await unauthedClient.PostAsJsonAsync($"/api/clients/{randomClientId}/photos/{randomPhotoId}/analyze", new AnalyzePhotoRequestDto());
        res5.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var res6 = await unauthedClient.GetAsync($"/api/clients/{randomClientId}/photos/{randomPhotoId}/observation");
        res6.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadPhoto_Returns201AndStoresSanitizedRecord()
    {
        var token = await RegisterAndLoginCoachAsync("upload_test");
        var client = await CreateClientForCoachAsync(token, "Ahmed", "Zaki");

        var uploadReq = new UploadPhotoRequestDto
        {
            FileBytes = CreateSampleJpegBytes(),
            MimeType = "image/jpeg",
            PhotoSetType = PhotoSetType.FrontRelaxed,
            Notes = "Baseline physique check"
        };

        var response = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/photos", uploadReq);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var photo = await response.Content.ReadFromJsonAsync<ClientPhotoDto>();
        photo.Should().NotBeNull();
        photo!.ClientId.Should().Be(client.Id);
        photo.PhotoSetType.Should().Be(PhotoSetType.FrontRelaxed);
        photo.MimeType.Should().Be("image/jpeg");
        photo.Notes.Should().Be("Baseline physique check");
        photo.IsAnonymized.Should().BeFalse();
        photo.ObservationRecordId.Should().BeNull();

        // Verify photo list contains the record
        var listResponse = await _client.GetAsync($"/api/clients/{client.Id}/photos");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<List<ClientPhotoSummaryDto>>();
        list.Should().Contain(p => p.Id == photo.Id);
    }

    [Fact]
    public async Task GetSignedUrl_ReturnsValidSignedToken_AndEnforcesCoachIsolation()
    {
        var coachAToken = await RegisterAndLoginCoachAsync("signed_url_a");
        var clientA = await CreateClientForCoachAsync(coachAToken, "Sami", "Youssef");

        var uploadReq = new UploadPhotoRequestDto
        {
            FileBytes = CreateSampleJpegBytes(),
            MimeType = "image/jpeg",
            PhotoSetType = PhotoSetType.SideRelaxed
        };

        var uploadResponse = await _client.PostAsJsonAsync($"/api/clients/{clientA.Id}/photos", uploadReq);
        var photoA = await uploadResponse.Content.ReadFromJsonAsync<ClientPhotoDto>();

        // Coach A gets signed URL -> 200 OK
        var urlResponse = await _client.GetAsync($"/api/clients/{clientA.Id}/photos/{photoA!.Id}/url");
        urlResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var signedUrl = await urlResponse.Content.ReadFromJsonAsync<SignedPhotoUrlDto>();
        signedUrl.Should().NotBeNull();
        signedUrl!.Url.Should().Contain("/api/photos/view?token=");

        // Coach B attempts to get signed URL for Client A's photo -> Unauthorized / Forbidden / 404
        var coachBToken = await RegisterAndLoginCoachAsync("signed_url_b");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", coachBToken);

        var unauthorizedResponse = await _client.GetAsync($"/api/clients/{clientA.Id}/photos/{photoA.Id}/url");
        unauthorizedResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task AnalyzePhoto_ExecutesVisionObservation_CreatesMemoryRecordAndLinksPhoto()
    {
        var token = await RegisterAndLoginCoachAsync("analyze_test");
        var client = await CreateClientForCoachAsync(token, "Omar", "Lotfy");

        var uploadReq = new UploadPhotoRequestDto
        {
            FileBytes = CreateSampleJpegBytes(),
            MimeType = "image/jpeg",
            PhotoSetType = PhotoSetType.FrontRelaxed
        };

        var uploadResponse = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/photos", uploadReq);
        var photo = await uploadResponse.Content.ReadFromJsonAsync<ClientPhotoDto>();

        // Act: Trigger vision analysis
        var analyzeResponse = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/photos/{photo!.Id}/analyze", new AnalyzePhotoRequestDto
        {
            CoachPrompt = "Check symmetry in shoulder alignment"
        });

        analyzeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var observation = await analyzeResponse.Content.ReadFromJsonAsync<PhysiqueObservationResultDto>();

        observation.Should().NotBeNull();
        observation!.PhotoId.Should().Be(photo.Id);
        observation.MemoryRecordId.Should().NotBeEmpty();
        observation.GeneralObservations.Should().NotBeNullOrWhiteSpace();
        observation.ApparentSymmetryNotes.Should().NotBeNullOrWhiteSpace();
        observation.LimitationsStatement.Should().NotBeNullOrWhiteSpace();
        observation.CoachActionRequired.Should().BeTrue();
        observation.ComparisonNotes.Should().NotBeNullOrWhiteSpace();
        observation.ConfidenceStatement.Should().NotBeNullOrWhiteSpace();

        // Verify observation endpoint returns same data
        var getObsResponse = await _client.GetAsync($"/api/clients/{client.Id}/photos/{photo.Id}/observation");
        getObsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedObs = await getObsResponse.Content.ReadFromJsonAsync<PhysiqueObservationResultDto>();
        fetchedObs.Should().NotBeNull();
        fetchedObs!.MemoryRecordId.Should().Be(observation.MemoryRecordId);
    }

    [Fact]
    public async Task AnalyzePhoto_Reanalysis_CreatesNewMemoryRecordAndPreservesPreviousRecord()
    {
        var token = await RegisterAndLoginCoachAsync("reanalysis_test");
        var client = await CreateClientForCoachAsync(token, "Hassan", "Kamel");

        var uploadReq = new UploadPhotoRequestDto
        {
            FileBytes = CreateSampleJpegBytes(),
            MimeType = "image/jpeg",
            PhotoSetType = PhotoSetType.FrontFlexed
        };
        var uploadRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/photos", uploadReq);
        var photo = await uploadRes.Content.ReadFromJsonAsync<ClientPhotoDto>();

        // First analysis
        var firstAnalyzeRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/photos/{photo!.Id}/analyze", new AnalyzePhotoRequestDto());
        firstAnalyzeRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstObs = await firstAnalyzeRes.Content.ReadFromJsonAsync<PhysiqueObservationResultDto>();

        // Second analysis (Re-analysis)
        var secondAnalyzeRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/photos/{photo.Id}/analyze", new AnalyzePhotoRequestDto
        {
            CoachPrompt = "Re-evaluating under adjusted lighting"
        });
        secondAnalyzeRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondObs = await secondAnalyzeRes.Content.ReadFromJsonAsync<PhysiqueObservationResultDto>();

        // Assert new memory record ID was created
        secondObs!.MemoryRecordId.Should().NotBe(firstObs!.MemoryRecordId);

        // Assert photo ObservationRecordId points to the newest observation
        var getObsRes = await _client.GetAsync($"/api/clients/{client.Id}/photos/{photo.Id}/observation");
        var currentObs = await getObsRes.Content.ReadFromJsonAsync<PhysiqueObservationResultDto>();
        currentObs!.MemoryRecordId.Should().Be(secondObs.MemoryRecordId);

        // Assert all memories for client include both records
        var memoriesRes = await _client.GetAsync($"/api/clients/{client.Id}/memory");
        memoriesRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var memoryList = await memoriesRes.Content.ReadFromJsonAsync<List<ClientMemoryRecordDto>>();
        memoryList.Should().Contain(m => m.Id == firstObs.MemoryRecordId);
        memoryList.Should().Contain(m => m.Id == secondObs.MemoryRecordId);
    }

    [Fact]
    public async Task AnalyzePhoto_WithBaseline_ExecutesComparativeAnalysis()
    {
        var token = await RegisterAndLoginCoachAsync("baseline_test");
        var client = await CreateClientForCoachAsync(token, "Mostafa", "Gad");

        // Upload baseline photo (Day 0)
        var baselineReq = new UploadPhotoRequestDto
        {
            FileBytes = CreateSampleJpegBytes(),
            MimeType = "image/jpeg",
            PhotoSetType = PhotoSetType.FrontRelaxed,
            Notes = "Day 1 baseline"
        };
        var baselineRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/photos", baselineReq);
        var baselinePhoto = await baselineRes.Content.ReadFromJsonAsync<ClientPhotoDto>();

        // Upload current photo (Day 30)
        var currentReq = new UploadPhotoRequestDto
        {
            FileBytes = CreateSampleJpegBytes(),
            MimeType = "image/jpeg",
            PhotoSetType = PhotoSetType.FrontRelaxed,
            Notes = "Day 30 follow-up"
        };
        var currentRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/photos", currentReq);
        var currentPhoto = await currentRes.Content.ReadFromJsonAsync<ClientPhotoDto>();

        // Act: Analyze current photo with baseline comparison
        var analyzeRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/photos/{currentPhoto!.Id}/analyze", new AnalyzePhotoRequestDto
        {
            BaselinePhotoId = baselinePhoto!.Id,
            CoachPrompt = "Compare lat development"
        });

        analyzeRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var obs = await analyzeRes.Content.ReadFromJsonAsync<PhysiqueObservationResultDto>();

        obs.Should().NotBeNull();
        obs!.BaselinePhotoId.Should().Be(baselinePhoto.Id);
        obs.ComparisonNotes.Should().NotContain("unavailable");
        obs.CoachActionRequired.Should().BeTrue();
    }

    [Fact]
    public async Task AnonymizeClientMemories_DeletesPhotoStorageFileAndAnonymizesObservationMemory()
    {
        var token = await RegisterAndLoginCoachAsync("anon_photo");
        var client = await CreateClientForCoachAsync(token, "Nader", "Salama");

        var uploadReq = new UploadPhotoRequestDto
        {
            FileBytes = CreateSampleJpegBytes(),
            MimeType = "image/jpeg",
            PhotoSetType = PhotoSetType.BackRelaxed,
            Notes = "Confidential back photo"
        };
        var uploadRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/photos", uploadReq);
        var photo = await uploadRes.Content.ReadFromJsonAsync<ClientPhotoDto>();

        // Analyze photo to generate linked ClientMemoryRecord
        var analyzeRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/photos/{photo!.Id}/analyze", new AnalyzePhotoRequestDto());
        var obs = await analyzeRes.Content.ReadFromJsonAsync<PhysiqueObservationResultDto>();

        // Act: Anonymize client
        var anonRes = await _client.PostAsJsonAsync($"/api/clients/{client.Id}/memory/anonymize", new AnonymizeClientMemoryRequestDto
        {
            AnonymizationReason = "Client GDPR deletion request"
        });
        anonRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Active photo list must now exclude the anonymized photo
        var listRes = await _client.GetAsync($"/api/clients/{client.Id}/photos");
        var activePhotos = await listRes.Content.ReadFromJsonAsync<List<ClientPhotoSummaryDto>>();
        activePhotos.Should().NotContain(p => p.Id == photo!.Id);

        // Attempting to get signed URL for anonymized photo fails
        var urlRes = await _client.GetAsync($"/api/clients/{client.Id}/photos/{photo!.Id}/url");
        urlRes.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);

        // Linked observation memory record must be anonymized
        var memRes = await _client.GetAsync($"/api/clients/{client.Id}/memory?includeAnonymized=true");
        var memories = await memRes.Content.ReadFromJsonAsync<List<ClientMemoryRecordDto>>();
        var linkedMem = memories!.FirstOrDefault(m => m.Id == obs!.MemoryRecordId);
        linkedMem.Should().NotBeNull();
        linkedMem!.IsAnonymized.Should().BeTrue();
        linkedMem.Content.Should().Be(ClientMemoryRecord.AnonymizedContentSentinel);
    }
}
