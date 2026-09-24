using AiCoachOs.Application.Nutrition.Dtos;
using AiCoachOs.Domain.Nutrition;

namespace AiCoachOs.Application.Nutrition.Interfaces;

public interface INutritionService
{
    Task<ClientNutritionProfileDto?> GetClientNutritionProfileAsync(
        Guid coachId,
        Guid clientId,
        CancellationToken cancellationToken = default);

    Task<ClientNutritionProfileDto> CreateOrUpdateProfileAsync(
        Guid coachId,
        CreateOrUpdateNutritionProfileRequestDto request,
        CancellationToken cancellationToken = default);

    Task<CalculateNutritionTargetsResponseDto> CalculateTargetsAsync(
        Guid coachId,
        CalculateNutritionTargetsRequestDto request,
        CancellationToken cancellationToken = default);

    Task<NutritionCalibrationRecordDto> RecordCalibrationAsync(
        Guid coachId,
        Guid clientId,
        RecordCalibrationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<NutritionCalibrationRecordDto> RecordCalibrationDecisionAsync(
        Guid coachId,
        Guid calibrationRecordId,
        RecordCalibrationDecisionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EgyptianFoodDto>> GetEgyptianFoodsAsync(
        Guid coachId,
        BudgetTier? budgetTier = null,
        FoodCategory? category = null,
        CancellationToken cancellationToken = default);

    Task<AiCoachOs.Application.Nutrition.Engine.FoodSuggestionResult> GetFoodSuggestionsAsync(
        Guid coachId,
        Guid clientId,
        BudgetTier? overrideBudgetTier = null,
        CancellationToken cancellationToken = default);
}
