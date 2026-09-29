using System.Text.Json.Serialization;

namespace AiCoachOs.Application.Videos.Dtos;

public class VideoObservationResult
{
    [JsonPropertyName("movement_execution_notes")]
    public string MovementExecutionNotes { get; set; } = string.Empty;

    [JsonPropertyName("joint_alignment_notes")]
    public string JointAlignmentNotes { get; set; } = string.Empty;

    [JsonPropertyName("range_of_motion_notes")]
    public string RangeOfMotionNotes { get; set; } = string.Empty;

    [JsonPropertyName("tempo_and_control_notes")]
    public string TempoAndControlNotes { get; set; } = string.Empty;

    [JsonPropertyName("limitations_statement")]
    public string LimitationsStatement { get; set; } = "Visual observations from video frames are qualitative movement cues and do not constitute biomechanical lab measurement or medical diagnosis.";

    [JsonPropertyName("coach_action_required")]
    public bool CoachActionRequired { get; set; } = true;

    [JsonPropertyName("confidence_statement")]
    public string ConfidenceStatement { get; set; } = string.Empty;
}
