using System.Text.RegularExpressions;
using AiCoachOs.Application.Photos.Dtos;

namespace AiCoachOs.Infrastructure.Photos;

/// <summary>
/// Deterministic post-parsing language sanitizer for AI vision observation results.
/// The LLM is never trusted to enforce safety boundaries by system prompt alone.
/// Strips quantitative body-fat %, medical diagnoses, pathology claims, and absolute certainty claims.
/// </summary>
public static class PhotoVisionSafetySanitizer
{
    private static readonly Regex BodyFatPercentageRegex = new(
        @"\b(\d{1,2}(?:\.\d+)?)\s*%\s*(?:body\s*fat|bf|bodyfat)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex BodyFatPrefixRegex = new(
        @"\b(?:body\s*fat|bf|bodyfat)\s*(?:is|at|estimated\s*at|around|approximately)?\s*(\d{1,2}(?:\.\d+)?)\s*%",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MuscleMassPercentageRegex = new(
        @"\b(\d{1,2}(?:\.\d+)?)\s*%\s*(?:muscle\s*mass|skeletal\s*muscle|lean\s*mass)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MedicalDiagnosisRegex = new(
        @"\b(diagnos(?:e|is|ed|ing)|pathology|pathological|scoliosis|kyphosis\s*disease|gynecomastia|edema\s*condition|clinical\s*disorder)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ProhibitedCertaintyRegex = new(
        @"\b(definitely\s+proves|conclusive\s+evidence\s+of|unquestionable\s+proof|absolute\s+certainty)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static PhysiqueObservationResult Sanitize(PhysiqueObservationResult input)
    {
        if (input == null)
            return new PhysiqueObservationResult();

        var sanitized = new PhysiqueObservationResult
        {
            GeneralObservations = SanitizeText(input.GeneralObservations),
            ApparentSymmetryNotes = SanitizeText(input.ApparentSymmetryNotes),
            PostureObservations = SanitizeText(input.PostureObservations),
            MuscularDevelopmentNotes = SanitizeText(input.MuscularDevelopmentNotes),
            ComparisonNotes = SanitizeText(input.ComparisonNotes),
            LimitationsStatement = string.IsNullOrWhiteSpace(input.LimitationsStatement)
                ? "Visual observations are qualitative estimates from 2D photos and do not constitute diagnostic or quantitative composition measurement."
                : SanitizeText(input.LimitationsStatement),
            CoachActionRequired = true, // Deterministically forced true
            ConfidenceStatement = string.IsNullOrWhiteSpace(input.ConfidenceStatement)
                ? "Qualitative observational assessment based on available visual lighting and posture."
                : SanitizeText(input.ConfidenceStatement)
        };

        return sanitized;
    }

    public static string SanitizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var result = text;

        // Redact body fat percentage claims
        result = BodyFatPercentageRegex.Replace(result, "[body composition definition]");
        result = BodyFatPrefixRegex.Replace(result, "visual muscular definition");

        // Redact muscle mass percentage claims
        result = MuscleMassPercentageRegex.Replace(result, "[muscular development]");

        // Neutralize medical diagnosis terms
        result = MedicalDiagnosisRegex.Replace(result, "observed visual alignment/contour");

        // Neutralize prohibited certainty terms
        result = ProhibitedCertaintyRegex.Replace(result, "observations suggest");

        return result.Trim();
    }
}
