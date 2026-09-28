using System.Text.Json.Serialization;

namespace AiCoachOs.Domain.Memory;

/// <summary>
/// Structured domain content for Preference and Aversion memory records.
/// </summary>
public record PreferenceContent
{
    public PreferenceSubjectType SubjectType { get; init; }
    public string SubjectId { get; init; } = string.Empty;
    public string SubjectLabel { get; init; } = string.Empty;
    public PreferenceSentiment Sentiment { get; init; }
    public string? StrengthNote { get; init; }
    public string? CoachNote { get; init; }

    [JsonConstructor]
    public PreferenceContent(
        PreferenceSubjectType subjectType,
        string subjectId,
        string subjectLabel,
        PreferenceSentiment sentiment,
        string? strengthNote = null,
        string? coachNote = null)
    {
        SubjectType = subjectType;
        SubjectId = subjectId?.Trim() ?? string.Empty;
        SubjectLabel = subjectLabel?.Trim() ?? string.Empty;
        Sentiment = sentiment;
        StrengthNote = string.IsNullOrWhiteSpace(strengthNote) ? null : strengthNote.Trim();
        CoachNote = string.IsNullOrWhiteSpace(coachNote) ? null : coachNote.Trim();
    }
}

/// <summary>
/// Structured domain content for PainObservation memory records.
/// </summary>
public record PainObservationContent
{
    public string AnatomicalRegion { get; init; } = string.Empty;
    public string AnatomicalRegionKey { get; init; } = string.Empty;
    public string? PainCharacter { get; init; }
    public string? OnsetContext { get; init; }
    public int? Severity { get; init; }
    public string? CoachNote { get; init; }

    [JsonConstructor]
    public PainObservationContent(
        string anatomicalRegion,
        string anatomicalRegionKey,
        string? painCharacter = null,
        string? onsetContext = null,
        int? severity = null,
        string? coachNote = null)
    {
        AnatomicalRegion = anatomicalRegion?.Trim() ?? string.Empty;
        AnatomicalRegionKey = anatomicalRegionKey?.Trim().ToLowerInvariant() ?? string.Empty;
        PainCharacter = string.IsNullOrWhiteSpace(painCharacter) ? null : painCharacter.Trim();
        OnsetContext = string.IsNullOrWhiteSpace(onsetContext) ? null : onsetContext.Trim();
        Severity = severity;
        CoachNote = string.IsNullOrWhiteSpace(coachNote) ? null : coachNote.Trim();
    }
}
