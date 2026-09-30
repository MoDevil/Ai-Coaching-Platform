using System.Text.RegularExpressions;

namespace AiCoachOs.Infrastructure.ExpertIngestion;

/// <summary>
/// Deterministic scanner for clinical, pharmaceutical, or medical claims.
/// Detects medical terminology and flags content to prevent unsupervised medical coaching advice.
/// Zero LLM calls.
/// </summary>
public static class MedicalContentDetector
{
    public const string MedicalWarningNotice = "This content may contain medical claims. Review carefully. AI Coach OS does not validate medical advice.";

    private static readonly Regex MedicalTermsRegex = new(
        @"\b(treat(?:ment|s|ing|ed)?|cure|curing|prescrib(?:e|ed|ing|tion)|diagnos(?:e|ed|ing|is)|pathology|pathological|oncology|chemotherapy|radiation\s+therapy|insulin\s+protocol|pharmacology|pharmaceutical|steroid\s+cycle|anabolic\s+steroid\s+protocol|injury\s+rehab(?:ilitation)?\s+protocol|clinical\s+condition|disease\s+management|pediatric\s+treatment|cardiac\s+rehab)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly string[] MedicalWarningPhrases = new[]
    {
        "diagnosed with",
        "cure for",
        "prescription for",
        "medical treatment for",
        "treat cancer",
        "treat diabetes",
        "treat hypertension",
        "cure disease",
        "medical therapy"
    };

    public static bool ScanForMedicalContent(string text, out string? detectionReason)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            detectionReason = null;
            return false;
        }

        foreach (var phrase in MedicalWarningPhrases)
        {
            if (text.Contains(phrase, StringComparison.OrdinalIgnoreCase))
            {
                detectionReason = $"Detected medical phrase: '{phrase}'.";
                return true;
            }
        }

        var match = MedicalTermsRegex.Match(text);
        if (match.Success)
        {
            detectionReason = $"Detected clinical/medical terminology: '{match.Value}'.";
            return true;
        }

        detectionReason = null;
        return false;
    }
}
