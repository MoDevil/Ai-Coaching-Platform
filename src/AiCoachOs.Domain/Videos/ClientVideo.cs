using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Memory;

namespace AiCoachOs.Domain.Videos;

/// <summary>
/// Represents an uploaded client exercise technique video record.
/// Privacy, Security & Biomechanics Invariants:
/// - Duration must be between 2 and 180 seconds inclusive.
/// - File size cannot exceed 100 MB.
/// - StorageKey and FrameStorageKeys hold opaque UUID references, never public URLs.
/// - Frame count is bounded to maximum 8 frames.
/// - ClientId, CoachId, ExerciseId, and ObservationRecordId use FK RESTRICT.
/// </summary>
public class ClientVideo : Entity<Guid>
{
    public const int MinDurationSeconds = 2;
    public const int MaxDurationSeconds = 180;
    public const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB
    public const int MaxFrameCount = 8;

    public Guid ClientId { get; private set; }
    public Client? Client { get; private set; }

    public Guid CoachId { get; private set; }

    public Guid? ExerciseId { get; private set; }
    public Exercise? Exercise { get; private set; }

    public string ExerciseName { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public int DurationSeconds { get; private set; }
    public int FrameCount { get; private set; }
    public string FrameStorageKeys { get; private set; } = "[]"; // JSON array of frame keys
    public DateTimeOffset UploadedAt { get; private set; }
    public string? CoachNotes { get; private set; }

    public Guid? ObservationRecordId { get; private set; }
    public ClientMemoryRecord? ObservationRecord { get; private set; }

    public bool IsAnonymized { get; private set; }
    public DateTimeOffset? AnonymizedAt { get; private set; }

    private ClientVideo() { } // EF Core

    public ClientVideo(
        Guid id,
        Guid clientId,
        Guid coachId,
        string exerciseName,
        string storageKey,
        string mimeType,
        long fileSizeBytes,
        int durationSeconds,
        int frameCount,
        string frameStorageKeys,
        Guid? exerciseId = null,
        string? coachNotes = null,
        DateTimeOffset? uploadedAt = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new ArgumentException("ExerciseName cannot be empty.", nameof(exerciseName));
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException("StorageKey cannot be empty.", nameof(storageKey));
        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("MimeType cannot be empty.", nameof(mimeType));
        if (fileSizeBytes <= 0)
            throw new ArgumentException("File size must be greater than 0.", nameof(fileSizeBytes));
        if (fileSizeBytes > MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds the maximum limit of {MaxFileSizeBytes} bytes (100MB).", nameof(fileSizeBytes));
        if (durationSeconds < MinDurationSeconds || durationSeconds > MaxDurationSeconds)
            throw new ArgumentException($"Video duration must be between {MinDurationSeconds} and {MaxDurationSeconds} seconds inclusive. Provided: {durationSeconds}s.", nameof(durationSeconds));
        if (frameCount < 1 || frameCount > MaxFrameCount)
            throw new ArgumentException($"Frame count must be between 1 and {MaxFrameCount}. Provided: {frameCount}.", nameof(frameCount));
        if (string.IsNullOrWhiteSpace(frameStorageKeys))
            throw new ArgumentException("FrameStorageKeys cannot be empty.", nameof(frameStorageKeys));
        if (coachNotes != null && coachNotes.Length > 500)
            throw new ArgumentException("CoachNotes cannot exceed 500 characters.", nameof(coachNotes));

        var normalizedMime = mimeType.Trim().ToLowerInvariant();
        if (normalizedMime != "video/mp4" && normalizedMime != "video/quicktime" && normalizedMime != "video/webm")
        {
            throw new ArgumentException($"Unsupported video MIME type: {mimeType}. Only MP4, QuickTime, and WebM are supported.", nameof(mimeType));
        }

        ClientId = clientId;
        CoachId = coachId;
        ExerciseId = exerciseId;
        ExerciseName = exerciseName.Trim();
        StorageKey = storageKey.Trim();
        MimeType = normalizedMime;
        FileSizeBytes = fileSizeBytes;
        DurationSeconds = durationSeconds;
        FrameCount = frameCount;
        FrameStorageKeys = frameStorageKeys.Trim();
        UploadedAt = uploadedAt ?? DateTimeOffset.UtcNow;
        CoachNotes = coachNotes?.Trim();
        IsAnonymized = false;
    }

    public void LinkObservationRecord(Guid observationRecordId)
    {
        if (observationRecordId == Guid.Empty)
            throw new ArgumentException("ObservationRecordId cannot be empty.", nameof(observationRecordId));

        if (IsAnonymized)
            throw new InvalidOperationException("Cannot link an observation record to an anonymized video.");

        ObservationRecordId = observationRecordId;
        MarkUpdated();
    }

    public void MarkAnonymized(DateTimeOffset? anonymizedAt = null)
    {
        if (IsAnonymized)
            return;

        IsAnonymized = true;
        AnonymizedAt = anonymizedAt ?? DateTimeOffset.UtcNow;
        StorageKey = "ANONYMIZED";
        FrameStorageKeys = "[]";
        CoachNotes = null;
        MarkUpdated();
    }
}
