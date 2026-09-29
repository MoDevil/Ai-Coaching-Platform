using System.Text.Json.Serialization;

namespace AiCoachOs.Application.Videos.Dtos;

public class UploadVideoRequestDto
{
    [JsonPropertyName("fileBytes")]
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("exerciseName")]
    public string ExerciseName { get; set; } = string.Empty;

    [JsonPropertyName("exerciseId")]
    public Guid? ExerciseId { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

public class ClientVideoDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("coachId")]
    public Guid CoachId { get; set; }

    [JsonPropertyName("exerciseId")]
    public Guid? ExerciseId { get; set; }

    [JsonPropertyName("exerciseName")]
    public string ExerciseName { get; set; } = string.Empty;

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("fileSizeBytes")]
    public long FileSizeBytes { get; set; }

    [JsonPropertyName("durationSeconds")]
    public int DurationSeconds { get; set; }

    [JsonPropertyName("frameCount")]
    public int FrameCount { get; set; }

    [JsonPropertyName("uploadedAt")]
    public DateTimeOffset UploadedAt { get; set; }

    [JsonPropertyName("coachNotes")]
    public string? CoachNotes { get; set; }

    [JsonPropertyName("observationRecordId")]
    public Guid? ObservationRecordId { get; set; }

    [JsonPropertyName("isAnonymized")]
    public bool IsAnonymized { get; set; }

    [JsonPropertyName("anonymizedAt")]
    public DateTimeOffset? AnonymizedAt { get; set; }
}

public class ClientVideoSummaryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("exerciseId")]
    public Guid? ExerciseId { get; set; }

    [JsonPropertyName("exerciseName")]
    public string ExerciseName { get; set; } = string.Empty;

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("fileSizeBytes")]
    public long FileSizeBytes { get; set; }

    [JsonPropertyName("durationSeconds")]
    public int DurationSeconds { get; set; }

    [JsonPropertyName("frameCount")]
    public int FrameCount { get; set; }

    [JsonPropertyName("uploadedAt")]
    public DateTimeOffset UploadedAt { get; set; }

    [JsonPropertyName("hasObservation")]
    public bool HasObservation { get; set; }

    [JsonPropertyName("observationRecordId")]
    public Guid? ObservationRecordId { get; set; }

    [JsonPropertyName("isAnonymized")]
    public bool IsAnonymized { get; set; }
}

public class SignedMediaUrlDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("expiresAtUtc")]
    public DateTimeOffset ExpiresAtUtc { get; set; }
}

public class VideoFrameDto
{
    [JsonPropertyName("frameIndex")]
    public int FrameIndex { get; set; }

    [JsonPropertyName("timestampSeconds")]
    public decimal TimestampSeconds { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("expiresAtUtc")]
    public DateTimeOffset ExpiresAtUtc { get; set; }
}

public class StoredFrameMetadata
{
    [JsonPropertyName("frameIndex")]
    public int FrameIndex { get; set; }

    [JsonPropertyName("timestampSeconds")]
    public decimal TimestampSeconds { get; set; }

    [JsonPropertyName("storageKey")]
    public string StorageKey { get; set; } = string.Empty;
}

public class EnqueueVideoAnalysisResponseDto
{
    [JsonPropertyName("jobId")]
    public Guid JobId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Queued";
}

public class VideoAnalysisJobStatusDto
{
    [JsonPropertyName("jobId")]
    public Guid JobId { get; set; }

    [JsonPropertyName("videoId")]
    public Guid VideoId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Queued";

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("observationRecordId")]
    public Guid? ObservationRecordId { get; set; }

    [JsonPropertyName("createdAtUtc")]
    public DateTimeOffset CreatedAtUtc { get; set; }

    [JsonPropertyName("completedAtUtc")]
    public DateTimeOffset? CompletedAtUtc { get; set; }
}

public class ExerciseTechniqueObservationResultDto
{
    [JsonPropertyName("videoId")]
    public Guid VideoId { get; set; }

    [JsonPropertyName("memoryRecordId")]
    public Guid MemoryRecordId { get; set; }

    [JsonPropertyName("exerciseName")]
    public string ExerciseName { get; set; } = string.Empty;

    [JsonPropertyName("movementExecutionNotes")]
    public string MovementExecutionNotes { get; set; } = string.Empty;

    [JsonPropertyName("jointAlignmentNotes")]
    public string JointAlignmentNotes { get; set; } = string.Empty;

    [JsonPropertyName("rangeOfMotionNotes")]
    public string RangeOfMotionNotes { get; set; } = string.Empty;

    [JsonPropertyName("tempoAndControlNotes")]
    public string TempoAndControlNotes { get; set; } = string.Empty;

    [JsonPropertyName("limitationsStatement")]
    public string LimitationsStatement { get; set; } = string.Empty;

    [JsonPropertyName("coachActionRequired")]
    public bool CoachActionRequired { get; set; } = true;

    [JsonPropertyName("confidenceStatement")]
    public string ConfidenceStatement { get; set; } = string.Empty;

    [JsonPropertyName("analyzedAtUtc")]
    public DateTimeOffset AnalyzedAtUtc { get; set; }
}
