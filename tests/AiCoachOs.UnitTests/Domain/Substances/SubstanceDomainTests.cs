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
            supplementCategory: SupplementCategory.Performance,
            evidenceLevel: EvidenceLevel.MetaAnalysis,
            description: "Ergogenic supplement.",
            evidenceSummary: "Supported by extensive evidence.",
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
            supplementCategory: SupplementCategory.Performance,
            evidenceLevel: EvidenceLevel.MetaAnalysis,
            description: "Increases intramuscular phosphocreatine.",
            evidenceSummary: "Meta-analyses demonstrate significant strength gains.",
            uncertaintyStatement: "High responders vs. non-responders exist based on baseline dietary intake.",
            commonForms: "Monohydrate",
            typicalDoseRange: "3-5 g/day",
            isEgyptianMarketAvailable: true);

        supplement.Name.Should().Be("Creatine Monohydrate");
        supplement.Category.Should().Be(SubstanceCategory.Supplement);
        supplement.UncertaintyStatement.Should().NotBeNullOrWhiteSpace();
        supplement.IsActive.Should().BeTrue();
    }

    [Fact]
    public void HormoneKnowledge_WhenUncertaintyStatementEmpty_ThrowsArgumentException()
    {
        var act = () => new HormoneKnowledge(
            id: Guid.NewGuid(),
            name: "Testosterone",
            hormoneAxis: HormoneAxis.HPTA,
            description: "Primary androgen.",
            physiologicalRole: "Protein synthesis and androgenic signaling.",
            trainingImpactSummary: "Heavy resistance training induces acute transient spikes.",
            evidenceSummary: "Clinical endocrinology guidelines.",
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
            hormoneAxis: HormoneAxis.Adrenal,
            description: "Primary glucocorticoid.",
            physiologicalRole: "Substrate mobilization and anti-inflammatory signaling.",
            trainingImpactSummary: "Acute post-exercise rise is normal; chronic elevation indicates overtraining.",
            evidenceSummary: "Extensively documented in endocrinology.",
            uncertaintyStatement: "Single morning serum samples have high acute noise.");

        hormone.Name.Should().Be("Cortisol");
        hormone.Category.Should().Be(SubstanceCategory.Hormone);
        hormone.HormoneAxis.Should().Be(HormoneAxis.Adrenal);
        hormone.UncertaintyStatement.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PEDSafetyRecord_WhenValid_ContainsEducationalDisclaimerAndNoProhibitedFields()
    {
        var ped = new PEDSafetyRecord(
            id: Guid.NewGuid(),
            name: "Anabolic-Androgenic Steroids",
            pedCategory: PEDCategory.AAS,
            description: "Synthetic testosterone derivatives.",
            mechanismSummary: "Androgen receptor agonism increasing protein synthesis.",
            healthRisksSummary: "Cardiovascular remodeling, atherogenic dyslipidemia, HPTA shutdown, hepatotoxicity.",
            evidenceSummary: "Comprehensive consensus reviews.");

        ped.Category.Should().Be(SubstanceCategory.PED);
        ped.SafetyDisclaimer.Should().Contain("HARM REDUCTION ONLY");
        ped.SafetyDisclaimer.Should().Contain("prohibits prescribing, cycle planning, dosing, sourcing");
    }

    [Fact]
    public void PEDSafetyRecord_AddRisk_AddsRiskProperly()
    {
        var ped = new PEDSafetyRecord(
            id: Guid.NewGuid(),
            name: "SARMs",
            pedCategory: PEDCategory.SARM,
            description: "Selective androgen receptor modulators.",
            mechanismSummary: "Tissue-selective androgen receptor binding.",
            healthRisksSummary: "Endocrine suppression and drug-induced liver injury.",
            evidenceSummary: "Clinical warning reports.");

        var risk = new PEDRiskRecord(
            id: Guid.NewGuid(),
            pedSafetyRecordId: ped.Id,
            organSystem: OrganSystem.Hepatic,
            severity: PEDRiskSeverity.High,
            riskDescription: "Drug-induced liver injury and cholestatic jaundice.",
            reversibilityNotes: "Usually reversible upon discontinuation.");

        ped.AddRisk(risk);

        ped.Risks.Should().HaveCount(1);
        ped.Risks.First().OrganSystem.Should().Be(OrganSystem.Hepatic);
        ped.Risks.First().Severity.Should().Be(PEDRiskSeverity.High);
    }

    [Fact]
    public void SubstanceEscalationRecord_DoesNotContainClientIdProperty()
    {
        // Architecturally verify that SubstanceEscalationRecord is coach-owned and contains NO ClientId
        var property = typeof(SubstanceEscalationRecord).GetProperty("ClientId");
        property.Should().BeNull("SubstanceEscalationRecord must remain strictly coach-owned and cannot link to Client entities.");
    }

    [Fact]
    public void SubstanceEscalationRecord_WhenCoachIdEmpty_ThrowsArgumentException()
    {
        var act = () => new SubstanceEscalationRecord(
            id: Guid.NewGuid(),
            coachId: Guid.Empty,
            escalationLevel: SubstanceEscalationLevel.EmergencyMedicalAttention,
            summaryRationale: "Cardiovascular emergency",
            recommendedAction: "Call 123");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*CoachId cannot be empty*");
    }

    [Fact]
    public void SubstanceSafetyFlag_WhenValid_CreatesInstanceSuccessfully()
    {
        var flag = new SubstanceSafetyFlag(
            flagType: "Renal Disease Contraindication",
            severity: SubstanceEscalationLevel.CautionCoachReview,
            message: "Consult nephrologist prior to high-dose use.",
            evidenceBasis: "Clinical nephrology guidelines.");

        flag.FlagType.Should().Be("Renal Disease Contraindication");
        flag.Severity.Should().Be(SubstanceEscalationLevel.CautionCoachReview);
        flag.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void SubstanceRecord_SafetyFlags_CanBeAddedAndCleared()
    {
        var supplement = new SupplementKnowledge(
            id: Guid.NewGuid(),
            name: "Caffeine",
            supplementCategory: SupplementCategory.Performance,
            evidenceLevel: EvidenceLevel.MetaAnalysis,
            description: "Stimulant",
            evidenceSummary: "Meta-analytic evidence",
            uncertaintyStatement: "Tolerance develops with habituation.");

        supplement.AddSafetyFlag(new SubstanceSafetyFlag(
            "Hypertension Precaution",
            SubstanceEscalationLevel.CautionCoachReview,
            "Avoid high doses if uncontrolled hypertension",
            "ISSN 2021"));

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
            supplementCategory: SupplementCategory.BodyComposition,
            evidenceLevel: EvidenceLevel.MetaAnalysis,
            description: "Protein powder",
            evidenceSummary: "Morton et al. 2018",
            uncertaintyStatement: "No unique advantage over equal whole-food protein.");

        var reviewedAt = new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc);
        supplement.UpdateReview("Dr. Ahmed", reviewedAt);

        supplement.ReviewedBy.Should().Be("Dr. Ahmed");
        supplement.LastReviewedAtUtc.Should().Be(reviewedAt);
    }
}
