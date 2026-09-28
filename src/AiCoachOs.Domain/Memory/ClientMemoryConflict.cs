using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Memory;

/// <summary>
/// Represents a conflict between two memory records for a client.
/// Can be auto-detected (deterministic preference or pain rules) or manually flagged by a coach.
/// </summary>
public class ClientMemoryConflict : Entity<Guid>
{
    public Guid ClientId { get; private set; }
    public Guid RecordAId { get; private set; }
    public ClientMemoryRecord? RecordA { get; private set; }
    public Guid RecordBId { get; private set; }
    public ClientMemoryRecord? RecordB { get; private set; }

    public string ConflictDescription { get; private set; } = string.Empty;
    public DateTime DetectedAtUtc { get; private set; }
    public bool IsAutoDetected { get; private set; }

    public bool IsResolved { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public Guid? ResolvedByCoachId { get; private set; }
    public string? ResolutionNote { get; private set; }
    public Guid? WinningRecordId { get; private set; }

    private ClientMemoryConflict() { } // EF Core

    public ClientMemoryConflict(
        Guid id,
        Guid clientId,
        Guid recordAId,
        Guid recordBId,
        string conflictDescription,
        bool isAutoDetected = false,
        DateTime? detectedAtUtc = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        if (recordAId == Guid.Empty)
            throw new ArgumentException("RecordAId cannot be empty.", nameof(recordAId));
        if (recordBId == Guid.Empty)
            throw new ArgumentException("RecordBId cannot be empty.", nameof(recordBId));
        if (recordAId == recordBId)
            throw new ArgumentException("Cannot create a conflict between a record and itself.", nameof(recordBId));
        if (string.IsNullOrWhiteSpace(conflictDescription))
            throw new ArgumentException("Conflict description cannot be empty.", nameof(conflictDescription));

        ClientId = clientId;
        RecordAId = recordAId;
        RecordBId = recordBId;
        ConflictDescription = conflictDescription.Trim();
        IsAutoDetected = isAutoDetected;
        DetectedAtUtc = detectedAtUtc ?? DateTime.UtcNow;
        IsResolved = false;
    }

    public void Resolve(Guid resolvedByCoachId, string resolutionNote, Guid? winningRecordId = null)
    {
        if (resolvedByCoachId == Guid.Empty)
            throw new ArgumentException("ResolvedByCoachId cannot be empty.", nameof(resolvedByCoachId));
        if (string.IsNullOrWhiteSpace(resolutionNote))
            throw new ArgumentException("Resolution note is required.", nameof(resolutionNote));

        IsResolved = true;
        ResolvedAtUtc = DateTime.UtcNow;
        ResolvedByCoachId = resolvedByCoachId;
        ResolutionNote = resolutionNote.Trim();
        WinningRecordId = winningRecordId;
        MarkUpdated();
    }
}
