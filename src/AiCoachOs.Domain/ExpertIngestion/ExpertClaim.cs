using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.ExpertIngestion;

/// <summary>
/// Represents a structured, actionable, and falsifiable coaching claim extracted from an expert source.
/// Holds deterministic links to supporting or conflicting M3 KnowledgeClaims.
/// </summary>
public class ExpertClaim : Entity<Guid>
{
    public Guid IngestionId { get; private set; }
    public ExpertContentIngestion? Ingestion { get; private set; }

    public string Topic { get; private set; } = null!;
    public string? SubTopic { get; private set; }
    public string ClaimText { get; private set; } = null!;
    public string? ContextOrTimestamp { get; private set; }
    public bool DirectQuote { get; private set; }
    public ClaimNature NatureOfClaim { get; private set; }

    /// <summary>
    /// Deterministic M3 support link (zero LLM calls).
    /// </summary>
    public Guid? SupportingClaimId { get; private set; }
    public KnowledgeClaim? SupportingClaim { get; private set; }

    /// <summary>
    /// Deterministic M3 conflict link (zero LLM calls).
    /// </summary>
    public Guid? ConflictingClaimId { get; private set; }
    public KnowledgeClaim? ConflictingClaim { get; private set; }

    public ExpertClaimReviewStatus ReviewStatus { get; private set; }
    public string? CoachNotes { get; private set; }

    /// <summary>
    /// Foreign key link to the active KnowledgeClaim created or linked upon coach approval.
    /// </summary>
    public Guid? ApprovedKnowledgeClaimId { get; private set; }
    public KnowledgeClaim? ApprovedKnowledgeClaim { get; private set; }

    public DateTime? ReviewedAtUtc { get; private set; }
    public Guid? ReviewedByCoachId { get; private set; }

    private ExpertClaim() { } // EF Core

    public ExpertClaim(
        Guid id,
        Guid ingestionId,
        string topic,
        string claimText,
        ClaimNature natureOfClaim,
        string? subTopic = null,
        string? contextOrTimestamp = null,
        bool directQuote = false) : base(id)
    {
        if (ingestionId == Guid.Empty)
            throw new ArgumentException("IngestionId cannot be empty.", nameof(ingestionId));
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be empty.", nameof(topic));
        if (string.IsNullOrWhiteSpace(claimText))
            throw new ArgumentException("ClaimText cannot be empty.", nameof(claimText));

        IngestionId = ingestionId;
        Topic = topic.Trim();
        SubTopic = subTopic?.Trim();
        ClaimText = claimText.Trim();
        ContextOrTimestamp = contextOrTimestamp?.Trim();
        DirectQuote = directQuote;
        NatureOfClaim = natureOfClaim;
        ReviewStatus = ExpertClaimReviewStatus.Pending;
    }

    public void SetDeterministicM3Comparison(Guid? supportingClaimId, Guid? conflictingClaimId)
    {
        SupportingClaimId = supportingClaimId;
        ConflictingClaimId = conflictingClaimId;
        MarkUpdated();
    }

    public void Approve(Guid approvedKnowledgeClaimId, Guid coachId, string? notes = null)
    {
        if (approvedKnowledgeClaimId == Guid.Empty)
            throw new ArgumentException("ApprovedKnowledgeClaimId cannot be empty.", nameof(approvedKnowledgeClaimId));
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));

        ReviewStatus = ExpertClaimReviewStatus.Approved;
        ApprovedKnowledgeClaimId = approvedKnowledgeClaimId;
        ReviewedByCoachId = coachId;
        ReviewedAtUtc = DateTime.UtcNow;
        CoachNotes = notes?.Trim();
        MarkUpdated();
    }

    public void Reject(Guid coachId, string? notes = null)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));

        ReviewStatus = ExpertClaimReviewStatus.Rejected;
        ApprovedKnowledgeClaimId = null;
        ReviewedByCoachId = coachId;
        ReviewedAtUtc = DateTime.UtcNow;
        CoachNotes = notes?.Trim();
        MarkUpdated();
    }

    public void Defer(Guid coachId, string? notes = null)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));

        ReviewStatus = ExpertClaimReviewStatus.Deferred;
        ApprovedKnowledgeClaimId = null;
        ReviewedByCoachId = coachId;
        ReviewedAtUtc = DateTime.UtcNow;
        CoachNotes = notes?.Trim();
        MarkUpdated();
    }
}
