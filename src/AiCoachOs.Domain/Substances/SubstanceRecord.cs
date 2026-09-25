using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Abstract shared base class for all structured substance knowledge entities (Supplements, Hormones, PED Safety).
/// Follows evidence-linked architecture with M3 KnowledgeClaim integration.
/// </summary>
public abstract class SubstanceRecord : Entity<Guid>
{
    private readonly List<SubstanceSafetyFlag> _safetyFlags = new();

    public string Name { get; protected set; } = string.Empty;
    public SubstanceCategory Category { get; protected set; }
    public string Description { get; protected set; } = string.Empty;
    public string EvidenceSummary { get; protected set; } = string.Empty;
    
    public Guid? PrimaryKnowledgeClaimId { get; protected set; }
    public KnowledgeClaim? PrimaryKnowledgeClaim { get; protected set; }
    
    public DateTime? LastReviewedAtUtc { get; protected set; }
    public string? ReviewedBy { get; protected set; }
    public bool IsActive { get; protected set; }

    public IReadOnlyCollection<SubstanceSafetyFlag> SafetyFlags => _safetyFlags;

    protected SubstanceRecord() { } // EF Core

    protected SubstanceRecord(
        Guid id,
        string name,
        SubstanceCategory category,
        string description,
        string evidenceSummary,
        Guid? primaryKnowledgeClaimId = null,
        DateTime? lastReviewedAtUtc = null,
        string? reviewedBy = null,
        bool isActive = true) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Substance name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Substance description cannot be empty.", nameof(description));

        Name = name.Trim();
        Category = category;
        Description = description.Trim();
        EvidenceSummary = string.IsNullOrWhiteSpace(evidenceSummary) ? "No evidence summary provided." : evidenceSummary.Trim();
        PrimaryKnowledgeClaimId = primaryKnowledgeClaimId;
        LastReviewedAtUtc = lastReviewedAtUtc;
        ReviewedBy = string.IsNullOrWhiteSpace(reviewedBy) ? null : reviewedBy.Trim();
        IsActive = isActive;
    }

    public void AddSafetyFlag(SubstanceSafetyFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag);
        _safetyFlags.Add(flag);
        MarkUpdated();
    }

    public void ClearSafetyFlags()
    {
        _safetyFlags.Clear();
        MarkUpdated();
    }

    public void UpdateReview(string reviewedBy, DateTime? reviewedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(reviewedBy))
            throw new ArgumentException("Reviewer identifier cannot be empty.", nameof(reviewedBy));

        ReviewedBy = reviewedBy.Trim();
        LastReviewedAtUtc = reviewedAtUtc ?? DateTime.UtcNow;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }
}
