using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Substances;

namespace AiCoachOs.Application.Substances.Dtos;

public class SubstanceSafetyFlagDto
{
    public string FlagType { get; set; } = string.Empty;
    public SubstanceEscalationLevel Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string EvidenceBasis { get; set; } = string.Empty;
}

public class EvidenceCitationDto
{
    public Guid ClaimId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string ClaimText { get; set; } = string.Empty;
    public EvidenceLevel EvidenceLevel { get; set; }
    public ClaimStatus Status { get; set; }
    public IReadOnlyList<string> SourceCitations { get; set; } = new List<string>();
}

public class SupplementKnowledgeSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SupplementCategory SupplementCategory { get; set; }
    public EvidenceLevel EvidenceLevel { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsEgyptianMarketAvailable { get; set; }
    public int SafetyFlagCount { get; set; }
    public DateTime? LastReviewedAtUtc { get; set; }
}

public class SupplementKnowledgeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SubstanceCategory Category { get; set; }
    public SupplementCategory SupplementCategory { get; set; }
    public EvidenceLevel EvidenceLevel { get; set; }
    public string Description { get; set; } = string.Empty;
    public string EvidenceSummary { get; set; } = string.Empty;
    public string UncertaintyStatement { get; set; } = string.Empty;
    public string? CommonForms { get; set; }
    public string? TypicalDoseRange { get; set; }
    public string? TimingRecommendation { get; set; }
    public string? InteractionsAndNotes { get; set; }
    public bool IsEgyptianMarketAvailable { get; set; }
    public DateTime? LastReviewedAtUtc { get; set; }
    public string? ReviewedBy { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<SubstanceSafetyFlagDto> SafetyFlags { get; set; } = new List<SubstanceSafetyFlagDto>();
    public EvidenceCitationDto? EvidenceClaim { get; set; }
}

public class HormoneKnowledgeSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public HormoneAxis HormoneAxis { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? LastReviewedAtUtc { get; set; }
}

public class HormoneKnowledgeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SubstanceCategory Category { get; set; }
    public HormoneAxis HormoneAxis { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PhysiologicalRole { get; set; } = string.Empty;
    public string TrainingImpactSummary { get; set; } = string.Empty;
    public string EvidenceSummary { get; set; } = string.Empty;
    public string UncertaintyStatement { get; set; } = string.Empty;
    public string? BiomarkerReferenceNotes { get; set; }
    public DateTime? LastReviewedAtUtc { get; set; }
    public string? ReviewedBy { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<SubstanceSafetyFlagDto> SafetyFlags { get; set; } = new List<SubstanceSafetyFlagDto>();
    public EvidenceCitationDto? EvidenceClaim { get; set; }
}

public class PEDRiskRecordDto
{
    public Guid Id { get; set; }
    public OrganSystem OrganSystem { get; set; }
    public PEDRiskSeverity Severity { get; set; }
    public string RiskDescription { get; set; } = string.Empty;
    public string? ReversibilityNotes { get; set; }
    public Guid? KnowledgeClaimId { get; set; }
}

public class PEDSafetyRecordSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PEDCategory PEDCategory { get; set; }
    public string Description { get; set; } = string.Empty;
    public int RiskCount { get; set; }
    public DateTime? LastReviewedAtUtc { get; set; }
}

public class PEDSafetyRecordDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SubstanceCategory Category { get; set; }
    public PEDCategory PEDCategory { get; set; }
    public string Description { get; set; } = string.Empty;
    public string MechanismSummary { get; set; } = string.Empty;
    public string HealthRisksSummary { get; set; } = string.Empty;
    public string EvidenceSummary { get; set; } = string.Empty;
    public string SafetyDisclaimer { get; set; } = string.Empty;
    public DateTime? LastReviewedAtUtc { get; set; }
    public string? ReviewedBy { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<PEDRiskRecordDto> Risks { get; set; } = new List<PEDRiskRecordDto>();
    public IReadOnlyList<SubstanceSafetyFlagDto> SafetyFlags { get; set; } = new List<SubstanceSafetyFlagDto>();
    public EvidenceCitationDto? EvidenceClaim { get; set; }
}

public class PEDRedFlagRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SignalPattern { get; set; } = string.Empty;
    public SubstanceEscalationLevel EscalationLevel { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public string EvidenceBasis { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class EvaluateSubstanceSafetyRequestDto
{
    public Guid? SubstanceRecordId { get; set; }
    public List<string> ReportedSignals { get; set; } = new();
    public string? ContextNotes { get; set; }
}

public class SubstanceSafetyEvaluationResultDto
{
    public Guid? EscalationRecordId { get; set; }
    public SubstanceEscalationLevel EscalationLevel { get; set; }
    public string SummaryRationale { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public string Disclaimer { get; set; } = string.Empty;
    public IReadOnlyList<string> MatchedRedFlags { get; set; } = new List<string>();
    public IReadOnlyList<string> ReportedSignals { get; set; } = new List<string>();
    public DateTime CreatedAtUtc { get; set; }
}

public class SubstanceEscalationRecordDto
{
    public Guid Id { get; set; }
    public Guid CoachId { get; set; }
    public Guid? SubstanceRecordId { get; set; }
    public string? SubstanceName { get; set; }
    public SubstanceEscalationLevel EscalationLevel { get; set; }
    public string SummaryRationale { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public string Disclaimer { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyList<string> ReportedSignals { get; set; } = new List<string>();
    public IReadOnlyList<string> MatchedRedFlags { get; set; } = new List<string>();
}
