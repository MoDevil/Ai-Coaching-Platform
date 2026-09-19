namespace AiCoachOs.Domain.AnatomyAndBiomechanics;

/// <summary>
/// Represents the epistemic status of a biomechanical principle or coaching inference.
/// Avoids arbitrary numerical confidence scores by enforcing a qualitative hierarchy.
/// </summary>
public enum CertaintyLevel
{
    /// <summary>
    /// Directly supported by fundamental physical laws, verified biomechanical principles, or strong literature.
    /// </summary>
    Established = 1,

    /// <summary>
    /// A reasonable mechanical inference derived from established principles, but context-dependent or indirectly verified.
    /// </summary>
    Inferred = 2,

    /// <summary>
    /// A proposed hypothesis or context-sensitive interpretation where empirical and mechanical evidence remains uncertain.
    /// </summary>
    Hypothesis = 3
}
