using AiCoachOs.Domain.Substances;

namespace AiCoachOs.Application.Substances.Engine;

public interface ISubstanceSafetyEvaluator
{
    SubstanceSafetyEvaluationResult Evaluate(
        IReadOnlyList<string> reportedSignals,
        IReadOnlyList<PEDRedFlagRule> activeRules,
        SubstanceRecord? substance = null);
}

public class SubstanceSafetyEvaluationResult
{
    public EscalationLevel EscalationLevel { get; set; }
    public string SummaryRationale { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public string Disclaimer { get; set; } = "MEDICAL DECISION SUPPORT DISCLAIMER: AI Coach OS does not provide medical diagnoses or treatment prescriptions. Any flagged severe or emergency symptoms require immediate referral to qualified medical professionals.";
    public List<string> MatchedRedFlags { get; set; } = new();
    public List<string> TriggeredActions { get; set; } = new();
}
