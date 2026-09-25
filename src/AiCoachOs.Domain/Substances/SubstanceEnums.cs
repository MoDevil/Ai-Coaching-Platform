namespace AiCoachOs.Domain.Substances;

public enum SubstanceCategory
{
    Supplement = 1,
    Hormone = 2,
    PED = 3
}

public enum SupplementEvidenceStatus
{
    StrongEvidence = 1,
    ModerateEvidence = 2,
    Preliminary = 3,
    InsufficientEvidence = 4,
    Disproven = 5
}

public enum EffectMagnitude
{
    None = 0,
    Small = 1,
    Moderate = 2,
    Large = 3,
    Unclear = 4
}

public enum SafetyFlagCategory
{
    Contraindication = 1,
    AdverseInteraction = 2,
    HighDoseToxicity = 3,
    SpecialPopulationPrecaution = 4,
    OrganStressPrecaution = 5
}

public enum HormoneCategory
{
    Androgen = 1,
    Glucocorticoid = 2,
    Thyroid = 3,
    PeptideGrowth = 4,
    MetabolicEnergy = 5,
    EstrogenProgestin = 6
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

public enum RiskCategory
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

public enum EscalationLevel
{
    None = 0,
    CoachAwareness = 1,
    HealthcareProfessionalReferral = 2,
    UrgentMedicalAttention = 3
}
