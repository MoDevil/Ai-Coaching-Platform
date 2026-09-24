namespace AiCoachOs.Domain.Nutrition;

public enum BudgetTier
{
    Constrained = 1,
    Moderate = 2,
    Flexible = 3
}

public enum ActivityLevel
{
    Sedentary = 1,
    LightlyActive = 2,
    ModeratelyActive = 3,
    VeryActive = 4
}

public enum NutritionGoalType
{
    Hypertrophy = 1,
    Strength = 2,
    Recomposition = 3,
    FatLoss = 4,
    General = 5
}

public enum AdjustmentRecommendation
{
    Increase = 1,
    Decrease = 2,
    Maintain = 3,
    InsufficientData = 4
}

public enum CalibrationDecision
{
    Pending = 1,
    Applied = 2,
    Rejected = 3
}

public enum FoodCategory
{
    Legumes = 1,
    Grains = 2,
    Protein = 3,
    Dairy = 4,
    Vegetables = 5,
    Fruits = 6,
    Fats = 7
}

public enum DataConfidence
{
    High = 1,
    Moderate = 2,
    Low = 3
}
