using AiCoachOs.Domain.Clients;

namespace AiCoachOs.Application.Clients.DTOs;

public record ClientGoalDto(
    string PrimaryGoal,
    int? TargetTimelineWeeks = null,
    string? Notes = null
);

public record ConsentRecordDto(
    Guid Id,
    Guid ClientId,
    string ConsentType,
    bool IsGranted,
    DateTime GrantedAtUtc,
    string? Notes = null
);

public record CreateClientRequestDto(
    string FirstName,
    string LastName,
    string? Email = null,
    string? Phone = null,
    DateTime? DateOfBirth = null,
    Gender? Gender = null,
    ClientGoalDto? Goal = null,
    string? IntakeNotes = null
);

public record UpdateClientRequestDto(
    string FirstName,
    string LastName,
    string? Email = null,
    string? Phone = null,
    DateTime? DateOfBirth = null,
    Gender? Gender = null,
    ClientGoalDto? Goal = null,
    string? IntakeNotes = null,
    ClientStatus? Status = null
);

public record ClientDto(
    Guid Id,
    Guid CoachId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateTime? DateOfBirth,
    Gender? Gender,
    ClientStatus Status,
    ClientGoalDto? Goal,
    string? IntakeNotes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record ClientSummaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    ClientStatus Status,
    string? PrimaryGoal,
    DateTime CreatedAtUtc
);
