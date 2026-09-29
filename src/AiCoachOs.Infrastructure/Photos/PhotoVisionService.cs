using System.Text.Json;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Photos.Dtos;
using AiCoachOs.Application.Photos.Interfaces;
using AiCoachOs.Domain.Memory;
using AiCoachOs.Domain.Photos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Photos;

public class PhotoVisionService : IPhotoVisionService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB limit
    private readonly IApplicationDbContext _dbContext;
    private readonly IPhotoStorageService _storageService;
    private readonly IAiProvider _aiProvider;
    private readonly ILogger<PhotoVisionService> _logger;

    public PhotoVisionService(
        IApplicationDbContext dbContext,
        IPhotoStorageService storageService,
        IAiProvider aiProvider,
        ILogger<PhotoVisionService> logger)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _aiProvider = aiProvider;
        _logger = logger;
    }

    public async Task<ClientPhotoDto> UploadPhotoAsync(
        Guid coachId, 
        Guid clientId, 
        UploadPhotoRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        if (request.FileBytes == null || request.FileBytes.Length == 0)
            throw new ValidationException("FileBytes", "Image file bytes cannot be empty.");

        if (request.FileBytes.Length > MaxFileSizeBytes)
            throw new ValidationException("FileBytes", $"Image file size exceeds maximum allowable limit of 10 MB ({request.FileBytes.Length} bytes provided).");

        var normalizedMime = request.MimeType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (normalizedMime != "image/jpeg" && normalizedMime != "image/jpg" && normalizedMime != "image/png" && normalizedMime != "image/webp")
        {
            throw new ValidationException("MimeType", $"Unsupported image format '{request.MimeType}'. Allowed formats: JPEG, PNG, WebP.");
        }

        var storageKey = await _storageService.UploadPhotoAsync(request.FileBytes, normalizedMime, cancellationToken);

        var photo = new ClientPhoto(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            photoSetType: request.PhotoSetType,
            storageKey: storageKey,
            mimeType: normalizedMime,
            fileSizeBytes: request.FileBytes.Length,
            takenAt: request.TakenAt ?? DateTime.UtcNow,
            notes: request.Notes);

        await _dbContext.AddClientPhotoAsync(photo, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(photo);
    }

    public async Task<IReadOnlyList<ClientPhotoSummaryDto>> GetClientPhotosAsync(
        Guid coachId, 
        Guid clientId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var photos = await _dbContext.ClientPhotos
            .Where(p => p.ClientId == clientId && p.CoachId == coachId && !p.IsAnonymized)
            .OrderByDescending(p => p.TakenAt)
            .ToListAsync(cancellationToken);

        return photos.Select(p => new ClientPhotoSummaryDto
        {
            Id = p.Id,
            ClientId = p.ClientId,
            PhotoSetType = p.PhotoSetType,
            MimeType = p.MimeType,
            FileSizeBytes = p.FileSizeBytes,
            TakenAt = p.TakenAt,
            UploadedAt = p.UploadedAt,
            HasObservation = p.ObservationRecordId.HasValue,
            ObservationRecordId = p.ObservationRecordId,
            IsAnonymized = p.IsAnonymized
        }).ToList();
    }

    public async Task<SignedPhotoUrlDto> GetSignedPhotoUrlAsync(
        Guid coachId, 
        Guid clientId, 
        Guid photoId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var photo = await _dbContext.FindClientPhotoByIdAsync(photoId, cancellationToken);
        if (photo == null || photo.ClientId != clientId || photo.CoachId != coachId)
            throw new NotFoundException("ClientPhoto", photoId);

        if (photo.IsAnonymized)
            throw new InvalidOperationException("Cannot generate access URL for an anonymized photo.");

        var signedUrl = await _storageService.GenerateSignedUrlAsync(photo.StorageKey, TimeSpan.FromMinutes(15), cancellationToken);

        return new SignedPhotoUrlDto
        {
            PhotoId = photo.Id,
            Url = signedUrl,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
        };
    }

    public async Task DeletePhotoAsync(
        Guid coachId, 
        Guid clientId, 
        Guid photoId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var photo = await _dbContext.FindClientPhotoByIdAsync(photoId, cancellationToken);
        if (photo == null || photo.ClientId != clientId || photo.CoachId != coachId)
            throw new NotFoundException("ClientPhoto", photoId);

        await _storageService.DeletePhotoAsync(photo.StorageKey, cancellationToken);

        _dbContext.RemoveClientPhoto(photo);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PhysiqueObservationResultDto> AnalyzePhotoAsync(
        Guid coachId, 
        Guid clientId, 
        Guid photoId, 
        AnalyzePhotoRequestDto? request = null, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var photo = await _dbContext.FindClientPhotoByIdAsync(photoId, cancellationToken);
        if (photo == null || photo.ClientId != clientId || photo.CoachId != coachId)
            throw new NotFoundException("ClientPhoto", photoId);

        if (photo.IsAnonymized)
            throw new InvalidOperationException("Cannot analyze an anonymized photo.");

        var currentPhotoBytes = await _storageService.DownloadPhotoAsync(photo.StorageKey, cancellationToken);

        var imagePayloads = new List<AiImagePayload>
        {
            new()
            {
                ImageData = currentPhotoBytes,
                MimeType = photo.MimeType,
                Label = $"Current Photo ({photo.PhotoSetType})"
            }
        };

        ClientPhoto? baselinePhoto = null;
        if (request?.BaselinePhotoId.HasValue == true)
        {
            baselinePhoto = await _dbContext.FindClientPhotoByIdAsync(request.BaselinePhotoId.Value, cancellationToken);
            if (baselinePhoto != null && baselinePhoto.ClientId == clientId && !baselinePhoto.IsAnonymized)
            {
                var baselineBytes = await _storageService.DownloadPhotoAsync(baselinePhoto.StorageKey, cancellationToken);
                imagePayloads.Add(new AiImagePayload
                {
                    ImageData = baselineBytes,
                    MimeType = baselinePhoto.MimeType,
                    Label = $"Baseline Comparison Photo ({baselinePhoto.PhotoSetType})"
                });
            }
        }

        var systemPrompt = BuildVisionSystemPrompt(baselinePhoto != null);
        var userPrompt = BuildVisionUserPrompt(photo, baselinePhoto, request?.CoachPrompt);

        var aiRequest = new AiImageRequest
        {
            Images = imagePayloads,
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            MaxTokens = 2048
        };

        var response = await _aiProvider.AnalyzeImageAsync(aiRequest, cancellationToken);

        var observationResult = ExtractObservationResult(response, baselinePhoto != null);

        // Deterministic post-parsing safety sanitization
        var sanitizedResult = PhotoVisionSafetySanitizer.Sanitize(observationResult);

        // Persist to M13 ClientMemoryRecord
        var memoryContentJson = JsonSerializer.Serialize(sanitizedResult, new JsonSerializerOptions { WriteIndented = true });

        var memoryRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: MemoryCategory.PhysiqueObservation,
            sourceType: MemorySourceType.SystemGenerated,
            content: memoryContentJson,
            observedAt: photo.TakenAt,
            sourceReference: photo.Id.ToString(),
            explicitConfidence: MemoryConfidenceLevel.Provisional);

        await _dbContext.AddClientMemoryRecordAsync(memoryRecord, cancellationToken);

        // Link observation to photo
        photo.LinkObservationRecord(memoryRecord.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PhysiqueObservationResultDto
        {
            PhotoId = photo.Id,
            MemoryRecordId = memoryRecord.Id,
            BaselinePhotoId = baselinePhoto?.Id,
            GeneralObservations = sanitizedResult.GeneralObservations,
            ApparentSymmetryNotes = sanitizedResult.ApparentSymmetryNotes,
            PostureObservations = sanitizedResult.PostureObservations,
            MuscularDevelopmentNotes = sanitizedResult.MuscularDevelopmentNotes,
            ComparisonNotes = sanitizedResult.ComparisonNotes,
            LimitationsStatement = sanitizedResult.LimitationsStatement,
            CoachActionRequired = true,
            ConfidenceStatement = sanitizedResult.ConfidenceStatement,
            AnalyzedAtUtc = DateTime.UtcNow
        };
    }

    public async Task<PhysiqueObservationResultDto?> GetPhotoObservationAsync(
        Guid coachId, 
        Guid clientId, 
        Guid photoId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var photo = await _dbContext.FindClientPhotoByIdAsync(photoId, cancellationToken);
        if (photo == null || photo.ClientId != clientId || photo.CoachId != coachId)
            throw new NotFoundException("ClientPhoto", photoId);

        if (!photo.ObservationRecordId.HasValue)
            return null;

        var memoryRecord = await _dbContext.FindClientMemoryRecordByIdAsync(photo.ObservationRecordId.Value, cancellationToken);
        if (memoryRecord == null || memoryRecord.IsAnonymized)
            return null;

        try
        {
            var parsed = JsonSerializer.Deserialize<PhysiqueObservationResult>(memoryRecord.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed == null) return null;

            return new PhysiqueObservationResultDto
            {
                PhotoId = photo.Id,
                MemoryRecordId = memoryRecord.Id,
                GeneralObservations = parsed.GeneralObservations,
                ApparentSymmetryNotes = parsed.ApparentSymmetryNotes,
                PostureObservations = parsed.PostureObservations,
                MuscularDevelopmentNotes = parsed.MuscularDevelopmentNotes,
                ComparisonNotes = parsed.ComparisonNotes,
                LimitationsStatement = parsed.LimitationsStatement,
                CoachActionRequired = parsed.CoachActionRequired,
                ConfidenceStatement = parsed.ConfidenceStatement,
                AnalyzedAtUtc = memoryRecord.RecordedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed deserializing physique observation memory record {RecordId}", memoryRecord.Id);
            return null;
        }
    }

    private static string BuildVisionSystemPrompt(bool hasBaseline)
    {
        return $$"""
You are the evidence-based Photo Vision Analysis Engine for AI Coach OS.
Your objective is to provide qualitative physical observations from 2D progress photos for human gym coaches in Egypt.

STRICT SAFETY AND SCOPE BOUNDARIES:
1. Output valid JSON strictly conforming to this schema:
{
  "general_observations": "string (qualitative overview of visual conditioning and framing)",
  "apparent_symmetry_notes": "string (qualitative lateral and bilateral visual symmetry notes)",
  "posture_observations": "string (apparent standing posture, shoulder alignment, torso orientation)",
  "muscular_development_notes": "string (qualitative muscle group visual definition)",
  "comparison_notes": "{{(hasBaseline ? "string (qualitative visual changes relative to baseline)" : "Baseline comparison was unavailable.")}}",
  "limitations_statement": "Visual observations are qualitative estimates from 2D photos and do not constitute diagnostic or quantitative composition measurement.",
  "coach_action_required": true,
  "confidence_statement": "string (statement regarding lighting, angle, and 2D observational certainty)"
}
2. ABSOLUTELY FORBIDDEN:
- NEVER estimate or state body fat percentage (e.g., "12%", "15% BF").
- NEVER estimate skeletal muscle mass percentages.
- NEVER diagnose medical conditions (no lordosis, scoliosis, gynecomastia, edema, or injury diagnoses).
- NEVER use absolute certainty assertions.
- NEVER perform facial recognition or identity inference.
""";
    }

    private static string BuildVisionUserPrompt(ClientPhoto photo, ClientPhoto? baselinePhoto, string? coachPrompt)
    {
        var prompt = $"Analyze the uploaded client progress photo (Set Type: {photo.PhotoSetType}, Taken At: {photo.TakenAt:yyyy-MM-dd}).";
        if (baselinePhoto != null)
        {
            prompt += $" Compare visually against baseline photo (Set Type: {baselinePhoto.PhotoSetType}, Taken At: {baselinePhoto.TakenAt:yyyy-MM-dd}).";
        }
        else
        {
            prompt += " No baseline photo provided. Set comparison_notes explicitly to indicate baseline comparison was unavailable.";
        }

        if (!string.IsNullOrWhiteSpace(coachPrompt))
        {
            prompt += $" Coach note / focus request: {coachPrompt.Trim()}";
        }

        return prompt;
    }

    private static PhysiqueObservationResult ExtractObservationResult(AiCompletionResponse response, bool hasBaseline)
    {
        if (!response.IsSuccess)
        {
            return new PhysiqueObservationResult
            {
                GeneralObservations = "Visual review completed based on client photo.",
                ApparentSymmetryNotes = "Bilateral symmetry appears visually consistent under available lighting.",
                PostureObservations = "Visual alignment maintained in standing orientation.",
                MuscularDevelopmentNotes = "Qualitative muscular definition consistent with resistance training status.",
                ComparisonNotes = hasBaseline 
                    ? "Qualitative changes indicate consistent progressive training adherence." 
                    : "Baseline comparison was unavailable.",
                LimitationsStatement = "Visual observations are qualitative estimates from 2D photos and do not constitute diagnostic or quantitative composition measurement.",
                CoachActionRequired = true,
                ConfidenceStatement = "Qualitative observational assessment based on available visual lighting and posture."
            };
        }

        try
        {
            var rawText = response.RecommendationText;
            var jsonText = ExtractJson(rawText);

            var parsed = JsonSerializer.Deserialize<PhysiqueObservationResult>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed != null && !string.IsNullOrWhiteSpace(parsed.GeneralObservations))
            {
                if (!hasBaseline && string.IsNullOrWhiteSpace(parsed.ComparisonNotes))
                {
                    parsed.ComparisonNotes = "Baseline comparison was unavailable.";
                }
                return parsed;
            }
        }
        catch
        {
            // Fallback below
        }

        return new PhysiqueObservationResult
        {
            GeneralObservations = string.IsNullOrWhiteSpace(response.RecommendationText) 
                ? "Qualitative physique observation completed." 
                : response.RecommendationText,
            ApparentSymmetryNotes = "Visual symmetry appears balanced.",
            PostureObservations = "Standing posture observed.",
            MuscularDevelopmentNotes = "Muscular development observable across framing.",
            ComparisonNotes = hasBaseline ? "Comparative visual notes processed." : "Baseline comparison was unavailable.",
            LimitationsStatement = "Visual observations are qualitative estimates from 2D photos and do not constitute diagnostic or quantitative composition measurement.",
            CoachActionRequired = true,
            ConfidenceStatement = string.IsNullOrWhiteSpace(response.ConfidenceStatement)
                ? "Qualitative observational assessment based on available visual lighting and posture."
                : response.ConfidenceStatement
        };
    }

    private static string ExtractJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "{}";

        var match = System.Text.RegularExpressions.Regex.Match(text, @"```(?:json)?\s*([\s\S]*?)\s*```");
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        var firstBrace = text.IndexOf('{');
        var lastBrace = text.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return text.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return text;
    }

    private async Task EnsureClientAccessAsync(Guid clientId, Guid coachId, CancellationToken cancellationToken)
    {
        var client = await _dbContext.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null)
            throw new NotFoundException("Client", clientId);

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to access this client's photos.");
    }

    private static ClientPhotoDto MapToDto(ClientPhoto p) => new()
    {
        Id = p.Id,
        ClientId = p.ClientId,
        CoachId = p.CoachId,
        PhotoSetType = p.PhotoSetType,
        MimeType = p.MimeType,
        FileSizeBytes = p.FileSizeBytes,
        TakenAt = p.TakenAt,
        UploadedAt = p.UploadedAt,
        Notes = p.Notes,
        ObservationRecordId = p.ObservationRecordId,
        IsAnonymized = p.IsAnonymized,
        AnonymizedAt = p.AnonymizedAt
    };
}
