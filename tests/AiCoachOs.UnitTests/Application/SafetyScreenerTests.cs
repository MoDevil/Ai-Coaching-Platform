using AiCoachOs.Application.Safety.Engine;
using AiCoachOs.Domain.Safety;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class SafetyScreenerTests
{
    private readonly SafetyScreener _screener = new();
    private readonly IReadOnlyList<RedFlagRule> _seedRules = RedFlagSeedData.GetInitialSeedRules();

    [Fact]
    public void Screen_WhenNoSignals_ReturnsNoSafetyConcern_AndNoActionRequired()
    {
        var clientId = Guid.NewGuid();
        var result = _screener.Screen(
            clientId,
            TriggeredByType.WorkoutSession,
            Array.Empty<ReportedSignal>(),
            Array.Empty<ReportedSignal>(),
            _seedRules);

        result.ScreeningResult.Should().Be(SafetyCategory.NoSafetyConcern);
        result.RecommendedAction.Should().Be(SafetyActionType.NoActionRequired);
        result.RequiresCoachAcknowledgment.Should().BeFalse();
        result.RedFlagsMatched.Should().BeEmpty();
    }

    [Fact]
    public void Screen_WhenCardiovascularChestPain_ReturnsUrgentMedicalAttention()
    {
        var clientId = Guid.NewGuid();
        var currentSignals = new List<ReportedSignal>
        {
            new ReportedSignal(
                bodyRegion: "Chest",
                signalType: SignalType.Pain,
                onset: SignalOnset.Sudden,
                timing: SignalTiming.DuringExercise,
                severity: SignalSeverity.Severe,
                freeText: "Felt acute chest pressure and shortness of breath while pressing")
        };

        var result = _screener.Screen(
            clientId,
            TriggeredByType.WorkoutSession,
            currentSignals,
            Array.Empty<ReportedSignal>(),
            _seedRules);

        result.ScreeningResult.Should().Be(SafetyCategory.UrgentMedicalAttention);
        result.RecommendedAction.Should().Be(SafetyActionType.UrgentMedicalAttention);
        result.RequiresCoachAcknowledgment.Should().BeTrue();
        result.RedFlagsMatched.Should().Contain("Cardiovascular Symptoms During/After Exercise");
    }

    [Fact]
    public void Screen_WhenSevereAndSudden_ReturnsUrgentMedicalAttention()
    {
        var clientId = Guid.NewGuid();
        var currentSignals = new List<ReportedSignal>
        {
            new ReportedSignal(
                bodyRegion: "Right Knee",
                signalType: SignalType.Pain,
                onset: SignalOnset.Sudden,
                timing: SignalTiming.DuringExercise,
                severity: SignalSeverity.Severe,
                freeText: "Audible pop with sudden severe pain")
        };

        var result = _screener.Screen(
            clientId,
            TriggeredByType.WorkoutSession,
            currentSignals,
            Array.Empty<ReportedSignal>(),
            _seedRules);

        result.ScreeningResult.Should().Be(SafetyCategory.UrgentMedicalAttention);
        result.RecommendedAction.Should().Be(SafetyActionType.UrgentMedicalAttention);
        result.RequiresCoachAcknowledgment.Should().BeTrue();
        result.RedFlagsMatched.Should().Contain("Acute Severe Musculoskeletal Trauma");
    }

    [Fact]
    public void Screen_WhenNeurologicalDeficit_ReturnsReferToHealthcareProfessional()
    {
        var clientId = Guid.NewGuid();
        var currentSignals = new List<ReportedSignal>
        {
            new ReportedSignal(
                bodyRegion: "Left Arm",
                signalType: SignalType.Numbness,
                onset: SignalOnset.Gradual,
                timing: SignalTiming.AfterExercise,
                severity: SignalSeverity.Moderate)
        };

        var result = _screener.Screen(
            clientId,
            TriggeredByType.WorkoutSession,
            currentSignals,
            Array.Empty<ReportedSignal>(),
            _seedRules);

        result.ScreeningResult.Should().Be(SafetyCategory.ReferToHealthcareProfessional);
        result.RecommendedAction.Should().Be(SafetyActionType.ReferToHealthcareProfessional);
        result.RequiresCoachAcknowledgment.Should().BeTrue();
        result.RedFlagsMatched.Should().Contain("New Neurological Deficits");
    }

    [Fact]
    public void Screen_WhenSymptomPersistsAcross3Sessions_ReturnsReferToHealthcareProfessional()
    {
        var clientId = Guid.NewGuid();
        var currentSignals = new List<ReportedSignal>
        {
            new ReportedSignal("Right Shoulder", SignalType.Pain, SignalOnset.Gradual, SignalTiming.DuringExercise, SignalSeverity.Mild)
        };

        var history = new List<ReportedSignal>
        {
            new ReportedSignal("Right Shoulder", SignalType.Pain, SignalOnset.Gradual, SignalTiming.DuringExercise, SignalSeverity.Mild),
            new ReportedSignal("Right Shoulder", SignalType.Pain, SignalOnset.Gradual, SignalTiming.DuringExercise, SignalSeverity.Mild)
        };

        var result = _screener.Screen(
            clientId,
            TriggeredByType.WorkoutSession,
            currentSignals,
            history,
            _seedRules);

        result.ScreeningResult.Should().Be(SafetyCategory.ReferToHealthcareProfessional);
        result.RequiresCoachAcknowledgment.Should().BeTrue();
        result.RedFlagsMatched.Should().Contain("Symptom Persistence Across >=3 Sessions");
    }

    [Fact]
    public void Screen_WhenSeverityWorsensAcrossSessions_ReturnsReferToHealthcareProfessional_AndPauseActivity()
    {
        var clientId = Guid.NewGuid();
        var currentSignals = new List<ReportedSignal>
        {
            new ReportedSignal("Lower Back", SignalType.Pain, SignalOnset.Gradual, SignalTiming.AfterExercise, SignalSeverity.Moderate)
        };

        var history = new List<ReportedSignal>
        {
            new ReportedSignal("Lower Back", SignalType.Pain, SignalOnset.Gradual, SignalTiming.AfterExercise, SignalSeverity.Mild)
        };

        var result = _screener.Screen(
            clientId,
            TriggeredByType.WorkoutSession,
            currentSignals,
            history,
            _seedRules);

        result.ScreeningResult.Should().Be(SafetyCategory.ReferToHealthcareProfessional);
        result.RecommendedAction.Should().Be(SafetyActionType.PauseActivityPendingAssessment);
        result.RequiresCoachAcknowledgment.Should().BeTrue();
        result.RedFlagsMatched.Should().Contain("Progressive / Worsening Severity Across Sessions");
    }

    [Fact]
    public void Screen_WhenMildPainWithoutRedFlags_ReturnsCautionCoachReview()
    {
        var clientId = Guid.NewGuid();
        var currentSignals = new List<ReportedSignal>
        {
            new ReportedSignal("Left Quad", SignalType.Discomfort, SignalOnset.Gradual, SignalTiming.DuringExercise, SignalSeverity.Mild)
        };

        var result = _screener.Screen(
            clientId,
            TriggeredByType.WorkoutSession,
            currentSignals,
            Array.Empty<ReportedSignal>(),
            _seedRules);

        result.ScreeningResult.Should().Be(SafetyCategory.CautionCoachReview);
        result.RecommendedAction.Should().Be(SafetyActionType.CoachReviewRequired);
        result.RequiresCoachAcknowledgment.Should().BeTrue();
        result.RedFlagsMatched.Should().BeEmpty();
    }

    [Fact]
    public void Screen_IsDeterministic_SameInputsProduceExactSameOutputs()
    {
        var clientId = Guid.NewGuid();
        var currentSignals = new List<ReportedSignal>
        {
            new ReportedSignal("Left Elbow", SignalType.Pain, SignalOnset.Gradual, SignalTiming.DuringExercise, SignalSeverity.Moderate)
        };
        var history = new List<ReportedSignal>
        {
            new ReportedSignal("Left Elbow", SignalType.Pain, SignalOnset.Gradual, SignalTiming.DuringExercise, SignalSeverity.Mild)
        };

        var result1 = _screener.Screen(clientId, TriggeredByType.WorkoutSession, currentSignals, history, _seedRules);
        var result2 = _screener.Screen(clientId, TriggeredByType.WorkoutSession, currentSignals, history, _seedRules);

        result1.ScreeningResult.Should().Be(result2.ScreeningResult);
        result1.RecommendedAction.Should().Be(result2.RecommendedAction);
        result1.RequiresCoachAcknowledgment.Should().Be(result2.RequiresCoachAcknowledgment);
        result1.SummaryRationale.Should().Be(result2.SummaryRationale);
        result1.RedFlagsMatched.Should().BeEquivalentTo(result2.RedFlagsMatched);
    }

    [Fact]
    public void Screen_RationaleNeverContainsDiagnosticPhrases()
    {
        var clientId = Guid.NewGuid();
        var currentSignals = new List<ReportedSignal>
        {
            new ReportedSignal("Patellar Region", SignalType.Pain, SignalOnset.Sudden, SignalTiming.DuringExercise, SignalSeverity.Severe, freeText: "Tendon snap felt")
        };

        var result = _screener.Screen(clientId, TriggeredByType.WorkoutSession, currentSignals, Array.Empty<ReportedSignal>(), _seedRules);

        var rationale = result.SummaryRationale.ToLowerInvariant();
        rationale.Should().NotContain("this sounds like");
        rationale.Should().NotContain("this is likely");
        rationale.Should().NotContain("probable");
        rationale.Should().NotContain("diagnosis");
        rationale.Should().NotContain("diagnosed");
        rationale.Should().NotContain("tendonitis");
        rationale.Should().NotContain("tendinopathy");
    }
}
