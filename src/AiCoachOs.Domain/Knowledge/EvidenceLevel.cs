namespace AiCoachOs.Domain.Knowledge;

/// <summary>
/// Categorical hierarchy of scientific and empirical evidence quality in AI Coach OS.
/// Follows the Master Plan evidence pyramid: syntheses/meta-analyses at the top,
/// down to mechanistic rationale and anecdotal observations.
/// </summary>
public enum EvidenceLevel
{
    Anecdotal = 1,
    Mechanistic = 2,
    ExpertConsensus = 3,
    RandomizedControlledTrial = 4,
    MetaAnalysis = 5
}
