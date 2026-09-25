using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Abstract shared base class for all structured substance knowledge entities (Supplements, Hormones, PED Safety).
/// Follows evidence-linked architecture with M3 KnowledgeClaim integration.
/// Locked schema: Id, Name, CommonAliases, SubstanceCategory, Description, IsProvisional, RequiresClinicalReview,
/// ClaimStatus, CreatedAt, LastReviewedAt, ReviewDueAt.
/// </summary>
public abstract class SubstanceRecord : Entity<Guid>
{
    private readonly List<string> _commonAliases = new();
    private readonly List<SubstanceSafetyFlag> _safetyFlags = new();

    public string Name { get; protected set; } = string.Empty;
    public SubstanceCategory SubstanceCategory { get; protected set; }
    public string Description { get; protected set; } = string.Empty;
    public bool IsProvisional { get; protected set; }
    public bool RequiresClinicalReview { get; protected set; }
    public ClaimStatus ClaimStatus { get; protected set; }
    public DateTime? LastReviewedAtUtc { get; protected set; }
    public DateTime? ReviewDueAtUtc { get; protected set; }
    public string? ReviewedBy { get; protected set; }
    public bool IsActive { get; protected set; }

    public Guid? PrimaryKnowledgeClaimId { get; protected set; }
    public KnowledgeClaim? PrimaryKnowledgeClaim { get; protected set; }

    public IReadOnlyCollection<string> CommonAliases => _commonAliases;
    public IReadOnlyCollection<SubstanceSafetyFlag> SafetyFlags => _safetyFlags;

    protected SubstanceRecord() { } // EF Core

    protected SubstanceRecord(
        Guid id,
        string name,
        SubstanceCategory substanceCategory,
        string description,
        bool isProvisional = false,
        bool requiresClinicalReview = false,
        ClaimStatus claimStatus = ClaimStatus.Active,
        DateTime? lastReviewedAtUtc = null,
        DateTime? reviewDueAtUtc = null,
        string? reviewedBy = null,
        bool isActive = true,
        Guid? primaryKnowledgeClaimId = null,
        IEnumerable<string>? commonAliases = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Substance name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Substance description cannot be empty.", nameof(description));

        Name = name.Trim();
        SubstanceCategory = substanceCategory;
        Description = description.Trim();
        IsProvisional = isProvisional;
        RequiresClinicalReview = requiresClinicalReview;
        ClaimStatus = claimStatus;
        LastReviewedAtUtc = lastReviewedAtUtc;
        ReviewedBy = string.IsNullOrWhiteSpace(reviewedBy) ? null : reviewedBy.Trim();
        IsActive = isActive;
        PrimaryKnowledgeClaimId = primaryKnowledgeClaimId;

        // ReviewDueAt calculation: 24 months for Supplement & Hormone, 12 months for PED
        int reviewMonths = substanceCategory == SubstanceCategory.PED ? 12 : 24;
        ReviewDueAtUtc = reviewDueAtUtc ?? (lastReviewedAtUtc ?? CreatedAtUtc).AddMonths(reviewMonths);

        if (commonAliases != null)
        {
            foreach (var alias in commonAliases)
            {
                if (!string.IsNullOrWhiteSpace(alias))
                    _commonAliases.Add(alias.Trim());
            }
        }
    }

    public void AddCommonAlias(string alias)
    {
        if (!string.IsNullOrWhiteSpace(alias) && !_commonAliases.Contains(alias.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            _commonAliases.Add(alias.Trim());
            MarkUpdated();
        }
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

    public void UpdateReview(string reviewedBy, DateTime? reviewedAtUtc = null, int? reviewMonths = null)
    {
        if (string.IsNullOrWhiteSpace(reviewedBy))
            throw new ArgumentException("Reviewer identifier cannot be empty.", nameof(reviewedBy));

        ReviewedBy = reviewedBy.Trim();
        LastReviewedAtUtc = reviewedAtUtc ?? DateTime.UtcNow;
        int months = reviewMonths ?? (SubstanceCategory == SubstanceCategory.PED ? 12 : 24);
        ReviewDueAtUtc = LastReviewedAtUtc.Value.AddMonths(months);
        RequiresClinicalReview = false;
        MarkUpdated();
    }

    public void MarkProvisional(bool isProvisional = true)
    {
        IsProvisional = isProvisional;
        ClaimStatus = isProvisional ? ClaimStatus.Provisional : ClaimStatus.Active;
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
