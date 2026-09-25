using AiCoachOs.Application.Substances.Engine;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Substances;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class SubstanceSafetyEvaluatorTests
{
    private readonly SubstanceSafetyEvaluator _evaluator = new();
    private readonly IReadOnlyList<PEDRedFlagRule> _seedRules = SubstanceSeedData.GetPEDRedFlagRules();

    [Fact]
    public void Evaluate_WhenNoSignals_ReturnsNoneEscalationLevel()
    {
        var result = _evaluator.Evaluate(
            reportedSignals: Array.Empty<string>(),
            activeRules: _seedRules);

        result.EscalationLevel.Should().Be(EscalationLevel.None);
        result.SummaryRationale.Should().Contain("No adverse signals reported");
        result.MatchedRedFlags.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenCardiovascularChestPainSignal_ReturnsUrgentMedicalAttention()
    {
        var signals = new List<string> { "Client reported crushing chest pressure and sudden shortness of breath during workout" };

        var result = _evaluator.Evaluate(
            reportedSignals: signals,
            activeRules: _seedRules);

        result.EscalationLevel.Should().Be(EscalationLevel.UrgentMedicalAttention);
        result.MatchedRedFlags.Should().Contain("PED Cardiovascular Emergency Symptoms");
        result.RecommendedAction.Should().Contain("IMMEDIATE EMERGENCY MEDICAL ATTENTION REQUIRED");
    }

    [Fact]
    public void Evaluate_WhenCardiovascularSyncopeSignal_ReturnsUrgentMedicalAttention()
    {
        var signals = new List<string> { "Severe heart racing and fainted after set" };

        var result = _evaluator.Evaluate(
            reportedSignals: signals,
            activeRules: _seedRules);

        result.EscalationLevel.Should().Be(EscalationLevel.UrgentMedicalAttention);
        result.MatchedRedFlags.Should().Contain("PED Cardiovascular Emergency Symptoms");
    }

    [Fact]
    public void Evaluate_WhenHypertensiveCrisisSymptoms_ReturnsUrgentMedicalAttention()
    {
        var signals = new List<string> { "Severe acute occipital headache with blurred vision and sudden nosebleed" };

        var result = _evaluator.Evaluate(
            reportedSignals: signals,
            activeRules: _seedRules);

        result.EscalationLevel.Should().Be(EscalationLevel.UrgentMedicalAttention);
        result.MatchedRedFlags.Should().Contain("Hypertensive Crisis & Neurological Symptoms");
    }

    [Fact]
    public void Evaluate_WhenHepaticJaundiceSymptoms_ReturnsHealthcareProfessionalReferral()
    {
        var signals = new List<string> { "Noticeable yellowing of sclera (yellow eyes), dark tea-colored urine, and right upper quadrant pain" };

        var result = _evaluator.Evaluate(
            reportedSignals: signals,
            activeRules: _seedRules);

        result.EscalationLevel.Should().Be(EscalationLevel.HealthcareProfessionalReferral);
        result.MatchedRedFlags.Should().Contain("Hepatic Toxicity & Cholestatic Jaundice");
        result.RecommendedAction.Should().Contain("URGENT MEDICAL REFERRAL");
    }

    [Fact]
    public void Evaluate_WhenPsychiatricEmergencySymptoms_ReturnsHealthcareProfessionalReferral()
    {
        var signals = new List<string> { "Extreme uncontrollable rage, severe paranoia, and acute suicidal thoughts" };

        var result = _evaluator.Evaluate(
            reportedSignals: signals,
            activeRules: _seedRules);

        result.EscalationLevel.Should().Be(EscalationLevel.HealthcareProfessionalReferral);
        result.MatchedRedFlags.Should().Contain("Acute Neuropsychiatric & Mood Disturbance");
        result.RecommendedAction.Should().Contain("URGENT PSYCHIATRIC / MEDICAL REFERRAL");
    }

    [Fact]
    public void Evaluate_WhenEndocrineSuppressionSymptoms_ReturnsCoachAwareness()
    {
        var signals = new List<string> { "Testicular atrophy, profound fatigue, and severe libido loss after cessation" };

        var result = _evaluator.Evaluate(
            reportedSignals: signals,
            activeRules: _seedRules);

        result.EscalationLevel.Should().Be(EscalationLevel.CoachAwareness);
        result.MatchedRedFlags.Should().Contain("Severe Endocrine Axis Suppression");
    }

    [Fact]
    public void Evaluate_HighestEscalationWins_UrgentOverridesReferralAndCaution()
    {
        // Combined signals: Endocrine (CoachAwareness) + Jaundice (HealthcareProfessionalReferral) + Chest Pain (UrgentMedicalAttention)
        var signals = new List<string>
        {
            "Testicular atrophy and low libido",
            "Yellow eyes and dark urine",
            "Severe chest pain and palpitations"
        };

        var result = _evaluator.Evaluate(
            reportedSignals: signals,
            activeRules: _seedRules);

        // UrgentMedicalAttention MUST WIN
        result.EscalationLevel.Should().Be(EscalationLevel.UrgentMedicalAttention);
        result.MatchedRedFlags.Should().Contain("PED Cardiovascular Emergency Symptoms");
        result.MatchedRedFlags.Should().Contain("Hepatic Toxicity & Cholestatic Jaundice");
        result.MatchedRedFlags.Should().Contain("Severe Endocrine Axis Suppression");
    }

    [Fact]
    public void Evaluate_UrgentEscalationCannotBeDowngradedByMildSignals()
    {
        var signals = new List<string>
        {
            "Slight delayed onset muscle soreness in biceps",
            "Yellow skin and dark tea-colored urine"
        };

        var result = _evaluator.Evaluate(
            reportedSignals: signals,
            activeRules: _seedRules);

        result.EscalationLevel.Should().Be(EscalationLevel.HealthcareProfessionalReferral);
    }

    [Fact]
    public void Evaluate_WithSubstanceSpecificCautionFlag_IncorporatesFlagSeverity()
    {
        var supplement = new SupplementKnowledge(
            id: Guid.NewGuid(),
            name: "Caffeine Extreme",
            primaryClaimedBenefit: "High intensity energy",
            evidenceStatus: SupplementEvidenceStatus.StrongEvidence,
            effectMagnitude: EffectMagnitude.Moderate,
            description: "High dose stimulant",
            uncertaintyStatement: "High sensitivity in naive users");

        supplement.AddSafetyFlag(new SubstanceSafetyFlag(
            category: SafetyFlagCategory.HighDoseToxicity,
            description: "High Dose Cardiovascular Precaution",
            escalationLevel: EscalationLevel.CoachAwareness,
            coachNote: "Avoid combining with other stimulants or pre-workouts."));

        var signals = new List<string> { "Mild jitters and elevated heart rate" };

        var result = _evaluator.Evaluate(
            reportedSignals: signals,
            activeRules: _seedRules,
            substance: supplement);

        result.EscalationLevel.Should().Be(EscalationLevel.CoachAwareness);
        result.SummaryRationale.Should().Contain("High Dose Cardiovascular Precaution");
    }

    [Fact]
    public void Evaluate_WhenSignalsContainOnlyWhitespace_ReturnsNone()
    {
        var signals = new List<string> { "   ", "" };

        var result = _evaluator.Evaluate(
            reportedSignals: signals,
            activeRules: _seedRules);

        result.EscalationLevel.Should().Be(EscalationLevel.None);
    }
}
