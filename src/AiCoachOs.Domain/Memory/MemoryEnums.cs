namespace AiCoachOs.Domain.Memory;

public enum MemoryCategory
{
    Preference = 1,
    Aversion = 2,
    PainObservation = 3,
    LifeEvent = 4,
    GoalContext = 5,
    EquipmentConstraint = 6,
    ScheduleConstraint = 7,
    NutritionHabit = 8,
    AdherenceNote = 9,
    RecoveryNote = 10,
    GeneralNote = 11,
    UnresolvedQuestion = 12,
    PhysiqueObservation = 13,
    ExerciseTechniqueObservation = 14
}

public enum MemorySourceType
{
    SystemGenerated = 1,
    CoachRecorded = 2,
    CoachCorrected = 3,
    AIGenerated = 4
}

public enum MemoryConfidenceLevel
{
    Confirmed = 1,
    Provisional = 2,
    Uncertain = 3,
    Conflicted = 4,
    Superseded = 5
}

public enum MemoryRecordStatus
{
    Active = 1,
    Conflicted = 2,
    Superseded = 3,
    Archived = 4,
    Anonymized = 5
}

public enum PreferenceSubjectType
{
    Exercise = 1,
    MuscleGroup = 2,
    TrainingTime = 3,
    Equipment = 4,
    FoodItem = 5
}

public enum PreferenceSentiment
{
    Prefers = 1,
    Avoids = 2,
    Neutral = 3
}

public enum SnapshotGenerationTrigger
{
    Manual = 1,
    ProgramDesign = 2,
    SessionReview = 3,
    CheckIn = 4
}

public enum AIRecommendationCategory
{
    ProgramAdaptationReview = 1,
    NutritionAdjustmentReview = 2,
    ExerciseModificationReview = 3,
    SafetyContextSummary = 4,
    GeneralCoachingNote = 5,
    ExerciseTechniqueObservation = 6
}

public enum ReasoningCategory
{
    ProgramAdaptationReview = 1,
    NutritionAdjustmentReview = 2,
    ExerciseModificationReview = 3,
    SafetyContextSummary = 4,
    GeneralCoachingNote = 5,
    ExerciseTechniqueObservation = 6
}

public enum AIRecommendationReviewStatus
{
    PendingReview = 1,
    UnderReview = 2,
    Accepted = 3,
    Rejected = 4,
    Archived = 5
}

public enum CoachDecisionOutcome
{
    Accepted = 1,
    AcceptedWithModifications = 2,
    Rejected = 3,
    Deferred = 4
}
