using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Substances;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain.Substances;

public class SubstanceDomainTests
{
    [Fact]
    public void SupplementKnowledge_WhenUncertaintyStatementEmpty_ThrowsArgumentException()
    {
        var act = () => new SupplementKnowledge(
            id: Guid.NewGuid(),
            name: "Creatine",
            primaryClaimedBenefit: "High-intensity energy resynthesis",
            evidenceStatus: SupplementEvidenceStatus.StrongEvidence,
            effectMagnitude: EffectMagnitude.Moderate,
            description: "Ergogenic supplement.",
            uncertaintyStatement: "   "); // Empty

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Uncertainty statement is mandatory*");
    }

    [Fact]
    public void SupplementKnowledge_WhenValid_CreatesInstanceSuccessfully()
    {
        var supplement = new SupplementKnowledge(
            id: Guid.NewGuid(),
            name: "Creatine Monohydrate",
            primaryClaimedBenefit: "Increases phosphocreatine stores",
            evidenceStatus: SupplementEvidenceStatus.StrongEvidence,
            effectMagnitude: EffectMagnitude.Moderate,
            description: "Increases intramuscular phosphocreatine.",
            uncertaintyStatement: "High responders vs. non-responders exist based on baseline dietary intake.",
            efficacyClaim: "Meta-analyses demonstrate significant strength gains.",
            populationNote: "Healthy exercising adults",
            typicalDoseRangeMin: 3m,
            typicalDoseRangeMax: 5m,
            doseUnit: "g/day",
            isEgyptianMarketAvailable: true,
            commonAliases: new[] { "Creatine", "Creapure" });

        supplement.Name.Should().Be("Creatine Monohydrate");
        supplement.SubstanceCategory.Should().Be(SubstanceCategory.Supplement);
        supplement.UncertaintyStatement.Should().NotBeNullOrWhiteSpace();
        supplement.PrimaryClaimedBenefit.Should().Be("Increases phosphocreatine stores");
        supplement.EvidenceStatus.Should().Be(SupplementEvidenceStatus.StrongEvidence);
        supplement.EffectMagnitude.Should().Be(EffectMagnitude.Moderate);
        supplement.TypicalDoseRangeMin.Should().Be(3m);
        supplement.TypicalDoseRangeMax.Should().Be(5m);
        supplement.CommonAliases.Should().Contain("Creapure");
        supplement.IsActive.Should().BeTrue();
        // Supplement ReviewDueAt is 24 months
        supplement.ReviewDueAtUtc.Should().NotBeNull();
        supplement.ReviewDueAtUtc!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMonths(24), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void HormoneKnowledge_WhenUncertaintyStatementEmpty_ThrowsArgumentException()
    {
        var act = () => new HormoneKnowledge(
            id: Guid.NewGuid(),
            name: "Testosterone",
            hormoneCategory: HormoneCategory.Androgen,
            description: "Primary androgen.",
            physiologicalRole: "Protein synthesis and androgenic signaling.",
            trainingRelevance: "Heavy resistance training induces acute transient spikes.",
            uncertaintyStatement: ""); // Empty

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Uncertainty statement is mandatory*");
    }

    [Fact]
    public void HormoneKnowledge_WhenValid_CreatesInstanceSuccessfully()
    {
        var hormone = new HormoneKnowledge(
            id: Guid.NewGuid(),
            name: "Cortisol",
            hormoneCategory: HormoneCategory.Glucocorticoid,
            description: "Primary glucocorticoid.",
            physiologicalRole: "Substrate mobilization and anti-inflammatory signaling.",
            trainingRelevance: "Acute post-exercise rise is normal; chronic elevation indicates overtraining.",
            uncertaintyStatement: "Single morning serum samples have high acute noise.",
            medicalEvaluationTriggers: new[] { "Cushingoid features", "Chronic unremitting fatigue" });

        hormone.Name.Should().Be("Cortisol");
        hormone.SubstanceCategory.Should().Be(SubstanceCategory.Hormone);
        hormone.HormoneCategory.Should().Be(HormoneCategory.Glucocorticoid);
        hormone.PhysiologicalRole.Should().Be("Substrate mobilization and anti-inflammatory signaling.");
        hormone.TrainingRelevance.Should().Be("Acute post-exercise rise is normal; chronic elevation indicates overtraining.");
        hormone.UncertaintyStatement.Should().NotBeNullOrWhiteSpace();
        hormone.MedicalEvaluationTriggers.Should().Contain("Cushingoid features");
        // Hormone ReviewDueAt is 24 months
        hormone.ReviewDueAtUtc!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMonths(24), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void PEDSafetyRecord_WhenValid_ContainsEducationalDisclaimerAnd12MonthReviewCycle()
    {
        var ped = new PEDSafetyRecord(
            id: Guid.NewGuid(),
            name: "Anabolic-Androgenic Steroids",
            pedCategory: PEDCategory.AAS,
            description: "Synthetic testosterone derivatives.",
            mechanismSummary: "Androgen receptor agonism increasing protein synthesis.",
            monitoringConcepts: new[] { "Lipid Panel", "Complete Blood Count", "Liver Enzymes" });

        ped.SubstanceCategory.Should().Be(SubstanceCategory.PED);
        ped.PEDCategory.Should().Be(PEDCategory.AAS);
        ped.SafetyDisclaimer.Should().Contain("HARM REDUCTION ONLY");
        ped.SafetyDisclaimer.Should().Contain("prohibits prescribing, cycle planning, dosing, sourcing");
        ped.MonitoringConcepts.Should().Contain("Lipid Panel");
        // PED ReviewDueAt is 12 months
        ped.ReviewDueAtUtc!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMonths(12), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void PEDSafetyRecord_AddRisk_AddsRiskProperly()
    {
        var ped = new PEDSafetyRecord(
            id: Guid.NewGuid(),
            name: "SARMs",
            pedCategory: PEDCategory.SARM,
            description: "Selective androgen receptor modulators.",
            mechanismSummary: "Tissue-selective androgen receptor binding.");

        var risk = new PEDRiskRecord(
            id: Guid.NewGuid(),
            pedSafetyRecordId: ped.Id,
            riskCategory: RiskCategory.Hepatic,
            severity: PEDRiskSeverity.High,
            description: "Drug-induced liver injury and cholestatic jaundice.",
            evidenceLevel: EvidenceLevel.ClinicalGuideline,
            reversibilityNotes: "Usually reversible upon discontinuation.");

        ped.AddRisk(risk);

        ped.DocumentedRisks.Should().HaveCount(1);
        ped.DocumentedRisks.First().RiskCategory.Should().Be(RiskCategory.Hepatic);
        ped.DocumentedRisks.First().Severity.Should().Be(PEDRiskSeverity.High);
        ped.DocumentedRisks.First().EvidenceLevel.Should().Be(EvidenceLevel.ClinicalGuideline);
    }

    [Fact]
    public void PEDRedFlagRule_WhenValid_InitializesWithLockedDefaults()
    {
        var rule = new PEDRedFlagRule(
            id: Guid.NewGuid(),
            name: "Cardio Red Flag",
            description: "Chest pain",
            signalPattern: "PED_Emergency_Cardiovascular",
            escalationLevel: EscalationLevel.UrgentMedicalAttention,
            recommendedAction: "Call 123 immediately",
            evidenceBasis: "Emergency triage guidelines",
            pedCategory: PEDCategory.AAS);

        rule.Name.Should().Be("Cardio Red Flag");
        rule.EscalationLevel.Should().Be(EscalationLevel.UrgentMedicalAttention);
        rule.RequiresClinicalReview.Should().BeTrue();
        rule.PEDCategory.Should().Be(PEDCategory.AAS);
        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SubstanceEscalationRecord_DoesNotContainClientIdProperty()
    {
        // Strict contract verification: SubstanceEscalationRecord is coach-owned and contains NO ClientId
        var property = typeof(SubstanceEscalationRecord).GetProperty("ClientId");
        property.Should().BeNull("SubstanceEscalationRecord must remain strictly coach-owned and cannot link to Client entities.");
    }

    [Fact]
    public void SubstanceEscalationRecord_WhenCoachIdEmpty_ThrowsArgumentException()
    {
        var act = () => new SubstanceEscalationRecord(
            id: Guid.NewGuid(),
            coachId: Guid.Empty,
            escalationLevel: EscalationLevel.UrgentMedicalAttention,
            summaryRationale: "Cardiovascular emergency",
            recommendedAction: "Call 123");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*CoachId cannot be empty*");
    }

    [Fact]
    public void SubstanceSafetyFlag_WhenValid_CreatesInstanceSuccessfully()
    {
        var flag = new SubstanceSafetyFlag(
            category: SafetyFlagCategory.SpecialPopulationPrecaution,
            description: "Renal Disease Precaution",
            escalationLevel: EscalationLevel.CoachAwareness,
            coachNote: "Consult nephrologist prior to high-dose use.");

        flag.Category.Should().Be(SafetyFlagCategory.SpecialPopulationPrecaution);
        flag.EscalationLevel.Should().Be(EscalationLevel.CoachAwareness);
        flag.CoachNote.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void SubstanceRecord_SafetyFlags_CanBeAddedAndCleared()
    {
        var supplement = new SupplementKnowledge(
            id: Guid.NewGuid(),
            name: "Caffeine",
            primaryClaimedBenefit: "Alertness and ergogenic support",
            evidenceStatus: SupplementEvidenceStatus.StrongEvidence,
            effectMagnitude: EffectMagnitude.Moderate,
            description: "Stimulant",
            uncertaintyStatement: "Tolerance develops with habituation.");

        supplement.AddSafetyFlag(new SubstanceSafetyFlag(
            SafetyFlagCategory.Contraindication,
            "Hypertension Precaution",
            EscalationLevel.CoachAwareness,
            "Avoid high doses if uncontrolled hypertension"));

        supplement.SafetyFlags.Should().HaveCount(1);

        supplement.ClearSafetyFlags();
        supplement.SafetyFlags.Should().BeEmpty();
    }

    [Fact]
    public void SubstanceRecord_UpdateReview_SetsReviewDetailsAndTimestamp()
    {
        var supplement = new SupplementKnowledge(
            id: Guid.NewGuid(),
            name: "Whey Protein",
            primaryClaimedBenefit: "Muscle protein synthesis support",
            evidenceStatus: SupplementEvidenceStatus.StrongEvidence,
            effectMagnitude: EffectMagnitude.Moderate,
            description: "Protein powder",
            uncertaintyStatement: "No unique advantage over equal whole-food protein.");

        var reviewedAt = new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc);
        supplement.UpdateReview("Dr. Ahmed", reviewedAt);

        supplement.ReviewedBy.Should().Be("Dr. Ahmed");
        supplement.LastReviewedAtUtc.Should().Be(reviewedAt);
        supplement.ReviewDueAtUtc.Should().Be(reviewedAt.AddMonths(24));
    }
}
