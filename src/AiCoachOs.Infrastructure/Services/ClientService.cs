using AiCoachOs.Application.Clients.DTOs;
using AiCoachOs.Application.Clients.Services;
using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentCoachService _currentCoachService;

    public ClientService(IApplicationDbContext dbContext, ICurrentCoachService currentCoachService)
    {
        _dbContext = dbContext;
        _currentCoachService = currentCoachService;
    }

    public async Task<ClientDto> CreateClientAsync(CreateClientRequestDto request, CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);

        ClientGoal? goal = null;
        if (request.Goal != null && !string.IsNullOrWhiteSpace(request.Goal.PrimaryGoal))
        {
            goal = new ClientGoal(
                request.Goal.PrimaryGoal,
                request.Goal.TargetTimelineWeeks,
                request.Goal.Notes
            );
        }

        var client = new Client(
            id: Guid.NewGuid(),
            coachId: coachId,
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            phone: request.Phone,
            dateOfBirth: request.DateOfBirth,
            gender: request.Gender,
            goal: goal,
            intakeNotes: request.IntakeNotes
        );

        await _dbContext.AddClientAsync(client, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(client);
    }

    public async Task<IReadOnlyList<ClientSummaryDto>> GetCoachClientsAsync(CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);

        var pendingCounts = await _dbContext.AIRecommendationRecords
            .Where(r => r.CoachId == coachId && r.ReviewStatus == Domain.Memory.AIRecommendationReviewStatus.PendingReview)
            .GroupBy(r => r.ClientId)
            .Select(g => new { ClientId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClientId, x => x.Count, cancellationToken);

        var clients = await _dbContext.Clients
            .Where(c => c.CoachId == coachId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return clients.Select(c => new ClientSummaryDto(
            c.Id,
            c.FirstName,
            c.LastName,
            c.Email,
            c.Status,
            c.Goal != null ? c.Goal.PrimaryGoal : null,
            c.CreatedAtUtc,
            pendingCounts.TryGetValue(c.Id, out var count) ? count : 0
        )).ToList();
    }

    public async Task<ClientDto> GetClientByIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);

        var client = await _dbContext.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null || client.CoachId != coachId)
        {
            // Do not leak existence of another coach's client
            throw new NotFoundException("Client", clientId);
        }

        var pendingCount = await _dbContext.AIRecommendationRecords
            .CountAsync(r => r.ClientId == clientId && r.ReviewStatus == Domain.Memory.AIRecommendationReviewStatus.PendingReview, cancellationToken);

        return MapToDto(client, pendingCount);
    }

    public async Task<ClientDto> UpdateClientAsync(Guid clientId, UpdateClientRequestDto request, CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);

        var client = await _dbContext.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null || client.CoachId != coachId)
        {
            throw new NotFoundException("Client", clientId);
        }

        client.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.DateOfBirth,
            request.Gender,
            request.IntakeNotes
        );

        if (request.Goal != null && !string.IsNullOrWhiteSpace(request.Goal.PrimaryGoal))
        {
            client.SetGoal(new ClientGoal(
                request.Goal.PrimaryGoal,
                request.Goal.TargetTimelineWeeks,
                request.Goal.Notes
            ));
        }

        if (request.Status.HasValue)
        {
            if (request.Status.Value == ClientStatus.Archived)
            {
                client.Archive();
            }
            else if (request.Status.Value == ClientStatus.Active)
            {
                client.Activate();
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(client);
    }

    public async Task<bool> ArchiveClientAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);

        var client = await _dbContext.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null || client.CoachId != coachId)
        {
            throw new NotFoundException("Client", clientId);
        }

        client.Archive();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<ConsentRecordDto> RecordConsentAsync(Guid clientId, string consentType, bool isGranted, string? notes = null, CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);

        var client = await _dbContext.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null || client.CoachId != coachId)
        {
            throw new NotFoundException("Client", clientId);
        }

        var consent = new ConsentRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            consentType: consentType,
            isGranted: isGranted,
            notes: notes
        );

        await _dbContext.AddConsentRecordAsync(consent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ConsentRecordDto(
            consent.Id,
            consent.ClientId,
            consent.ConsentType,
            consent.IsGranted,
            consent.GrantedAtUtc,
            consent.Notes
        );
    }

    public async Task<IReadOnlyList<ConsentRecordDto>> GetClientConsentsAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);

        var client = await _dbContext.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null || client.CoachId != coachId)
        {
            throw new NotFoundException("Client", clientId);
        }

        var consents = await _dbContext.ConsentRecords
            .Where(c => c.ClientId == clientId)
            .OrderByDescending(c => c.GrantedAtUtc)
            .Select(c => new ConsentRecordDto(
                c.Id,
                c.ClientId,
                c.ConsentType,
                c.IsGranted,
                c.GrantedAtUtc,
                c.Notes
            ))
            .ToListAsync(cancellationToken);

        return consents;
    }

    private static ClientDto MapToDto(Client client, int pendingRecommendationCount = 0)
    {
        ClientGoalDto? goalDto = null;
        if (client.Goal != null)
        {
            goalDto = new ClientGoalDto(
                client.Goal.PrimaryGoal,
                client.Goal.TargetTimelineWeeks,
                client.Goal.Notes
            );
        }

        return new ClientDto(
            client.Id,
            client.CoachId,
            client.FirstName,
            client.LastName,
            client.Email,
            client.Phone,
            client.DateOfBirth,
            client.Gender,
            client.Status,
            goalDto,
            client.IntakeNotes,
            client.CreatedAtUtc,
            client.UpdatedAtUtc,
            pendingRecommendationCount
        );
    }
}
