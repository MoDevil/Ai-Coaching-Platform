using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Substances;

namespace AiCoachOs.Application.Substances.Dtos;

public class SubstanceSafetyFlagDto
{
    public SafetyFlagCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AffectedPopulation { get; set; }
    public Guid? SourceClaimId { get; set; }
    public EscalationLevel EscalationLevel { get; set; }
    public string CoachNote { get; set; } = string.Empty;
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
    public IReadOnlyList<string> CommonAliases { get; set; } = new List<string>();
    public string PrimaryClaimedBenefit { get; set; } = string.Empty;
    public SupplementEvidenceStatus EvidenceStatus { get; set; }
    public EffectMagnitude EffectMagnitude { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsEgyptianMarketAvailable { get; set; }
    public bool IsProvisional { get; set; }
    public bool RequiresClinicalReview { get; set; }
    public int SafetyFlagCount { get; set; }
    public DateTime? LastReviewedAtUtc { get; set; }
    public DateTime? ReviewDueAtUtc { get; set; }
}

public class SupplementKnowledgeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<string> CommonAliases { get; set; } = new List<string>();
    public SubstanceCategory SubstanceCategory { get; set; }
    public string PrimaryClaimedBenefit { get; set; } = string.Empty;
    public string? EfficacyClaim { get; set; }
    public SupplementEvidenceStatus EvidenceStatus { get; set; }
    public EffectMagnitude EffectMagnitude { get; set; }
    public string? PopulationNote { get; set; }
    public string Description { get; set; } = string.Empty;
    public string UncertaintyStatement { get; set; } = string.Empty;
    public decimal? TypicalDoseRangeMin { get; set; }
    public decimal? TypicalDoseRangeMax { get; set; }
    public string? DoseUnit { get; set; }
    public Guid? DoseSourceClaimId { get; set; }
    public string? TimingNote { get; set; }
    public string? CommonForms { get; set; }
    public string? InteractionsAndNotes { get; set; }
    public bool IsEgyptianMarketAvailable { get; set; }
    public bool IsProvisional { get; set; }
    public bool RequiresClinicalReview { get; set; }
    public ClaimStatus ClaimStatus { get; set; }
    public DateTime? LastReviewedAtUtc { get; set; }
    public DateTime? ReviewDueAtUtc { get; set; }
    public string? ReviewedBy { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<SubstanceSafetyFlagDto> SafetyFlags { get; set; } = new List<SubstanceSafetyFlagDto>();
    public EvidenceCitationDto? EvidenceClaim { get; set; }
}

public class HormoneKnowledgeSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<string> CommonAliases { get; set; } = new List<string>();
    public HormoneCategory HormoneCategory { get; set; }
    public string PhysiologicalRole { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsProvisional { get; set; }
    public bool RequiresClinicalReview { get; set; }
    public DateTime? LastReviewedAtUtc { get; set; }
    public DateTime? ReviewDueAtUtc { get; set; }
}

public class HormoneKnowledgeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<string> CommonAliases { get; set; } = new List<string>();
    public SubstanceCategory SubstanceCategory { get; set; }
    public HormoneCategory HormoneCategory { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PhysiologicalRole { get; set; } = string.Empty;
    public string TrainingRelevance { get; set; } = string.Empty;
    public string UncertaintyStatement { get; set; } = string.Empty;
    public string? BiomarkerReferenceNotes { get; set; }
    public IReadOnlyList<Guid> EvidenceClaimIds { get; set; } = new List<Guid>();
    public IReadOnlyList<string> MedicalEvaluationTriggers { get; set; } = new List<string>();
    public bool IsProvisional { get; set; }
    public bool RequiresClinicalReview { get; set; }
    public ClaimStatus ClaimStatus { get; set; }
    public DateTime? LastReviewedAtUtc { get; set; }
    public DateTime? ReviewDueAtUtc { get; set; }
    public string? ReviewedBy { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<SubstanceSafetyFlagDto> SafetyFlags { get; set; } = new List<SubstanceSafetyFlagDto>();
    public EvidenceCitationDto? EvidenceClaim { get; set; }
}

public class PEDRiskRecordDto
{
    public Guid Id { get; set; }
    public Guid PEDSafetyRecordId { get; set; }
    public RiskCategory RiskCategory { get; set; }
    public PEDRiskSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public EvidenceLevel EvidenceLevel { get; set; }
    public string? ReversibilityNotes { get; set; }
    public Guid? EvidenceClaimId { get; set; }
}

public class PEDSafetyRecordSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<string> CommonAliases { get; set; } = new List<string>();
    public PEDCategory PEDCategory { get; set; }
    public string Description { get; set; } = string.Empty;
    public int RiskCount { get; set; }
    public bool IsProvisional { get; set; }
    public bool RequiresClinicalReview { get; set; }
    public DateTime? LastReviewedAtUtc { get; set; }
    public DateTime? ReviewDueAtUtc { get; set; }
}

public class PEDSafetyRecordDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<string> CommonAliases { get; set; } = new List<string>();
    public SubstanceCategory SubstanceCategory { get; set; }
    public PEDCategory PEDCategory { get; set; }
    public string Description { get; set; } = string.Empty;
    public string MechanismSummary { get; set; } = string.Empty;
    public string SafetyDisclaimer { get; set; } = string.Empty;
    public IReadOnlyList<string> MonitoringConcepts { get; set; } = new List<string>();
    public bool IsProvisional { get; set; }
    public bool RequiresClinicalReview { get; set; }
    public ClaimStatus ClaimStatus { get; set; }
    public DateTime? LastReviewedAtUtc { get; set; }
    public DateTime? ReviewDueAtUtc { get; set; }
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
    public PEDCategory? PEDCategory { get; set; }
    public string Description { get; set; } = string.Empty;
    public string SignalPattern { get; set; } = string.Empty;
    public EscalationLevel EscalationLevel { get; set; }
    public bool RequiresClinicalReview { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public string EvidenceBasis { get; set; } = string.Empty;
    public Guid? SourceClaimId { get; set; }
    public bool IsActive { get; set; }
}

public class EvaluateSubstanceSafetyRequestDto
{
    public Guid? SubstanceRecordId { get; set; }
    public List<string> ReportedSignals { get; set; } = new();
    public string? CoachNote { get; set; }
}

public class SubstanceSafetyEvaluationResultDto
{
    public Guid? EscalationRecordId { get; set; }
    public EscalationLevel EscalationLevel { get; set; }
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
    public EscalationLevel EscalationLevel { get; set; }
    public string? CoachNote { get; set; }
    public string SummaryRationale { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public string Disclaimer { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyList<string> ReportedSignals { get; set; } = new List<string>();
    public IReadOnlyList<string> TriggeredFlagIds { get; set; } = new List<string>();
}
