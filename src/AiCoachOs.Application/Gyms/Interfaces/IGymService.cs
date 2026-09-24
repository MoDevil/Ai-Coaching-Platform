using AiCoachOs.Application.Gyms.Dtos;
using AiCoachOs.Domain.Gyms;

namespace AiCoachOs.Application.Gyms.Interfaces;

public interface IGymService
{
    Task<GymProfileDto> CreateGymProfileAsync(
        Guid coachId,
        CreateGymProfileRequestDto request,
        CancellationToken cancellationToken = default);

    Task<GymProfileDto> UpdateGymProfileAsync(
        Guid coachId,
        Guid gymId,
        UpdateGymProfileRequestDto request,
        CancellationToken cancellationToken = default);

    Task<GymProfileDto?> GetGymProfileByIdAsync(
        Guid coachId,
        Guid gymId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GymProfileDto>> GetGymProfilesAsync(
        Guid coachId,
        CancellationToken cancellationToken = default);

    Task<bool> AssignClientGymAsync(
        Guid coachId,
        AssignClientGymRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<Guid>> ResolveClientAvailableEquipmentAsync(
        Guid coachId,
        Guid clientId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteGymProfileAsync(
        Guid coachId,
        Guid gymId,
        CancellationToken cancellationToken = default);
}
