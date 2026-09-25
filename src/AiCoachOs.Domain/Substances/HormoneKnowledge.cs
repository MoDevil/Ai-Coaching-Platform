using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents structured, evidence-based educational knowledge on endocrine and hormonal regulation in training context.
/// Locked contract: HormoneCategory, PhysiologicalRole, TrainingRelevance, EvidenceClaimIds, UncertaintyStatement,
/// MedicalEvaluationTriggers, SafetyFlags.
/// Enforces mandatory non-empty uncertainty statements and educational boundaries.
/// </summary>
public class HormoneKnowledge : SubstanceRecord
{
    private readonly List<Guid> _evidenceClaimIds = new();
    private readonly List<string> _medicalEvaluationTriggers = new();

    public HormoneCategory HormoneCategory { get; private set; }
    public string PhysiologicalRole { get; private set; } = string.Empty;
    public string TrainingRelevance { get; private set; } = string.Empty;
    public string UncertaintyStatement { get; private set; } = string.Empty;
    public string? BiomarkerReferenceNotes { get; private set; }

    public IReadOnlyCollection<Guid> EvidenceClaimIds => _evidenceClaimIds;
    public IReadOnlyCollection<string> MedicalEvaluationTriggers => _medicalEvaluationTriggers;

    private HormoneKnowledge() { } // EF Core

    public HormoneKnowledge(
        Guid id,
        string name,
        HormoneCategory hormoneCategory,
        string description,
        string physiologicalRole,
        string trainingRelevance,
        string uncertaintyStatement,
        Guid? primaryKnowledgeClaimId = null,
        IEnumerable<Guid>? evidenceClaimIds = null,
        IEnumerable<string>? medicalEvaluationTriggers = null,
        string? biomarkerReferenceNotes = null,
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
            substanceCategory: SubstanceCategory.Hormone,
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
        if (string.IsNullOrWhiteSpace(physiologicalRole))
            throw new ArgumentException("Physiological role cannot be empty.", nameof(physiologicalRole));
        if (string.IsNullOrWhiteSpace(trainingRelevance))
            throw new ArgumentException("Training relevance cannot be empty.", nameof(trainingRelevance));
        if (string.IsNullOrWhiteSpace(uncertaintyStatement))
            throw new ArgumentException("Uncertainty statement is mandatory and cannot be empty for hormone knowledge.", nameof(uncertaintyStatement));

        HormoneCategory = hormoneCategory;
        PhysiologicalRole = physiologicalRole.Trim();
        TrainingRelevance = trainingRelevance.Trim();
        UncertaintyStatement = uncertaintyStatement.Trim();
        BiomarkerReferenceNotes = string.IsNullOrWhiteSpace(biomarkerReferenceNotes) ? null : biomarkerReferenceNotes.Trim();

        if (evidenceClaimIds != null)
        {
            foreach (var claimId in evidenceClaimIds)
            {
                if (claimId != Guid.Empty && !_evidenceClaimIds.Contains(claimId))
                    _evidenceClaimIds.Add(claimId);
            }
        }

        if (medicalEvaluationTriggers != null)
        {
            foreach (var trigger in medicalEvaluationTriggers)
            {
                if (!string.IsNullOrWhiteSpace(trigger))
                    _medicalEvaluationTriggers.Add(trigger.Trim());
            }
        }
    }

    public void AddMedicalEvaluationTrigger(string trigger)
    {
        if (!string.IsNullOrWhiteSpace(trigger) && !_medicalEvaluationTriggers.Contains(trigger.Trim()))
        {
            _medicalEvaluationTriggers.Add(trigger.Trim());
            MarkUpdated();
        }
    }

    public void AddEvidenceClaimId(Guid claimId)
    {
        if (claimId != Guid.Empty && !_evidenceClaimIds.Contains(claimId))
        {
            _evidenceClaimIds.Add(claimId);
            MarkUpdated();
        }
    }

    public void UpdateUncertaintyStatement(string uncertaintyStatement)
    {
        if (string.IsNullOrWhiteSpace(uncertaintyStatement))
            throw new ArgumentException("Uncertainty statement cannot be empty.", nameof(uncertaintyStatement));

        UncertaintyStatement = uncertaintyStatement.Trim();
        MarkUpdated();
    }
}
