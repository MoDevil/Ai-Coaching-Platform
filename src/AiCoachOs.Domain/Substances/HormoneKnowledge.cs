namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents structured, evidence-based educational knowledge on endocrine and hormonal regulation in training context.
/// Enforces mandatory uncertainty statements and educational boundaries.
/// </summary>
public class HormoneKnowledge : SubstanceRecord
{
    public HormoneAxis HormoneAxis { get; private set; }
    public string PhysiologicalRole { get; private set; } = string.Empty;
    public string TrainingImpactSummary { get; private set; } = string.Empty;
    public string UncertaintyStatement { get; private set; } = string.Empty;
    public string? BiomarkerReferenceNotes { get; private set; }

    private HormoneKnowledge() { } // EF Core

    public HormoneKnowledge(
        Guid id,
        string name,
        HormoneAxis hormoneAxis,
        string description,
        string physiologicalRole,
        string trainingImpactSummary,
        string evidenceSummary,
        string uncertaintyStatement,
        Guid? primaryKnowledgeClaimId = null,
        string? biomarkerReferenceNotes = null,
        DateTime? lastReviewedAtUtc = null,
        string? reviewedBy = null,
        bool isActive = true)
        : base(
            id: id,
            name: name,
            category: SubstanceCategory.Hormone,
            description: description,
            evidenceSummary: evidenceSummary,
            primaryKnowledgeClaimId: primaryKnowledgeClaimId,
            lastReviewedAtUtc: lastReviewedAtUtc,
            reviewedBy: reviewedBy,
            isActive: isActive)
    {
        if (string.IsNullOrWhiteSpace(physiologicalRole))
            throw new ArgumentException("Physiological role cannot be empty.", nameof(physiologicalRole));
        if (string.IsNullOrWhiteSpace(trainingImpactSummary))
            throw new ArgumentException("Training impact summary cannot be empty.", nameof(trainingImpactSummary));
        if (string.IsNullOrWhiteSpace(uncertaintyStatement))
            throw new ArgumentException("Uncertainty statement is mandatory and cannot be empty for hormone knowledge.", nameof(uncertaintyStatement));

        HormoneAxis = hormoneAxis;
        PhysiologicalRole = physiologicalRole.Trim();
        TrainingImpactSummary = trainingImpactSummary.Trim();
        UncertaintyStatement = uncertaintyStatement.Trim();
        BiomarkerReferenceNotes = string.IsNullOrWhiteSpace(biomarkerReferenceNotes) ? null : biomarkerReferenceNotes.Trim();
    }

    public void UpdateUncertaintyStatement(string uncertaintyStatement)
    {
        if (string.IsNullOrWhiteSpace(uncertaintyStatement))
            throw new ArgumentException("Uncertainty statement cannot be empty.", nameof(uncertaintyStatement));

        UncertaintyStatement = uncertaintyStatement.Trim();
        MarkUpdated();
    }
}
