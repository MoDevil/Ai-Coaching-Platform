using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Nutrition.Dtos;
using AiCoachOs.Application.Nutrition.Engine;
using AiCoachOs.Application.Nutrition.Interfaces;
using AiCoachOs.Domain.Nutrition;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class NutritionService : INutritionService
{
    private readonly IApplicationDbContext _context;
    private readonly INutritionCalculator _calculator;
    private readonly IFoodSuggestionEngine _suggestionEngine;

    public NutritionService(
        IApplicationDbContext context,
        INutritionCalculator calculator,
        IFoodSuggestionEngine suggestionEngine)
    {
        _context = context;
        _calculator = calculator;
        _suggestionEngine = suggestionEngine;
    }

    public async Task<ClientNutritionProfileDto?> GetClientNutritionProfileAsync(
        Guid coachId,
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null)
            throw new KeyNotFoundException($"Client '{clientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to access nutrition data for this client.");

        var profile = await _context.FindNutritionProfileByClientIdAsync(clientId, cancellationToken);
        return profile == null ? null : MapToDto(profile);
    }

    public async Task<ClientNutritionProfileDto> CreateOrUpdateProfileAsync(
        Guid coachId,
        CreateOrUpdateNutritionProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(request.ClientId, cancellationToken);
        if (client == null)
            throw new KeyNotFoundException($"Client '{request.ClientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to update nutrition profile for this client.");

        var profile = await _context.FindNutritionProfileByClientIdAsync(request.ClientId, cancellationToken);
        if (profile == null)
        {
            profile = new ClientNutritionProfile(
                id: Guid.NewGuid(),
                clientId: request.ClientId,
                budgetTier: request.BudgetTier,
                dietaryPreferences: request.DietaryPreferences,
                foodExclusions: request.FoodExclusions,
                mealsPerDay: request.MealsPerDay);

            await _context.AddClientNutritionProfileAsync(profile, cancellationToken);
        }
        else
        {
            profile.UpdateProfile(
                budgetTier: request.BudgetTier,
                dietaryPreferences: request.DietaryPreferences,
                foodExclusions: request.FoodExclusions,
                mealsPerDay: request.MealsPerDay);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return MapToDto(profile);
    }

    public async Task<CalculateNutritionTargetsResponseDto> CalculateTargetsAsync(
        Guid coachId,
        CalculateNutritionTargetsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(request.ClientId, cancellationToken);
        if (client == null)
            throw new KeyNotFoundException($"Client '{request.ClientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to calculate nutrition targets for this client.");

        var calorieEstimate = _calculator.CalculateCalorieEstimate(
            weightKg: request.WeightKg,
            heightCm: request.HeightCm,
            age: request.Age,
            gender: request.Gender,
            activityLevel: request.ActivityLevel);

        var proteinTarget = _calculator.CalculateProteinTarget(
            weightKg: request.WeightKg,
            goalType: request.GoalType);

        var goalCalorie = _calculator.CalculateGoalCalorieTarget(
            estimatedTdee: calorieEstimate.Tdee,
            goalType: request.GoalType,
            coachRequestedDeficitOrSurplus: request.CoachRequestedDeficitOrSurplus);

        if (request.ApplyToProfile)
        {
            var profile = await _context.FindNutritionProfileByClientIdAsync(request.ClientId, cancellationToken);
            if (profile == null)
            {
                profile = new ClientNutritionProfile(
                    id: Guid.NewGuid(),
                    clientId: request.ClientId,
                    budgetTier: BudgetTier.Moderate,
                    currentCalorieTarget: goalCalorie.TargetCalories,
                    currentProteinTargetGrams: proteinTarget.TargetGrams,
                    targetSetAtUtc: DateTime.UtcNow,
                    targetSetMethod: calorieEstimate.Method);

                await _context.AddClientNutritionProfileAsync(profile, cancellationToken);
            }
            else
            {
                profile.SetTargets(
                    calorieTarget: goalCalorie.TargetCalories,
                    proteinTargetGrams: proteinTarget.TargetGrams,
                    method: calorieEstimate.Method,
                    setAtUtc: DateTime.UtcNow);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        return new CalculateNutritionTargetsResponseDto(
            Bmr: calorieEstimate.Bmr,
            Tdee: calorieEstimate.Tdee,
            EstimatedCalories: calorieEstimate.EstimatedCalories,
            Method: calorieEstimate.Method,
            Uncertainty: calorieEstimate.Uncertainty,
            IsHypothesis: calorieEstimate.IsHypothesis,
            MinimumProteinGrams: proteinTarget.MinimumGrams,
            TargetProteinGrams: proteinTarget.TargetGrams,
            MaximumProteinGrams: proteinTarget.MaximumGrams,
            ProteinBasisNote: proteinTarget.BasisNote,
            GoalTargetCalories: goalCalorie.TargetCalories,
            SuggestedSurplusOrDeficitKcal: goalCalorie.SuggestedSurplusOrDeficitKcal,
            MinRangeKcal: goalCalorie.MinRangeKcal,
            MaxRangeKcal: goalCalorie.MaxRangeKcal,
            HasDeficitWarning: goalCalorie.HasDeficitWarning,
            WarningMessage: goalCalorie.WarningMessage);
    }

    public async Task<NutritionCalibrationRecordDto> RecordCalibrationAsync(
        Guid coachId,
        Guid clientId,
        RecordCalibrationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null)
            throw new KeyNotFoundException($"Client '{clientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to record calibration for this client.");

        var profile = await _context.FindNutritionProfileByClientIdAsync(clientId, cancellationToken);
        if (profile == null)
        {
            profile = new ClientNutritionProfile(
                id: Guid.NewGuid(),
                clientId: clientId,
                budgetTier: BudgetTier.Moderate);

            await _context.AddClientNutritionProfileAsync(profile, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var calibrationResult = _calculator.EvaluateCalibration(
            weeklyWeightAverages: request.WeeklyWeightAverages,
            goalType: request.GoalType,
            currentWeightKg: request.WeightKg);

        var record = new NutritionCalibrationRecord(
            id: Guid.NewGuid(),
            clientNutritionProfileId: profile.Id,
            recordedAtUtc: DateTime.UtcNow,
            weightKg: request.WeightKg,
            adjustmentRecommendation: calibrationResult.Recommendation,
            weeksObserved: calibrationResult.WeeksObserved,
            weeklyWeightAverages: request.WeeklyWeightAverages,
            estimatedTDEE: request.EstimatedTDEE,
            adjustmentKcal: calibrationResult.AdjustmentKcal,
            coachDecision: CalibrationDecision.Pending,
            coachNote: calibrationResult.Rationale);

        await _context.AddNutritionCalibrationRecordAsync(record, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapCalibrationRecordToDto(record);
    }

    public async Task<NutritionCalibrationRecordDto> RecordCalibrationDecisionAsync(
        Guid coachId,
        Guid calibrationRecordId,
        RecordCalibrationDecisionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var record = await _context.FindNutritionCalibrationRecordByIdAsync(calibrationRecordId, cancellationToken);
        if (record == null)
            throw new KeyNotFoundException($"Calibration record '{calibrationRecordId}' was not found.");

        var client = record.ClientNutritionProfile?.Client;
        if (client == null && record.ClientNutritionProfile != null)
        {
            client = await _context.FindClientByIdAsync(record.ClientNutritionProfile.ClientId, cancellationToken);
        }

        if (client == null || client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to record decision on this calibration record.");

        record.ApplyDecision(request.Decision, request.CoachNote);

        if (request.Decision == CalibrationDecision.Applied &&
            record.AdjustmentKcal.HasValue &&
            record.ClientNutritionProfile?.CurrentCalorieTarget != null)
        {
            var profile = record.ClientNutritionProfile;
            int newTarget = profile.CurrentCalorieTarget.Value + record.AdjustmentKcal.Value;
            profile.SetTargets(newTarget, profile.CurrentProteinTargetGrams ?? 0, "Calibration Adjustment", DateTime.UtcNow);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return MapCalibrationRecordToDto(record);
    }

    public async Task<IReadOnlyList<EgyptianFoodDto>> GetEgyptianFoodsAsync(
        Guid coachId,
        BudgetTier? budgetTier = null,
        FoodCategory? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.EgyptianFoods.AsQueryable();

        if (budgetTier.HasValue)
        {
            switch (budgetTier.Value)
            {
                case BudgetTier.Constrained:
                    query = query.Where(f => f.IsAffordableLow);
                    break;
                case BudgetTier.Moderate:
                    query = query.Where(f => f.IsAffordableLow || f.IsAffordableMid);
                    break;
                case BudgetTier.Flexible:
                    // All foods
                    break;
            }
        }

        if (category.HasValue)
        {
            query = query.Where(f => f.FoodCategory == category.Value);
        }

        var foods = await query.OrderBy(f => f.NameEn).ToListAsync(cancellationToken);

        return foods.Select(f => new EgyptianFoodDto(
            f.Id,
            f.NameAr,
            f.NameEn,
            f.ServingDescription,
            f.ServingGrams,
            f.CaloriesPer100g,
            f.ProteinPer100g,
            f.CarbsPer100g,
            f.FatPer100g,
            f.FiberPer100g,
            f.FoodCategory,
            f.IsAffordableLow,
            f.IsAffordableMid,
            f.DataSource,
            f.DataConfidence,
            f.VariabilityNote)).ToList();
    }

    public async Task<FoodSuggestionResult> GetFoodSuggestionsAsync(
        Guid coachId,
        Guid clientId,
        BudgetTier? overrideBudgetTier = null,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null)
            throw new KeyNotFoundException($"Client '{clientId}' was not found.");

        if (client.CoachId != coachId)
            throw new UnauthorizedAccessException("Coach is not authorized to access nutrition data for this client.");

        var profile = await _context.FindNutritionProfileByClientIdAsync(clientId, cancellationToken);
        var budgetTier = overrideBudgetTier ?? profile?.BudgetTier ?? BudgetTier.Moderate;
        var exclusions = profile?.FoodExclusions?.ToList() ?? new List<string>();

        var allFoods = await _context.EgyptianFoods.ToListAsync(cancellationToken);

        return _suggestionEngine.SuggestFoods(
            allFoods: allFoods,
            budgetTier: budgetTier,
            goalType: NutritionGoalType.General,
            targetProteinGrams: profile?.CurrentProteinTargetGrams,
            excludedKeywords: exclusions);
    }

    private static ClientNutritionProfileDto MapToDto(ClientNutritionProfile p)
    {
        var calibrationDtos = p.CalibrationRecords
            .OrderByDescending(r => r.RecordedAtUtc)
            .Select(MapCalibrationRecordToDto)
            .ToList();

        return new ClientNutritionProfileDto(
            p.Id,
            p.ClientId,
            p.BudgetTier,
            p.DietaryPreferences.ToList(),
            p.FoodExclusions.ToList(),
            p.MealsPerDay,
            p.CurrentCalorieTarget,
            p.CurrentProteinTargetGrams,
            p.TargetSetAtUtc,
            p.TargetSetMethod,
            calibrationDtos);
    }

    private static NutritionCalibrationRecordDto MapCalibrationRecordToDto(NutritionCalibrationRecord r)
    {
        return new NutritionCalibrationRecordDto(
            r.Id,
            r.ClientNutritionProfileId,
            r.RecordedAtUtc,
            r.WeightKg,
            r.EstimatedTDEE,
            r.AdjustmentRecommendation,
            r.AdjustmentKcal,
            r.WeeksObserved,
            r.CoachDecision,
            r.CoachNote,
            r.WeeklyWeightAverages.ToList());
    }
}
