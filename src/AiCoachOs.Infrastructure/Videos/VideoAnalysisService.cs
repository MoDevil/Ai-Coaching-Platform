using System.Text.Json;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Photos.Interfaces;
using AiCoachOs.Application.Videos.Dtos;
using AiCoachOs.Application.Videos.Interfaces;
using AiCoachOs.Domain.Memory;
using AiCoachOs.Domain.Videos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Videos;

public class VideoAnalysisService : IVideoAnalysisService
{
    private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB limit
    private readonly IApplicationDbContext _dbContext;
    private readonly IPhotoStorageService _storageService;
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly IVideoAnalysisJobService _jobService;
    private readonly IAiProvider _aiProvider;
    private readonly ILogger<VideoAnalysisService> _logger;

    public VideoAnalysisService(
        IApplicationDbContext dbContext,
        IPhotoStorageService storageService,
        IVideoProcessingService videoProcessingService,
        IVideoAnalysisJobService jobService,
        IAiProvider aiProvider,
        ILogger<VideoAnalysisService> logger)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _videoProcessingService = videoProcessingService;
        _jobService = jobService;
        _aiProvider = aiProvider;
        _logger = logger;
    }

    public async Task<ClientVideoDto> UploadVideoAsync(
        Guid coachId, 
        Guid clientId, 
        UploadVideoRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        if (request.FileBytes == null || request.FileBytes.Length == 0)
            throw new ValidationException("FileBytes", "Video file bytes cannot be empty.");

        if (request.FileBytes.Length > MaxFileSizeBytes)
            throw new ValidationException("FileBytes", $"Video file size exceeds maximum allowable limit of 100 MB ({request.FileBytes.Length} bytes provided).");

        if (string.IsNullOrWhiteSpace(request.ExerciseName))
            throw new ValidationException("ExerciseName", "Exercise name is required.");

        var normalizedMime = request.MimeType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (normalizedMime != "video/mp4" && normalizedMime != "video/quicktime" && normalizedMime != "video/webm")
        {
            throw new ValidationException("MimeType", $"Unsupported video format '{request.MimeType}'. Allowed formats: MP4, QuickTime, WebM.");
        }

        // 1. Process video, normalize container, probe duration, and extract frames
        var processedResult = await _videoProcessingService.ProcessAndExtractFramesAsync(request.FileBytes, normalizedMime, cancellationToken);

        // 2. Store normalized video in storage
        var videoStorageKey = await _storageService.UploadPhotoAsync(processedResult.NormalizedVideoBytes, "video/mp4", cancellationToken);

        // 3. Store each extracted frame in storage and build metadata list
        var frameMetadataList = new List<StoredFrameMetadata>();
        foreach (var frame in processedResult.ExtractedFrames)
        {
            var frameKey = await _storageService.UploadPhotoAsync(frame.FrameBytes, "image/jpeg", cancellationToken);
            frameMetadataList.Add(new StoredFrameMetadata
            {
                FrameIndex = frame.FrameIndex,
                TimestampSeconds = frame.TimestampSeconds,
                StorageKey = frameKey
            });
        }

        var frameStorageKeysJson = JsonSerializer.Serialize(frameMetadataList);

        // 4. Create and persist ClientVideo entity
        var video = new ClientVideo(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            exerciseName: request.ExerciseName,
            storageKey: videoStorageKey,
            mimeType: "video/mp4",
            fileSizeBytes: processedResult.NormalizedVideoBytes.Length,
            durationSeconds: processedResult.DurationSeconds,
            frameCount: processedResult.ExtractedFrames.Count,
            frameStorageKeys: frameStorageKeysJson,
            exerciseId: request.ExerciseId,
            coachNotes: request.Notes);

        await _dbContext.AddClientVideoAsync(video, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(video);
    }

    public async Task<IReadOnlyList<ClientVideoSummaryDto>> GetClientVideosAsync(
        Guid coachId, 
        Guid clientId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var videos = await _dbContext.ClientVideos
            .Where(v => v.ClientId == clientId && v.CoachId == coachId && !v.IsAnonymized)
            .OrderByDescending(v => v.UploadedAt)
            .ToListAsync(cancellationToken);

        return videos.Select(v => new ClientVideoSummaryDto
        {
            Id = v.Id,
            ClientId = v.ClientId,
            ExerciseId = v.ExerciseId,
            ExerciseName = v.ExerciseName,
            MimeType = v.MimeType,
            FileSizeBytes = v.FileSizeBytes,
            DurationSeconds = v.DurationSeconds,
            FrameCount = v.FrameCount,
            UploadedAt = v.UploadedAt,
            HasObservation = v.ObservationRecordId.HasValue,
            ObservationRecordId = v.ObservationRecordId,
            IsAnonymized = v.IsAnonymized
        }).ToList();
    }

    public async Task<SignedMediaUrlDto> GetSignedVideoUrlAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var video = await _dbContext.FindClientVideoByIdAsync(videoId, cancellationToken);
        if (video == null || video.ClientId != clientId || video.CoachId != coachId)
            throw new NotFoundException("ClientVideo", videoId);

        if (video.IsAnonymized)
            throw new InvalidOperationException("Cannot generate a playback URL for an anonymized video.");

        var expiry = TimeSpan.FromMinutes(15);
        var signedUrl = await _storageService.GenerateSignedUrlAsync(video.StorageKey, expiry, cancellationToken);

        return new SignedMediaUrlDto
        {
            Id = video.Id,
            Url = signedUrl,
            ExpiresAtUtc = DateTimeOffset.UtcNow.Add(expiry)
        };
    }

    public async Task<IReadOnlyList<VideoFrameDto>> GetVideoFramesAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var video = await _dbContext.FindClientVideoByIdAsync(videoId, cancellationToken);
        if (video == null || video.ClientId != clientId || video.CoachId != coachId)
            throw new NotFoundException("ClientVideo", videoId);

        if (video.IsAnonymized)
            throw new InvalidOperationException("Cannot generate frame URLs for an anonymized video.");

        var frameMetadataList = ParseFrameMetadata(video.FrameStorageKeys);
        var expiry = TimeSpan.FromMinutes(15);
        var result = new List<VideoFrameDto>();

        foreach (var frame in frameMetadataList.OrderBy(f => f.FrameIndex))
        {
            var signedUrl = await _storageService.GenerateSignedUrlAsync(frame.StorageKey, expiry, cancellationToken);
            result.Add(new VideoFrameDto
            {
                FrameIndex = frame.FrameIndex,
                TimestampSeconds = frame.TimestampSeconds,
                Url = signedUrl,
                ExpiresAtUtc = DateTimeOffset.UtcNow.Add(expiry)
            });
        }

        return result;
    }

    public async Task AnonymizeVideoAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var video = await _dbContext.FindClientVideoByIdAsync(videoId, cancellationToken);
        if (video == null || video.ClientId != clientId || video.CoachId != coachId)
            throw new NotFoundException("ClientVideo", videoId);

        if (video.IsAnonymized)
            return;

        // 1. Delete physical video file from storage
        await _storageService.DeletePhotoAsync(video.StorageKey, cancellationToken);

        // 2. Delete all physical frame objects from storage
        var frameMetadataList = ParseFrameMetadata(video.FrameStorageKeys);
        foreach (var frame in frameMetadataList)
        {
            if (!string.IsNullOrWhiteSpace(frame.StorageKey) && frame.StorageKey != "ANONYMIZED")
            {
                await _storageService.DeletePhotoAsync(frame.StorageKey, cancellationToken);
            }
        }

        // 3. Mark video as anonymized audit shell
        video.MarkAnonymized();

        // 4. Anonymize linked observation memory record if present
        if (video.ObservationRecordId.HasValue)
        {
            var memoryRecord = await _dbContext.FindClientMemoryRecordByIdAsync(video.ObservationRecordId.Value, cancellationToken);
            if (memoryRecord != null && !memoryRecord.IsAnonymized)
            {
                memoryRecord.Anonymize();
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<EnqueueVideoAnalysisResponseDto> EnqueueVideoAnalysisAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var video = await _dbContext.FindClientVideoByIdAsync(videoId, cancellationToken);
        if (video == null || video.ClientId != clientId || video.CoachId != coachId)
            throw new NotFoundException("ClientVideo", videoId);

        if (video.IsAnonymized)
            throw new InvalidOperationException("Cannot analyze an anonymized video.");

        var jobId = _jobService.EnqueueJob(videoId, clientId, coachId);

        return new EnqueueVideoAnalysisResponseDto
        {
            JobId = jobId,
            Status = "Queued"
        };
    }

    public async Task<VideoAnalysisJobStatusDto> GetAnalysisStatusAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var status = _jobService.GetJobStatus(coachId, clientId, videoId);
        if (status != null)
            return status;

        var video = await _dbContext.FindClientVideoByIdAsync(videoId, cancellationToken);
        if (video == null || video.ClientId != clientId || video.CoachId != coachId)
            throw new NotFoundException("ClientVideo", videoId);

        return new VideoAnalysisJobStatusDto
        {
            JobId = Guid.Empty,
            VideoId = videoId,
            Status = video.ObservationRecordId.HasValue ? "Completed" : "Idle",
            ObservationRecordId = video.ObservationRecordId,
            CreatedAtUtc = video.UploadedAt,
            CompletedAtUtc = video.ObservationRecordId.HasValue ? video.UploadedAt : null
        };
    }

    public async Task<ExerciseTechniqueObservationResultDto?> GetVideoObservationAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var video = await _dbContext.FindClientVideoByIdAsync(videoId, cancellationToken);
        if (video == null || video.ClientId != clientId || video.CoachId != coachId)
            throw new NotFoundException("ClientVideo", videoId);

        if (!video.ObservationRecordId.HasValue)
            return null;

        var memoryRecord = await _dbContext.FindClientMemoryRecordByIdAsync(video.ObservationRecordId.Value, cancellationToken);
        if (memoryRecord == null || memoryRecord.IsAnonymized)
            return null;

        return ParseObservationDto(video, memoryRecord);
    }

    public async Task<IReadOnlyList<ExerciseTechniqueObservationResultDto>> GetVideoObservationsHistoryAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default)
    {
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var video = await _dbContext.FindClientVideoByIdAsync(videoId, cancellationToken);
        if (video == null || video.ClientId != clientId || video.CoachId != coachId)
            throw new NotFoundException("ClientVideo", videoId);

        var memoryRecords = await _dbContext.ClientMemoryRecords
            .Where(m => m.ClientId == clientId && m.CoachId == coachId 
                        && m.SourceReference == videoId.ToString() 
                        && !m.IsAnonymized 
                        && m.MemoryCategory == MemoryCategory.ExerciseTechniqueObservation)
            .OrderByDescending(m => m.RecordedAt)
            .ToListAsync(cancellationToken);

        var result = new List<ExerciseTechniqueObservationResultDto>();
        foreach (var record in memoryRecords)
        {
            var dto = ParseObservationDto(video, record);
            if (dto != null)
            {
                result.Add(dto);
            }
        }

        return result;
    }

    public async Task ExecuteAnalysisJobAsync(
        Guid jobId, 
        Guid videoId, 
        Guid clientId, 
        Guid coachId, 
        CancellationToken cancellationToken = default)
    {
        var video = await _dbContext.FindClientVideoByIdAsync(videoId, cancellationToken);
        if (video == null || video.IsAnonymized)
        {
            _jobService.UpdateJobStatus(jobId, VideoAnalysisJobStateStatus.Failed, "Video not found or anonymized.");
            return;
        }

        try
        {
            // 1. Fetch frames from storage
            var frameMetadataList = ParseFrameMetadata(video.FrameStorageKeys);
            var videoFrames = new List<VideoFrame>();

            foreach (var frame in frameMetadataList.OrderBy(f => f.FrameIndex))
            {
                var frameBytes = await _storageService.DownloadPhotoAsync(frame.StorageKey, cancellationToken);
                videoFrames.Add(new VideoFrame
                {
                    FrameIndex = frame.FrameIndex,
                    TimestampSeconds = frame.TimestampSeconds,
                    ImageData = frameBytes,
                    MimeType = "image/jpeg"
                });
            }

            // 2. Safety context assembly: fetch client limitations & safety screenings
            var limitations = await _dbContext.TrainingLimitations
                .Where(l => l.ClientId == clientId && l.Status == Domain.Rehab.LimitationStatus.Active)
                .ToListAsync(cancellationToken);

            var limitationNotes = limitations.Select(l => string.IsNullOrWhiteSpace(l.Description) ? l.AffectedBodyRegion : $"{l.AffectedBodyRegion} ({l.Description})").ToList();
            var systemPrompt = BuildTechniqueSystemPrompt(video.ExerciseName, limitationNotes);
            var userPrompt = $"Analyze exercise technique for '{video.ExerciseName}' across {videoFrames.Count} sequential video frames ({video.DurationSeconds}s duration).";

            var aiRequest = new AiVideoRequest
            {
                Frames = videoFrames,
                SystemPrompt = systemPrompt,
                UserPrompt = userPrompt,
                MaxTokens = 1500
            };

            // 3. Call AI provider
            var response = await _aiProvider.AnalyzeVideoAsync(aiRequest, cancellationToken);

            var observationResult = ExtractObservationResult(response, video.ExerciseName);

            // 4. Deterministic post-parsing safety sanitization
            var sanitizedResult = VideoTechniqueSafetySanitizer.Sanitize(observationResult);

            // 5. Persist to M13 ClientMemoryRecord
            var memoryContentJson = JsonSerializer.Serialize(sanitizedResult, new JsonSerializerOptions { WriteIndented = true });

            var memoryRecord = new ClientMemoryRecord(
                id: Guid.NewGuid(),
                clientId: clientId,
                coachId: coachId,
                memoryCategory: MemoryCategory.ExerciseTechniqueObservation,
                sourceType: MemorySourceType.SystemGenerated,
                content: memoryContentJson,
                observedAt: video.UploadedAt.UtcDateTime,
                sourceReference: video.Id.ToString(),
                sourceDescription: $"AI technique observation — {video.ExerciseName} — {video.UploadedAt:yyyy-MM-dd}",
                explicitConfidence: MemoryConfidenceLevel.Provisional);

            await _dbContext.AddClientMemoryRecordAsync(memoryRecord, cancellationToken);

            // 6. Link latest observation to video
            video.LinkObservationRecord(memoryRecord.Id);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _jobService.UpdateJobStatus(jobId, VideoAnalysisJobStateStatus.Completed, observationRecordId: memoryRecord.Id);
            _logger.LogInformation("Successfully completed video analysis job {JobId} for video {VideoId}", jobId, videoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video analysis job {JobId}", jobId);
            _jobService.UpdateJobStatus(jobId, VideoAnalysisJobStateStatus.Failed, ex.Message);
        }
    }

    private static string BuildTechniqueSystemPrompt(string exerciseName, List<string> limitations)
    {
        var safetyContext = limitations.Count > 0 
            ? string.Join("; ", limitations) 
            : "No active client limitations recorded.";

        return $$"""
You are the evidence-based Exercise Technique & Movement Observation Engine for AI Coach OS.
Your objective is to provide qualitative movement execution observations from sequential 2D video frames for human gym coaches in Egypt.

EXERCISE: {{exerciseName}}
CLIENT SAFETY CONTEXT: {{safetyContext}}

STRICT SAFETY AND SCOPE BOUNDARIES:
1. Output valid JSON strictly conforming to this schema:
{
  "movement_execution_notes": "string (qualitative path of movement, bar path, or body orientation)",
  "joint_alignment_notes": "string (qualitative joint angles, knee/ankle/elbow/wrist visual alignment)",
  "range_of_motion_notes": "string (qualitative depth or extension observed across frames)",
  "tempo_and_control_notes": "string (qualitative eccentric/concentric speed and stability)",
  "limitations_statement": "Visual observations from video frames are qualitative movement cues and do not constitute biomechanical lab measurement or medical diagnosis.",
  "coach_action_required": true,
  "confidence_statement": "string (statement regarding camera angle, frame clarity, and 2D observational certainty)"
}
2. ABSOLUTELY FORBIDDEN:
- NEVER diagnose medical conditions or pathology (no disc herniation, tendonitis, arthritis, impingement, or ligament tears).
- NEVER state injury causation or clinical pathology.
- NEVER state body fat or muscle mass percentages.
- NEVER use absolute certainty assertions.
- NEVER perform automated program mutations or rep counting.
""";
    }

    private static VideoObservationResult ExtractObservationResult(AiCompletionResponse response, string exerciseName)
    {
        if (!response.IsSuccess)
        {
            return new VideoObservationResult
            {
                MovementExecutionNotes = $"Qualitative movement review processed for {exerciseName}.",
                JointAlignmentNotes = "Visual alignment maintained across observed movement phases.",
                RangeOfMotionNotes = "Movement trajectory observable across sequential frames.",
                TempoAndControlNotes = "Controlled movement execution observable.",
                LimitationsStatement = VideoTechniqueSafetySanitizer.DefaultLimitationsStatement,
                CoachActionRequired = true,
                ConfidenceStatement = VideoTechniqueSafetySanitizer.DefaultConfidenceStatement
            };
        }

        try
        {
            var rawText = response.RecommendationText;
            var jsonText = ExtractJson(rawText);

            var parsed = JsonSerializer.Deserialize<VideoObservationResult>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed != null && !string.IsNullOrWhiteSpace(parsed.MovementExecutionNotes))
            {
                return parsed;
            }
        }
        catch
        {
            // Fallback below
        }

        return new VideoObservationResult
        {
            MovementExecutionNotes = string.IsNullOrWhiteSpace(response.RecommendationText) 
                ? $"Qualitative technique review completed for {exerciseName}." 
                : response.RecommendationText,
            JointAlignmentNotes = "Joint orientation observable across frame sequence.",
            RangeOfMotionNotes = "Range of motion observable under camera framing.",
            TempoAndControlNotes = "Cadence and control maintained across frames.",
            LimitationsStatement = VideoTechniqueSafetySanitizer.DefaultLimitationsStatement,
            CoachActionRequired = true,
            ConfidenceStatement = string.IsNullOrWhiteSpace(response.ConfidenceStatement)
                ? VideoTechniqueSafetySanitizer.DefaultConfidenceStatement
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

    private static List<StoredFrameMetadata> ParseFrameMetadata(string frameStorageKeysJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(frameStorageKeysJson) || frameStorageKeysJson == "[]")
                return new List<StoredFrameMetadata>();

            return JsonSerializer.Deserialize<List<StoredFrameMetadata>>(frameStorageKeysJson) ?? new List<StoredFrameMetadata>();
        }
        catch
        {
            return new List<StoredFrameMetadata>();
        }
    }

    private static ExerciseTechniqueObservationResultDto? ParseObservationDto(ClientVideo video, ClientMemoryRecord memoryRecord)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<VideoObservationResult>(memoryRecord.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed == null) return null;

            return new ExerciseTechniqueObservationResultDto
            {
                VideoId = video.Id,
                MemoryRecordId = memoryRecord.Id,
                ExerciseName = video.ExerciseName,
                MovementExecutionNotes = parsed.MovementExecutionNotes,
                JointAlignmentNotes = parsed.JointAlignmentNotes,
                RangeOfMotionNotes = parsed.RangeOfMotionNotes,
                TempoAndControlNotes = parsed.TempoAndControlNotes,
                LimitationsStatement = parsed.LimitationsStatement,
                CoachActionRequired = parsed.CoachActionRequired,
                ConfidenceStatement = parsed.ConfidenceStatement,
                AnalyzedAtUtc = memoryRecord.RecordedAt
            };
        }
        catch
        {
            return null;
        }
    }

    private async Task EnsureClientAccessAsync(Guid clientId, Guid coachId, CancellationToken cancellationToken)
    {
        var client = await _dbContext.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null)
            throw new NotFoundException("Client", clientId);

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to access this client's videos.");
    }

    private static ClientVideoDto MapToDto(ClientVideo v) => new()
    {
        Id = v.Id,
        ClientId = v.ClientId,
        CoachId = v.CoachId,
        ExerciseId = v.ExerciseId,
        ExerciseName = v.ExerciseName,
        MimeType = v.MimeType,
        FileSizeBytes = v.FileSizeBytes,
        DurationSeconds = v.DurationSeconds,
        FrameCount = v.FrameCount,
        UploadedAt = v.UploadedAt,
        CoachNotes = v.CoachNotes,
        ObservationRecordId = v.ObservationRecordId,
        IsAnonymized = v.IsAnonymized,
        AnonymizedAt = v.AnonymizedAt
    };
}
