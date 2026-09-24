using AiCoachOs.Application.Rehab.Engine;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Rehab;
using AiCoachOs.Domain.Safety;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class RehabAwarenessEngineTests
{
    private readonly RehabAwarenessEngine _engine = new();

    [Fact]
    public void GenerateConsiderations_WhenExerciseProvided_GeneratesAll11LockedConsiderationTypes()
    {
        var limitation = new TrainingLimitation(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            affectedBodyRegion: "Right Shoulder",
            limitationSource: LimitationSource.ReportedByClient,
            reportedAtUtc: DateTime.UtcNow);

        var exercise = new Exercise(
            id: Guid.NewGuid(),
            name: "Barbell Overhead Press",
            category: ExerciseCategory.Compound,
            movementPatternId: Guid.NewGuid(),
            stabilityRequirement: QualitativeRating.Moderate,
            technicalDemand: QualitativeRating.High,
            localFatigueCost: QualitativeRating.Moderate,
            systemicFatigueCost: QualitativeRating.High,
            stimulusPotential: QualitativeRating.High,
            progressionPotential: QualitativeRating.High,
            resistanceProfile: ResistanceProfile.Even);

        var considerations = _engine.GenerateConsiderations(
            limitation: limitation,
            safetyScreening: null,
            exercise: exercise,
            substitutions: Array.Empty<ExerciseSubstitution>(),
            knowledgeClaims: Array.Empty<KnowledgeClaim>(),
            generatedAtUtc: DateTime.UtcNow);

        considerations.Should().HaveCount(11);

        var types = considerations.Select(c => c.ConsiderationType).ToList();
        types.Should().Contain(ConsiderationType.ReduceLoad);
        types.Should().Contain(ConsiderationType.ReduceROM);
        types.Should().Contain(ConsiderationType.ReduceProximityToFailure);
        types.Should().Contain(ConsiderationType.ReduceSets);
        types.Should().Contain(ConsiderationType.ReduceFrequencyOnRegion);
        types.Should().Contain(ConsiderationType.IncreaseRest);
        types.Should().Contain(ConsiderationType.ModifyTempo);
        types.Should().Contain(ConsiderationType.TechniqueSetupModification);
        types.Should().Contain(ConsiderationType.TemporaryExerciseExclusion);
        types.Should().Contain(ConsiderationType.AlternativeExercise);
        types.Should().Contain(ConsiderationType.GradedLoadingConsideration);

        considerations.All(c => c.Status == ConsiderationStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public void GenerateConsiderations_ExplicitlyVerifiesFourTargetedTypes()
    {
        var limitation = new TrainingLimitation(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            affectedBodyRegion: "Left Knee",
            limitationSource: LimitationSource.ReportedByClient,
            reportedAtUtc: DateTime.UtcNow);

        var considerations = _engine.GenerateConsiderations(
            limitation: limitation,
            safetyScreening: null,
            exercise: null,
            substitutions: Array.Empty<ExerciseSubstitution>(),
            knowledgeClaims: Array.Empty<KnowledgeClaim>(),
            generatedAtUtc: DateTime.UtcNow);

        // 1. ReduceFrequencyOnRegion
        var freq = considerations.FirstOrDefault(c => c.ConsiderationType == ConsiderationType.ReduceFrequencyOnRegion);
        freq.Should().NotBeNull();
        freq!.ConsiderationText.Should().Contain("frequency").And.Contain("Left Knee");

        // 2. IncreaseRest
        var rest = considerations.FirstOrDefault(c => c.ConsiderationType == ConsiderationType.IncreaseRest);
        rest.Should().NotBeNull();
        rest!.ConsiderationText.Should().Contain("rest intervals").And.Contain("Left Knee");

        // 3. ModifyTempo
        var tempo = considerations.FirstOrDefault(c => c.ConsiderationType == ConsiderationType.ModifyTempo);
        tempo.Should().NotBeNull();
        tempo!.ConsiderationText.Should().Contain("tempo").And.Contain("Left Knee");

        // 4. TechniqueSetupModification
        var tech = considerations.FirstOrDefault(c => c.ConsiderationType == ConsiderationType.TechniqueSetupModification);
        tech.Should().NotBeNull();
        tech!.ConsiderationText.Should().Contain("setup").And.Contain("Left Knee");
    }

    [Fact]
    public void GenerateConsiderations_WhenUrgentMedicalAttention_ReturnsZeroConsiderations()
    {
        var limitation = new TrainingLimitation(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            affectedBodyRegion: "Knee",
            limitationSource: LimitationSource.ReportedByClient,
            reportedAtUtc: DateTime.UtcNow);

        var urgentScreening = new SafetyScreening(
            id: Guid.NewGuid(),
            clientId: limitation.ClientId,
            triggeredByType: TriggeredByType.DirectReport,
            screeningResult: SafetyCategory.UrgentMedicalAttention,
            recommendedAction: SafetyActionType.UrgentMedicalAttention,
            summaryRationale: "Urgent concern detected.",
            generatedAtUtc: DateTime.UtcNow);

        var considerations = _engine.GenerateConsiderations(
            limitation: limitation,
            safetyScreening: urgentScreening,
            exercise: null,
            substitutions: Array.Empty<ExerciseSubstitution>(),
            knowledgeClaims: Array.Empty<KnowledgeClaim>(),
            generatedAtUtc: DateTime.UtcNow);

        considerations.Should().BeEmpty("M9 must be completely blocked under UrgentMedicalAttention");
    }

    [Fact]
    public void GenerateConsiderations_WhenReferToHealthcareProfessional_AndNotActivated_ReturnsZeroConsiderations()
    {
        var limitation = new TrainingLimitation(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            affectedBodyRegion: "Lumbar Spine",
            limitationSource: LimitationSource.PostReferral,
            reportedAtUtc: DateTime.UtcNow);

        var referScreening = new SafetyScreening(
            id: Guid.NewGuid(),
            clientId: limitation.ClientId,
            triggeredByType: TriggeredByType.DirectReport,
            screeningResult: SafetyCategory.ReferToHealthcareProfessional,
            recommendedAction: SafetyActionType.ReferToHealthcareProfessional,
            summaryRationale: "Referral required.",
            generatedAtUtc: DateTime.UtcNow);

        var considerations = _engine.GenerateConsiderations(
            limitation: limitation,
            safetyScreening: referScreening,
            exercise: null,
            substitutions: Array.Empty<ExerciseSubstitution>(),
            knowledgeClaims: Array.Empty<KnowledgeClaim>(),
            generatedAtUtc: DateTime.UtcNow);

        considerations.Should().BeEmpty("M9 is blocked on referral category without explicit coach activation note");
    }

    [Fact]
    public void GenerateConsiderations_WhenReferToHealthcareProfessional_AndActivatedByCoach_GeneratesConsiderations()
    {
        var limitation = new TrainingLimitation(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            affectedBodyRegion: "Lumbar Spine",
            limitationSource: LimitationSource.PostReferral,
            reportedAtUtc: DateTime.UtcNow);

        limitation.ActivateByCoach(DateTime.UtcNow, "Client cleared for gentle loading by physiotherapist on 2026-09-20.");

        var referScreening = new SafetyScreening(
            id: Guid.NewGuid(),
            clientId: limitation.ClientId,
            triggeredByType: TriggeredByType.DirectReport,
            screeningResult: SafetyCategory.ReferToHealthcareProfessional,
            recommendedAction: SafetyActionType.ReferToHealthcareProfessional,
            summaryRationale: "Referral required.",
            generatedAtUtc: DateTime.UtcNow);

        var considerations = _engine.GenerateConsiderations(
            limitation: limitation,
            safetyScreening: referScreening,
            exercise: null,
            substitutions: Array.Empty<ExerciseSubstitution>(),
            knowledgeClaims: Array.Empty<KnowledgeClaim>(),
            generatedAtUtc: DateTime.UtcNow);

        considerations.Should().NotBeEmpty();
        considerations.All(c => c.Status == ConsiderationStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public void GenerateConsiderations_WhenCautionCoachReview_AndNotAcknowledged_ReturnsZeroConsiderations()
    {
        var limitation = new TrainingLimitation(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            affectedBodyRegion: "Right Shoulder",
            limitationSource: LimitationSource.ReportedByClient,
            reportedAtUtc: DateTime.UtcNow);

        var cautionScreening = new SafetyScreening(
            id: Guid.NewGuid(),
            clientId: limitation.ClientId,
            triggeredByType: TriggeredByType.WorkoutSession,
            screeningResult: SafetyCategory.CautionCoachReview,
            recommendedAction: SafetyActionType.CoachReviewRequired,
            summaryRationale: "Persistent moderate shoulder discomfort.",
            generatedAtUtc: DateTime.UtcNow,
            requiresCoachAcknowledgment: true);

        var considerations = _engine.GenerateConsiderations(
            limitation: limitation,
            safetyScreening: cautionScreening,
            exercise: null,
            substitutions: Array.Empty<ExerciseSubstitution>(),
            knowledgeClaims: Array.Empty<KnowledgeClaim>(),
            generatedAtUtc: DateTime.UtcNow);

        considerations.Should().BeEmpty("M9 is blocked until M8 coach acknowledgment is recorded");
    }

    [Fact]
    public void GenerateConsiderations_WhenCautionCoachReview_AndAcknowledged_GeneratesConservativeConsiderations()
    {
        var limitation = new TrainingLimitation(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            affectedBodyRegion: "Right Shoulder",
            limitationSource: LimitationSource.ReportedByClient,
            reportedAtUtc: DateTime.UtcNow);

        var cautionScreening = new SafetyScreening(
            id: Guid.NewGuid(),
            clientId: limitation.ClientId,
            triggeredByType: TriggeredByType.WorkoutSession,
            screeningResult: SafetyCategory.CautionCoachReview,
            recommendedAction: SafetyActionType.CoachReviewRequired,
            summaryRationale: "Persistent moderate shoulder discomfort.",
            generatedAtUtc: DateTime.UtcNow,
            requiresCoachAcknowledgment: true);

        cautionScreening.AcknowledgeByCoach(DateTime.UtcNow, "Reviewed discomfort report.");

        var considerations = _engine.GenerateConsiderations(
            limitation: limitation,
            safetyScreening: cautionScreening,
            exercise: null,
            substitutions: Array.Empty<ExerciseSubstitution>(),
            knowledgeClaims: Array.Empty<KnowledgeClaim>(),
            generatedAtUtc: DateTime.UtcNow);

        considerations.Should().NotBeEmpty();
        considerations.Should().Contain(c => c.ConsiderationType == ConsiderationType.ReduceLoad);
        considerations.Should().Contain(c => c.ConsiderationType == ConsiderationType.ReduceROM);
        considerations.Should().Contain(c => c.ConsiderationType == ConsiderationType.GradedLoadingConsideration);
    }

    [Fact]
    public void GenerateConsiderations_IncludesFixedDisclaimer_AndNonDiagnosticWording()
    {
        var limitation = new TrainingLimitation(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            affectedBodyRegion: "Left Knee",
            limitationSource: LimitationSource.ReportedByClient,
            reportedAtUtc: DateTime.UtcNow);

        var considerations = _engine.GenerateConsiderations(
            limitation: limitation,
            safetyScreening: null,
            exercise: null,
            substitutions: Array.Empty<ExerciseSubstitution>(),
            knowledgeClaims: Array.Empty<KnowledgeClaim>(),
            generatedAtUtc: DateTime.UtcNow);

        foreach (var c in considerations)
        {
            c.Disclaimer.Should().Be("AI Coach OS does not diagnose medical conditions. This assessment is based on reported limitations only. Clinical evaluation is required for any health concern.");
            c.ConsiderationText.Should().NotContain("tear");
            c.ConsiderationText.Should().NotContain("tendinopathy");
            c.ConsiderationText.Should().NotContain("syndrome");
            c.ConsiderationText.Should().NotContain("prescribe");
        }
    }

    [Fact]
    public void GenerateConsiderations_WhenKnowledgeClaimMatches_LinksClaimAndEvidenceBasis()
    {
        var limitation = new TrainingLimitation(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            affectedBodyRegion: "Knee",
            limitationSource: LimitationSource.ReportedByClient,
            reportedAtUtc: DateTime.UtcNow);

        var claim = new KnowledgeClaim(
            id: Guid.NewGuid(),
            topic: "Knee Load Management",
            question: "How to manage knee discomfort?",
            claimText: "Conservative load modification allows tissue tolerance recovery.",
            evidenceLevel: EvidenceLevel.ClinicalGuideline,
            status: ClaimStatus.Active);

        var considerations = _engine.GenerateConsiderations(
            limitation: limitation,
            safetyScreening: null,
            exercise: null,
            substitutions: Array.Empty<ExerciseSubstitution>(),
            knowledgeClaims: new[] { claim },
            generatedAtUtc: DateTime.UtcNow);

        considerations.All(c => c.KnowledgeClaimId == claim.Id).Should().BeTrue();
        considerations.All(c => c.EvidenceBasis != null && c.EvidenceBasis.Contains("ClinicalGuideline")).Should().BeTrue();
    }

    [Fact]
    public void GenerateConsiderations_IsPureDeterministic_ProducesSameOutputAcrossMultipleInvocations()
    {
        var limitation = new TrainingLimitation(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            affectedBodyRegion: "Elbow",
            limitationSource: LimitationSource.ReportedByClient,
            reportedAtUtc: DateTime.UtcNow);

        var fixedTime = new DateTime(2026, 9, 22, 10, 0, 0, DateTimeKind.Utc);

        var run1 = _engine.GenerateConsiderations(limitation, null, null, Array.Empty<ExerciseSubstitution>(), Array.Empty<KnowledgeClaim>(), fixedTime);
        var run2 = _engine.GenerateConsiderations(limitation, null, null, Array.Empty<ExerciseSubstitution>(), Array.Empty<KnowledgeClaim>(), fixedTime);

        run1.Count.Should().Be(run2.Count);
        for (int i = 0; i < run1.Count; i++)
        {
            run1[i].ConsiderationType.Should().Be(run2[i].ConsiderationType);
            run1[i].ConsiderationText.Should().Be(run2[i].ConsiderationText);
            run1[i].Disclaimer.Should().Be(run2[i].Disclaimer);
        }
    }
}
