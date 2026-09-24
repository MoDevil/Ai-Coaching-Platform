using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Nutrition;

public class EgyptianFood : Entity<Guid>
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string ServingDescription { get; private set; } = string.Empty;
    public decimal ServingGrams { get; private set; }

    public decimal CaloriesPer100g { get; private set; }
    public decimal ProteinPer100g { get; private set; }
    public decimal CarbsPer100g { get; private set; }
    public decimal FatPer100g { get; private set; }
    public decimal? FiberPer100g { get; private set; }

    public FoodCategory FoodCategory { get; private set; }
    public bool IsAffordableLow { get; private set; }
    public bool IsAffordableMid { get; private set; }

    public string DataSource { get; private set; } = string.Empty;
    public DataConfidence DataConfidence { get; private set; }
    public string? VariabilityNote { get; private set; }

    private EgyptianFood() { } // EF Core

    public EgyptianFood(
        Guid id,
        string nameAr,
        string nameEn,
        string servingDescription,
        decimal servingGrams,
        decimal caloriesPer100g,
        decimal proteinPer100g,
        decimal carbsPer100g,
        decimal fatPer100g,
        FoodCategory foodCategory,
        bool isAffordableLow,
        bool isAffordableMid,
        string dataSource,
        DataConfidence dataConfidence,
        decimal? fiberPer100g = null,
        string? variabilityNote = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new ArgumentException("Arabic name cannot be empty.", nameof(nameAr));
        if (string.IsNullOrWhiteSpace(nameEn))
            throw new ArgumentException("English name cannot be empty.", nameof(nameEn));
        if (string.IsNullOrWhiteSpace(servingDescription))
            throw new ArgumentException("Serving description cannot be empty.", nameof(servingDescription));
        if (servingGrams <= 0)
            throw new ArgumentException("Serving grams must be greater than zero.", nameof(servingGrams));
        if (string.IsNullOrWhiteSpace(dataSource))
            throw new ArgumentException("DataSource cannot be empty.", nameof(dataSource));

        NameAr = nameAr.Trim();
        NameEn = nameEn.Trim();
        ServingDescription = servingDescription.Trim();
        ServingGrams = servingGrams;
        CaloriesPer100g = caloriesPer100g;
        ProteinPer100g = proteinPer100g;
        CarbsPer100g = carbsPer100g;
        FatPer100g = fatPer100g;
        FoodCategory = foodCategory;
        IsAffordableLow = isAffordableLow;
        IsAffordableMid = isAffordableMid;
        DataSource = dataSource.Trim();
        DataConfidence = dataConfidence;
        FiberPer100g = fiberPer100g;
        VariabilityNote = variabilityNote?.Trim();
    }
}
