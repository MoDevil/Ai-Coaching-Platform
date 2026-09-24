using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Coaches;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.TrainingProfiles;

namespace AiCoachOs.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Coach> Coaches { get; }
    IQueryable<Client> Clients { get; }
    IQueryable<ConsentRecord> ConsentRecords { get; }

    IQueryable<Exercise> Exercises { get; }
    IQueryable<Muscle> Muscles { get; }
    IQueryable<MovementPattern> MovementPatterns { get; }
    IQueryable<Equipment> Equipment { get; }
    IQueryable<ExerciseSubstitution> ExerciseSubstitutions { get; }
    IQueryable<ClientTrainingProfile> ClientTrainingProfiles { get; }
    IQueryable<ClientTrainingPriority> ClientTrainingPriorities { get; }

    IQueryable<KnowledgeSource> KnowledgeSources { get; }
    IQueryable<KnowledgeClaim> KnowledgeClaims { get; }
    IQueryable<KnowledgeClaimSource> KnowledgeClaimSources { get; }

    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.AnatomicalRegion> AnatomicalRegions { get; }
    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.Joint> Joints { get; }
    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.JointAction> JointActions { get; }
    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.MuscleJointAction> MuscleJointActions { get; }
    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.ExerciseJointAction> ExerciseJointActions { get; }
    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.BiomechanicalConsideration> BiomechanicalConsiderations { get; }

    IQueryable<AiCoachOs.Domain.Programs.Program> Programs { get; }
    IQueryable<AiCoachOs.Domain.Programs.ProgramVersion> ProgramVersions { get; }
    IQueryable<AiCoachOs.Domain.Programs.TrainingWeek> TrainingWeeks { get; }
    IQueryable<AiCoachOs.Domain.Programs.TrainingSession> TrainingSessions { get; }
    IQueryable<AiCoachOs.Domain.Programs.ExerciseSlot> ExerciseSlots { get; }
    IQueryable<AiCoachOs.Domain.Programs.ProgramMusclePriority> ProgramMusclePriorities { get; }

    IQueryable<AiCoachOs.Domain.Workouts.WorkoutSession> WorkoutSessions { get; }
    IQueryable<AiCoachOs.Domain.Workouts.WorkoutExercise> WorkoutExercises { get; }
    IQueryable<AiCoachOs.Domain.Workouts.WorkoutSet> WorkoutSets { get; }

    IQueryable<AiCoachOs.Domain.Adaptations.AdaptationAssessment> AdaptationAssessments { get; }
    IQueryable<AiCoachOs.Domain.Adaptations.ExerciseAdaptationRecord> ExerciseAdaptationRecords { get; }
    IQueryable<AiCoachOs.Domain.Adaptations.AdaptationRecommendation> AdaptationRecommendations { get; }

    IQueryable<AiCoachOs.Domain.Safety.SafetyScreening> SafetyScreenings { get; }
    IQueryable<AiCoachOs.Domain.Safety.RedFlagRule> RedFlagRules { get; }

    IQueryable<AiCoachOs.Domain.Rehab.TrainingLimitation> TrainingLimitations { get; }
    IQueryable<AiCoachOs.Domain.Rehab.RehabAwarenessConsideration> RehabAwarenessConsiderations { get; }

    IQueryable<AiCoachOs.Domain.Nutrition.ClientNutritionProfile> ClientNutritionProfiles { get; }
    IQueryable<AiCoachOs.Domain.Nutrition.NutritionCalibrationRecord> NutritionCalibrationRecords { get; }
    IQueryable<AiCoachOs.Domain.Nutrition.EgyptianFood> EgyptianFoods { get; }

    IQueryable<AiCoachOs.Domain.Gyms.GymProfile> GymProfiles { get; }

    Task AddCoachAsync(Coach coach, CancellationToken cancellationToken = default);
    Task AddClientAsync(Client client, CancellationToken cancellationToken = default);
    Task AddConsentRecordAsync(ConsentRecord consentRecord, CancellationToken cancellationToken = default);
    Task AddExerciseAsync(Exercise exercise, CancellationToken cancellationToken = default);
    Task AddTrainingProfileAsync(ClientTrainingProfile profile, CancellationToken cancellationToken = default);
    Task AddKnowledgeSourceAsync(KnowledgeSource source, CancellationToken cancellationToken = default);
    Task AddKnowledgeClaimAsync(KnowledgeClaim claim, CancellationToken cancellationToken = default);
    Task AddBiomechanicalConsiderationAsync(AiCoachOs.Domain.AnatomyAndBiomechanics.BiomechanicalConsideration consideration, CancellationToken cancellationToken = default);
    Task AddProgramAsync(AiCoachOs.Domain.Programs.Program program, CancellationToken cancellationToken = default);
    Task AddProgramVersionAsync(AiCoachOs.Domain.Programs.ProgramVersion version, CancellationToken cancellationToken = default);
    Task AddWorkoutSessionAsync(AiCoachOs.Domain.Workouts.WorkoutSession session, CancellationToken cancellationToken = default);
    Task AddWorkoutExerciseAsync(AiCoachOs.Domain.Workouts.WorkoutExercise exercise, CancellationToken cancellationToken = default);
    Task AddWorkoutSetAsync(AiCoachOs.Domain.Workouts.WorkoutSet set, CancellationToken cancellationToken = default);
    Task AddAdaptationAssessmentAsync(AiCoachOs.Domain.Adaptations.AdaptationAssessment assessment, CancellationToken cancellationToken = default);
    Task AddSafetyScreeningAsync(AiCoachOs.Domain.Safety.SafetyScreening screening, CancellationToken cancellationToken = default);
    Task AddRedFlagRuleAsync(AiCoachOs.Domain.Safety.RedFlagRule rule, CancellationToken cancellationToken = default);
    Task AddTrainingLimitationAsync(AiCoachOs.Domain.Rehab.TrainingLimitation limitation, CancellationToken cancellationToken = default);
    Task AddRehabAwarenessConsiderationAsync(AiCoachOs.Domain.Rehab.RehabAwarenessConsideration consideration, CancellationToken cancellationToken = default);
    Task AddClientNutritionProfileAsync(AiCoachOs.Domain.Nutrition.ClientNutritionProfile profile, CancellationToken cancellationToken = default);
    Task AddNutritionCalibrationRecordAsync(AiCoachOs.Domain.Nutrition.NutritionCalibrationRecord record, CancellationToken cancellationToken = default);
    Task AddEgyptianFoodAsync(AiCoachOs.Domain.Nutrition.EgyptianFood food, CancellationToken cancellationToken = default);
    Task AddGymProfileAsync(AiCoachOs.Domain.Gyms.GymProfile gym, CancellationToken cancellationToken = default);
    void RemoveGymProfile(AiCoachOs.Domain.Gyms.GymProfile gym);

    Task<Coach?> FindCoachByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Coach?> FindCoachByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);
    Task<Client?> FindClientByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Exercise?> FindExerciseByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClientTrainingProfile?> FindTrainingProfileByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<KnowledgeSource?> FindKnowledgeSourceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeClaim?> FindKnowledgeClaimByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.AnatomyAndBiomechanics.AnatomicalRegion?> FindAnatomicalRegionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.AnatomyAndBiomechanics.Joint?> FindJointByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.AnatomyAndBiomechanics.JointAction?> FindJointActionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.Programs.Program?> FindProgramByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.Workouts.WorkoutSession?> FindWorkoutSessionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.Adaptations.AdaptationAssessment?> FindAdaptationAssessmentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.Safety.SafetyScreening?> FindSafetyScreeningByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.Safety.RedFlagRule?> FindRedFlagRuleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.Rehab.TrainingLimitation?> FindTrainingLimitationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.Rehab.RehabAwarenessConsideration?> FindRehabAwarenessConsiderationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.Nutrition.ClientNutritionProfile?> FindNutritionProfileByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.Nutrition.NutritionCalibrationRecord?> FindNutritionCalibrationRecordByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.Nutrition.EgyptianFood?> FindEgyptianFoodByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiCoachOs.Domain.Gyms.GymProfile?> FindGymProfileByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
