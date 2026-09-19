using AiCoachOs.Application.AnatomyAndBiomechanics.DTOs;

namespace AiCoachOs.Application.AnatomyAndBiomechanics.Services;

public interface IAnatomyService
{
    Task<IReadOnlyList<AnatomicalRegionSummaryDto>> GetRegionsAsync(CancellationToken ct = default);
    Task<AnatomicalRegionDto> GetRegionByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<JointSummaryDto>> GetJointsAsync(Guid? regionId = null, CancellationToken ct = default);
    Task<JointDto> GetJointByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<JointActionSummaryDto>> GetJointActionsAsync(Guid? jointId = null, CancellationToken ct = default);
    Task<JointActionDto> GetJointActionByIdAsync(Guid id, CancellationToken ct = default);
    Task<MuscleAnatomyDto> GetMuscleAnatomyAsync(Guid muscleId, CancellationToken ct = default);
}
