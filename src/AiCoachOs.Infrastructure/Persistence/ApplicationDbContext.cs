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

    // Explicit implementation of IApplicationDbContext
    IQueryable<Coach> IApplicationDbContext.Coaches => CoachesDbSet.AsNoTracking();
    IQueryable<Client> IApplicationDbContext.Clients => ClientsDbSet.AsNoTracking();
    IQueryable<ConsentRecord> IApplicationDbContext.ConsentRecords => ConsentRecordsDbSet.AsNoTracking();

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
