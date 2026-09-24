using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Nutrition;

namespace AiCoachOs.Application.Nutrition.Dtos;

public record NutritionCalibrationRecordDto(
    Guid Id,
    Guid ClientNutritionProfileId,
    DateTime RecordedAtUtc,
    decimal WeightKg,
    int? EstimatedTDEE,
    AdjustmentRecommendation AdjustmentRecommendation,
    int? AdjustmentKcal,
    int WeeksObserved,
    CalibrationDecision CoachDecision,
    string? CoachNote,
    IReadOnlyList<decimal> WeeklyWeightAverages);

public record ClientNutritionProfileDto(
    Guid Id,
    Guid ClientId,
    BudgetTier BudgetTier,
    IReadOnlyList<string> DietaryPreferences,
    IReadOnlyList<string> FoodExclusions,
    int? MealsPerDay,
    int? CurrentCalorieTarget,
    int? CurrentProteinTargetGrams,
    DateTime? TargetSetAtUtc,
    string? TargetSetMethod,
    IReadOnlyList<NutritionCalibrationRecordDto> CalibrationRecords);

public record CreateOrUpdateNutritionProfileRequestDto(
    Guid ClientId,
    BudgetTier BudgetTier,
    List<string>? DietaryPreferences = null,
    List<string>? FoodExclusions = null,
    int? MealsPerDay = null);

public record CalculateNutritionTargetsRequestDto(
    Guid ClientId,
    decimal WeightKg,
    decimal HeightCm,
    int Age,
    Gender Gender,
    ActivityLevel ActivityLevel,
    NutritionGoalType GoalType,
    int? CoachRequestedDeficitOrSurplus = null,
    bool ApplyToProfile = false);

public record CalculateNutritionTargetsResponseDto(
    int Bmr,
    int Tdee,
    int EstimatedCalories,
    string Method,
    string Uncertainty,
    bool IsHypothesis,
    int MinimumProteinGrams,
    int TargetProteinGrams,
    int MaximumProteinGrams,
    string ProteinBasisNote,
    int GoalTargetCalories,
    int SuggestedSurplusOrDeficitKcal,
    int MinRangeKcal,
    int MaxRangeKcal,
    bool HasDeficitWarning,
    string? WarningMessage);

public record RecordCalibrationRequestDto(
    decimal WeightKg,
    List<decimal> WeeklyWeightAverages,
    NutritionGoalType GoalType,
    int? EstimatedTDEE = null);

public record RecordCalibrationDecisionRequestDto(
    CalibrationDecision Decision,
    string? CoachNote = null);

public record EgyptianFoodDto(
    Guid Id,
    string NameAr,
    string NameEn,
    string ServingDescription,
    decimal ServingGrams,
    decimal CaloriesPer100g,
    decimal ProteinPer100g,
    decimal CarbsPer100g,
    decimal FatPer100g,
    decimal? FiberPer100g,
    FoodCategory FoodCategory,
    bool IsAffordableLow,
    bool IsAffordableMid,
    string DataSource,
    DataConfidence DataConfidence,
    string? VariabilityNote);
