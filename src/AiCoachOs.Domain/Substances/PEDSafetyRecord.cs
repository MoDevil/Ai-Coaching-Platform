namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents structured educational and harm-reduction knowledge regarding Performance Enhancing Drugs (PEDs).
/// STRICT SAFETY BOUNDARY: This entity does NOT contain dosage instructions, cycle design, stacking advice,
/// PCT protocols, sourcing, or masking information. It exists strictly for coach health awareness,
/// risk education, and medical escalation triage.
/// </summary>
public class PEDSafetyRecord : SubstanceRecord
{
    private readonly List<PEDRiskRecord> _risks = new();

    public PEDCategory PEDCategory { get; private set; }
    public string MechanismSummary { get; private set; } = string.Empty;
    public string HealthRisksSummary { get; private set; } = string.Empty;
    public string SafetyDisclaimer { get; private set; } = "EDUCATIONAL & HARM REDUCTION ONLY: AI Coach OS strictly prohibits prescribing, cycle planning, dosing, sourcing, or facilitating the use of PEDs. Any client exhibiting adverse symptoms or red-flag signs must be immediately escalated or referred to qualified medical professionals.";

    public IReadOnlyCollection<PEDRiskRecord> Risks => _risks;

    private PEDSafetyRecord() { } // EF Core

    public PEDSafetyRecord(
        Guid id,
        string name,
        PEDCategory pedCategory,
        string description,
        string mechanismSummary,
        string healthRisksSummary,
        string evidenceSummary,
        Guid? primaryKnowledgeClaimId = null,
        DateTime? lastReviewedAtUtc = null,
        string? reviewedBy = null,
        bool isActive = true)
        : base(
            id: id,
            name: name,
            category: SubstanceCategory.PED,
            description: description,
            evidenceSummary: evidenceSummary,
            primaryKnowledgeClaimId: primaryKnowledgeClaimId,
            lastReviewedAtUtc: lastReviewedAtUtc,
            reviewedBy: reviewedBy,
            isActive: isActive)
    {
        if (string.IsNullOrWhiteSpace(mechanismSummary))
            throw new ArgumentException("Mechanism summary cannot be empty.", nameof(mechanismSummary));
        if (string.IsNullOrWhiteSpace(healthRisksSummary))
            throw new ArgumentException("Health risks summary cannot be empty.", nameof(healthRisksSummary));

        PEDCategory = pedCategory;
        MechanismSummary = mechanismSummary.Trim();
        HealthRisksSummary = healthRisksSummary.Trim();
    }

    public void AddRisk(PEDRiskRecord risk)
    {
        ArgumentNullException.ThrowIfNull(risk);
        _risks.Add(risk);
        MarkUpdated();
    }

    public void ClearRisks()
    {
        _risks.Clear();
        MarkUpdated();
    }
}
