namespace AiCoachOs.Domain.Safety;

public enum SafetyCategory
{
    NoSafetyConcern = 1,
    CautionCoachReview = 2,
    ReferToHealthcareProfessional = 3,
    UrgentMedicalAttention = 4
}

public enum SignalType
{
    Pain = 1,
    Discomfort = 2,
    Numbness = 3,
    Swelling = 4,
    Weakness = 5,
    Other = 6
}

public enum SignalOnset
{
    Unknown = 0,
    Sudden = 1,
    Gradual = 2
}

public enum SignalTiming
{
    Unknown = 0,
    DuringExercise = 1,
    AfterExercise = 2,
    AtRest = 3,
    Persistent = 4
}

public enum SignalSeverity
{
    Unknown = 0,
    Mild = 1,
    Moderate = 2,
    Severe = 3
}

public enum SignalDuration
{
    Unknown = 0,
    Acute = 1,
    Subacute = 2,
    Chronic = 3
}

public enum SafetyActionType
{
    NoActionRequired = 1,
    ContinueWithCaution = 2,
    PauseActivityPendingAssessment = 3,
    CoachReviewRequired = 4,
    ReferToHealthcareProfessional = 5,
    UrgentMedicalAttention = 6
}

public enum TriggeredByType
{
    WorkoutSession = 1,
    DirectReport = 2,
    AdaptationReview = 3
}
