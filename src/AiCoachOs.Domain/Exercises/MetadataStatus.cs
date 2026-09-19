namespace AiCoachOs.Domain.Exercises;

/// <summary>
/// Indicates the verification status of qualitative coaching metadata for an exercise.
/// Differentiates provisional coaching heuristics/estimates from future evidence-backed claims.
/// </summary>
public enum MetadataStatus
{
    Provisional = 1,
    Verified = 2
}
