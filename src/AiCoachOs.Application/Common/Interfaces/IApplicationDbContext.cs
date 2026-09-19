using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Coaches;

namespace AiCoachOs.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Coach> Coaches { get; }
    IQueryable<Client> Clients { get; }
    IQueryable<ConsentRecord> ConsentRecords { get; }

    Task AddCoachAsync(Coach coach, CancellationToken cancellationToken = default);
    Task AddClientAsync(Client client, CancellationToken cancellationToken = default);
    Task AddConsentRecordAsync(ConsentRecord consentRecord, CancellationToken cancellationToken = default);

    Task<Coach?> FindCoachByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Coach?> FindCoachByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);
    Task<Client?> FindClientByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
