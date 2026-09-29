using System.Text.RegularExpressions;
using AiCoachOs.Application.Photos.Dtos;

namespace AiCoachOs.Infrastructure.Photos;

/// <summary>
/// Deterministic post-parsing language sanitizer for AI vision observation results.
/// The LLM is never trusted to enforce safety boundaries by system prompt alone.
/// Redacts quantitative body-fat %, muscle mass %, medical diagnoses, injury inferences,
/// and prohibited certainty claims with '[observation removed — language boundary]'.
/// </summary>
public static class PhotoVisionSafetySanitizer
{
    public const string RemovalSentinel = "[observation removed — language boundary]";

    private static readonly Regex BodyFatPercentageRegex = new(
        @"\b(\d{1,2}(?:\.\d+)?)\s*%\s*(?:body\s*fat|bf|bodyfat)\b|\b(?:body\s*fat|bf|bodyfat)\s*(?:is|at|estimated\s*at|around|approximately)?\s*(\d{1,2}(?:\.\d+)?)\s*%",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MuscleMassPercentageRegex = new(
        @"\b(\d{1,2}(?:\.\d+)?)\s*%\s*(?:muscle\s*mass|skeletal\s*muscle|lean\s*mass|body\s*composition)\b|\b(?:muscle\s*mass|skeletal\s*muscle|lean\s*mass)\s*(?:is|at|estimated\s*at|around|approximately)?\s*(\d{1,2}(?:\.\d+)?)\s*%",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DiagnosisAndInferenceRegex = new(
        @"\b(you\s+have|this\s+indicates|this\s+is\s+consistent\s+with|diagnos(?:e|is|ed|ing)|pathology|pathological|scoliosis|lordosis|kyphosis|gynecomastia|edema|clinical\s*disorder|injury|torn|strain|sprain|herniated|inflammation|tendonitis|impingement|bursitis)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ProhibitedCertaintyRegex = new(
        @"(?:\b(?:clearly|definitely|certainly|guaranteed)\b|100\s*%)",
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

        // Redact body fat % claims
        result = BodyFatPercentageRegex.Replace(result, RemovalSentinel);

        // Redact muscle / body-composition % claims
        result = MuscleMassPercentageRegex.Replace(result, RemovalSentinel);

        // Neutralize diagnosis / pathology / medical inference phrases
        result = DiagnosisAndInferenceRegex.Replace(result, RemovalSentinel);

        // Neutralize prohibited certainty terms
        result = ProhibitedCertaintyRegex.Replace(result, RemovalSentinel);

        return result.Trim();
    }
}
