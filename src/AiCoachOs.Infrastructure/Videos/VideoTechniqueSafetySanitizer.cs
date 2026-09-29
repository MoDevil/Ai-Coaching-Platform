using System.Text.RegularExpressions;
using AiCoachOs.Application.Videos.Dtos;

namespace AiCoachOs.Infrastructure.Videos;

/// <summary>
/// Deterministic post-parsing language sanitizer for AI video technique observation results.
/// Redacts diagnostic claims, injury inferences, body fat/composition claims, and prohibited certainty assertions.
/// </summary>
public static class VideoTechniqueSafetySanitizer
{
    public const string RemovalSentinel = "[observation removed — language boundary]";
    public const string DefaultLimitationsStatement = "Visual observations from video frames are qualitative movement cues and do not constitute biomechanical lab measurement or medical diagnosis.";
    public const string DefaultConfidenceStatement = "Qualitative observational movement review based on extracted 2D video frames.";

    private static readonly Regex BodyFatPercentageRegex = new(
        @"\b(\d{1,2}(?:\.\d+)?)\s*%\s*(?:body\s*fat|bf|bodyfat)\b|\b(?:body\s*fat|bf|bodyfat)\s*(?:is|at|estimated\s*at|around|approximately)?\s*(\d{1,2}(?:\.\d+)?)\s*%",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MuscleMassPercentageRegex = new(
        @"\b(\d{1,2}(?:\.\d+)?)\s*%\s*(?:muscle\s*mass|skeletal\s*muscle|lean\s*mass|body\s*composition)\b|\b(?:muscle\s*mass|skeletal\s*muscle|lean\s*mass)\s*(?:is|at|estimated\s*at|around|approximately)?\s*(\d{1,2}(?:\.\d+)?)\s*%",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DiagnosisAndInferenceRegex = new(
        @"\b(you\s+have|this\s+indicates|(?:\w+\s+)?is\s+consistent\s+with|diagnos(?:e|is|ed|ing)|pathology|pathological|scoliosis|lordosis|kyphosis|disc\s*herniation|herniated|herniation|impingement|bursitis|tendonitis|arthritis|tear|torn|rupture|clinical\s*disorder|injury|strain|sprain|inflammation|damage|chronic\s*condition|syndrome|patellofemoral|surgery|surgical)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ProhibitedCertaintyRegex = new(
        @"(?:\b(?:clearly|definitely|certainly|guaranteed)\b|100\s*%)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static VideoObservationResult Sanitize(VideoObservationResult input)
    {
        if (input == null)
            return new VideoObservationResult();

        var sanitized = new VideoObservationResult
        {
            MovementExecutionNotes = SanitizeText(input.MovementExecutionNotes),
            JointAlignmentNotes = SanitizeText(input.JointAlignmentNotes),
            RangeOfMotionNotes = SanitizeText(input.RangeOfMotionNotes),
            TempoAndControlNotes = SanitizeText(input.TempoAndControlNotes),
            LimitationsStatement = string.IsNullOrWhiteSpace(input.LimitationsStatement)
                ? DefaultLimitationsStatement
                : SanitizeText(input.LimitationsStatement),
            CoachActionRequired = true, // Deterministically forced true
            ConfidenceStatement = string.IsNullOrWhiteSpace(input.ConfidenceStatement)
                ? DefaultConfidenceStatement
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

        // Redact muscle mass / body composition % claims
        result = MuscleMassPercentageRegex.Replace(result, RemovalSentinel);

        // Redact diagnosis, pathology, and injury inferences
        result = DiagnosisAndInferenceRegex.Replace(result, RemovalSentinel);

        // Neutralize prohibited certainty terms
        result = ProhibitedCertaintyRegex.Replace(result, RemovalSentinel);

        return result.Trim();
    }
}
