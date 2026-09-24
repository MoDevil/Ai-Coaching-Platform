using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Nutrition;

namespace AiCoachOs.Application.Nutrition.Engine;

public class NutritionCalculator : INutritionCalculator
{
    public CalorieEstimateResult CalculateCalorieEstimate(
        decimal weightKg,
        decimal heightCm,
        int age,
        Gender gender,
        ActivityLevel activityLevel)
    {
        if (weightKg <= 0) throw new ArgumentException("Weight must be greater than zero.", nameof(weightKg));
        if (heightCm <= 0) throw new ArgumentException("Height must be greater than zero.", nameof(heightCm));
        if (age <= 0) throw new ArgumentException("Age must be greater than zero.", nameof(age));

        // Mifflin-St Jeor Formula
        // Male: BMR = (10 × weightKg) + (6.25 × heightCm) − (5 × age) + 5
        // Female: BMR = (10 × weightKg) + (6.25 × heightCm) − (5 × age) − 161
        decimal bmrRaw;
        if (gender == Gender.Female)
        {
            bmrRaw = (10m * weightKg) + (6.25m * heightCm) - (5m * age) - 161m;
        }
        else
        {
            bmrRaw = (10m * weightKg) + (6.25m * heightCm) - (5m * age) + 5m;
        }

        int bmr = (int)Math.Round(bmrRaw);

        decimal activityMultiplier = activityLevel switch
        {
            ActivityLevel.Sedentary => 1.2m,
            ActivityLevel.LightlyActive => 1.375m,
            ActivityLevel.ModeratelyActive => 1.55m,
            ActivityLevel.VeryActive => 1.725m,
            _ => 1.2m
        };

        decimal tdeeRaw = bmrRaw * activityMultiplier;
        int tdee = (int)Math.Round(tdeeRaw);

        // Rounded to nearest 50
        int estimatedCalories = (int)Math.Round(tdee / 50.0) * 50;

        return new CalorieEstimateResult(
            Bmr: bmr,
            Tdee: tdee,
            EstimatedCalories: estimatedCalories,
            Method: "Mifflin-St Jeor + Activity Multiplier",
            Uncertainty: "±15-25% individual variation expected",
            IsHypothesis: true);
    }

    public ProteinTargetResult CalculateProteinTarget(
        decimal weightKg,
        NutritionGoalType goalType)
    {
        if (weightKg <= 0) throw new ArgumentException("Weight must be greater than zero.", nameof(weightKg));

        // Protein targets based on systematic review evidence for resistance training:
        // Minimum: 1.6 g/kg
        // Standard Target: 2.0 g/kg (adjusted to 2.2 g/kg in FatLoss deficit)
        // Maximum: 2.4 g/kg
        int minGrams = (int)Math.Round(weightKg * 1.6m);
        int maxGrams = (int)Math.Round(weightKg * 2.4m);

        int targetGrams = goalType switch
        {
            NutritionGoalType.FatLoss => (int)Math.Round(weightKg * 2.2m),
            NutritionGoalType.Hypertrophy => (int)Math.Round(weightKg * 2.0m),
            NutritionGoalType.Strength => (int)Math.Round(weightKg * 2.0m),
            NutritionGoalType.Recomposition => (int)Math.Round(weightKg * 2.0m),
            NutritionGoalType.General => (int)Math.Round(weightKg * 1.8m),
            _ => (int)Math.Round(weightKg * 2.0m)
        };

        if (targetGrams > maxGrams) targetGrams = maxGrams;
        if (targetGrams < minGrams) targetGrams = minGrams;

        return new ProteinTargetResult(
            MinimumGrams: minGrams,
            TargetGrams: targetGrams,
            MaximumGrams: maxGrams,
            BasisNote: "Based on current systematic review evidence for resistance training");
    }

    public CalorieGoalResult CalculateGoalCalorieTarget(
        int estimatedTdee,
        NutritionGoalType goalType,
        int? coachRequestedDeficitOrSurplus = null)
    {
        if (estimatedTdee <= 0) throw new ArgumentException("Estimated TDEE must be greater than zero.", nameof(estimatedTdee));

        int defaultDelta;
        int minRange;
        int maxRange;

        switch (goalType)
        {
            case NutritionGoalType.Hypertrophy:
                defaultDelta = 250;
                minRange = 200;
                maxRange = 300;
                break;
            case NutritionGoalType.Strength:
                defaultDelta = 150;
                minRange = 100;
                maxRange = 200;
                break;
            case NutritionGoalType.Recomposition:
                defaultDelta = 0;
                minRange = 0;
                maxRange = 0;
                break;
            case NutritionGoalType.FatLoss:
                defaultDelta = -400;
                minRange = -500;
                maxRange = -300;
                break;
            case NutritionGoalType.General:
            default:
                defaultDelta = -100;
                minRange = -200;
                maxRange = 0;
                break;
        }

        int appliedDelta = coachRequestedDeficitOrSurplus ?? defaultDelta;

        bool hasWarning = false;
        string? warningMessage = null;

        // Never automatically recommend or apply deficit > 500 kcal without warning
        if (appliedDelta < -500)
        {
            hasWarning = true;
            warningMessage = "Warning: Recommended calorie deficit exceeds 500 kcal/day safety threshold. Explicit coach review and decision required.";
        }

        int targetCalories = (int)Math.Round((estimatedTdee + appliedDelta) / 50.0) * 50;

        return new CalorieGoalResult(
            TargetCalories: targetCalories,
            SuggestedSurplusOrDeficitKcal: appliedDelta,
            MinRangeKcal: estimatedTdee + minRange,
            MaxRangeKcal: estimatedTdee + maxRange,
            HasDeficitWarning: hasWarning,
            WarningMessage: warningMessage);
    }

