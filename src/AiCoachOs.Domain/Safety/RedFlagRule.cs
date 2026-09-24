using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Safety;

public class RedFlagRule : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SignalPattern { get; private set; } = string.Empty; // Pattern descriptor e.g. "ChestPain_Cardiovascular", "AcuteSevere_Sudden", "Persistence_3Sessions"
    public SafetyCategory SafetyCategoryTriggered { get; private set; }
    public SafetyActionType RecommendedAction { get; private set; }
    public string EvidenceBasis { get; private set; } = string.Empty;
    public bool RequiresClinicalReview { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastReviewedAtUtc { get; private set; }
    public string? ReviewedBy { get; private set; }
    public Guid? KnowledgeClaimId { get; private set; }
    public KnowledgeClaim? KnowledgeClaim { get; private set; }

    private RedFlagRule() { } // EF Core

    public RedFlagRule(
        Guid id,
        string name,
        string description,
        string signalPattern,
        SafetyCategory safetyCategoryTriggered,
        SafetyActionType recommendedAction,
        string evidenceBasis,
        bool requiresClinicalReview = true,
        bool isActive = true,
        DateTime? lastReviewedAtUtc = null,
        string? reviewedBy = "M8 initial seed — requires clinical validation",
        Guid? knowledgeClaimId = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        if (string.IsNullOrWhiteSpace(signalPattern))
            throw new ArgumentException("Signal pattern cannot be empty.", nameof(signalPattern));
        if (string.IsNullOrWhiteSpace(evidenceBasis))
            throw new ArgumentException("Evidence basis cannot be empty.", nameof(evidenceBasis));

        Name = name.Trim();
        Description = description.Trim();
        SignalPattern = signalPattern.Trim();
        SafetyCategoryTriggered = safetyCategoryTriggered;
        RecommendedAction = recommendedAction;
        EvidenceBasis = evidenceBasis.Trim();
        RequiresClinicalReview = requiresClinicalReview;
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
