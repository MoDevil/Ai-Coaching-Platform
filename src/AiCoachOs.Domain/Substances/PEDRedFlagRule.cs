using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents a deterministic safety screening rule for adverse signs, red flags, or emergency symptoms
/// related to substance and PED exposure.
/// </summary>
public class PEDRedFlagRule : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SignalPattern { get; private set; } = string.Empty;
    public SubstanceEscalationLevel EscalationLevel { get; private set; }
    public string RecommendedAction { get; private set; } = string.Empty;
    public string EvidenceBasis { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime? LastReviewedAtUtc { get; private set; }
    public string? ReviewedBy { get; private set; }

    public Guid? KnowledgeClaimId { get; private set; }
    public KnowledgeClaim? KnowledgeClaim { get; private set; }

    private PEDRedFlagRule() { } // EF Core

    public PEDRedFlagRule(
        Guid id,
        string name,
        string description,
        string signalPattern,
        SubstanceEscalationLevel escalationLevel,
        string recommendedAction,
        string evidenceBasis,
        bool isActive = true,
        DateTime? lastReviewedAtUtc = null,
        string? reviewedBy = "M12 initial clinical safety rule seed",
        Guid? knowledgeClaimId = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        if (string.IsNullOrWhiteSpace(signalPattern))
            throw new ArgumentException("Signal pattern cannot be empty.", nameof(signalPattern));
        if (string.IsNullOrWhiteSpace(recommendedAction))
            throw new ArgumentException("Recommended action cannot be empty.", nameof(recommendedAction));

        Name = name.Trim();
        Description = description.Trim();
        SignalPattern = signalPattern.Trim();
        EscalationLevel = escalationLevel;
        RecommendedAction = recommendedAction.Trim();
        EvidenceBasis = string.IsNullOrWhiteSpace(evidenceBasis) ? "Clinical emergency triage and endocrine safety guidelines." : evidenceBasis.Trim();
        IsActive = isActive;
        LastReviewedAtUtc = lastReviewedAtUtc;
        ReviewedBy = string.IsNullOrWhiteSpace(reviewedBy) ? null : reviewedBy.Trim();
        KnowledgeClaimId = knowledgeClaimId;
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
