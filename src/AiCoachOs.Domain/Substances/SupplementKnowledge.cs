using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents structured, evidence-backed knowledge regarding dietary and performance supplements.
/// Enforces mandatory uncertainty statements and evidence-level integration.
/// </summary>
public class SupplementKnowledge : SubstanceRecord
{
    public SupplementCategory SupplementCategory { get; private set; }
    public EvidenceLevel EvidenceLevel { get; private set; }
    public string UncertaintyStatement { get; private set; } = string.Empty;
    public string? CommonForms { get; private set; }
    public string? TypicalDoseRange { get; private set; }
    public string? TimingRecommendation { get; private set; }
    public string? InteractionsAndNotes { get; private set; }
    public bool IsEgyptianMarketAvailable { get; private set; }

    private SupplementKnowledge() { } // EF Core

    public SupplementKnowledge(
        Guid id,
        string name,
        SupplementCategory supplementCategory,
        EvidenceLevel evidenceLevel,
        string description,
        string evidenceSummary,
        string uncertaintyStatement,
        Guid? primaryKnowledgeClaimId = null,
        string? commonForms = null,
        string? typicalDoseRange = null,
        string? timingRecommendation = null,
        string? interactionsAndNotes = null,
        bool isEgyptianMarketAvailable = true,
        DateTime? lastReviewedAtUtc = null,
        string? reviewedBy = null,
        bool isActive = true)
        : base(
            id: id,
            name: name,
            category: SubstanceCategory.Supplement,
            description: description,
            evidenceSummary: evidenceSummary,
            primaryKnowledgeClaimId: primaryKnowledgeClaimId,
            lastReviewedAtUtc: lastReviewedAtUtc,
            reviewedBy: reviewedBy,
            isActive: isActive)
    {
        if (string.IsNullOrWhiteSpace(uncertaintyStatement))
            throw new ArgumentException("Uncertainty statement is mandatory and cannot be empty for supplement knowledge.", nameof(uncertaintyStatement));

        SupplementCategory = supplementCategory;
        EvidenceLevel = evidenceLevel;
        UncertaintyStatement = uncertaintyStatement.Trim();
        CommonForms = string.IsNullOrWhiteSpace(commonForms) ? null : commonForms.Trim();
        TypicalDoseRange = string.IsNullOrWhiteSpace(typicalDoseRange) ? null : typicalDoseRange.Trim();
        TimingRecommendation = string.IsNullOrWhiteSpace(timingRecommendation) ? null : timingRecommendation.Trim();
        InteractionsAndNotes = string.IsNullOrWhiteSpace(interactionsAndNotes) ? null : interactionsAndNotes.Trim();
        IsEgyptianMarketAvailable = isEgyptianMarketAvailable;
    }

    public void UpdateUncertaintyStatement(string uncertaintyStatement)
    {
        if (string.IsNullOrWhiteSpace(uncertaintyStatement))
            throw new ArgumentException("Uncertainty statement cannot be empty.", nameof(uncertaintyStatement));

        UncertaintyStatement = uncertaintyStatement.Trim();
        MarkUpdated();
    }
}
