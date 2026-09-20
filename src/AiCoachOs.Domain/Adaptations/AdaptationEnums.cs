namespace AiCoachOs.Domain.Adaptations;

public enum AdaptationOverallStatus
{
    ProgramWorking = 1,
    ReviewRecommended = 2,
    ActionRequired = 3
}

public enum EffortAlignmentStatus
{
    Aligned = 1,
    HigherThanTarget = 2,
    LowerThanTarget = 3,
    InsufficientEvidence = 4
}

public enum PerformanceTrend
{
    Improving = 1,
    Stable = 2,
    Declining = 3,
    Insufficient = 4
}

public enum AdaptationActionType
{
    NoChange = 1,
    ProgressCurrentExercise = 2,
    ModifyEffortGuideline = 3,
    ModifySets = 4,
    ModifyRest = 5,
    ChangeExercise = 6,
    RedistributeSessionVolume = 7,
    CoachReview = 8,
    ReferToM8M9 = 9
}

public enum RecommendationStatus
{
    Pending = 1,
    ApprovedByCoach = 2,
    RejectedByCoach = 3,
    Applied = 4
}

public enum RecommendationConfidence
{
    Low = 1,
    Moderate = 2,
    High = 3
}
