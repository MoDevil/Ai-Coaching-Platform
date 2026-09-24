using AiCoachOs.Application.Nutrition.Engine;
using AiCoachOs.Domain.Nutrition;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class FoodSuggestionEngineTests
{
    private readonly FoodSuggestionEngine _engine = new();

    /// <summary>
    /// In-memory synthetic test fixtures used exclusively to verify engine filtering and sorting logic.
    /// No real-world nutritional claims or fabricated seed records.
    /// </summary>
    private static List<EgyptianFood> CreateSyntheticTestFoods()
    {
        return new List<EgyptianFood>
        {
            new EgyptianFood(
                id: Guid.NewGuid(),
                nameAr: "فول مدمس تجريبي",
                nameEn: "Test Legume Item",
                servingDescription: "1 cup (200g)",
                servingGrams: 200,
                caloriesPer100g: 110,
                proteinPer100g: 8.0m,
                carbsPer100g: 20.0m,
                fatPer100g: 1.0m,
                fiberPer100g: 5.0m,
                foodCategory: FoodCategory.Legumes,
                isAffordableLow: true,
                isAffordableMid: true,
                dataSource: "Synthetic Unit Test Fixture",
                dataConfidence: DataConfidence.Moderate,
                variabilityNote: "Test fixture mock"),

            new EgyptianFood(
                id: Guid.NewGuid(),
                nameAr: "عيش بلدي تجريبي",
                nameEn: "Test Grain Item",
                servingDescription: "1 loaf (100g)",
                servingGrams: 100,
                caloriesPer100g: 250,
                proteinPer100g: 9.0m,
                carbsPer100g: 50.0m,
                fatPer100g: 1.0m,
                fiberPer100g: 3.0m,
                foodCategory: FoodCategory.Grains,
                isAffordableLow: true,
                isAffordableMid: true,
                dataSource: "Synthetic Unit Test Fixture",
                dataConfidence: DataConfidence.Moderate,
                variabilityNote: "Test fixture mock"),

            new EgyptianFood(
                id: Guid.NewGuid(),
                nameAr: "جبنة تجريبية",
                nameEn: "Test Low Cost Dairy",
                servingDescription: "100g",
                servingGrams: 100,
                caloriesPer100g: 100,
                proteinPer100g: 18.0m,
                carbsPer100g: 2.0m,
                fatPer100g: 2.0m,
                fiberPer100g: 0,
                foodCategory: FoodCategory.Dairy,
                isAffordableLow: true,
                isAffordableMid: true,
                dataSource: "Synthetic Unit Test Fixture",
                dataConfidence: DataConfidence.Moderate,
                variabilityNote: "Test fixture mock"),

            new EgyptianFood(
                id: Guid.NewGuid(),
                nameAr: "دجاج تجريبي",
                nameEn: "Test Mid Cost Protein",
                servingDescription: "150g",
                servingGrams: 150,
                caloriesPer100g: 160,
                proteinPer100g: 30.0m,
                carbsPer100g: 0m,
                fatPer100g: 3.0m,
                fiberPer100g: 0,
                foodCategory: FoodCategory.Protein,
                isAffordableLow: false,
                isAffordableMid: true,
                dataSource: "Synthetic Unit Test Fixture",
                dataConfidence: DataConfidence.Moderate,
                variabilityNote: "Test fixture mock"),

            new EgyptianFood(
                id: Guid.NewGuid(),
                nameAr: "لحم تجريبي فاخر",
                nameEn: "Test Flexible Tier Protein",
                servingDescription: "200g",
                servingGrams: 200,
                caloriesPer100g: 250,
                proteinPer100g: 25.0m,
                carbsPer100g: 0m,
                fatPer100g: 15.0m,
                fiberPer100g: 0,
                foodCategory: FoodCategory.Protein,
                isAffordableLow: false,
                isAffordableMid: false,
                dataSource: "Synthetic Unit Test Fixture",
                dataConfidence: DataConfidence.Moderate,
                variabilityNote: "Test fixture mock")
        };
    }

    [Fact]
    public void SuggestFoods_WhenFoodListIsEmpty_ReturnsNoDataAvailableSafely()
    {
        // Act
        var result = _engine.SuggestFoods(
            new List<EgyptianFood>(),
            BudgetTier.Moderate,
            NutritionGoalType.General);

        // Assert
        result.Status.Should().Be(FoodSuggestionStatus.NoDataAvailable);
        result.DirectProteinOptions.Should().BeEmpty();
        result.GeneralFoodOptions.Should().BeEmpty();
        result.ComplementaryCombinations.Should().BeEmpty();
        result.ScientificFramingNote.Should().Contain("24 hours");
        result.ContextNote.Should().Contain("pending");
    }

    [Fact]
    public void SuggestFoods_BudgetTierConstrained_OnlyIncludesLowCostAffordableFoods()
    {
        // Arrange
        var foods = CreateSyntheticTestFoods();

        // Act
        var result = _engine.SuggestFoods(
            foods,
            BudgetTier.Constrained,
            NutritionGoalType.Hypertrophy);

        // Assert
        result.Status.Should().Be(FoodSuggestionStatus.Success);
        result.GeneralFoodOptions.Should().HaveCount(3); // Test Legume, Test Grain, Test Low Cost Dairy
        result.GeneralFoodOptions.Select(f => f.NameEn).Should().Contain(new[] { "Test Legume Item", "Test Grain Item", "Test Low Cost Dairy" });
        result.GeneralFoodOptions.Select(f => f.NameEn).Should().NotContain("Test Mid Cost Protein");
        result.GeneralFoodOptions.Select(f => f.NameEn).Should().NotContain("Test Flexible Tier Protein");
    }

    [Fact]
    public void SuggestFoods_BudgetTierModerate_IncludesLowAndMidCostFoods()
    {
        // Arrange
        var foods = CreateSyntheticTestFoods();

        // Act
        var result = _engine.SuggestFoods(
            foods,
            BudgetTier.Moderate,
            NutritionGoalType.General);

        // Assert
        result.Status.Should().Be(FoodSuggestionStatus.Success);
        result.GeneralFoodOptions.Should().HaveCount(4);
        result.GeneralFoodOptions.Select(f => f.NameEn).Should().Contain("Test Mid Cost Protein");
        result.GeneralFoodOptions.Select(f => f.NameEn).Should().NotContain("Test Flexible Tier Protein");
    }

    [Fact]
    public void SuggestFoods_BudgetTierFlexible_IncludesAllFoods()
    {
        // Arrange
        var foods = CreateSyntheticTestFoods();

        // Act
        var result = _engine.SuggestFoods(
            foods,
            BudgetTier.Flexible,
            NutritionGoalType.FatLoss);

        // Assert
        result.Status.Should().Be(FoodSuggestionStatus.Success);
        result.GeneralFoodOptions.Should().HaveCount(5);
        result.GeneralFoodOptions.Select(f => f.NameEn).Should().Contain("Test Flexible Tier Protein");
    }

    [Fact]
    public void SuggestFoods_GeneratesLegumeAndGrainComplementaryPairs_With24HourFraming()
    {
        // Arrange
        var foods = CreateSyntheticTestFoods();

        // Act
        var result = _engine.SuggestFoods(
            foods,
            BudgetTier.Constrained,
            NutritionGoalType.Hypertrophy);

        // Assert
        result.ComplementaryCombinations.Should().NotBeEmpty();
        var pair = result.ComplementaryCombinations.First();
        pair.LegumeSource.NameEn.Should().Be("Test Legume Item");
        pair.GrainSource.NameEn.Should().Be("Test Grain Item");
        pair.CombinationTitle.Should().Contain("Test Legume Item + Test Grain Item");
        result.ScientificFramingNote.Should().Contain("24 hours");
    }

    [Fact]
    public void SuggestFoods_ExclusionFilter_ExcludesMatchingKeywords()
    {
        // Arrange
        var foods = CreateSyntheticTestFoods();

        // Act
        var result = _engine.SuggestFoods(
            foods,
            BudgetTier.Moderate,
            NutritionGoalType.General,
            excludedKeywords: new[] { "Dairy", "تجريبي" });

        // Assert: Excludes matching keywords
        result.Status.Should().Be(FoodSuggestionStatus.InsufficientData);
    }

    [Fact]
    public void ClientNutritionProfile_StoresBudgetTierDirectly()
    {
        // Arrange & Act
        var profile = new ClientNutritionProfile(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            budgetTier: BudgetTier.Constrained);

        // Assert
        profile.BudgetTier.Should().Be(BudgetTier.Constrained);

        // Update
        profile.UpdateProfile(BudgetTier.Flexible, null, null, 4);
        profile.BudgetTier.Should().Be(BudgetTier.Flexible);
        profile.MealsPerDay.Should().Be(4);
    }
}
