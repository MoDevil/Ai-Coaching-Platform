namespace AiCoachOs.Domain.Substances;

public enum SubstanceCategory
{
    Supplement = 1,
    Hormone = 2,
    PED = 3
}

public enum SupplementCategory
{
    Performance = 1,
    HealthAndWellness = 2,
    Recovery = 3,
    BodyComposition = 4
}

public enum HormoneAxis
{
    HPTA = 1,
    Thyroid = 2,
    Adrenal = 3,
    GrowthHormone = 4,
    InsulinGlucose = 5
}

public enum PEDCategory
{
    AAS = 1,
    SARM = 2,
    Peptide = 3,
    GrowthHormoneSecretagogue = 4,
    Stimulant = 5,
    Diuretic = 6,
    SERM = 7,
    AromataseInhibitor = 8,
    Other = 9
}

public enum OrganSystem
{
    Cardiovascular = 1,
    Hepatic = 2,
    Renal = 3,
    Endocrine = 4,
    Psychiatric = 5,
    Dermatological = 6,
    Hematological = 7,
    Musculoskeletal = 8,
    Other = 9
}

public enum PEDRiskSeverity
{
    Low = 1,
    Moderate = 2,
    High = 3,
    Critical = 4
}

public enum SubstanceEscalationLevel
{
    None = 0,
    CautionCoachReview = 1,
    UrgentMedicalReferral = 2,
    EmergencyMedicalAttention = 3
}
