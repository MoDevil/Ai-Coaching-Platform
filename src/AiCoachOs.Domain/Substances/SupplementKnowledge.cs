using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents structured, evidence-backed knowledge regarding dietary and performance supplements.
/// Locked contract: PrimaryClaimedBenefit, EfficacyClaim, EvidenceStatus, EffectMagnitude, PopulationNote,
/// UncertaintyStatement, TypicalDoseRangeMin/Max, DoseUnit, DoseSourceClaimId, TimingNote, SafetyFlags.
/// Enforces mandatory non-empty uncertainty statement.
/// </summary>
public class SupplementKnowledge : SubstanceRecord
{
    public string PrimaryClaimedBenefit { get; private set; } = string.Empty;
    public string? EfficacyClaim { get; private set; }
    public SupplementEvidenceStatus EvidenceStatus { get; private set; }
    public EffectMagnitude EffectMagnitude { get; private set; }
    public string? PopulationNote { get; private set; }
    public string UncertaintyStatement { get; private set; } = string.Empty;
    public decimal? TypicalDoseRangeMin { get; private set; }
    public decimal? TypicalDoseRangeMax { get; private set; }
    public string? DoseUnit { get; private set; }
    public Guid? DoseSourceClaimId { get; private set; }
    public string? TimingNote { get; private set; }
    public string? CommonForms { get; private set; }
    public string? InteractionsAndNotes { get; private set; }
    public bool IsEgyptianMarketAvailable { get; private set; }

    private SupplementKnowledge() { } // EF Core

    public SupplementKnowledge(
        Guid id,
        string name,
        string primaryClaimedBenefit,
        SupplementEvidenceStatus evidenceStatus,
        EffectMagnitude effectMagnitude,
        string description,
        string uncertaintyStatement,
        string? efficacyClaim = null,
        string? populationNote = null,
        decimal? typicalDoseRangeMin = null,
        decimal? typicalDoseRangeMax = null,
        string? doseUnit = null,
        Guid? doseSourceClaimId = null,
        string? timingNote = null,
        string? commonForms = null,
        string? interactionsAndNotes = null,
        bool isEgyptianMarketAvailable = true,
        Guid? primaryKnowledgeClaimId = null,
        bool isProvisional = false,
        bool requiresClinicalReview = false,
        ClaimStatus claimStatus = ClaimStatus.Active,
        DateTime? lastReviewedAtUtc = null,
        DateTime? reviewDueAtUtc = null,
        string? reviewedBy = null,
        bool isActive = true,
        IEnumerable<string>? commonAliases = null)
        : base(
            id: id,
            name: name,
            substanceCategory: SubstanceCategory.Supplement,
            description: description,
            isProvisional: isProvisional,
            requiresClinicalReview: requiresClinicalReview,
            claimStatus: claimStatus,
            lastReviewedAtUtc: lastReviewedAtUtc,
            reviewDueAtUtc: reviewDueAtUtc,
            reviewedBy: reviewedBy,
            isActive: isActive,
            primaryKnowledgeClaimId: primaryKnowledgeClaimId,
            commonAliases: commonAliases)
    {
        if (string.IsNullOrWhiteSpace(primaryClaimedBenefit))
            throw new ArgumentException("Primary claimed benefit cannot be empty.", nameof(primaryClaimedBenefit));
        if (string.IsNullOrWhiteSpace(uncertaintyStatement))
            throw new ArgumentException("Uncertainty statement is mandatory and cannot be empty for supplement knowledge.", nameof(uncertaintyStatement));

        PrimaryClaimedBenefit = primaryClaimedBenefit.Trim();
        EfficacyClaim = string.IsNullOrWhiteSpace(efficacyClaim) ? null : efficacyClaim.Trim();
        EvidenceStatus = evidenceStatus;
        EffectMagnitude = effectMagnitude;
        PopulationNote = string.IsNullOrWhiteSpace(populationNote) ? null : populationNote.Trim();
        UncertaintyStatement = uncertaintyStatement.Trim();
        TypicalDoseRangeMin = typicalDoseRangeMin;
        TypicalDoseRangeMax = typicalDoseRangeMax;
        DoseUnit = string.IsNullOrWhiteSpace(doseUnit) ? null : doseUnit.Trim();
        DoseSourceClaimId = doseSourceClaimId;
        TimingNote = string.IsNullOrWhiteSpace(timingNote) ? null : timingNote.Trim();
        CommonForms = string.IsNullOrWhiteSpace(commonForms) ? null : commonForms.Trim();
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
