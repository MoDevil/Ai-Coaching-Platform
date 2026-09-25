using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents a structured organ-system health risk associated with a PED class or compound.
/// Used strictly for educational awareness and safety screening.
/// </summary>
public class PEDRiskRecord : Entity<Guid>
{
    public Guid PEDSafetyRecordId { get; private set; }
    public PEDSafetyRecord? PEDSafetyRecord { get; private set; }

    public OrganSystem OrganSystem { get; private set; }
    public PEDRiskSeverity Severity { get; private set; }
    public string RiskDescription { get; private set; } = string.Empty;
    public string? ReversibilityNotes { get; private set; }

    public Guid? KnowledgeClaimId { get; private set; }
    public KnowledgeClaim? KnowledgeClaim { get; private set; }

    private PEDRiskRecord() { } // EF Core

    public PEDRiskRecord(
        Guid id,
        Guid pedSafetyRecordId,
        OrganSystem organSystem,
        PEDRiskSeverity severity,
        string riskDescription,
        string? reversibilityNotes = null,
        Guid? knowledgeClaimId = null) : base(id)
    {
        if (pedSafetyRecordId == Guid.Empty)
            throw new ArgumentException("PEDSafetyRecordId cannot be empty.", nameof(pedSafetyRecordId));
        if (string.IsNullOrWhiteSpace(riskDescription))
            throw new ArgumentException("Risk description cannot be empty.", nameof(riskDescription));

        PEDSafetyRecordId = pedSafetyRecordId;
        OrganSystem = organSystem;
        Severity = severity;
        RiskDescription = riskDescription.Trim();
        ReversibilityNotes = string.IsNullOrWhiteSpace(reversibilityNotes) ? null : reversibilityNotes.Trim();
        KnowledgeClaimId = knowledgeClaimId;
    }
}
