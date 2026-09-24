namespace AiCoachOs.Domain.Rehab;

public enum LimitationSource
{
    ReportedByClient = 1,
    CoachObserved = 2,
    PostReferral = 3
}

public enum LimitationStatus
{
    Active = 1,
    Resolved = 2,
    OnHold = 3
}

public enum ConsiderationType
{
    ReduceLoad = 1,
    ReduceROM = 2,
    ReduceProximityToFailure = 3,
    ReduceSets = 4,
    ReduceFrequencyOnRegion = 5,
    IncreaseRest = 6,
    ModifyTempo = 7,
    TemporaryExerciseExclusion = 8,
    AlternativeExercise = 9,
    TechniqueSetupModification = 10,
    GradedLoadingConsideration = 11
}

public enum ConsiderationStatus
{
    Pending = 1,
    ApprovedByCoach = 2,
    RejectedByCoach = 3,
    Applied = 4
}
