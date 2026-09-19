using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Coaches;
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

    // Explicit implementation of IApplicationDbContext
    IQueryable<Coach> IApplicationDbContext.Coaches => CoachesDbSet.AsNoTracking();
    IQueryable<Client> IApplicationDbContext.Clients => ClientsDbSet.AsNoTracking();
    IQueryable<ConsentRecord> IApplicationDbContext.ConsentRecords => ConsentRecordsDbSet.AsNoTracking();

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