    public CalibrationResult EvaluateCalibration(
        IReadOnlyList<decimal> weeklyWeightAverages,
        NutritionGoalType goalType,
        decimal currentWeightKg)
    {
        if (weeklyWeightAverages == null || weeklyWeightAverages.Count < 2)
        {
            return new CalibrationResult(
                Recommendation: AdjustmentRecommendation.InsufficientData,
                AdjustmentKcal: null,
                WeeksObserved: weeklyWeightAverages?.Count ?? 0,
                Rationale: "Minimum of 2 consecutive weekly weight averages required for calibration evaluation.");
        }

        int weeks = weeklyWeightAverages.Count;
        decimal firstAvg = weeklyWeightAverages[0];
        decimal latestAvg = weeklyWeightAverages[^1];
        decimal totalDelta = latestAvg - firstAvg;
        decimal weeklyRate = totalDelta / (weeks - 1);

        switch (goalType)
        {
            case NutritionGoalType.FatLoss:
                // Expected fat loss rate: -0.3kg to -0.8kg per week
                if (weeklyRate < -1.0m)
                {
                    // Losing faster than goal rate -> suggest +100 to +150 kcal
                    return new CalibrationResult(
                        Recommendation: AdjustmentRecommendation.Increase,
                        AdjustmentKcal: 150,
                        WeeksObserved: weeks,
                        Rationale: $"Weight loss rate ({weeklyRate:F2} kg/week) is faster than recommended safe threshold. Suggest increasing calorie intake by +150 kcal.");
                }
                else if (weeklyRate >= 0m || weeklyRate > -0.2m)
                {
                    // Losing slower than goal rate or gaining in deficit -> suggest -100 to -150 kcal
                    return new CalibrationResult(
                        Recommendation: AdjustmentRecommendation.Decrease,
                        AdjustmentKcal: -150,
                        WeeksObserved: weeks,
                        Rationale: $"Weight change ({weeklyRate:F2} kg/week) indicates lower than targeted deficit. Suggest decreasing calorie intake by -150 kcal.");
                }
                else
                {
                    // On track
                    return new CalibrationResult(
                        Recommendation: AdjustmentRecommendation.Maintain,
                        AdjustmentKcal: 0,
                        WeeksObserved: weeks,
                        Rationale: $"Weight trend ({weeklyRate:F2} kg/week) aligns with expected fat loss rate. Maintain current target.");
                }

            case NutritionGoalType.Hypertrophy:
                // Expected lean mass gain rate: +0.1kg to +0.35kg per week
                if (weeklyRate > 0.5m)
                {
                    // Gaining too rapidly
                    return new CalibrationResult(
                        Recommendation: AdjustmentRecommendation.Decrease,
                        AdjustmentKcal: -100,
                        WeeksObserved: weeks,
                        Rationale: $"Rate of weight gain ({weeklyRate:F2} kg/week) exceeds optimal hypertrophy surplus. Suggest decreasing calories by -100 kcal.");
                }
                else if (weeklyRate <= 0m)
                {
                    // Not gaining or losing
                    return new CalibrationResult(
                        Recommendation: AdjustmentRecommendation.Increase,
                        AdjustmentKcal: 150,
                        WeeksObserved: weeks,
                        Rationale: $"Weight is stagnant or declining ({weeklyRate:F2} kg/week) during hypertrophy phase. Suggest increasing calories by +150 kcal.");
                }
                else
                {
                    return new CalibrationResult(
                        Recommendation: AdjustmentRecommendation.Maintain,
                        AdjustmentKcal: 0,
                        WeeksObserved: weeks,
                        Rationale: $"Weight gain rate ({weeklyRate:F2} kg/week) aligns with expected hypertrophy progression. Maintain current target.");
                }

            case NutritionGoalType.Strength:
            case NutritionGoalType.Recomposition:
            case NutritionGoalType.General:
            default:
                if (Math.Abs(weeklyRate) <= 0.25m)
                {
                    return new CalibrationResult(
                        Recommendation: AdjustmentRecommendation.Maintain,
                        AdjustmentKcal: 0,
                        WeeksObserved: weeks,
                        Rationale: $"Weight is stable ({weeklyRate:F2} kg/week) as expected for maintenance/recomposition. Maintain current target.");
                }
                else if (weeklyRate > 0.25m)
                {
                    return new CalibrationResult(
                        Recommendation: AdjustmentRecommendation.Decrease,
                        AdjustmentKcal: -100,
                        WeeksObserved: weeks,
                        Rationale: $"Weight is trending upward ({weeklyRate:F2} kg/week). Suggest adjusting calories by -100 kcal.");
                }
                else
                {
                    return new CalibrationResult(
                        Recommendation: AdjustmentRecommendation.Increase,
                        AdjustmentKcal: 100,
                        WeeksObserved: weeks,
                        Rationale: $"Weight is trending downward ({weeklyRate:F2} kg/week). Suggest adjusting calories by +100 kcal.");
                }
        }
    }
}
