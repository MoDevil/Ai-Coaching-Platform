using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Memory;

namespace AiCoachOs.Domain.Photos;

/// <summary>
/// Represents a client physique progress photo metadata record.
/// Privacy & Security Invariants:
/// - Original filenames are discarded immediately and never persisted.
/// - StorageKey holds a UUID-based reference, never a permanent public URL.
/// - ClientId and CoachId use FK RESTRICT to prevent orphaned records.
/// - ObservationRecordId links to the M13 ClientMemoryRecord where the observation is stored.
/// - Maximum allowed photo size is 10 MB.
/// </summary>
public class ClientPhoto : Entity<Guid>
{
    public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public Guid ClientId { get; private set; }
    public Client? Client { get; private set; }

    public Guid CoachId { get; private set; }

    public PhotoSetType PhotoSetType { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }
    public string? Notes { get; private set; }
    public Guid? ObservationRecordId { get; private set; }
    public ClientMemoryRecord? ObservationRecord { get; private set; }

    public bool IsAnonymized { get; private set; }
    public DateTimeOffset? AnonymizedAt { get; private set; }

    private ClientPhoto() { } // EF Core

    public ClientPhoto(
        Guid id,
        Guid clientId,
        Guid coachId,
        PhotoSetType photoSetType,
        string storageKey,
        string mimeType,
        long fileSizeBytes,
        string? notes = null,
        DateTimeOffset? uploadedAt = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException("StorageKey cannot be empty.", nameof(storageKey));
        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("MimeType cannot be empty.", nameof(mimeType));
        if (fileSizeBytes <= 0)
            throw new ArgumentException("File size must be greater than 0.", nameof(fileSizeBytes));
        if (fileSizeBytes > MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds the maximum limit of {MaxFileSizeBytes} bytes (10MB).", nameof(fileSizeBytes));
        if (notes != null && notes.Length > 500)
            throw new ArgumentException("Notes cannot exceed 500 characters.", nameof(notes));

        var normalizedMime = mimeType.Trim().ToLowerInvariant();
        if (normalizedMime != "image/jpeg" && normalizedMime != "image/png" && normalizedMime != "image/webp")
        {
            throw new ArgumentException($"Unsupported image MIME type: {mimeType}. Only JPEG, PNG, and WebP are supported.", nameof(mimeType));
        }

        ClientId = clientId;
        CoachId = coachId;
        PhotoSetType = photoSetType;
        StorageKey = storageKey.Trim();
        MimeType = normalizedMime;
        FileSizeBytes = fileSizeBytes;
        UploadedAt = uploadedAt ?? DateTimeOffset.UtcNow;
        Notes = notes?.Trim();
        IsAnonymized = false;
    }

    public void LinkObservationRecord(Guid observationRecordId)
    {
        if (observationRecordId == Guid.Empty)
            throw new ArgumentException("ObservationRecordId cannot be empty.", nameof(observationRecordId));

        if (IsAnonymized)
            throw new InvalidOperationException("Cannot link an observation record to an anonymized photo.");

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
        Notes = null;
        MarkUpdated();
    }
}
