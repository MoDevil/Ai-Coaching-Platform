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
