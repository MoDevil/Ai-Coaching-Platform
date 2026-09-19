using AiCoachOs.Application.TrainingProfiles.DTOs;

namespace AiCoachOs.Application.TrainingProfiles.Services;

public interface ITrainingProfileService
{
    Task<TrainingProfileDto> GetProfileByClientIdAsync(Guid clientId, CancellationToken ct = default);
    Task<TrainingProfileDto> UpdateProfileAsync(Guid clientId, UpdateTrainingProfileRequestDto request, CancellationToken ct = default);
    Task<TrainingProfileDto> UpdateAvailabilityAsync(Guid clientId, TrainingAvailabilityDto availability, CancellationToken ct = default);
    Task<TrainingProfileDto> UpdatePrioritiesAsync(Guid clientId, List<ClientTrainingPriorityDto> priorities, CancellationToken ct = default);
}
