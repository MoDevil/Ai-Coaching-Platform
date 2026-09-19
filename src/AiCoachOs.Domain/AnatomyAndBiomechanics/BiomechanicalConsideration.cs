using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.AnatomyAndBiomechanics;

/// <summary>
/// Structured biomechanical consideration or setup variable for an exercise.
/// Captures mechanical context (e.g., moment arm changes, resistance direction, torso angle effects)
/// while distinguishing established principles from reasonable inference and uncertain hypotheses.
/// Links directly to M3 KnowledgeClaim where evidence exists.
/// </summary>
public class BiomechanicalConsideration : Entity<Guid>
{
    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;

    public BiomechanicalAspect Aspect { get; private set; }
    public CertaintyLevel Certainty { get; private set; }

    public string Summary { get; private set; } = null!;
    public string Explanation { get; private set; } = null!;
    public string? PracticalCues { get; private set; }

    /// <summary>
    /// Optional foreign key to an evidence-backed M3 Knowledge Claim.
    /// Preserves full scientific traceability without creating a second citation system.
    /// </summary>
    public Guid? KnowledgeClaimId { get; private set; }
    public KnowledgeClaim? KnowledgeClaim { get; private set; }

    private BiomechanicalConsideration() { } // EF Core

    public BiomechanicalConsideration(
        Guid id,
        Guid exerciseId,
        BiomechanicalAspect aspect,
        CertaintyLevel certainty,
        string summary,
        string explanation,
        string? practicalCues = null,
        Guid? knowledgeClaimId = null) : base(id)
    {
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Biomechanical consideration summary cannot be empty.", nameof(summary));
        if (string.IsNullOrWhiteSpace(explanation))
            throw new ArgumentException("Biomechanical consideration explanation cannot be empty.", nameof(explanation));

        ExerciseId = exerciseId;
        Aspect = aspect;
        Certainty = certainty;
        Summary = summary.Trim();
        Explanation = explanation.Trim();
        PracticalCues = practicalCues?.Trim();
        KnowledgeClaimId = knowledgeClaimId;
    }

    public void LinkKnowledgeClaim(Guid claimId)
    {
        if (claimId == Guid.Empty)
            throw new ArgumentException("KnowledgeClaimId cannot be empty.", nameof(claimId));

        KnowledgeClaimId = claimId;
        MarkUpdated();
    }
}
