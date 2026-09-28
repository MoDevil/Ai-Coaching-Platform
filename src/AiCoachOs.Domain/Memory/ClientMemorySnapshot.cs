using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Memory;

/// <summary>
/// Represents a deterministic, point-in-time structured view of a client's active memory.
/// Serves as the deterministic input foundation for downstream reasoning.
/// </summary>
public class ClientMemorySnapshot : Entity<Guid>
{
    private readonly List<Guid> _includedRecordIds = new();
    private readonly List<Guid> _excludedConflictIds = new();

    public Guid ClientId { get; private set; }
    public Guid CoachId { get; private set; }
    public DateTime GeneratedAtUtc { get; private set; }
    public SnapshotGenerationTrigger GenerationTrigger { get; private set; }
    public string SnapshotContentJson { get; private set; } = string.Empty;
    public bool IsStale { get; private set; }

    public IReadOnlyCollection<Guid> IncludedRecordIds => _includedRecordIds;
    public IReadOnlyCollection<Guid> ExcludedConflictIds => _excludedConflictIds;

    private ClientMemorySnapshot() { } // EF Core

    public ClientMemorySnapshot(
        Guid id,
        Guid clientId,
        Guid coachId,
        SnapshotGenerationTrigger generationTrigger,
        string snapshotContentJson,
        IEnumerable<Guid>? includedRecordIds = null,
        IEnumerable<Guid>? excludedConflictIds = null,
        DateTime? generatedAtUtc = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (string.IsNullOrWhiteSpace(snapshotContentJson))
            throw new ArgumentException("Snapshot content JSON cannot be empty.", nameof(snapshotContentJson));

        ClientId = clientId;
        CoachId = coachId;
        GenerationTrigger = generationTrigger;
        SnapshotContentJson = snapshotContentJson.Trim();
        GeneratedAtUtc = generatedAtUtc ?? DateTime.UtcNow;
        IsStale = false;

        if (includedRecordIds != null)
        {
            _includedRecordIds.AddRange(includedRecordIds);
        }

        if (excludedConflictIds != null)
        {
            _excludedConflictIds.AddRange(excludedConflictIds);
        }
    }

    public void MarkStale()
    {
        IsStale = true;
        MarkUpdated();
    }
}
