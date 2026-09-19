namespace AiCoachOs.Domain.Knowledge;

/// <summary>
/// Explicit join entity linking a KnowledgeClaim to a supporting KnowledgeSource.
/// Allows capturing contextual relevance and notes on how the source supports the assertion.
/// </summary>
public class KnowledgeClaimSource
{
    public Guid ClaimId { get; private set; }
    public KnowledgeClaim Claim { get; private set; } = null!;

    public Guid SourceId { get; private set; }
    public KnowledgeSource Source { get; private set; } = null!;

    public string? RelevanceNote { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private KnowledgeClaimSource() { } // EF Core

    public KnowledgeClaimSource(Guid claimId, Guid sourceId, string? relevanceNote = null)
    {
        if (claimId == Guid.Empty)
            throw new ArgumentException("ClaimId cannot be empty.", nameof(claimId));
        if (sourceId == Guid.Empty)
            throw new ArgumentException("SourceId cannot be empty.", nameof(sourceId));

        ClaimId = claimId;
        SourceId = sourceId;
        RelevanceNote = relevanceNote?.Trim();
        CreatedAtUtc = DateTime.UtcNow;
    }
}
