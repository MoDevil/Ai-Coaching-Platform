using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Memory;

/// <summary>
/// Represents a deterministic, provenance-enforced, versioned client memory record.
/// Follows strict state machine, immutable creation provenance, and forward-pointing supersession.
/// </summary>
public class ClientMemoryRecord : Entity<Guid>
{
    public const string AnonymizedContentSentinel = "[anonymized]";

    public Guid ClientId { get; private set; }
    public Guid CoachId { get; private set; }
    public MemoryCategory MemoryCategory { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public DateTime? ObservedAt { get; private set; }
    public MemorySourceType SourceType { get; private set; }
    public string? SourceReference { get; private set; }
    public string? SourceDescription { get; private set; }
    public MemoryConfidenceLevel ConfidenceLevel { get; private set; }
    public MemoryRecordStatus RecordStatus { get; private set; }
    public string Content { get; private set; } = string.Empty;

    public Guid? SupersededById { get; private set; }
    public ClientMemoryRecord? SupersededBy { get; private set; }
    public DateTime? SupersededAt { get; private set; }
    public string? SupersessionReason { get; private set; }

    public bool IsConflicted { get; private set; }
    public MemoryConfidenceLevel? PreConflictConfidenceLevel { get; private set; }
    public string? ConflictNotes { get; private set; }
    public string? CoachCorrectionNote { get; private set; }
    public DateTime? CorrectedAt { get; private set; }

    public bool IsAnonymized { get; private set; }
    public DateTime? AnonymizedAt { get; private set; }

    private ClientMemoryRecord() { } // EF Core

    public ClientMemoryRecord(
        Guid id,
        Guid clientId,
        Guid coachId,
        MemoryCategory memoryCategory,
        MemorySourceType sourceType,
        string content,
        DateTime? observedAt = null,
        string? sourceReference = null,
        string? sourceDescription = null,
        MemoryConfidenceLevel? explicitConfidence = null,
        string? coachCorrectionNote = null,
        DateTime? recordedAt = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        ClientId = clientId;
        CoachId = coachId;
        MemoryCategory = memoryCategory;
        SourceType = sourceType;
        Content = content.Trim();
        SourceReference = string.IsNullOrWhiteSpace(sourceReference) ? null : sourceReference.Trim();
        SourceDescription = string.IsNullOrWhiteSpace(sourceDescription) ? null : sourceDescription.Trim();
        CoachCorrectionNote = string.IsNullOrWhiteSpace(coachCorrectionNote) ? null : coachCorrectionNote.Trim();

        // Server-generated UTC creation time is immutable
        RecordedAt = recordedAt ?? DateTime.UtcNow;

        if (observedAt.HasValue && observedAt.Value > RecordedAt)
        {
            throw new ArgumentException("ObservedAt cannot be in the future relative to RecordedAt.", nameof(observedAt));
        }
        ObservedAt = observedAt;

        // Creation state machine validation
        RecordStatus = MemoryRecordStatus.Active;
        IsConflicted = false;
        IsAnonymized = false;

        if (explicitConfidence.HasValue)
        {
            if (explicitConfidence.Value == MemoryConfidenceLevel.Uncertain ||
                explicitConfidence.Value == MemoryConfidenceLevel.Conflicted ||
                explicitConfidence.Value == MemoryConfidenceLevel.Superseded)
            {
                throw new InvalidOperationException($"Cannot initialize a memory record with confidence {explicitConfidence.Value}.");
            }
            ConfidenceLevel = explicitConfidence.Value;
        }
        else
        {
            ConfidenceLevel = sourceType switch
            {
                MemorySourceType.SystemGenerated => MemoryConfidenceLevel.Confirmed,
                MemorySourceType.CoachRecorded => MemoryConfidenceLevel.Provisional,
                MemorySourceType.CoachCorrected => MemoryConfidenceLevel.Confirmed,
                MemorySourceType.AIGenerated => MemoryConfidenceLevel.Provisional,
                _ => MemoryConfidenceLevel.Provisional
            };
        }

        if (sourceType == MemorySourceType.CoachCorrected)
        {
            CorrectedAt = RecordedAt;
        }
    }

    public void FlagUncertain()
    {
        if (IsAnonymized || RecordStatus == MemoryRecordStatus.Anonymized)
            throw new InvalidOperationException("Cannot flag an anonymized record as uncertain.");
        if (RecordStatus != MemoryRecordStatus.Active)
            throw new InvalidOperationException($"Cannot flag a record with status {RecordStatus} as uncertain. Only Active records can be flagged.");
        if (IsConflicted || ConfidenceLevel == MemoryConfidenceLevel.Conflicted)
            throw new InvalidOperationException("Cannot flag a conflicted record as uncertain.");
        if (ConfidenceLevel == MemoryConfidenceLevel.Superseded)
            throw new InvalidOperationException("Cannot flag a superseded record as uncertain.");

        ConfidenceLevel = MemoryConfidenceLevel.Uncertain;
        MarkUpdated();
    }

    public void FlagConflicted(string conflictNote)
    {
        if (IsAnonymized || RecordStatus == MemoryRecordStatus.Anonymized)
            throw new InvalidOperationException("Cannot conflict an anonymized record.");
        if (RecordStatus != MemoryRecordStatus.Active)
            throw new InvalidOperationException($"Cannot conflict a record with status {RecordStatus}.");

        if (ConfidenceLevel != MemoryConfidenceLevel.Conflicted)
        {
            PreConflictConfidenceLevel = ConfidenceLevel;
        }

        IsConflicted = true;
        ConfidenceLevel = MemoryConfidenceLevel.Conflicted;
        RecordStatus = MemoryRecordStatus.Conflicted;
        ConflictNotes = string.IsNullOrWhiteSpace(conflictNote) ? "Detected conflicting information." : conflictNote.Trim();
        MarkUpdated();
    }

    public void ResolveConflict(MemoryConfidenceLevel? explicitRestoredConfidence = null)
    {
        if (RecordStatus != MemoryRecordStatus.Conflicted && !IsConflicted)
            throw new InvalidOperationException("Record is not in a conflicted state.");

        var restoredConfidence = explicitRestoredConfidence 
            ?? PreConflictConfidenceLevel 
            ?? (SourceType == MemorySourceType.SystemGenerated || SourceType == MemorySourceType.CoachCorrected ? MemoryConfidenceLevel.Confirmed : MemoryConfidenceLevel.Provisional);

        if (restoredConfidence == MemoryConfidenceLevel.Conflicted || restoredConfidence == MemoryConfidenceLevel.Superseded)
            throw new InvalidOperationException($"Cannot restore conflict with confidence {restoredConfidence}.");

        IsConflicted = false;
        RecordStatus = MemoryRecordStatus.Active;
        ConfidenceLevel = restoredConfidence;
        PreConflictConfidenceLevel = null;
        ConflictNotes = null;
        MarkUpdated();
    }

    public void Supersede(Guid newRecordId, string reason)
    {
        if (IsAnonymized || RecordStatus == MemoryRecordStatus.Anonymized)
            throw new InvalidOperationException("Cannot supersede an anonymized record.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Supersession reason is required.", nameof(reason));
        if (newRecordId == Guid.Empty)
            throw new ArgumentException("SupersededById cannot be empty.", nameof(newRecordId));

        RecordStatus = MemoryRecordStatus.Superseded;
        ConfidenceLevel = MemoryConfidenceLevel.Superseded;
        SupersededById = newRecordId;
        SupersededAt = DateTime.UtcNow;
        SupersessionReason = reason.Trim();
        IsConflicted = false;
        MarkUpdated();
    }

    public void Archive()
    {
        if (IsAnonymized || RecordStatus == MemoryRecordStatus.Anonymized)
            throw new InvalidOperationException("Cannot archive an anonymized record.");
        if (IsConflicted || RecordStatus == MemoryRecordStatus.Conflicted)
            throw new InvalidOperationException("Cannot archive a conflicted record.");
        if (RecordStatus == MemoryRecordStatus.Superseded)
            throw new InvalidOperationException("Cannot archive a superseded record.");
        if (RecordStatus != MemoryRecordStatus.Active)
            throw new InvalidOperationException($"Cannot archive a record with status {RecordStatus}.");

        RecordStatus = MemoryRecordStatus.Archived;
        MarkUpdated();
    }

    public void Anonymize()
    {
        if (IsAnonymized || RecordStatus == MemoryRecordStatus.Anonymized)
            throw new InvalidOperationException("Record is already anonymized.");

        RecordStatus = MemoryRecordStatus.Anonymized;
        ConfidenceLevel = MemoryConfidenceLevel.Confirmed;
        IsAnonymized = true;
        AnonymizedAt = DateTime.UtcNow;

        // Clear all personal memory content
        Content = AnonymizedContentSentinel;
        SourceDescription = AnonymizedContentSentinel;
        SourceReference = null;
        CoachCorrectionNote = null;
        ConflictNotes = null;
        SupersessionReason = null;
        ObservedAt = null;

        MarkUpdated();
    }
}
