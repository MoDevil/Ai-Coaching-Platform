using System.Text.Json.Serialization;

namespace AiCoachOs.Application.Photos.Dtos;

public class AnalyzePhotoRequestDto
{
    [JsonPropertyName("baselinePhotoId")]
    public Guid? BaselinePhotoId { get; set; }

    [JsonPropertyName("coachPrompt")]
    public string? CoachPrompt { get; set; }
}

public class PhysiqueObservationResult
{
    [JsonPropertyName("general_observations")]
    public string GeneralObservations { get; set; } = string.Empty;

    [JsonPropertyName("apparent_symmetry_notes")]
    public string ApparentSymmetryNotes { get; set; } = string.Empty;

    [JsonPropertyName("posture_observations")]
    public string PostureObservations { get; set; } = string.Empty;

    [JsonPropertyName("muscular_development_notes")]
    public string MuscularDevelopmentNotes { get; set; } = string.Empty;

    [JsonPropertyName("comparison_notes")]
    public string ComparisonNotes { get; set; } = string.Empty;

    [JsonPropertyName("limitations_statement")]
    public string LimitationsStatement { get; set; } = "Visual observations are qualitative estimates from 2D photos and do not constitute diagnostic or quantitative composition measurement.";

    [JsonPropertyName("coach_action_required")]
    public bool CoachActionRequired { get; set; } = true;

    [JsonPropertyName("confidence_statement")]
    public string ConfidenceStatement { get; set; } = "Qualitative observational assessment based on available visual lighting and posture.";
}

public class PhysiqueObservationResultDto
{
    [JsonPropertyName("photoId")]
    public Guid PhotoId { get; set; }

    [JsonPropertyName("memoryRecordId")]
    public Guid MemoryRecordId { get; set; }

    [JsonPropertyName("baselinePhotoId")]
    public Guid? BaselinePhotoId { get; set; }

    [JsonPropertyName("generalObservations")]
    public string GeneralObservations { get; set; } = string.Empty;

    [JsonPropertyName("apparentSymmetryNotes")]
    public string ApparentSymmetryNotes { get; set; } = string.Empty;

    [JsonPropertyName("postureObservations")]
    public string PostureObservations { get; set; } = string.Empty;

    [JsonPropertyName("muscularDevelopmentNotes")]
    public string MuscularDevelopmentNotes { get; set; } = string.Empty;

    [JsonPropertyName("comparisonNotes")]
    public string ComparisonNotes { get; set; } = string.Empty;

    [JsonPropertyName("limitationsStatement")]
    public string LimitationsStatement { get; set; } = string.Empty;

    [JsonPropertyName("coachActionRequired")]
    public bool CoachActionRequired { get; set; } = true;

    [JsonPropertyName("confidenceStatement")]
    public string ConfidenceStatement { get; set; } = string.Empty;

    [JsonPropertyName("analyzedAtUtc")]
    public DateTime AnalyzedAtUtc { get; set; }
}
