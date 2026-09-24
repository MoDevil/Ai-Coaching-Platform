using AiCoachOs.Domain.Safety;

namespace AiCoachOs.Application.Safety.Engine;

public interface ISafetyScreener
{
    SafetyScreening Screen(
        Guid clientId,
        TriggeredByType triggeredByType,
        IReadOnlyList<ReportedSignal> currentSignals,
        IReadOnlyList<ReportedSignal> historicalSignals,
        IReadOnlyList<RedFlagRule> activeRules,
        Guid? triggeredByEntityId = null);
}

public class SafetyScreener : ISafetyScreener
{
    public SafetyScreening Screen(
        Guid clientId,
        TriggeredByType triggeredByType,
        IReadOnlyList<ReportedSignal> currentSignals,
        IReadOnlyList<ReportedSignal> historicalSignals,
        IReadOnlyList<RedFlagRule> activeRules,
        Guid? triggeredByEntityId = null)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));

        var screeningId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        if (currentSignals.Count == 0)
        {
            return new SafetyScreening(
                id: screeningId,
                clientId: clientId,
                triggeredByType: triggeredByType,
                screeningResult: SafetyCategory.NoSafetyConcern,
                recommendedAction: SafetyActionType.NoActionRequired,
                summaryRationale: "No safety-relevant signals reported. Training may proceed normally.",
                generatedAtUtc: now,
                triggeredByEntityId: triggeredByEntityId,
                requiresCoachAcknowledgment: false);
        }

        var matchedRuleNames = new List<string>();
        var triggeredCategories = new List<SafetyCategory>();
        var triggeredActions = new List<SafetyActionType>();
        var rationales = new List<string>();

        // 1. Evaluate Red Flag rules and deterministic patterns
        foreach (var signal in currentSignals)
        {
            // Cardiovascular Emergency
            if (IsCardiovascularSignal(signal))
            {
                matchedRuleNames.Add("Cardiovascular Symptoms During/After Exercise");
                triggeredCategories.Add(SafetyCategory.UrgentMedicalAttention);
                triggeredActions.Add(SafetyActionType.UrgentMedicalAttention);
                rationales.Add($"Potential emergency cardiovascular symptom reported ({signal.BodyRegion}: {signal.SignalType}, Timing: {signal.Timing}). Immediate medical triage required.");
            }

            // Severe + Sudden -> UrgentMedicalAttention
            if (signal.Severity == SignalSeverity.Severe && signal.Onset == SignalOnset.Sudden)
            {
                matchedRuleNames.Add("Acute Severe Musculoskeletal Trauma");
                triggeredCategories.Add(SafetyCategory.UrgentMedicalAttention);
                triggeredActions.Add(SafetyActionType.UrgentMedicalAttention);
                rationales.Add($"Sudden onset severe symptom reported ({signal.BodyRegion}: {signal.SignalType}). Immediate medical evaluation recommended.");
            }

            // Neurological
            if (signal.SignalType == SignalType.Numbness || signal.SignalType == SignalType.Weakness)
            {
                matchedRuleNames.Add("New Neurological Deficits");
                triggeredCategories.Add(SafetyCategory.ReferToHealthcareProfessional);
                triggeredActions.Add(SafetyActionType.ReferToHealthcareProfessional);
                rationales.Add($"Neurological signal reported ({signal.BodyRegion}: {signal.SignalType}). Clinical healthcare assessment recommended.");
            }

            // Pain at Rest / Persistent
            if (signal.Timing == SignalTiming.AtRest)
            {
                matchedRuleNames.Add("Persistent Non-Mechanical or Night Pain");
                triggeredCategories.Add(SafetyCategory.ReferToHealthcareProfessional);
                triggeredActions.Add(SafetyActionType.ReferToHealthcareProfessional);
                rationales.Add($"Non-mechanical resting pain reported in {signal.BodyRegion}. Referral to healthcare professional advised.");
            }
        }

        // 2. Evaluate History Patterns: Persistence & Worsening Severity
        if (historicalSignals.Count > 0)
        {
            foreach (var currentSig in currentSignals)
            {
                var matchingHistory = historicalSignals
                    .Where(h => string.Equals(h.BodyRegion, currentSig.BodyRegion, StringComparison.OrdinalIgnoreCase) &&
                                h.SignalType == currentSig.SignalType)
                    .ToList();

                // Persistence: >= 3 total occurrences across sessions (including current)
                int totalOccurrences = matchingHistory.Count + 1;
                if (totalOccurrences >= 3)
                {
                    matchedRuleNames.Add("Symptom Persistence Across >=3 Sessions");
                    triggeredCategories.Add(SafetyCategory.ReferToHealthcareProfessional);
                    triggeredActions.Add(SafetyActionType.ReferToHealthcareProfessional);
                    rationales.Add($"Persistent symptom pattern detected: {currentSig.BodyRegion} ({currentSig.SignalType}) reported across {totalOccurrences} sessions. Professional clinical evaluation recommended.");
                }

                // Worsening Severity
                if (currentSig.Worsening == true ||
                    matchingHistory.Any(h => h.Severity != SignalSeverity.Unknown && currentSig.Severity > h.Severity))
                {
                    matchedRuleNames.Add("Progressive / Worsening Severity Across Sessions");
                    triggeredCategories.Add(SafetyCategory.ReferToHealthcareProfessional);
                    triggeredActions.Add(SafetyActionType.PauseActivityPendingAssessment);
                    rationales.Add($"Progressive symptom escalation detected in {currentSig.BodyRegion}. Activity should be paused pending clinical assessment.");
                }
            }
        }

        // 3. Determine Overall Safety Category & Action
        SafetyCategory finalCategory;
        SafetyActionType finalAction;
        bool requiresAcknowledgment;

        if (triggeredCategories.Contains(SafetyCategory.UrgentMedicalAttention))
        {
            finalCategory = SafetyCategory.UrgentMedicalAttention;
            finalAction = SafetyActionType.UrgentMedicalAttention;
            requiresAcknowledgment = true;
        }
        else if (triggeredCategories.Contains(SafetyCategory.ReferToHealthcareProfessional))
        {
            finalCategory = SafetyCategory.ReferToHealthcareProfessional;
            finalAction = triggeredActions.Contains(SafetyActionType.PauseActivityPendingAssessment)
                ? SafetyActionType.PauseActivityPendingAssessment
                : SafetyActionType.ReferToHealthcareProfessional;
            requiresAcknowledgment = true;
        }
        else if (currentSignals.Any(s => s.SignalType == SignalType.Pain || s.SignalType == SignalType.Discomfort || s.SignalType == SignalType.Swelling))
        {
            finalCategory = SafetyCategory.CautionCoachReview;
            finalAction = SafetyActionType.CoachReviewRequired;
            requiresAcknowledgment = true;
            rationales.Add("Mild to moderate signal reported without red-flag criteria. Coach awareness review required before proceeding.");
        }
        else
        {
            finalCategory = SafetyCategory.NoSafetyConcern;
            finalAction = SafetyActionType.NoActionRequired;
            requiresAcknowledgment = false;
            rationales.Add("Reported signal does not match safety escalation criteria. Normal training unblocked.");
        }

        string summary = string.Join(" ", rationales.Distinct());

        var screening = new SafetyScreening(
            id: screeningId,
            clientId: clientId,
            triggeredByType: triggeredByType,
            screeningResult: finalCategory,
            recommendedAction: finalAction,
            summaryRationale: summary,
            generatedAtUtc: now,
            triggeredByEntityId: triggeredByEntityId,
            requiresCoachAcknowledgment: requiresAcknowledgment);

        foreach (var sig in currentSignals)
            screening.AddReportedSignal(sig);

        foreach (var ruleName in matchedRuleNames.Distinct())
            screening.AddMatchedRedFlag(ruleName);

        return screening;
    }

    private static bool IsCardiovascularSignal(ReportedSignal signal)
    {
        var region = signal.BodyRegion.ToLowerInvariant();
        if (region.Contains("chest") || region.Contains("heart") || region.Contains("cardio"))
            return true;

        if (signal.FreeText != null)
        {
            var text = signal.FreeText.ToLowerInvariant();
            if (text.Contains("chest pain") || text.Contains("chest pressure") ||
                text.Contains("shortness of breath") || text.Contains("palpitation") ||
                text.Contains("fainted") || text.Contains("dizzy") || text.Contains("blackout"))
            {
                return true;
            }
        }

        return false;
    }
}
