using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Exercises;

namespace AiCoachOs.Domain.Knowledge;

/// <summary>
/// Represents a structured, evidence-backed scientific or coaching claim.
/// Captures what source-backed literature asserts regarding training stimulus, adaptation,
/// constraints, or specific exercises, without converting claims into immutable exercise properties.
/// </summary>
public class KnowledgeClaim : Entity<Guid>
{
    private readonly List<KnowledgeClaimSource> _sources = new();

    public string Topic { get; private set; } = null!;
    public string Question { get; private set; } = null!;
    public string ClaimText { get; private set; } = null!;
    public EvidenceLevel EvidenceLevel { get; private set; }
    public string? Population { get; private set; }
    public string? Limitations { get; private set; }
    public string? PracticalApplication { get; private set; }
    public ClaimStatus Status { get; private set; }

    /// <summary>
    /// Optional link to an Exercise. Nullable for general coaching/scientific principles.
    /// Note: Associating a claim with an exercise does NOT mutate Exercise properties
    /// or alter its MetadataStatus.
    /// </summary>
    public Guid? ExerciseId { get; private set; }
    public Exercise? Exercise { get; private set; }

    public DateTime? ReviewedAtUtc { get; private set; }
    public string? ReviewedBy { get; private set; }

    /// <summary>
    /// Self-referencing link to the replacement claim when superseded.
    /// Preserves a deterministic versioning chain without silent overwriting.
    /// </summary>
    public Guid? SupersededByClaimId { get; private set; }
    public KnowledgeClaim? SupersededByClaim { get; private set; }
    public DateTime? SupersededAtUtc { get; private set; }
    public string? SupersessionReason { get; private set; }

    public IReadOnlyCollection<KnowledgeClaimSource> Sources => _sources;

    private KnowledgeClaim() { } // EF Core

    public KnowledgeClaim(
        Guid id,
        string topic,
        string question,
        string claimText,
        EvidenceLevel evidenceLevel,
        ClaimStatus status = ClaimStatus.Active,
        Guid? exerciseId = null,
        string? population = null,
        string? limitations = null,
        string? practicalApplication = null,
        DateTime? reviewedAtUtc = null,
        string? reviewedBy = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Claim topic cannot be empty.", nameof(topic));
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Claim question cannot be empty.", nameof(question));
        if (string.IsNullOrWhiteSpace(claimText))
            throw new ArgumentException("Claim text cannot be empty.", nameof(claimText));

        Topic = topic.Trim();
        Question = question.Trim();
        ClaimText = claimText.Trim();
        EvidenceLevel = evidenceLevel;
        Status = status;
        ExerciseId = exerciseId;
        Population = population?.Trim();
        Limitations = limitations?.Trim();
        PracticalApplication = practicalApplication?.Trim();
        ReviewedAtUtc = reviewedAtUtc;
        ReviewedBy = reviewedBy?.Trim();
    }

    public void AddSource(Guid sourceId, string? relevanceNote = null)
    {
        if (sourceId == Guid.Empty)
            throw new ArgumentException("SourceId cannot be empty.", nameof(sourceId));

        if (_sources.Any(s => s.SourceId == sourceId))
            return;

        _sources.Add(new KnowledgeClaimSource(Id, sourceId, relevanceNote));
        MarkUpdated();
    }

    public void Supersede(KnowledgeClaim replacementClaim, string reason, DateTime? supersededAt = null)
    {
        ArgumentNullException.ThrowIfNull(replacementClaim);

        if (replacementClaim.Id == Id)
            throw new InvalidOperationException("A knowledge claim cannot supersede itself.");

        if (Status == ClaimStatus.Superseded)
            throw new InvalidOperationException("This knowledge claim is already superseded.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Supersession reason cannot be empty.", nameof(reason));

        Status = ClaimStatus.Superseded;
        SupersededByClaimId = replacementClaim.Id;
        SupersededAtUtc = supersededAt ?? DateTime.UtcNow;
        SupersessionReason = reason.Trim();
        MarkUpdated();
    }

    public void MarkReviewed(string reviewedBy, DateTime? reviewedAt = null)
    {
        if (string.IsNullOrWhiteSpace(reviewedBy))
            throw new ArgumentException("Reviewer identifier cannot be empty.", nameof(reviewedBy));

        ReviewedBy = reviewedBy.Trim();
        ReviewedAtUtc = reviewedAt ?? DateTime.UtcNow;
        MarkUpdated();
    }

    public void UpdateStatus(ClaimStatus newStatus)
    {
        Status = newStatus;
        MarkUpdated();
    }
}
