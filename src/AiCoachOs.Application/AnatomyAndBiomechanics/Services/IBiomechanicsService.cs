using AiCoachOs.Application.AnatomyAndBiomechanics.DTOs;

namespace AiCoachOs.Application.AnatomyAndBiomechanics.Services;

public interface IBiomechanicsService
{
    Task<ExerciseBiomechanicsDto> GetExerciseBiomechanicsAsync(Guid exerciseId, CancellationToken ct = default);
    Task<BiomechanicalConsiderationDto> AddConsiderationAsync(Guid exerciseId, CreateBiomechanicalConsiderationDto dto, CancellationToken ct = default);
}
