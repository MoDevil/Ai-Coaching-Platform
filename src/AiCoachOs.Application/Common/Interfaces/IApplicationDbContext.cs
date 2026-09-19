using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Coaches;
using AiCoachOs.Domain.Exercises;
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

    Task AddCoachAsync(Coach coach, CancellationToken cancellationToken = default);
    Task AddClientAsync(Client client, CancellationToken cancellationToken = default);
    Task AddConsentRecordAsync(ConsentRecord consentRecord, CancellationToken cancellationToken = default);
    Task AddExerciseAsync(Exercise exercise, CancellationToken cancellationToken = default);
    Task AddTrainingProfileAsync(ClientTrainingProfile profile, CancellationToken cancellationToken = default);

    Task<Coach?> FindCoachByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Coach?> FindCoachByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);
    Task<Client?> FindClientByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Exercise?> FindExerciseByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClientTrainingProfile?> FindTrainingProfileByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
