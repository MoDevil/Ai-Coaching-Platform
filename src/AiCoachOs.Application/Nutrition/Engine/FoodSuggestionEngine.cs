using AiCoachOs.Domain.Nutrition;

namespace AiCoachOs.Application.Nutrition.Engine;

public enum FoodSuggestionStatus
{
    Success = 1,
    InsufficientData = 2,
    NoDataAvailable = 3
}

public record FoodOptionDto(
    Guid FoodId,
    string NameAr,
    string NameEn,
    string ServingDescription,
    decimal ServingGrams,
    decimal CaloriesPer100g,
    decimal ProteinPer100g,
    decimal CarbsPer100g,
    decimal FatPer100g,
    FoodCategory FoodCategory,
    string DataSource,
    DataConfidence DataConfidence,
    string? VariabilityNote);

public record ComplementaryProteinPairDto(
    FoodOptionDto LegumeSource,
    FoodOptionDto GrainSource,
    string CombinationTitle,
    string GuidanceNote);

public record FoodSuggestionResult(
    FoodSuggestionStatus Status,
    IReadOnlyList<FoodOptionDto> DirectProteinOptions,
    IReadOnlyList<FoodOptionDto> GeneralFoodOptions,
    IReadOnlyList<ComplementaryProteinPairDto> ComplementaryCombinations,
    string ContextNote,
    string ScientificFramingNote);

public interface IFoodSuggestionEngine
{
    FoodSuggestionResult SuggestFoods(
        IReadOnlyList<EgyptianFood> allFoods,
        BudgetTier budgetTier,
        NutritionGoalType goalType,
        decimal? targetProteinGrams = null,
        IReadOnlyList<string>? excludedKeywords = null);
}

public class FoodSuggestionEngine : IFoodSuggestionEngine
{
    public const string ScientificFraming =
        "Complementary plant-based food combinations are practical dietary options useful for meeting daily protein targets. Modern nutritional science does not require complementary proteins to be consumed within the same meal; achieving target daily amino acid balance over 24 hours is sufficient.";

    public FoodSuggestionResult SuggestFoods(
        IReadOnlyList<EgyptianFood> allFoods,
        BudgetTier budgetTier,
        NutritionGoalType goalType,
        decimal? targetProteinGrams = null,
        IReadOnlyList<string>? excludedKeywords = null)
    {
        if (allFoods == null || allFoods.Count == 0)
        {
            return new FoodSuggestionResult(
                Status: FoodSuggestionStatus.NoDataAvailable,
                DirectProteinOptions: Array.Empty<FoodOptionDto>(),
                GeneralFoodOptions: Array.Empty<FoodOptionDto>(),
                ComplementaryCombinations: Array.Empty<ComplementaryProteinPairDto>(),
                ContextNote: "No verified Egyptian food records currently available in the database. Sourced dataset pending.",
                ScientificFramingNote: ScientificFraming);
        }

        // 1. Budget Tier Filtering:
        // BudgetConstrained: IsAffordableLow == true
        // ModerateBudget: IsAffordableLow || IsAffordableMid
        // FlexibleBudget: all
        var budgetFiltered = allFoods.Where(f => budgetTier switch
        {
            BudgetTier.Constrained => f.IsAffordableLow,
            BudgetTier.Moderate => f.IsAffordableLow || f.IsAffordableMid,
            BudgetTier.Flexible => true,
            _ => true
        }).ToList();

        // 2. Exclusions filtering (if coach/client provided food exclusions)
        if (excludedKeywords != null && excludedKeywords.Count > 0)
        {
            budgetFiltered = budgetFiltered.Where(f =>
                !excludedKeywords.Any(ex =>
                    f.NameEn.Contains(ex, StringComparison.OrdinalIgnoreCase) ||
                    f.NameAr.Contains(ex, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        if (budgetFiltered.Count == 0)
        {
            return new FoodSuggestionResult(
                Status: FoodSuggestionStatus.InsufficientData,
                DirectProteinOptions: Array.Empty<FoodOptionDto>(),
                GeneralFoodOptions: Array.Empty<FoodOptionDto>(),
                ComplementaryCombinations: Array.Empty<ComplementaryProteinPairDto>(),
                ContextNote: "No foods found matching the specified budget tier and exclusion criteria.",
                ScientificFramingNote: ScientificFraming);
        }

        // 3. Direct Protein dense options (sorted by ProteinPer100g descending)
        var proteinOptions = budgetFiltered
            .Where(f => f.FoodCategory == FoodCategory.Protein || f.FoodCategory == FoodCategory.Dairy || f.FoodCategory == FoodCategory.Legumes)
            .OrderByDescending(f => f.ProteinPer100g)
            .Select(MapToOptionDto)
            .ToList();

        var generalOptions = budgetFiltered
            .OrderByDescending(f => f.ProteinPer100g)
            .Select(MapToOptionDto)
            .ToList();

        // 4. Complementary Protein Pairings (Legumes + Grains)
        var legumes = budgetFiltered.Where(f => f.FoodCategory == FoodCategory.Legumes).Select(MapToOptionDto).ToList();
        var grains = budgetFiltered.Where(f => f.FoodCategory == FoodCategory.Grains).Select(MapToOptionDto).ToList();

        var complementaryPairs = new List<ComplementaryProteinPairDto>();
        foreach (var leg in legumes)
        {
            foreach (var gr in grains)
            {
                complementaryPairs.Add(new ComplementaryProteinPairDto(
                    LegumeSource: leg,
                    GrainSource: gr,
                    CombinationTitle: $"{leg.NameEn} + {gr.NameEn} ({leg.NameAr} مع {gr.NameAr})",
                    GuidanceNote: "Practical plant-based combination providing complementary amino acid profiles across the day."));
            }
        }

        return new FoodSuggestionResult(
            Status: FoodSuggestionStatus.Success,
            DirectProteinOptions: proteinOptions,
            GeneralFoodOptions: generalOptions,
            ComplementaryCombinations: complementaryPairs,
            ContextNote: $"Showing food suggestions for budget tier '{budgetTier}'.",
            ScientificFramingNote: ScientificFraming);
    }

    private static FoodOptionDto MapToOptionDto(EgyptianFood f)
    {
        return new FoodOptionDto(
            FoodId: f.Id,
            NameAr: f.NameAr,
            NameEn: f.NameEn,
            ServingDescription: f.ServingDescription,
            ServingGrams: f.ServingGrams,
            CaloriesPer100g: f.CaloriesPer100g,
            ProteinPer100g: f.ProteinPer100g,
            CarbsPer100g: f.CarbsPer100g,
            FatPer100g: f.FatPer100g,
            FoodCategory: f.FoodCategory,
            DataSource: f.DataSource,
            DataConfidence: f.DataConfidence,
            VariabilityNote: f.VariabilityNote);
    }
}
