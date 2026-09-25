using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents structured educational and harm-reduction knowledge regarding Performance Enhancing Drugs (PEDs).
/// STRICT SAFETY BOUNDARY: This entity does NOT contain dosage instructions, cycle design, stacking advice,
/// PCT protocols, sourcing, or masking information. It exists strictly for coach health awareness,
/// risk education, and medical escalation triage.
/// Locked contract: PEDCategory, MechanismSummary, DocumentedRisks, SafetyFlags, MonitoringConcepts, SafetyDisclaimer.
/// </summary>
public class PEDSafetyRecord : SubstanceRecord
{
    private readonly List<PEDRiskRecord> _documentedRisks = new();
    private readonly List<string> _monitoringConcepts = new();

    public PEDCategory PEDCategory { get; private set; }
    public string MechanismSummary { get; private set; } = string.Empty;
    public string SafetyDisclaimer { get; private set; } = "EDUCATIONAL & HARM REDUCTION ONLY: AI Coach OS strictly prohibits prescribing, cycle planning, dosing, sourcing, or facilitating the use of PEDs. Any client exhibiting adverse symptoms or red-flag signs must be immediately escalated or referred to qualified medical professionals.";

    public IReadOnlyCollection<PEDRiskRecord> DocumentedRisks => _documentedRisks;
    public IReadOnlyCollection<string> MonitoringConcepts => _monitoringConcepts;

    private PEDSafetyRecord() { } // EF Core

    public PEDSafetyRecord(
        Guid id,
        string name,
        PEDCategory pedCategory,
        string description,
        string mechanismSummary,
        string? safetyDisclaimer = null,
        Guid? primaryKnowledgeClaimId = null,
        IEnumerable<string>? monitoringConcepts = null,
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
            substanceCategory: SubstanceCategory.PED,
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
        if (string.IsNullOrWhiteSpace(mechanismSummary))
            throw new ArgumentException("Mechanism summary cannot be empty.", nameof(mechanismSummary));

        PEDCategory = pedCategory;
        MechanismSummary = mechanismSummary.Trim();
        if (!string.IsNullOrWhiteSpace(safetyDisclaimer))
            SafetyDisclaimer = safetyDisclaimer.Trim();

        if (monitoringConcepts != null)
        {
            foreach (var concept in monitoringConcepts)
            {
                if (!string.IsNullOrWhiteSpace(concept))
                    _monitoringConcepts.Add(concept.Trim());
            }
        }
    }

    public void AddRisk(PEDRiskRecord risk)
    {
        ArgumentNullException.ThrowIfNull(risk);
        _documentedRisks.Add(risk);
        MarkUpdated();
    }

    public void ClearRisks()
    {
        _documentedRisks.Clear();
        MarkUpdated();
    }

    public void AddMonitoringConcept(string concept)
    {
        if (!string.IsNullOrWhiteSpace(concept) && !_monitoringConcepts.Contains(concept.Trim()))
        {
            _monitoringConcepts.Add(concept.Trim());
            MarkUpdated();
        }
    }
}
