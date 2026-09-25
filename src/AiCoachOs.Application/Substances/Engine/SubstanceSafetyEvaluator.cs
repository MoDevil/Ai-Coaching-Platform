using AiCoachOs.Domain.Substances;

namespace AiCoachOs.Application.Substances.Engine;

public class SubstanceSafetyEvaluator : ISubstanceSafetyEvaluator
{
    public SubstanceSafetyEvaluationResult Evaluate(
        IReadOnlyList<string> reportedSignals,
        IReadOnlyList<PEDRedFlagRule> activeRules,
        SubstanceRecord? substance = null)
    {
        var result = new SubstanceSafetyEvaluationResult();

        if (reportedSignals == null || reportedSignals.Count == 0)
        {
            result.EscalationLevel = EscalationLevel.None;
            result.SummaryRationale = "No adverse signals reported. No substance-related safety flags triggered.";
            result.RecommendedAction = "No action required. Maintain general training and health monitoring.";
            return result;
        }

        var normalizedSignals = reportedSignals
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim().ToLowerInvariant())
            .ToList();

        if (normalizedSignals.Count == 0)
        {
            result.EscalationLevel = EscalationLevel.None;
            result.SummaryRationale = "No adverse signals reported. No substance-related safety flags triggered.";
            result.RecommendedAction = "No action required. Maintain general training and health monitoring.";
            return result;
        }

        var matchedRules = new List<PEDRedFlagRule>();
        var rationales = new List<string>();
        var actions = new List<string>();
        var levels = new List<EscalationLevel>();

