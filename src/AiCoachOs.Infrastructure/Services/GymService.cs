using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Gyms.Dtos;
using AiCoachOs.Application.Gyms.Engine;
using AiCoachOs.Application.Gyms.Interfaces;
using AiCoachOs.Domain.Gyms;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class GymService : IGymService
{
    private readonly IApplicationDbContext _context;
    private readonly IGymEquipmentResolver _equipmentResolver;

    public GymService(IApplicationDbContext context, IGymEquipmentResolver equipmentResolver)
    {
        _context = context;
        _equipmentResolver = equipmentResolver;
    }

    public async Task<GymProfileDto> CreateGymProfileAsync(
        Guid coachId,
        CreateGymProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var gym = new GymProfile(
            id: Guid.NewGuid(),
            coachId: coachId,
            name: request.Name,
            tier: request.Tier,
            location: request.Location,
            explicitEquipmentIds: request.ExplicitEquipmentIds,
            isInventoryAuthoritative: request.IsInventoryAuthoritative);

        await _context.AddGymProfileAsync(gym, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var allEquipment = await _context.Equipment.ToListAsync(cancellationToken);
        return MapToDto(gym, allEquipment);
    }

    public async Task<GymProfileDto> UpdateGymProfileAsync(
        Guid coachId,
        Guid gymId,
        UpdateGymProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var gym = await _context.FindGymProfileByIdAsync(gymId, cancellationToken);
        if (gym == null)
            throw new KeyNotFoundException($"Gym profile '{gymId}' was not found.");

        if (gym.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to modify this gym profile.");

        gym.UpdateProfile(
            name: request.Name,
            tier: request.Tier,
            location: request.Location,
            explicitEquipmentIds: request.ExplicitEquipmentIds,
            isInventoryAuthoritative: request.IsInventoryAuthoritative);

        await _context.SaveChangesAsync(cancellationToken);

        var allEquipment = await _context.Equipment.ToListAsync(cancellationToken);
        return MapToDto(gym, allEquipment);
    }

    public async Task<GymProfileDto?> GetGymProfileByIdAsync(
        Guid coachId,
        Guid gymId,
        CancellationToken cancellationToken = default)
    {
        var gym = await _context.FindGymProfileByIdAsync(gymId, cancellationToken);
        if (gym == null) return null;

        if (gym.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to access this gym profile.");

        var allEquipment = await _context.Equipment.ToListAsync(cancellationToken);
        return MapToDto(gym, allEquipment);
    }

    public async Task<IReadOnlyList<GymProfileDto>> GetGymProfilesAsync(
        Guid coachId,
        CancellationToken cancellationToken = default)
    {
        var gyms = await _context.GymProfiles
            .Where(g => g.CoachId == coachId)
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);

        var allEquipment = await _context.Equipment.ToListAsync(cancellationToken);
        return gyms.Select(g => MapToDto(g, allEquipment)).ToList();
    }

    public async Task<bool> AssignClientGymAsync(
        Guid coachId,
        AssignClientGymRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(request.ClientId, cancellationToken);
        if (client == null)
            throw new KeyNotFoundException($"Client '{request.ClientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to manage this client.");

        if (request.GymProfileId.HasValue)
        {
            var gym = await _context.FindGymProfileByIdAsync(request.GymProfileId.Value, cancellationToken);
            if (gym == null)
                throw new KeyNotFoundException($"Gym profile '{request.GymProfileId.Value}' was not found.");

            if (gym.CoachId != coachId)
                throw new UnauthorizedAccessException("Coach is not authorized to assign this gym profile.");
        }

        var profile = await _context.FindTrainingProfileByClientIdAsync(request.ClientId, cancellationToken);
        if (profile == null)
        {
            // Auto-create training profile if not existing
            profile = new Domain.TrainingProfiles.ClientTrainingProfile(
                id: Guid.NewGuid(),
                clientId: request.ClientId,
                experienceLevel: Domain.TrainingProfiles.TrainingExperienceLevel.Intermediate,
                weeklyAvailability: new Domain.TrainingProfiles.TrainingAvailability(3),
                gymProfileId: request.GymProfileId);

            await _context.AddTrainingProfileAsync(profile, cancellationToken);
        }
        else
        {
            profile.SetGymProfile(request.GymProfileId);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlySet<Guid>> ResolveClientAvailableEquipmentAsync(
        Guid coachId,
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null)
            throw new KeyNotFoundException($"Client '{clientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to access this client.");

        var profile = await _context.FindTrainingProfileByClientIdAsync(clientId, cancellationToken);
        GymProfile? gym = null;
        if (profile?.GymProfileId != null)
        {
            gym = await _context.FindGymProfileByIdAsync(profile.GymProfileId.Value, cancellationToken);
        }

        return _equipmentResolver.ResolveAvailableEquipment(gym, profile?.AvailableEquipmentIds);
    }

    public async Task<bool> DeleteGymProfileAsync(
        Guid coachId,
        Guid gymId,
        CancellationToken cancellationToken = default)
    {
        var gym = await _context.FindGymProfileByIdAsync(gymId, cancellationToken);
        if (gym == null)
            return false;

        if (gym.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to delete this gym profile.");

        _context.RemoveGymProfile(gym);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private GymProfileDto MapToDto(GymProfile g, List<Domain.Exercises.Equipment> allEquipment)
    {
        var resolvedIds = _equipmentResolver.ResolveAvailableEquipment(g);
        var availableNames = allEquipment
            .Where(e => resolvedIds.Contains(e.Id))
            .Select(e => e.Name)
            .OrderBy(n => n)
            .ToList();

        return new GymProfileDto(
            Id: g.Id,
            CoachId: g.CoachId,
            Name: g.Name,
            Tier: g.Tier,
            Location: g.Location,
            ExplicitEquipmentIds: g.ExplicitEquipmentIds.ToList(),
            AvailableEquipmentNames: availableNames,
            HasExplicitInventory: g.ExplicitEquipmentIds.Count > 0,
            IsInventoryAuthoritative: g.IsInventoryAuthoritative);
    }
}
