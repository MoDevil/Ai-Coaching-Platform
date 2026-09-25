using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents a structured organ-system health risk associated with a PED class or compound.
/// Used strictly for educational awareness and safety screening.
/// Locked contract: RiskCategory, Description, EvidenceClaimId, EvidenceLevel, Severity, ReversibilityNotes.
/// </summary>
public class PEDRiskRecord : Entity<Guid>
{
    public Guid PEDSafetyRecordId { get; private set; }
    public PEDSafetyRecord? PEDSafetyRecord { get; private set; }

    public RiskCategory RiskCategory { get; private set; }
    public PEDRiskSeverity Severity { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ReversibilityNotes { get; private set; }

    public Guid? EvidenceClaimId { get; private set; }
    public KnowledgeClaim? EvidenceClaim { get; private set; }
    public EvidenceLevel EvidenceLevel { get; private set; }

    private PEDRiskRecord() { } // EF Core

    public PEDRiskRecord(
        Guid id,
        Guid pedSafetyRecordId,
        RiskCategory riskCategory,
        PEDRiskSeverity severity,
        string description,
        EvidenceLevel evidenceLevel = EvidenceLevel.ClinicalGuideline,
        string? reversibilityNotes = null,
        Guid? evidenceClaimId = null) : base(id)
    {
        if (pedSafetyRecordId == Guid.Empty)
            throw new ArgumentException("PEDSafetyRecordId cannot be empty.", nameof(pedSafetyRecordId));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Risk description cannot be empty.", nameof(description));

        PEDSafetyRecordId = pedSafetyRecordId;
        RiskCategory = riskCategory;
        Severity = severity;
        Description = description.Trim();
        EvidenceLevel = evidenceLevel;
        ReversibilityNotes = string.IsNullOrWhiteSpace(reversibilityNotes) ? null : reversibilityNotes.Trim();
        EvidenceClaimId = evidenceClaimId;
    }
}
