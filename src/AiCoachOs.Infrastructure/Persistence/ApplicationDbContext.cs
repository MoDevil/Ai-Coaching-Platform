using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Coaches;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.TrainingProfiles;
using AiCoachOs.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Coach> CoachesDbSet => Set<Coach>();
    public DbSet<Client> ClientsDbSet => Set<Client>();
    public DbSet<ConsentRecord> ConsentRecordsDbSet => Set<ConsentRecord>();

    public DbSet<Exercise> ExercisesDbSet => Set<Exercise>();
    public DbSet<Muscle> MusclesDbSet => Set<Muscle>();
    public DbSet<MovementPattern> MovementPatternsDbSet => Set<MovementPattern>();
    public DbSet<Equipment> EquipmentDbSet => Set<Equipment>();
    public DbSet<ExerciseMuscle> ExerciseMusclesDbSet => Set<ExerciseMuscle>();
    public DbSet<ExerciseEquipment> ExerciseEquipmentDbSet => Set<ExerciseEquipment>();
    public DbSet<ExerciseSubstitution> ExerciseSubstitutionsDbSet => Set<ExerciseSubstitution>();

    public DbSet<ClientTrainingProfile> ClientTrainingProfilesDbSet => Set<ClientTrainingProfile>();
    public DbSet<ClientTrainingPriority> ClientTrainingPrioritiesDbSet => Set<ClientTrainingPriority>();

    public DbSet<KnowledgeSource> KnowledgeSourcesDbSet => Set<KnowledgeSource>();
    public DbSet<KnowledgeClaim> KnowledgeClaimsDbSet => Set<KnowledgeClaim>();
    public DbSet<KnowledgeClaimSource> KnowledgeClaimSourcesDbSet => Set<KnowledgeClaimSource>();

    public DbSet<AiCoachOs.Domain.AnatomyAndBiomechanics.AnatomicalRegion> AnatomicalRegionsDbSet => Set<AiCoachOs.Domain.AnatomyAndBiomechanics.AnatomicalRegion>();
    public DbSet<AiCoachOs.Domain.AnatomyAndBiomechanics.Joint> JointsDbSet => Set<AiCoachOs.Domain.AnatomyAndBiomechanics.Joint>();
    public DbSet<AiCoachOs.Domain.AnatomyAndBiomechanics.JointAction> JointActionsDbSet => Set<AiCoachOs.Domain.AnatomyAndBiomechanics.JointAction>();
    public DbSet<AiCoachOs.Domain.AnatomyAndBiomechanics.MuscleJointAction> MuscleJointActionsDbSet => Set<AiCoachOs.Domain.AnatomyAndBiomechanics.MuscleJointAction>();
    public DbSet<AiCoachOs.Domain.AnatomyAndBiomechanics.ExerciseJointAction> ExerciseJointActionsDbSet => Set<AiCoachOs.Domain.AnatomyAndBiomechanics.ExerciseJointAction>();
    public DbSet<AiCoachOs.Domain.AnatomyAndBiomechanics.BiomechanicalConsideration> BiomechanicalConsiderationsDbSet => Set<AiCoachOs.Domain.AnatomyAndBiomechanics.BiomechanicalConsideration>();

    public DbSet<AiCoachOs.Domain.Programs.Program> ProgramsDbSet => Set<AiCoachOs.Domain.Programs.Program>();
    public DbSet<AiCoachOs.Domain.Programs.ProgramVersion> ProgramVersionsDbSet => Set<AiCoachOs.Domain.Programs.ProgramVersion>();
    public DbSet<AiCoachOs.Domain.Programs.TrainingWeek> TrainingWeeksDbSet => Set<AiCoachOs.Domain.Programs.TrainingWeek>();
    public DbSet<AiCoachOs.Domain.Programs.TrainingSession> TrainingSessionsDbSet => Set<AiCoachOs.Domain.Programs.TrainingSession>();
    public DbSet<AiCoachOs.Domain.Programs.ExerciseSlot> ExerciseSlotsDbSet => Set<AiCoachOs.Domain.Programs.ExerciseSlot>();
    public DbSet<AiCoachOs.Domain.Programs.ProgramMusclePriority> ProgramMusclePrioritiesDbSet => Set<AiCoachOs.Domain.Programs.ProgramMusclePriority>();

    public DbSet<AiCoachOs.Domain.Workouts.WorkoutSession> WorkoutSessionsDbSet => Set<AiCoachOs.Domain.Workouts.WorkoutSession>();
    public DbSet<AiCoachOs.Domain.Workouts.WorkoutExercise> WorkoutExercisesDbSet => Set<AiCoachOs.Domain.Workouts.WorkoutExercise>();
    public DbSet<AiCoachOs.Domain.Workouts.WorkoutSet> WorkoutSetsDbSet => Set<AiCoachOs.Domain.Workouts.WorkoutSet>();

    public DbSet<AiCoachOs.Domain.Adaptations.AdaptationAssessment> AdaptationAssessmentsDbSet => Set<AiCoachOs.Domain.Adaptations.AdaptationAssessment>();
    public DbSet<AiCoachOs.Domain.Adaptations.ExerciseAdaptationRecord> ExerciseAdaptationRecordsDbSet => Set<AiCoachOs.Domain.Adaptations.ExerciseAdaptationRecord>();
    public DbSet<AiCoachOs.Domain.Adaptations.AdaptationRecommendation> AdaptationRecommendationsDbSet => Set<AiCoachOs.Domain.Adaptations.AdaptationRecommendation>();

    public DbSet<AiCoachOs.Domain.Safety.SafetyScreening> SafetyScreeningsDbSet => Set<AiCoachOs.Domain.Safety.SafetyScreening>();
    public DbSet<AiCoachOs.Domain.Safety.RedFlagRule> RedFlagRulesDbSet => Set<AiCoachOs.Domain.Safety.RedFlagRule>();

    public DbSet<AiCoachOs.Domain.Rehab.TrainingLimitation> TrainingLimitationsDbSet => Set<AiCoachOs.Domain.Rehab.TrainingLimitation>();
    public DbSet<AiCoachOs.Domain.Rehab.RehabAwarenessConsideration> RehabAwarenessConsiderationsDbSet => Set<AiCoachOs.Domain.Rehab.RehabAwarenessConsideration>();

    public DbSet<AiCoachOs.Domain.Nutrition.ClientNutritionProfile> ClientNutritionProfilesDbSet => Set<AiCoachOs.Domain.Nutrition.ClientNutritionProfile>();
    public DbSet<AiCoachOs.Domain.Nutrition.NutritionCalibrationRecord> NutritionCalibrationRecordsDbSet => Set<AiCoachOs.Domain.Nutrition.NutritionCalibrationRecord>();
    public DbSet<AiCoachOs.Domain.Nutrition.EgyptianFood> EgyptianFoodsDbSet => Set<AiCoachOs.Domain.Nutrition.EgyptianFood>();

    public DbSet<AiCoachOs.Domain.Gyms.GymProfile> GymProfilesDbSet => Set<AiCoachOs.Domain.Gyms.GymProfile>();

    public DbSet<AiCoachOs.Domain.Substances.SubstanceRecord> SubstancesDbSet => Set<AiCoachOs.Domain.Substances.SubstanceRecord>();
    public DbSet<AiCoachOs.Domain.Substances.PEDRiskRecord> PEDRiskRecordsDbSet => Set<AiCoachOs.Domain.Substances.PEDRiskRecord>();
    public DbSet<AiCoachOs.Domain.Substances.PEDRedFlagRule> PEDRedFlagRulesDbSet => Set<AiCoachOs.Domain.Substances.PEDRedFlagRule>();
    public DbSet<AiCoachOs.Domain.Substances.SubstanceEscalationRecord> SubstanceEscalationRecordsDbSet => Set<AiCoachOs.Domain.Substances.SubstanceEscalationRecord>();

    public DbSet<AiCoachOs.Domain.Memory.ClientMemoryRecord> ClientMemoryRecordsDbSet => Set<AiCoachOs.Domain.Memory.ClientMemoryRecord>();
    public DbSet<AiCoachOs.Domain.Memory.ClientMemoryConflict> ClientMemoryConflictsDbSet => Set<AiCoachOs.Domain.Memory.ClientMemoryConflict>();
    public DbSet<AiCoachOs.Domain.Memory.ClientMemorySnapshot> ClientMemorySnapshotsDbSet => Set<AiCoachOs.Domain.Memory.ClientMemorySnapshot>();
    public DbSet<AiCoachOs.Domain.Memory.AIRecommendationRecord> AIRecommendationRecordsDbSet => Set<AiCoachOs.Domain.Memory.AIRecommendationRecord>();
    public DbSet<AiCoachOs.Domain.Memory.ClientAnonymizationLog> ClientAnonymizationLogsDbSet => Set<AiCoachOs.Domain.Memory.ClientAnonymizationLog>();

    public DbSet<AiCoachOs.Domain.Photos.ClientPhoto> ClientPhotosDbSet => Set<AiCoachOs.Domain.Photos.ClientPhoto>();
    public DbSet<AiCoachOs.Domain.Videos.ClientVideo> ClientVideosDbSet => Set<AiCoachOs.Domain.Videos.ClientVideo>();

    // Explicit implementation of IApplicationDbContext
    IQueryable<Coach> IApplicationDbContext.Coaches => CoachesDbSet.AsNoTracking();
    IQueryable<Client> IApplicationDbContext.Clients => ClientsDbSet.AsNoTracking();
    IQueryable<ConsentRecord> IApplicationDbContext.ConsentRecords => ConsentRecordsDbSet.AsNoTracking();

    IQueryable<AiCoachOs.Domain.Memory.ClientMemoryRecord> IApplicationDbContext.ClientMemoryRecords => ClientMemoryRecordsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Memory.ClientMemoryConflict> IApplicationDbContext.ClientMemoryConflicts => ClientMemoryConflictsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Memory.ClientMemorySnapshot> IApplicationDbContext.ClientMemorySnapshots => ClientMemorySnapshotsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Memory.AIRecommendationRecord> IApplicationDbContext.AIRecommendationRecords => AIRecommendationRecordsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Memory.ClientAnonymizationLog> IApplicationDbContext.ClientAnonymizationLogs => ClientAnonymizationLogsDbSet.AsNoTracking();

    IQueryable<AiCoachOs.Domain.Photos.ClientPhoto> IApplicationDbContext.ClientPhotos => ClientPhotosDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Videos.ClientVideo> IApplicationDbContext.ClientVideos => ClientVideosDbSet.AsNoTracking();

    IQueryable<Exercise> IApplicationDbContext.Exercises => ExercisesDbSet.AsNoTracking();
    IQueryable<Muscle> IApplicationDbContext.Muscles => MusclesDbSet.AsNoTracking();
    IQueryable<MovementPattern> IApplicationDbContext.MovementPatterns => MovementPatternsDbSet.AsNoTracking();
    IQueryable<Equipment> IApplicationDbContext.Equipment => EquipmentDbSet.AsNoTracking();
    IQueryable<ExerciseSubstitution> IApplicationDbContext.ExerciseSubstitutions => ExerciseSubstitutionsDbSet.AsNoTracking();
    IQueryable<ClientTrainingProfile> IApplicationDbContext.ClientTrainingProfiles => ClientTrainingProfilesDbSet.AsNoTracking();
    IQueryable<ClientTrainingPriority> IApplicationDbContext.ClientTrainingPriorities => ClientTrainingPrioritiesDbSet.AsNoTracking();

    IQueryable<KnowledgeSource> IApplicationDbContext.KnowledgeSources => KnowledgeSourcesDbSet.AsNoTracking();
    IQueryable<KnowledgeClaim> IApplicationDbContext.KnowledgeClaims => KnowledgeClaimsDbSet.AsNoTracking();
    IQueryable<KnowledgeClaimSource> IApplicationDbContext.KnowledgeClaimSources => KnowledgeClaimSourcesDbSet.AsNoTracking();

    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.AnatomicalRegion> IApplicationDbContext.AnatomicalRegions => AnatomicalRegionsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.Joint> IApplicationDbContext.Joints => JointsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.JointAction> IApplicationDbContext.JointActions => JointActionsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.MuscleJointAction> IApplicationDbContext.MuscleJointActions => MuscleJointActionsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.ExerciseJointAction> IApplicationDbContext.ExerciseJointActions => ExerciseJointActionsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.AnatomyAndBiomechanics.BiomechanicalConsideration> IApplicationDbContext.BiomechanicalConsiderations => BiomechanicalConsiderationsDbSet.AsNoTracking();

    IQueryable<AiCoachOs.Domain.Programs.Program> IApplicationDbContext.Programs => ProgramsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Programs.ProgramVersion> IApplicationDbContext.ProgramVersions => ProgramVersionsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Programs.TrainingWeek> IApplicationDbContext.TrainingWeeks => TrainingWeeksDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Programs.TrainingSession> IApplicationDbContext.TrainingSessions => TrainingSessionsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Programs.ExerciseSlot> IApplicationDbContext.ExerciseSlots => ExerciseSlotsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Programs.ProgramMusclePriority> IApplicationDbContext.ProgramMusclePriorities => ProgramMusclePrioritiesDbSet.AsNoTracking();

    IQueryable<AiCoachOs.Domain.Workouts.WorkoutSession> IApplicationDbContext.WorkoutSessions => WorkoutSessionsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Workouts.WorkoutExercise> IApplicationDbContext.WorkoutExercises => WorkoutExercisesDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Workouts.WorkoutSet> IApplicationDbContext.WorkoutSets => WorkoutSetsDbSet.AsNoTracking();

    IQueryable<AiCoachOs.Domain.Adaptations.AdaptationAssessment> IApplicationDbContext.AdaptationAssessments => AdaptationAssessmentsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Adaptations.ExerciseAdaptationRecord> IApplicationDbContext.ExerciseAdaptationRecords => ExerciseAdaptationRecordsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Adaptations.AdaptationRecommendation> IApplicationDbContext.AdaptationRecommendations => AdaptationRecommendationsDbSet.AsNoTracking();

    IQueryable<AiCoachOs.Domain.Safety.SafetyScreening> IApplicationDbContext.SafetyScreenings => SafetyScreeningsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Safety.RedFlagRule> IApplicationDbContext.RedFlagRules => RedFlagRulesDbSet.AsNoTracking();

    IQueryable<AiCoachOs.Domain.Rehab.TrainingLimitation> IApplicationDbContext.TrainingLimitations => TrainingLimitationsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Rehab.RehabAwarenessConsideration> IApplicationDbContext.RehabAwarenessConsiderations => RehabAwarenessConsiderationsDbSet.AsNoTracking();

    IQueryable<AiCoachOs.Domain.Nutrition.ClientNutritionProfile> IApplicationDbContext.ClientNutritionProfiles => ClientNutritionProfilesDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Nutrition.NutritionCalibrationRecord> IApplicationDbContext.NutritionCalibrationRecords => NutritionCalibrationRecordsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Nutrition.EgyptianFood> IApplicationDbContext.EgyptianFoods => EgyptianFoodsDbSet.AsNoTracking();

    IQueryable<AiCoachOs.Domain.Gyms.GymProfile> IApplicationDbContext.GymProfiles => GymProfilesDbSet.AsNoTracking();

    IQueryable<AiCoachOs.Domain.Substances.SubstanceRecord> IApplicationDbContext.Substances => SubstancesDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Substances.SupplementKnowledge> IApplicationDbContext.Supplements => Set<AiCoachOs.Domain.Substances.SupplementKnowledge>().AsNoTracking();
    IQueryable<AiCoachOs.Domain.Substances.HormoneKnowledge> IApplicationDbContext.Hormones => Set<AiCoachOs.Domain.Substances.HormoneKnowledge>().AsNoTracking();
    IQueryable<AiCoachOs.Domain.Substances.PEDSafetyRecord> IApplicationDbContext.PEDSafetyRecords => Set<AiCoachOs.Domain.Substances.PEDSafetyRecord>().AsNoTracking();
    IQueryable<AiCoachOs.Domain.Substances.PEDRiskRecord> IApplicationDbContext.PEDRiskRecords => PEDRiskRecordsDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Substances.PEDRedFlagRule> IApplicationDbContext.PEDRedFlagRules => PEDRedFlagRulesDbSet.AsNoTracking();
    IQueryable<AiCoachOs.Domain.Substances.SubstanceEscalationRecord> IApplicationDbContext.SubstanceEscalationRecords => SubstanceEscalationRecordsDbSet.AsNoTracking();

    public async Task AddCoachAsync(Coach coach, CancellationToken cancellationToken = default)
    {
        await CoachesDbSet.AddAsync(coach, cancellationToken);
    }

    public async Task AddClientAsync(Client client, CancellationToken cancellationToken = default)
    {
        await ClientsDbSet.AddAsync(client, cancellationToken);
    }

    public async Task AddConsentRecordAsync(ConsentRecord consentRecord, CancellationToken cancellationToken = default)
    {
        await ConsentRecordsDbSet.AddAsync(consentRecord, cancellationToken);
    }

    public async Task AddExerciseAsync(Exercise exercise, CancellationToken cancellationToken = default)
    {
        await ExercisesDbSet.AddAsync(exercise, cancellationToken);
    }

    public async Task AddTrainingProfileAsync(ClientTrainingProfile profile, CancellationToken cancellationToken = default)
    {
        await ClientTrainingProfilesDbSet.AddAsync(profile, cancellationToken);
    }

    public async Task AddKnowledgeSourceAsync(KnowledgeSource source, CancellationToken cancellationToken = default)
    {
        await KnowledgeSourcesDbSet.AddAsync(source, cancellationToken);
    }

    public async Task AddKnowledgeClaimAsync(KnowledgeClaim claim, CancellationToken cancellationToken = default)
    {
        await KnowledgeClaimsDbSet.AddAsync(claim, cancellationToken);
    }

    public async Task AddBiomechanicalConsiderationAsync(AiCoachOs.Domain.AnatomyAndBiomechanics.BiomechanicalConsideration consideration, CancellationToken cancellationToken = default)
    {
        await BiomechanicalConsiderationsDbSet.AddAsync(consideration, cancellationToken);
    }

    public async Task AddProgramAsync(AiCoachOs.Domain.Programs.Program program, CancellationToken cancellationToken = default)
    {
        await ProgramsDbSet.AddAsync(program, cancellationToken);
    }

    public async Task AddProgramVersionAsync(AiCoachOs.Domain.Programs.ProgramVersion version, CancellationToken cancellationToken = default)
    {
        await ProgramVersionsDbSet.AddAsync(version, cancellationToken);
    }

    public async Task AddWorkoutSessionAsync(AiCoachOs.Domain.Workouts.WorkoutSession session, CancellationToken cancellationToken = default)
    {
        await WorkoutSessionsDbSet.AddAsync(session, cancellationToken);
    }

    public async Task AddWorkoutExerciseAsync(AiCoachOs.Domain.Workouts.WorkoutExercise exercise, CancellationToken cancellationToken = default)
    {
        await WorkoutExercisesDbSet.AddAsync(exercise, cancellationToken);
    }

    public async Task AddWorkoutSetAsync(AiCoachOs.Domain.Workouts.WorkoutSet set, CancellationToken cancellationToken = default)
    {
        await WorkoutSetsDbSet.AddAsync(set, cancellationToken);
    }

    public async Task AddAdaptationAssessmentAsync(AiCoachOs.Domain.Adaptations.AdaptationAssessment assessment, CancellationToken cancellationToken = default)
    {
        await AdaptationAssessmentsDbSet.AddAsync(assessment, cancellationToken);
    }

    public async Task AddSafetyScreeningAsync(AiCoachOs.Domain.Safety.SafetyScreening screening, CancellationToken cancellationToken = default)
    {
        await SafetyScreeningsDbSet.AddAsync(screening, cancellationToken);
    }

    public async Task AddRedFlagRuleAsync(AiCoachOs.Domain.Safety.RedFlagRule rule, CancellationToken cancellationToken = default)
    {
        await RedFlagRulesDbSet.AddAsync(rule, cancellationToken);
    }

    public async Task AddTrainingLimitationAsync(AiCoachOs.Domain.Rehab.TrainingLimitation limitation, CancellationToken cancellationToken = default)
    {
        await TrainingLimitationsDbSet.AddAsync(limitation, cancellationToken);
    }

    public async Task AddRehabAwarenessConsiderationAsync(AiCoachOs.Domain.Rehab.RehabAwarenessConsideration consideration, CancellationToken cancellationToken = default)
    {
        await RehabAwarenessConsiderationsDbSet.AddAsync(consideration, cancellationToken);
    }

    public async Task AddClientNutritionProfileAsync(AiCoachOs.Domain.Nutrition.ClientNutritionProfile profile, CancellationToken cancellationToken = default)
    {
        await ClientNutritionProfilesDbSet.AddAsync(profile, cancellationToken);
    }

    public async Task AddNutritionCalibrationRecordAsync(AiCoachOs.Domain.Nutrition.NutritionCalibrationRecord record, CancellationToken cancellationToken = default)
    {
        await NutritionCalibrationRecordsDbSet.AddAsync(record, cancellationToken);
    }

    public async Task AddEgyptianFoodAsync(AiCoachOs.Domain.Nutrition.EgyptianFood food, CancellationToken cancellationToken = default)
    {
        await EgyptianFoodsDbSet.AddAsync(food, cancellationToken);
    }

    public async Task<Coach?> FindCoachByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await CoachesDbSet.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Coach?> FindCoachByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default)
    {
        return await CoachesDbSet.FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId, cancellationToken);
    }

    public async Task<Client?> FindClientByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ClientsDbSet.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Exercise?> FindExerciseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExercisesDbSet
            .Include(e => e.MovementPattern)
            .Include(e => e.Muscles)
                .ThenInclude(em => em.Muscle)
            .Include(e => e.Equipment)
                .ThenInclude(ee => ee.Equipment)
            .Include(e => e.Substitutions)
                .ThenInclude(es => es.SubstituteExercise)
                    .ThenInclude(se => se.MovementPattern)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<ClientTrainingProfile?> FindTrainingProfileByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await ClientTrainingProfilesDbSet
            .Include(p => p.Priorities)
            .FirstOrDefaultAsync(p => p.ClientId == clientId, cancellationToken);
    }

    public async Task<KnowledgeSource?> FindKnowledgeSourceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await KnowledgeSourcesDbSet.FirstOrDefaultAsync(ks => ks.Id == id, cancellationToken);
    }

    public async Task<KnowledgeClaim?> FindKnowledgeClaimByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await KnowledgeClaimsDbSet
            .Include(kc => kc.Exercise)
            .Include(kc => kc.SupersededByClaim)
            .Include(kc => kc.Sources)
                .ThenInclude(kcs => kcs.Source)
            .FirstOrDefaultAsync(kc => kc.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.AnatomyAndBiomechanics.AnatomicalRegion?> FindAnatomicalRegionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await AnatomicalRegionsDbSet
            .Include(ar => ar.Joints)
            .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.AnatomyAndBiomechanics.Joint?> FindJointByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await JointsDbSet
            .Include(j => j.Region)
            .Include(j => j.Actions)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.AnatomyAndBiomechanics.JointAction?> FindJointActionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await JointActionsDbSet
            .Include(ja => ja.Joint)
                .ThenInclude(j => j.Region)
            .Include(ja => ja.Muscles)
                .ThenInclude(mja => mja.Muscle)
            .FirstOrDefaultAsync(ja => ja.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Programs.Program?> FindProgramByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ProgramsDbSet
            .Include(p => p.Client)
            .Include(p => p.Versions)
                .ThenInclude(v => v.MusclePriorities)
                    .ThenInclude(mp => mp.Muscle)
            .Include(p => p.Versions)
                .ThenInclude(v => v.Weeks)
                    .ThenInclude(w => w.Sessions)
                        .ThenInclude(s => s.Slots)
                            .ThenInclude(sl => sl.Exercise)
                                .ThenInclude(e => e.MovementPattern)
            .Include(p => p.Versions)
                .ThenInclude(v => v.Weeks)
                    .ThenInclude(w => w.Sessions)
                        .ThenInclude(s => s.Slots)
                            .ThenInclude(sl => sl.Exercise)
                                .ThenInclude(e => e.Muscles)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Workouts.WorkoutSession?> FindWorkoutSessionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await WorkoutSessionsDbSet
            .Include(w => w.Client)
            .Include(w => w.Coach)
            .Include(w => w.TrainingSession)
                .ThenInclude(ts => ts!.Slots)
                    .ThenInclude(sl => sl.Exercise)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.ExerciseSlot)
                    .ThenInclude(sl => sl!.Exercise)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Adaptations.AdaptationAssessment?> FindAdaptationAssessmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await AdaptationAssessmentsDbSet
            .Include(a => a.ProgramVersion)
                .ThenInclude(pv => pv.Program)
                    .ThenInclude(p => p.Client)
            .Include(a => a.ExerciseRecords)
                .ThenInclude(r => r.Exercise)
            .Include(a => a.ExerciseRecords)
                .ThenInclude(r => r.ExerciseSlot)
            .Include(a => a.Recommendations)
                .ThenInclude(rec => rec.ExerciseAdaptationRecord)
                    .ThenInclude(r => r!.Exercise)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Safety.SafetyScreening?> FindSafetyScreeningByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await SafetyScreeningsDbSet
            .Include(s => s.Client)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Safety.RedFlagRule?> FindRedFlagRuleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await RedFlagRulesDbSet
            .Include(r => r.KnowledgeClaim)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Rehab.TrainingLimitation?> FindTrainingLimitationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await TrainingLimitationsDbSet
            .Include(t => t.Client)
            .Include(t => t.SafetyScreening)
            .Include(t => t.Considerations)
                .ThenInclude(c => c.Exercise)
            .Include(t => t.Considerations)
                .ThenInclude(c => c.KnowledgeClaim)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Rehab.RehabAwarenessConsideration?> FindRehabAwarenessConsiderationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await RehabAwarenessConsiderationsDbSet
            .Include(c => c.TrainingLimitation)
                .ThenInclude(t => t.Client)
            .Include(c => c.Exercise)
            .Include(c => c.KnowledgeClaim)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Nutrition.ClientNutritionProfile?> FindNutritionProfileByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await ClientNutritionProfilesDbSet
            .Include(p => p.Client)
            .Include(p => p.CalibrationRecords)
            .FirstOrDefaultAsync(p => p.ClientId == clientId, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Nutrition.NutritionCalibrationRecord?> FindNutritionCalibrationRecordByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await NutritionCalibrationRecordsDbSet
            .Include(r => r.ClientNutritionProfile)
                .ThenInclude(p => p.Client)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Nutrition.EgyptianFood?> FindEgyptianFoodByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await EgyptianFoodsDbSet.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task AddGymProfileAsync(AiCoachOs.Domain.Gyms.GymProfile gym, CancellationToken cancellationToken = default)
    {
        await GymProfilesDbSet.AddAsync(gym, cancellationToken);
    }

    public void RemoveGymProfile(AiCoachOs.Domain.Gyms.GymProfile gym)
    {
        GymProfilesDbSet.Remove(gym);
    }

    public async Task<AiCoachOs.Domain.Gyms.GymProfile?> FindGymProfileByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GymProfilesDbSet.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task AddSubstanceRecordAsync(AiCoachOs.Domain.Substances.SubstanceRecord substance, CancellationToken cancellationToken = default)
    {
        await SubstancesDbSet.AddAsync(substance, cancellationToken);
    }

    public async Task AddPEDRedFlagRuleAsync(AiCoachOs.Domain.Substances.PEDRedFlagRule rule, CancellationToken cancellationToken = default)
    {
        await PEDRedFlagRulesDbSet.AddAsync(rule, cancellationToken);
    }

    public async Task AddSubstanceEscalationRecordAsync(AiCoachOs.Domain.Substances.SubstanceEscalationRecord record, CancellationToken cancellationToken = default)
    {
        await SubstanceEscalationRecordsDbSet.AddAsync(record, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Substances.SupplementKnowledge?> FindSupplementByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Set<AiCoachOs.Domain.Substances.SupplementKnowledge>()
            .Include(s => s.PrimaryKnowledgeClaim)
                .ThenInclude(c => c!.Sources)
                    .ThenInclude(cs => cs.Source)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Substances.HormoneKnowledge?> FindHormoneByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Set<AiCoachOs.Domain.Substances.HormoneKnowledge>()
            .Include(h => h.PrimaryKnowledgeClaim)
                .ThenInclude(c => c!.Sources)
                    .ThenInclude(cs => cs.Source)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Substances.PEDSafetyRecord?> FindPEDSafetyRecordByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Set<AiCoachOs.Domain.Substances.PEDSafetyRecord>()
            .Include(p => p.DocumentedRisks)
                .ThenInclude(r => r.EvidenceClaim)
            .Include(p => p.PrimaryKnowledgeClaim)
                .ThenInclude(c => c!.Sources)
                    .ThenInclude(cs => cs.Source)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Substances.SubstanceRecord?> FindSubstanceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await SubstancesDbSet
            .Include(s => s.PrimaryKnowledgeClaim)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddClientMemoryRecordAsync(AiCoachOs.Domain.Memory.ClientMemoryRecord memoryRecord, CancellationToken cancellationToken = default)
    {
        await ClientMemoryRecordsDbSet.AddAsync(memoryRecord, cancellationToken);
    }

    public async Task AddClientMemoryConflictAsync(AiCoachOs.Domain.Memory.ClientMemoryConflict conflict, CancellationToken cancellationToken = default)
    {
        await ClientMemoryConflictsDbSet.AddAsync(conflict, cancellationToken);
    }

    public async Task AddClientMemorySnapshotAsync(AiCoachOs.Domain.Memory.ClientMemorySnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await ClientMemorySnapshotsDbSet.AddAsync(snapshot, cancellationToken);
    }

    public async Task AddAIRecommendationRecordAsync(AiCoachOs.Domain.Memory.AIRecommendationRecord recommendation, CancellationToken cancellationToken = default)
    {
        await AIRecommendationRecordsDbSet.AddAsync(recommendation, cancellationToken);
    }

    public async Task AddClientAnonymizationLogAsync(AiCoachOs.Domain.Memory.ClientAnonymizationLog log, CancellationToken cancellationToken = default)
    {
        await ClientAnonymizationLogsDbSet.AddAsync(log, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Memory.ClientMemoryRecord?> FindClientMemoryRecordByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ClientMemoryRecordsDbSet
            .Include(m => m.SupersededBy)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Memory.ClientMemoryConflict?> FindClientMemoryConflictByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ClientMemoryConflictsDbSet
            .Include(c => c.RecordA)
            .Include(c => c.RecordB)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Memory.ClientMemorySnapshot?> FindClientMemorySnapshotByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ClientMemorySnapshotsDbSet
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<AiCoachOs.Domain.Memory.AIRecommendationRecord?> FindAIRecommendationRecordByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await AIRecommendationRecordsDbSet
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task AddClientPhotoAsync(AiCoachOs.Domain.Photos.ClientPhoto photo, CancellationToken cancellationToken = default)
    {
        await ClientPhotosDbSet.AddAsync(photo, cancellationToken);
    }

    public void RemoveClientPhoto(AiCoachOs.Domain.Photos.ClientPhoto photo)
    {
        ClientPhotosDbSet.Remove(photo);
    }

    public async Task<AiCoachOs.Domain.Photos.ClientPhoto?> FindClientPhotoByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ClientPhotosDbSet
            .Include(p => p.ObservationRecord)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddClientVideoAsync(AiCoachOs.Domain.Videos.ClientVideo video, CancellationToken cancellationToken = default)
    {
        await ClientVideosDbSet.AddAsync(video, cancellationToken);
    }

    public void RemoveClientVideo(AiCoachOs.Domain.Videos.ClientVideo video)
    {
        ClientVideosDbSet.Remove(video);
    }

    public async Task<AiCoachOs.Domain.Videos.ClientVideo?> FindClientVideoByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ClientVideosDbSet
            .Include(v => v.ObservationRecord)
            .Include(v => v.Exercise)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
