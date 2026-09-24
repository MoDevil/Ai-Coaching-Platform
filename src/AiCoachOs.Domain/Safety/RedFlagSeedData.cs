using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Safety;

namespace AiCoachOs.Domain.Safety;

public static class RedFlagSeedData
{
    public static IReadOnlyList<RedFlagRule> GetInitialSeedRules()
    {
        return new List<RedFlagRule>
        {
            // Emergency / Cardiovascular -> UrgentMedicalAttention
            new RedFlagRule(
                id: Guid.Parse("11111111-0000-0000-0000-000000000001"),
                name: "Cardiovascular Symptoms During/After Exercise",
                description: "Chest pain, pressure, severe unexplained shortness of breath, palpitations, or loss of consciousness during or following physical activity.",
                signalPattern: "Emergency_Cardiovascular",
                safetyCategoryTriggered: SafetyCategory.UrgentMedicalAttention,
                recommendedAction: SafetyActionType.UrgentMedicalAttention,
                evidenceBasis: "Standard emergency triage guidelines for exercise-induced cardiopulmonary symptoms.",
                requiresClinicalReview: true,
                reviewedBy: "M8 initial seed — requires clinical validation"),

            // Neurological Symptoms -> ReferToHealthcareProfessional
            new RedFlagRule(
                id: Guid.Parse("11111111-0000-0000-0000-000000000002"),
                name: "New Neurological Deficits",
                description: "New onset of numbness, tingling, radiating nerve sensation, unexplained motor weakness, or loss of coordination.",
                signalPattern: "Neurological_Deficit",
                safetyCategoryTriggered: SafetyCategory.ReferToHealthcareProfessional,
                recommendedAction: SafetyActionType.ReferToHealthcareProfessional,
                evidenceBasis: "General screening guidelines for radicular and central neurological signs in athletic populations.",
                requiresClinicalReview: true,
                reviewedBy: "M8 initial seed — requires clinical validation"),

            // Acute Severe Musculoskeletal -> ReferToHealthcareProfessional / Urgent
            new RedFlagRule(
                id: Guid.Parse("11111111-0000-0000-0000-000000000003"),
                name: "Acute Severe Musculoskeletal Trauma",
                description: "Sudden onset severe pain, audible/felt pop or tearing sensation with immediate dysfunction, immediate rapid swelling, or inability to bear weight.",
                signalPattern: "Acute_Severe_MSK",
                safetyCategoryTriggered: SafetyCategory.ReferToHealthcareProfessional,
                recommendedAction: SafetyActionType.PauseActivityPendingAssessment,
                evidenceBasis: "Clinical sports medicine principles for acute structural soft tissue or bony injury screening.",
                requiresClinicalReview: true,
                reviewedBy: "M8 initial seed — requires clinical validation"),

            // Systemic & Contextual Red Flags -> ReferToHealthcareProfessional
            new RedFlagRule(
                id: Guid.Parse("11111111-0000-0000-0000-000000000004"),
                name: "Persistent Non-Mechanical or Night Pain",
                description: "Constant pain at rest unalleviated by positional changes, night pain disrupting sleep, or progressive pain with systemic malaise.",
                signalPattern: "Systemic_NonMechanical",
                safetyCategoryTriggered: SafetyCategory.ReferToHealthcareProfessional,
                recommendedAction: SafetyActionType.ReferToHealthcareProfessional,
                evidenceBasis: "Red flag screening protocols for non-musculoskeletal and systemic etiologies.",
                requiresClinicalReview: true,
                reviewedBy: "M8 initial seed — requires clinical validation"),

            // Persistence Pattern (>= 3 sessions) -> ReferToHealthcareProfessional
            new RedFlagRule(
                id: Guid.Parse("11111111-0000-0000-0000-000000000005"),
                name: "Symptom Persistence Across >=3 Sessions",
                description: "Same anatomical region and signal type persisting across 3 or more logged sessions without resolution.",
                signalPattern: "Persistence_3Sessions",
                safetyCategoryTriggered: SafetyCategory.ReferToHealthcareProfessional,
                recommendedAction: SafetyActionType.ReferToHealthcareProfessional,
                evidenceBasis: "Conservative load management principles: persistent localized symptoms warrant professional evaluation before progression.",
                requiresClinicalReview: true,
                reviewedBy: "M8 initial seed — requires clinical validation"),

            // Worsening Severity -> ReferToHealthcareProfessional
            new RedFlagRule(
                id: Guid.Parse("11111111-0000-0000-0000-000000000006"),
                name: "Progressive / Worsening Severity Across Sessions",
                description: "Reported symptom severity progressively increases across consecutive training exposures.",
                signalPattern: "Worsening_Severity",
                safetyCategoryTriggered: SafetyCategory.ReferToHealthcareProfessional,
                recommendedAction: SafetyActionType.PauseActivityPendingAssessment,
                evidenceBasis: "Progressive symptom escalation indicates non-tolerance to current loading and potential structural overload.",
                requiresClinicalReview: true,
                reviewedBy: "M8 initial seed — requires clinical validation")
        };
    }
}
