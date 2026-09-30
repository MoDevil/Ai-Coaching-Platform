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

    public string ClaimText { get; private set; } = null!;
    public ClaimCategory ClaimCategory { get; private set; }
    public EvidenceClassification EvidenceClassification { get; private set; }
    public CreatorConfidence CreatorConfidence { get; private set; }
    public bool DirectQuote { get; private set; }
    public string? SourceContext { get; private set; }

    /// <summary>
    /// Deterministic M3 conflict link (zero LLM calls).
    /// </summary>
    public Guid? ConflictingClaimId { get; private set; }
    public KnowledgeClaim? ConflictingClaim { get; private set; }

    /// <summary>
    /// Deterministic M3 support link (zero LLM calls).
    /// </summary>
    public Guid? SupportingClaimId { get; private set; }
    public KnowledgeClaim? SupportingClaim { get; private set; }

    public CoachReviewStatus CoachReviewStatus { get; private set; }
    public DateTime? CoachReviewedAt { get; private set; }
    public string? CoachNote { get; private set; }

    /// <summary>
    /// Foreign key link to the active or provisional KnowledgeClaim created or linked upon coach approval.
    /// </summary>
    public Guid? ApprovedKnowledgeClaimId { get; private set; }
    public KnowledgeClaim? ApprovedKnowledgeClaim { get; private set; }

    public Guid? ReviewedByCoachId { get; private set; }

    private ExpertClaim() { } // EF Core

    public ExpertClaim(
        Guid id,
        Guid ingestionId,
        string claimText,
        ClaimCategory claimCategory,
        EvidenceClassification evidenceClassification,
        CreatorConfidence creatorConfidence,
        bool directQuote = false,
        string? sourceContext = null) : base(id)
    {
        if (ingestionId == Guid.Empty)
            throw new ArgumentException("IngestionId cannot be empty.", nameof(ingestionId));
        if (string.IsNullOrWhiteSpace(claimText))
            throw new ArgumentException("ClaimText cannot be empty.", nameof(claimText));

        IngestionId = ingestionId;
        ClaimText = claimText.Trim();
        ClaimCategory = claimCategory;
        EvidenceClassification = evidenceClassification;
        CreatorConfidence = creatorConfidence;
        DirectQuote = directQuote;
        SourceContext = sourceContext?.Trim();
        CoachReviewStatus = CoachReviewStatus.PendingReview;
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

        CoachReviewStatus = CoachReviewStatus.Approved;
        ApprovedKnowledgeClaimId = approvedKnowledgeClaimId;
        ReviewedByCoachId = coachId;
        CoachReviewedAt = DateTime.UtcNow;
        CoachNote = notes?.Trim();
        MarkUpdated();
    }

    public void Reject(Guid coachId, string? notes = null)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));

        CoachReviewStatus = CoachReviewStatus.Rejected;
        ApprovedKnowledgeClaimId = null;
        ReviewedByCoachId = coachId;
        CoachReviewedAt = DateTime.UtcNow;
        CoachNote = notes?.Trim();
        MarkUpdated();
    }

    public void Defer(Guid coachId, string? notes = null)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));

        CoachReviewStatus = CoachReviewStatus.Deferred;
        ApprovedKnowledgeClaimId = null;
        ReviewedByCoachId = coachId;
        CoachReviewedAt = DateTime.UtcNow;
        CoachNote = notes?.Trim();
        MarkUpdated();
    }
}
