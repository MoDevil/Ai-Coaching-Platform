using AiCoachOs.Application.Exercises.DTOs;

namespace AiCoachOs.Application.Exercises.Services;

public interface IExerciseService
{
    Task<IReadOnlyList<ExerciseSummaryDto>> GetExercisesAsync(ExerciseFilterDto filter, CancellationToken ct = default);
    Task<ExerciseDetailDto> GetExerciseByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ExerciseSubstitutionDto>> GetExerciseSubstitutionsAsync(Guid exerciseId, CancellationToken ct = default);
    Task<IReadOnlyList<MovementPatternDto>> GetMovementPatternsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MuscleDto>> GetMusclesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EquipmentDto>> GetEquipmentAsync(CancellationToken ct = default);
}
