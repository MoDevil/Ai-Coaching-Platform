using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Nutrition;

namespace AiCoachOs.Application.Nutrition.Engine;

public record CalorieEstimateResult(
    int Bmr,
    int Tdee,
    int EstimatedCalories,
    string Method,
    string Uncertainty,
    bool IsHypothesis);

public record ProteinTargetResult(
    int MinimumGrams,
    int TargetGrams,
    int MaximumGrams,
    string BasisNote);

public record CalorieGoalResult(
    int TargetCalories,
    int SuggestedSurplusOrDeficitKcal,
    int MinRangeKcal,
    int MaxRangeKcal,
    bool HasDeficitWarning,
    string? WarningMessage);

public record CalibrationResult(
    AdjustmentRecommendation Recommendation,
    int? AdjustmentKcal,
    int WeeksObserved,
    string Rationale);

public interface INutritionCalculator
{
    CalorieEstimateResult CalculateCalorieEstimate(
        decimal weightKg,
        decimal heightCm,
        int age,
        Gender gender,
        ActivityLevel activityLevel);

    ProteinTargetResult CalculateProteinTarget(
        decimal weightKg,
        NutritionGoalType goalType);

    CalorieGoalResult CalculateGoalCalorieTarget(
        int estimatedTdee,
        NutritionGoalType goalType,
        int? coachRequestedDeficitOrSurplus = null);

    CalibrationResult EvaluateCalibration(
        IReadOnlyList<decimal> weeklyWeightAverages,
        NutritionGoalType goalType,
        decimal currentWeightKg);
}
