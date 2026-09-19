using AiCoachOs.Application.Clients.DTOs;

namespace AiCoachOs.Application.Clients.Services;

public interface IClientService
{
    Task<ClientDto> CreateClientAsync(CreateClientRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientSummaryDto>> GetCoachClientsAsync(CancellationToken cancellationToken = default);
    Task<ClientDto> GetClientByIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<ClientDto> UpdateClientAsync(Guid clientId, UpdateClientRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> ArchiveClientAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<ConsentRecordDto> RecordConsentAsync(Guid clientId, string consentType, bool isGranted, string? notes = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConsentRecordDto>> GetClientConsentsAsync(Guid clientId, CancellationToken cancellationToken = default);
}
