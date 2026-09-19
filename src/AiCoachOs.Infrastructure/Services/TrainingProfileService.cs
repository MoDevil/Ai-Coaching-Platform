using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.TrainingProfiles.DTOs;
using AiCoachOs.Application.TrainingProfiles.Services;
using AiCoachOs.Domain.TrainingProfiles;
using AiCoachOs.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class TrainingProfileService : ITrainingProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentCoachService _currentCoachService;

    public TrainingProfileService(ApplicationDbContext context, ICurrentCoachService currentCoachService)
    {
        _context = context;
        _currentCoachService = currentCoachService;
    }

    private async Task EnsureCoachOwnsClientAsync(Guid clientId, CancellationToken ct)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(ct);
        var client = await _context.ClientsDbSet.FirstOrDefaultAsync(c => c.Id == clientId, ct);

        if (client == null || client.CoachId != coachId)
        {
            throw new NotFoundException("Client", clientId);
        }
    }

    public async Task<TrainingProfileDto> GetProfileByClientIdAsync(Guid clientId, CancellationToken ct = default)
    {
        await EnsureCoachOwnsClientAsync(clientId, ct);

        var profile = await _context.FindTrainingProfileByClientIdAsync(clientId, ct);
        if (profile == null)
        {
            // Initialize default training profile for this client
            var defaultAvailability = new TrainingAvailability(
                sessionsPerWeek: 3,
                availableDays: new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                preferredDays: new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }
            );

            profile = new ClientTrainingProfile(
                id: Guid.NewGuid(),
                clientId: clientId,
                experienceLevel: TrainingExperienceLevel.Intermediate,
                weeklyAvailability: defaultAvailability,
                sessionDurationMinMinutes: 45,
                sessionDurationTargetMinutes: 60,
                sessionDurationMaxMinutes: 75
            );

            await _context.AddTrainingProfileAsync(profile, ct);
            await _context.SaveChangesAsync(ct);
        }

        return MapToDto(profile);
    }

    public async Task<TrainingProfileDto> UpdateProfileAsync(Guid clientId, UpdateTrainingProfileRequestDto request, CancellationToken ct = default)
    {
        await EnsureCoachOwnsClientAsync(clientId, ct);

        var profile = await _context.FindTrainingProfileByClientIdAsync(clientId, ct);
        if (profile == null)
        {
            var availability = new TrainingAvailability(
                request.WeeklyAvailability.SessionsPerWeek,
                request.WeeklyAvailability.AvailableDays,
                request.WeeklyAvailability.PreferredDays
            );

            profile = new ClientTrainingProfile(
                id: Guid.NewGuid(),
                clientId: clientId,
                experienceLevel: request.ExperienceLevel,
                weeklyAvailability: availability,
                sessionDurationMinMinutes: request.SessionDurationMinMinutes,
                sessionDurationTargetMinutes: request.SessionDurationTargetMinutes,
                sessionDurationMaxMinutes: request.SessionDurationMaxMinutes,
                availableEquipmentIds: request.AvailableEquipmentIds,
                exercisePreferences: request.ExercisePreferences,
                exerciseConstraints: request.ExerciseConstraints
            );

            if (request.Priorities != null)
            {
                profile.SetPriorities(request.Priorities.Select(p => (p.Order, p.FocusArea, p.Notes)));
            }

            await _context.AddTrainingProfileAsync(profile, ct);
        }
        else
        {
            profile.UpdateProfile(
                request.ExperienceLevel,
                request.SessionDurationMinMinutes,
                request.SessionDurationTargetMinutes,
                request.SessionDurationMaxMinutes,
                request.AvailableEquipmentIds,
                request.ExercisePreferences,
                request.ExerciseConstraints
            );

            var availability = new TrainingAvailability(
                request.WeeklyAvailability.SessionsPerWeek,
                request.WeeklyAvailability.AvailableDays,
                request.WeeklyAvailability.PreferredDays
            );
            profile.SetAvailability(availability);

            if (request.Priorities != null)
            {
                profile.SetPriorities(request.Priorities.Select(p => (p.Order, p.FocusArea, p.Notes)));
            }
        }

        await _context.SaveChangesAsync(ct);
        return MapToDto(profile);
    }

    public async Task<TrainingProfileDto> UpdateAvailabilityAsync(Guid clientId, TrainingAvailabilityDto availabilityDto, CancellationToken ct = default)
    {
        await EnsureCoachOwnsClientAsync(clientId, ct);

        var profile = await _context.FindTrainingProfileByClientIdAsync(clientId, ct);
        var availability = new TrainingAvailability(
            availabilityDto.SessionsPerWeek,
            availabilityDto.AvailableDays,
            availabilityDto.PreferredDays
        );

        if (profile == null)
        {
            profile = new ClientTrainingProfile(
                id: Guid.NewGuid(),
                clientId: clientId,
                experienceLevel: TrainingExperienceLevel.Intermediate,
                weeklyAvailability: availability
            );
            await _context.AddTrainingProfileAsync(profile, ct);
        }
        else
        {
            profile.SetAvailability(availability);
        }

        await _context.SaveChangesAsync(ct);
        return MapToDto(profile);
    }

    public async Task<TrainingProfileDto> UpdatePrioritiesAsync(Guid clientId, List<ClientTrainingPriorityDto> priorities, CancellationToken ct = default)
    {
        await EnsureCoachOwnsClientAsync(clientId, ct);

        var profile = await _context.FindTrainingProfileByClientIdAsync(clientId, ct);
        if (profile == null)
        {
            var defaultAvailability = new TrainingAvailability(3);
            profile = new ClientTrainingProfile(
                id: Guid.NewGuid(),
                clientId: clientId,
                experienceLevel: TrainingExperienceLevel.Intermediate,
                weeklyAvailability: defaultAvailability
            );
            await _context.AddTrainingProfileAsync(profile, ct);
        }

        profile.SetPriorities(priorities.Select(p => (p.Order, p.FocusArea, p.Notes)));
        await _context.SaveChangesAsync(ct);

        return MapToDto(profile);
    }

    private static TrainingProfileDto MapToDto(ClientTrainingProfile profile)
    {
        var availabilityDto = new TrainingAvailabilityDto(
            profile.WeeklyAvailability.SessionsPerWeek,
            profile.WeeklyAvailability.AvailableDays,
            profile.WeeklyAvailability.PreferredDays
        );

        var priorityDtos = profile.Priorities.Select(p => new ClientTrainingPriorityDto(
            p.Id,
            p.Order,
            p.FocusArea,
            p.Notes
        )).ToList();

        return new TrainingProfileDto(
            profile.Id,
            profile.ClientId,
            profile.ExperienceLevel,
            profile.SessionDurationMinMinutes,
            profile.SessionDurationTargetMinutes,
            profile.SessionDurationMaxMinutes,
            availabilityDto,
            profile.AvailableEquipmentIds.ToList(),
            profile.ExercisePreferences,
            profile.ExerciseConstraints,
            priorityDtos,
            profile.CreatedAtUtc,
            profile.UpdatedAtUtc
        );
    }
}
