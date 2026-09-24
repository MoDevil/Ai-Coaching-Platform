using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Nutrition;

public class ClientNutritionProfile : Entity<Guid>
{
    private readonly List<string> _dietaryPreferences = new();
    private readonly List<string> _foodExclusions = new();
    private readonly List<NutritionCalibrationRecord> _calibrationRecords = new();

    public Guid ClientId { get; private set; }
    public Client Client { get; private set; } = null!;

    public BudgetTier BudgetTier { get; private set; }
    public int? MealsPerDay { get; private set; }
    public int? CurrentCalorieTarget { get; private set; }
    public int? CurrentProteinTargetGrams { get; private set; }
    public DateTime? TargetSetAtUtc { get; private set; }
    public string? TargetSetMethod { get; private set; }

    public IReadOnlyCollection<string> DietaryPreferences => _dietaryPreferences.AsReadOnly();
    public IReadOnlyCollection<string> FoodExclusions => _foodExclusions.AsReadOnly();
    public IReadOnlyCollection<NutritionCalibrationRecord> CalibrationRecords => _calibrationRecords.AsReadOnly();

    private ClientNutritionProfile() { } // EF Core

    public ClientNutritionProfile(
        Guid id,
        Guid clientId,
        BudgetTier budgetTier,
        IEnumerable<string>? dietaryPreferences = null,
        IEnumerable<string>? foodExclusions = null,
        int? mealsPerDay = null,
        int? currentCalorieTarget = null,
        int? currentProteinTargetGrams = null,
        DateTime? targetSetAtUtc = null,
        string? targetSetMethod = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));

        ClientId = clientId;
        BudgetTier = budgetTier;
        MealsPerDay = mealsPerDay;
        CurrentCalorieTarget = currentCalorieTarget;
        CurrentProteinTargetGrams = currentProteinTargetGrams;
        TargetSetAtUtc = targetSetAtUtc;
        TargetSetMethod = targetSetMethod?.Trim();

        if (dietaryPreferences != null)
        {
            _dietaryPreferences.AddRange(dietaryPreferences.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()));
        }

        if (foodExclusions != null)
        {
            _foodExclusions.AddRange(foodExclusions.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()));
        }
    }

    public void UpdateProfile(
        BudgetTier budgetTier,
        IEnumerable<string>? dietaryPreferences,
        IEnumerable<string>? foodExclusions,
        int? mealsPerDay)
    {
        BudgetTier = budgetTier;
        MealsPerDay = mealsPerDay;

        _dietaryPreferences.Clear();
        if (dietaryPreferences != null)
        {
            _dietaryPreferences.AddRange(dietaryPreferences.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()));
        }

        _foodExclusions.Clear();
        if (foodExclusions != null)
        {
            _foodExclusions.AddRange(foodExclusions.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()));
        }

        MarkUpdated();
    }

    public void SetTargets(int calorieTarget, int proteinTargetGrams, string method, DateTime setAtUtc)
    {
        if (calorieTarget <= 0)
            throw new ArgumentException("Calorie target must be greater than zero.", nameof(calorieTarget));
        if (proteinTargetGrams <= 0)
            throw new ArgumentException("Protein target must be greater than zero.", nameof(proteinTargetGrams));

        CurrentCalorieTarget = calorieTarget;
        CurrentProteinTargetGrams = proteinTargetGrams;
        TargetSetMethod = method.Trim();
        TargetSetAtUtc = setAtUtc;
        MarkUpdated();
    }

    public void AddCalibrationRecord(NutritionCalibrationRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        _calibrationRecords.Add(record);
    }
}