        // 1. Evaluate Deterministic Clinical Patterns against Active Rules
        if (activeRules != null)
        {
            foreach (var rule in activeRules.Where(r => r.IsActive))
            {
                bool matches = false;

                if (rule.SignalPattern.Equals("PED_Emergency_Cardiovascular", StringComparison.OrdinalIgnoreCase))
                {
                    matches = normalizedSignals.Any(s =>
                        s.Contains("chest pain") || s.Contains("chest pressure") ||
                        s.Contains("palpitation") || s.Contains("heart racing") ||
                        s.Contains("shortness of breath") || s.Contains("dyspnea") ||
                        s.Contains("syncope") || s.Contains("passed out") ||
                        s.Contains("blackout") || s.Contains("fainted") ||
                        s.Contains("irregular heartbeat"));
                }
                else if (rule.SignalPattern.Equals("PED_Hypertensive_Crisis", StringComparison.OrdinalIgnoreCase))
                {
                    matches = normalizedSignals.Any(s =>
                        (s.Contains("headache") && (s.Contains("severe") || s.Contains("acute") || s.Contains("occipital"))) ||
                        s.Contains("hypertensive") || s.Contains("blurred vision") ||
                        s.Contains("nosebleed") || s.Contains("epistaxis") ||
                        s.Contains("extreme bp") || s.Contains("high blood pressure"));
                }
                else if (rule.SignalPattern.Equals("PED_Hepatic_Jaundice", StringComparison.OrdinalIgnoreCase))
                {
                    matches = normalizedSignals.Any(s =>
                        s.Contains("jaundice") || s.Contains("yellow eyes") || s.Contains("yellow skin") ||
                        s.Contains("icterus") || s.Contains("dark urine") || s.Contains("tea-colored urine") ||
                        s.Contains("pale stool") || s.Contains("ruq pain") || s.Contains("right upper quadrant") ||
                        s.Contains("liver pain"));
                }
                else if (rule.SignalPattern.Equals("PED_Psychiatric_Emergency", StringComparison.OrdinalIgnoreCase))
                {
                    matches = normalizedSignals.Any(s =>
                        s.Contains("psychosis") || s.Contains("hallucination") || s.Contains("paranoia") ||
                        s.Contains("mania") || s.Contains("suicidal") || s.Contains("suicide") ||
                        s.Contains("rage") || s.Contains("uncontrollable aggression") || s.Contains("violent"));
                }
                else if (rule.SignalPattern.Equals("PED_Endocrine_Suppression", StringComparison.OrdinalIgnoreCase))
                {
                    matches = normalizedSignals.Any(s =>
                        s.Contains("testicular atrophy") || s.Contains("hypogonadism") ||
                        s.Contains("erectile dysfunction") || s.Contains("zero libido") ||
                        s.Contains("endocrine crash") || s.Contains("suppression"));
                }
                else
                {
                    // Generic token matcher
                    var patternTokens = rule.SignalPattern.ToLowerInvariant().Split(new[] { '_', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
                    matches = normalizedSignals.Any(s => patternTokens.Any(token => s.Contains(token)));
                }

                if (matches)
                {
                    matchedRules.Add(rule);
                    levels.Add(rule.EscalationLevel);
                    rationales.Add($"Matched Red Flag [{rule.Name}]: {rule.Description}");
                    actions.Add(rule.RecommendedAction);
                }
            }
        }

        // 2. Direct Fallback Signal Triage (if database rules empty/inactive)
        if (levels.Count == 0)
        {
            if (normalizedSignals.Any(s =>
                s.Contains("chest pain") || s.Contains("chest pressure") ||
                s.Contains("syncope") || s.Contains("blackout") ||
                s.Contains("passed out") || s.Contains("severe palpitations")))
            {
                levels.Add(EscalationLevel.UrgentMedicalAttention);
                rationales.Add("Direct cardiovascular/cerebrovascular emergency symptoms reported.");
                actions.Add("URGENT MEDICAL ATTENTION: Cease all exertion immediately and summon emergency medical assistance.");
                matchedRules.Add(new PEDRedFlagRule(
                    Guid.NewGuid(),
                    "Cardiovascular / Cerebrovascular Emergency",
                    "Emergency cardiovascular symptom pattern detected.",
                    "PED_Emergency_Cardiovascular",
                    EscalationLevel.UrgentMedicalAttention,
                    "Immediate emergency evaluation required.",
                    "Emergency medicine triage standard."));
            }
            else if (normalizedSignals.Any(s => s.Contains("jaundice") || s.Contains("yellow eyes") || s.Contains("yellow skin") || s.Contains("liver pain") || s.Contains("suicid") || s.Contains("psychos")))
            {
                levels.Add(EscalationLevel.HealthcareProfessionalReferral);
                rationales.Add("Severe organ dysfunction or psychiatric symptom pattern reported.");
                actions.Add("HEALTHCARE PROFESSIONAL REFERRAL: Prompt clinical physician referral required.");
                matchedRules.Add(new PEDRedFlagRule(
                    Guid.NewGuid(),
                    "Severe Organ / Psychiatric Red Flag",
                    "Clinical red flag symptom pattern detected.",
                    "PED_Organ_Psychiatric_Urgent",
                    EscalationLevel.HealthcareProfessionalReferral,
                    "Prompt medical referral required.",
                    "Clinical referral guidelines."));
            }
        }

        // 3. Check Substance Specific Safety Flags
        if (substance != null && substance.SafetyFlags.Count > 0)
        {
            foreach (var flag in substance.SafetyFlags)
            {
                rationales.Add($"Substance Precaution [{flag.Category} - {flag.Description}]: {flag.CoachNote}");
                if (flag.EscalationLevel > EscalationLevel.None)
                {
                    levels.Add(flag.EscalationLevel);
                }
            }
        }

        // 4. Resolve Highest Escalation Priority (UrgentMedicalAttention > HealthcareProfessionalReferral > CoachAwareness > None)
        EscalationLevel finalLevel;
        if (levels.Contains(EscalationLevel.UrgentMedicalAttention))
        {
            finalLevel = EscalationLevel.UrgentMedicalAttention;
        }
        else if (levels.Contains(EscalationLevel.HealthcareProfessionalReferral))
        {
            finalLevel = EscalationLevel.HealthcareProfessionalReferral;
        }
        else if (levels.Contains(EscalationLevel.CoachAwareness))
        {
            finalLevel = EscalationLevel.CoachAwareness;
        }
        else if (levels.Count > 0)
        {
            finalLevel = levels.Max();
        }
        else
        {
            finalLevel = EscalationLevel.CoachAwareness;
            rationales.Add("Unclassified symptoms reported. General coach review and conservative monitoring advised.");
            actions.Add("Review client training volume and verify whether symptoms resolve with rest.");
        }

        result.EscalationLevel = finalLevel;
        result.MatchedRedFlags = matchedRules.Select(r => r.Name).Distinct().ToList();
        result.TriggeredActions = actions.Distinct().ToList();
        result.SummaryRationale = string.Join(" | ", rationales.Distinct());
        result.RecommendedAction = actions.Count > 0
            ? string.Join(" ", actions.Distinct())
            : "Review client condition and consult clinical guidelines.";

        return result;
    }
}
