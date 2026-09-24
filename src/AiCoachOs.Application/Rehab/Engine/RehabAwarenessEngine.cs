using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Rehab;
using AiCoachOs.Domain.Safety;

namespace AiCoachOs.Application.Rehab.Engine;

public class RehabAwarenessEngine : IRehabAwarenessEngine
{
    public IReadOnlyList<RehabAwarenessConsideration> GenerateConsiderations(
        TrainingLimitation limitation,
        SafetyScreening? safetyScreening,
        Exercise? exercise,
        IReadOnlyList<ExerciseSubstitution> substitutions,
        IReadOnlyList<KnowledgeClaim> knowledgeClaims,
        DateTime generatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(limitation);

        // 1. Hard Gating Rules
        if (limitation.Status != LimitationStatus.Active)
        {
            return Array.Empty<RehabAwarenessConsideration>();
        }

        if (safetyScreening != null)
        {
            // UrgentMedicalAttention: M9 completely blocked. Zero output.
            if (safetyScreening.ScreeningResult == SafetyCategory.UrgentMedicalAttention)
            {
                return Array.Empty<RehabAwarenessConsideration>();
            }

            // ReferToHealthcareProfessional: Blocked unless explicitly activated by coach
            if (safetyScreening.ScreeningResult == SafetyCategory.ReferToHealthcareProfessional)
            {
                if (limitation.CoachActivatedM9AtUtc == null || string.IsNullOrWhiteSpace(limitation.CoachActivationNote))
                {
                    return Array.Empty<RehabAwarenessConsideration>();
                }
            }

            // CautionCoachReview: Blocked until M8 coach acknowledgment exists
            if (safetyScreening.ScreeningResult == SafetyCategory.CautionCoachReview)
            {
                if (safetyScreening.CoachAcknowledgedAtUtc == null)
                {
                    return Array.Empty<RehabAwarenessConsideration>();
                }
            }
        }

        var results = new List<RehabAwarenessConsideration>();
        var region = limitation.AffectedBodyRegion;

        // Try to find relevant KnowledgeClaim for linking
        KnowledgeClaim? matchingClaim = null;
        if (knowledgeClaims != null && knowledgeClaims.Count > 0)
        {
            matchingClaim = knowledgeClaims.FirstOrDefault(c =>
                c.Status == ClaimStatus.Active &&
                (c.Topic.Contains(region, StringComparison.OrdinalIgnoreCase) ||
                 c.ClaimText.Contains(region, StringComparison.OrdinalIgnoreCase) ||
                 c.PracticalApplication?.Contains(region, StringComparison.OrdinalIgnoreCase) == true ||
                 (exercise != null && c.ExerciseId == exercise.Id)));

            if (matchingClaim == null)
            {
                matchingClaim = knowledgeClaims.FirstOrDefault(c =>
                    c.Status == ClaimStatus.Active &&
                    (c.EvidenceLevel == EvidenceLevel.ClinicalGuideline ||
                     c.EvidenceLevel == EvidenceLevel.ExpertConsensus));
            }
        }

        Guid? claimId = matchingClaim?.Id;
        string? evidenceBasis = matchingClaim != null
            ? $"{matchingClaim.EvidenceLevel}: {matchingClaim.ClaimText}"
            : null;

        // 1. Reduce Load
        results.Add(new RehabAwarenessConsideration(
            id: Guid.NewGuid(),
            trainingLimitationId: limitation.Id,
            considerationType: ConsiderationType.ReduceLoad,
            considerationText: $"Consider reducing working load on movements loading the {region} to remain well within tolerable limits.",
            generatedAtUtc: generatedAtUtc,
            exerciseId: exercise?.Id,
            knowledgeClaimId: claimId,
            evidenceBasis: evidenceBasis));

        // 2. Reduce ROM
        results.Add(new RehabAwarenessConsideration(
            id: Guid.NewGuid(),
            trainingLimitationId: limitation.Id,
            considerationType: ConsiderationType.ReduceROM,
            considerationText: $"Consider modifying or reducing the active range of motion to avoid symptom-provoking joint angles for the {region}.",
            generatedAtUtc: generatedAtUtc,
            exerciseId: exercise?.Id,
            knowledgeClaimId: claimId,
            evidenceBasis: evidenceBasis));

        // 3. Reduce Proximity to Failure
        results.Add(new RehabAwarenessConsideration(
            id: Guid.NewGuid(),
            trainingLimitationId: limitation.Id,
            considerationType: ConsiderationType.ReduceProximityToFailure,
            considerationText: $"Consider maintaining greater proximity to failure (e.g. 3-4 RIR) on movements engaging the {region} to prevent technical breakdown.",
            generatedAtUtc: generatedAtUtc,
            exerciseId: exercise?.Id,
            knowledgeClaimId: claimId,
            evidenceBasis: evidenceBasis));

        // 4. Reduce Sets
        results.Add(new RehabAwarenessConsideration(
            id: Guid.NewGuid(),
            trainingLimitationId: limitation.Id,
            considerationType: ConsiderationType.ReduceSets,
            considerationText: $"Consider reducing total weekly working sets directly loading the {region} to manage localized fatigue.",
            generatedAtUtc: generatedAtUtc,
            exerciseId: exercise?.Id,
            knowledgeClaimId: claimId,
            evidenceBasis: evidenceBasis));

        // 5. Reduce Frequency On Region
        results.Add(new RehabAwarenessConsideration(
            id: Guid.NewGuid(),
            trainingLimitationId: limitation.Id,
            considerationType: ConsiderationType.ReduceFrequencyOnRegion,
            considerationText: $"Consider reducing weekly training frequency on the {region} to allow sufficient recovery between loading exposures.",
            generatedAtUtc: generatedAtUtc,
            exerciseId: exercise?.Id,
            knowledgeClaimId: claimId,
            evidenceBasis: evidenceBasis));

        // 6. Increase Rest
        results.Add(new RehabAwarenessConsideration(
            id: Guid.NewGuid(),
            trainingLimitationId: limitation.Id,
            considerationType: ConsiderationType.IncreaseRest,
            considerationText: $"Consider extending rest intervals between sets on movements loading the {region} to manage intra-session fatigue accumulation.",
            generatedAtUtc: generatedAtUtc,
            exerciseId: exercise?.Id,
            knowledgeClaimId: claimId,
            evidenceBasis: evidenceBasis));

        // 7. Modify Tempo
        results.Add(new RehabAwarenessConsideration(
            id: Guid.NewGuid(),
            trainingLimitationId: limitation.Id,
            considerationType: ConsiderationType.ModifyTempo,
            considerationText: $"Consider utilizing a controlled tempo with a deliberate eccentric phase on exercises engaging the {region}.",
            generatedAtUtc: generatedAtUtc,
            exerciseId: exercise?.Id,
            knowledgeClaimId: claimId,
            evidenceBasis: evidenceBasis));

        // 8. Technique Setup Modification
        string setupText = exercise != null
            ? $"Consider modifying technical setup, grip, or stance on {exercise.Name} to minimize strain on the {region}."
            : $"Consider adjusting equipment setup, stance, or grip width to optimize joint alignment and comfort around the {region}.";

        results.Add(new RehabAwarenessConsideration(
            id: Guid.NewGuid(),
            trainingLimitationId: limitation.Id,
            considerationType: ConsiderationType.TechniqueSetupModification,
            considerationText: setupText,
            generatedAtUtc: generatedAtUtc,
            exerciseId: exercise?.Id,
            knowledgeClaimId: claimId,
            evidenceBasis: evidenceBasis));

        // 9. Temporary Exercise Exclusion (if exercise is provided)
        // 10. Alternative Exercise (if exercise is provided)
        if (exercise != null)
        {
            results.Add(new RehabAwarenessConsideration(
                id: Guid.NewGuid(),
                trainingLimitationId: limitation.Id,
                considerationType: ConsiderationType.TemporaryExerciseExclusion,
                considerationText: $"Consider temporarily pausing {exercise.Name} until tolerance in the {region} improves.",
                generatedAtUtc: generatedAtUtc,
                exerciseId: exercise.Id,
                knowledgeClaimId: claimId,
                evidenceBasis: evidenceBasis));

            string subText;
            if (substitutions != null && substitutions.Count > 0)
            {
                var names = string.Join(", ", substitutions.Select(s => s.SubstituteExercise?.Name ?? "alternative movement").Distinct());
                subText = $"Consider substituting {exercise.Name} with an alternative movement that reduces stress on the {region} ({names}).";
            }
            else
            {
                subText = $"Consider substituting {exercise.Name} with a machine-supported or stable alternative that unloads the {region}.";
            }

            results.Add(new RehabAwarenessConsideration(
                id: Guid.NewGuid(),
                trainingLimitationId: limitation.Id,
                considerationType: ConsiderationType.AlternativeExercise,
                considerationText: subText,
                generatedAtUtc: generatedAtUtc,
                exerciseId: exercise.Id,
                knowledgeClaimId: claimId,
                evidenceBasis: evidenceBasis));
        }

        // 11. Graded Loading Consideration (General conservative principle, no prescription/percentage jumps)
        results.Add(new RehabAwarenessConsideration(
            id: Guid.NewGuid(),
            trainingLimitationId: limitation.Id,
            considerationType: ConsiderationType.GradedLoadingConsideration,
            considerationText: "When symptoms stabilize, re-introduce loading gradually following a conservative progression based on client tolerance.",
            generatedAtUtc: generatedAtUtc,
            exerciseId: exercise?.Id,
            knowledgeClaimId: claimId,
            evidenceBasis: evidenceBasis));

        return results;
    }
}
