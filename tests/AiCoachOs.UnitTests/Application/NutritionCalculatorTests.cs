using AiCoachOs.Application.Nutrition.Engine;
using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Nutrition;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class NutritionCalculatorTests
{
    private readonly NutritionCalculator _calculator = new();

    [Fact]
    public void CalculateCalorieEstimate_MaleModeratelyActive_ReturnsCorrectBmrTdeeAndNearest50()
    {
        // Male, 80kg, 180cm, 30yo:
        // BMR = (10*80) + (6.25*180) - (5*30) + 5 = 800 + 1125 - 150 + 5 = 1780
        // Activity ModeratelyActive (1.55): TDEE = 1780 * 1.55 = 2759
        // Nearest 50: 2750 kcal
        var result = _calculator.CalculateCalorieEstimate(
            weightKg: 80,
            heightCm: 180,
            age: 30,
            gender: Gender.Male,
            activityLevel: ActivityLevel.ModeratelyActive);

        result.Bmr.Should().Be(1780);
        result.Tdee.Should().Be(2759);
        result.EstimatedCalories.Should().Be(2750);
        result.IsHypothesis.Should().BeTrue();
        result.Uncertainty.Should().Contain("±15-25%");
        result.Method.Should().Contain("Mifflin-St Jeor");
    }

    [Fact]
    public void CalculateCalorieEstimate_FemaleLightlyActive_ReturnsCorrectBmrTdee()
    {
        // Female, 65kg, 165cm, 28yo:
        // BMR = (10*65) + (6.25*165) - (5*28) - 161 = 650 + 1031.25 - 140 - 161 = 1380.25 -> 1380
        // Activity LightlyActive (1.375): TDEE = 1380.25 * 1.375 = 1897.84 -> 1898
        // Nearest 50: 1900 kcal
        var result = _calculator.CalculateCalorieEstimate(
            weightKg: 65,
            heightCm: 165,
            age: 28,
            gender: Gender.Female,
            activityLevel: ActivityLevel.LightlyActive);

        result.Bmr.Should().Be(1380);
        result.Tdee.Should().Be(1898);
        result.EstimatedCalories.Should().Be(1900);
        result.IsHypothesis.Should().BeTrue();
    }

    [Fact]
    public void CalculateProteinTarget_CalculatesEvidenceBasedRanges()
    {
        // 80kg
        // Hypertrophy: 1.6 to 2.4 g/kg (Target 2.0 = 160g, Max 2.4 = 192g)
        var hyp = _calculator.CalculateProteinTarget(80, NutritionGoalType.Hypertrophy);
        hyp.MinimumGrams.Should().Be(128); // 80 * 1.6
        hyp.TargetGrams.Should().Be(160);  // 80 * 2.0
        hyp.MaximumGrams.Should().Be(192); // 80 * 2.4

        // Fat Loss: 1.6 to 2.4 g/kg (Target 2.2 = 176g, Max 2.4 = 192g)
        var fatLoss = _calculator.CalculateProteinTarget(80, NutritionGoalType.FatLoss);
        fatLoss.MinimumGrams.Should().Be(128); // 80 * 1.6
        fatLoss.TargetGrams.Should().Be(176);  // 80 * 2.2
        fatLoss.MaximumGrams.Should().Be(192); // 80 * 2.4

        // Strength: 1.6 to 2.4 g/kg (Target 2.0 = 160g, Max 2.4 = 192g)
        var str = _calculator.CalculateProteinTarget(80, NutritionGoalType.Strength);
        str.MinimumGrams.Should().Be(128);
        str.TargetGrams.Should().Be(160);
        str.MaximumGrams.Should().Be(192);
    }

    [Fact]
    public void CalculateGoalCalorieTarget_StandardGoals_AppliesCorrectSurplusDeficitRanges()
    {
        int tdee = 2500;

        // Hypertrophy: +200 to +300 (target +250 = 2750)
        var hyp = _calculator.CalculateGoalCalorieTarget(tdee, NutritionGoalType.Hypertrophy);
        hyp.SuggestedSurplusOrDeficitKcal.Should().Be(250);
        hyp.TargetCalories.Should().Be(2750);
        hyp.MinRangeKcal.Should().Be(2700);
        hyp.MaxRangeKcal.Should().Be(2800);
        hyp.HasDeficitWarning.Should().BeFalse();

        // Strength: +100 to +200 (target +150 = 2650)
        var str = _calculator.CalculateGoalCalorieTarget(tdee, NutritionGoalType.Strength);
        str.SuggestedSurplusOrDeficitKcal.Should().Be(150);
        str.TargetCalories.Should().Be(2650);
        str.MinRangeKcal.Should().Be(2600);
        str.MaxRangeKcal.Should().Be(2700);

        // Recomposition: 0 (target 2500, range ±0)
        var recomp = _calculator.CalculateGoalCalorieTarget(tdee, NutritionGoalType.Recomposition);
        recomp.SuggestedSurplusOrDeficitKcal.Should().Be(0);
        recomp.TargetCalories.Should().Be(2500);
        recomp.MinRangeKcal.Should().Be(2500);
        recomp.MaxRangeKcal.Should().Be(2500);

        // General: ±0 to -200 (target -100 = 2400)
        var gen = _calculator.CalculateGoalCalorieTarget(tdee, NutritionGoalType.General);
        gen.SuggestedSurplusOrDeficitKcal.Should().Be(-100);
        gen.TargetCalories.Should().Be(2400);
        gen.MinRangeKcal.Should().Be(2300);
        gen.MaxRangeKcal.Should().Be(2500);

        // Fat Loss: -300 to -500 (target -400 = 2100)
        var fatLoss = _calculator.CalculateGoalCalorieTarget(tdee, NutritionGoalType.FatLoss);
        fatLoss.SuggestedSurplusOrDeficitKcal.Should().Be(-400);
        fatLoss.TargetCalories.Should().Be(2100);
        fatLoss.MinRangeKcal.Should().Be(2000);
        fatLoss.MaxRangeKcal.Should().Be(2200);
        fatLoss.HasDeficitWarning.Should().BeFalse();
    }

    [Fact]
    public void CalculateGoalCalorieTarget_ExcessiveDeficit_TriggersWarningMessage()
    {
        int tdee = 2500;
        var excessive = _calculator.CalculateGoalCalorieTarget(
            estimatedTdee: tdee,
            goalType: NutritionGoalType.FatLoss,
            coachRequestedDeficitOrSurplus: -750);

        excessive.HasDeficitWarning.Should().BeTrue();
        excessive.WarningMessage.Should().NotBeNullOrEmpty();
        excessive.WarningMessage.Should().Contain("500 kcal");
    }

    [Fact]
    public void EvaluateCalibration_InsufficientData_WhenLessThanTwoWeeks()
    {
        var result = _calculator.EvaluateCalibration(
            weeklyWeightAverages: new[] { 80.0m },
            goalType: NutritionGoalType.FatLoss,
            currentWeightKg: 80.0m);

        result.Recommendation.Should().Be(AdjustmentRecommendation.InsufficientData);
        result.AdjustmentKcal.Should().BeNull();
        result.WeeksObserved.Should().Be(1);
    }

    [Fact]
    public void EvaluateCalibration_FatLossTooSlow_RecommendsDecreaseCalories()
    {
        // -0.1kg on 80kg over 1 week interval = -0.125%/wk, slower than target -0.5% to -1.0%
        var result = _calculator.EvaluateCalibration(
            weeklyWeightAverages: new[] { 80.0m, 79.9m },
            goalType: NutritionGoalType.FatLoss,
            currentWeightKg: 79.9m);

        result.Recommendation.Should().Be(AdjustmentRecommendation.Decrease);
        result.AdjustmentKcal.Should().BeInRange(-150, -100);
        result.WeeksObserved.Should().Be(2);
    }

    [Fact]
    public void EvaluateCalibration_FatLossTooFast_RecommendsIncreaseCalories()
    {
        // -2.5kg on 80kg over 1 week interval = -3.125%/wk, faster than -1.0%
        var result = _calculator.EvaluateCalibration(
            weeklyWeightAverages: new[] { 80.0m, 77.5m },
            goalType: NutritionGoalType.FatLoss,
            currentWeightKg: 77.5m);

        result.Recommendation.Should().Be(AdjustmentRecommendation.Increase);
        result.AdjustmentKcal.Should().BeInRange(100, 150);
        result.WeeksObserved.Should().Be(2);
    }

    [Fact]
    public void EvaluateCalibration_FatLossOnTrack_RecommendsMaintain()
    {
        // -0.6kg on 80kg over 1 week = -0.75%/wk, within -0.5% to -1.0%
        var result = _calculator.EvaluateCalibration(
            weeklyWeightAverages: new[] { 80.0m, 79.4m },
            goalType: NutritionGoalType.FatLoss,
            currentWeightKg: 79.4m);

        result.Recommendation.Should().Be(AdjustmentRecommendation.Maintain);
        result.AdjustmentKcal.Should().Be(0);
        result.WeeksObserved.Should().Be(2);
    }

    [Fact]
    public void EvaluateCalibration_HypertrophyOptimal_RecommendsMaintain()
    {
        // +0.3kg on 80kg = +0.375%/wk, within +0.25% to +0.5%
        var result = _calculator.EvaluateCalibration(
            weeklyWeightAverages: new[] { 80.0m, 80.3m },
            goalType: NutritionGoalType.Hypertrophy,
            currentWeightKg: 80.3m);

        result.Recommendation.Should().Be(AdjustmentRecommendation.Maintain);
        result.AdjustmentKcal.Should().Be(0);
        result.WeeksObserved.Should().Be(2);
    }

    [Fact]
    public void EvaluateCalibration_HypertrophyTooSlow_RecommendsIncrease()
    {
        var result = _calculator.EvaluateCalibration(
            weeklyWeightAverages: new[] { 80.0m, 80.0m },
            goalType: NutritionGoalType.Hypertrophy,
            currentWeightKg: 80.0m);

        result.Recommendation.Should().Be(AdjustmentRecommendation.Increase);
        result.AdjustmentKcal.Should().BeInRange(100, 150);
        result.WeeksObserved.Should().Be(2);
    }

    [Fact]
    public void EvaluateCalibration_HypertrophyTooFast_RecommendsDecrease()
    {
        // +0.8kg on 80kg = +1.0%/wk, faster than +0.5%
        var result = _calculator.EvaluateCalibration(
            weeklyWeightAverages: new[] { 80.0m, 80.8m },
            goalType: NutritionGoalType.Hypertrophy,
            currentWeightKg: 80.8m);

        result.Recommendation.Should().Be(AdjustmentRecommendation.Decrease);
        result.AdjustmentKcal.Should().BeInRange(-150, -100);
        result.WeeksObserved.Should().Be(2);
    }
}
