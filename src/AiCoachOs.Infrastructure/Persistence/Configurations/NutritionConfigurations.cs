using System.Text.Json;
using AiCoachOs.Domain.Nutrition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class ClientNutritionProfileConfiguration : IEntityTypeConfiguration<ClientNutritionProfile>
{
    public void Configure(EntityTypeBuilder<ClientNutritionProfile> builder)
    {
        builder.ToTable("ClientNutritionProfiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClientId)
            .IsRequired();

        builder.HasIndex(p => p.ClientId)
            .IsUnique();

        builder.Property(p => p.BudgetTier)
            .IsRequired();

        builder.Property(p => p.TargetSetMethod)
            .HasMaxLength(200);

        builder.HasOne(p => p.Client)
            .WithOne()
            .HasForeignKey<ClientNutritionProfile>(p => p.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.CalibrationRecords)
            .WithOne(r => r.ClientNutritionProfile)
            .HasForeignKey(r => r.ClientNutritionProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        var stringsComparer = new ValueComparer<IReadOnlyCollection<string>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        builder.Property(p => p.DietaryPreferences)
            .HasConversion(
                list => JsonSerializer.Serialize(list, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<string>)new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnName("DietaryPreferencesJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(stringsComparer);

        builder.Property(p => p.FoodExclusions)
            .HasConversion(
                list => JsonSerializer.Serialize(list, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<string>)new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnName("FoodExclusionsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(stringsComparer);
    }
}

public class NutritionCalibrationRecordConfiguration : IEntityTypeConfiguration<NutritionCalibrationRecord>
{
    public void Configure(EntityTypeBuilder<NutritionCalibrationRecord> builder)
    {
        builder.ToTable("NutritionCalibrationRecords");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ClientNutritionProfileId)
            .IsRequired();

        builder.Property(r => r.RecordedAtUtc)
            .IsRequired();

        builder.Property(r => r.WeightKg)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.AdjustmentRecommendation)
            .IsRequired();

        builder.Property(r => r.CoachDecision)
            .IsRequired();

        builder.Property(r => r.CoachNote)
            .HasMaxLength(1000);

        var decimalsComparer = new ValueComparer<IReadOnlyCollection<decimal>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        builder.Property(r => r.WeeklyWeightAverages)
            .HasConversion(
                list => JsonSerializer.Serialize(list, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<decimal>)new List<decimal>()
                    : JsonSerializer.Deserialize<List<decimal>>(json, (JsonSerializerOptions?)null) ?? new List<decimal>())
            .HasColumnName("WeeklyWeightAveragesJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(decimalsComparer);

        builder.HasIndex(r => r.ClientNutritionProfileId);
        builder.HasIndex(r => r.RecordedAtUtc);
    }
}

public class EgyptianFoodConfiguration : IEntityTypeConfiguration<EgyptianFood>
{
    public void Configure(EntityTypeBuilder<EgyptianFood> builder)
    {
        builder.ToTable("EgyptianFoods");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.NameAr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.NameEn)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.ServingDescription)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.ServingGrams)
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(f => f.CaloriesPer100g)
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(f => f.ProteinPer100g)
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(f => f.CarbsPer100g)
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(f => f.FatPer100g)
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(f => f.FiberPer100g)
            .HasPrecision(6, 2);

        builder.Property(f => f.FoodCategory)
            .IsRequired();

        builder.Property(f => f.DataSource)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(f => f.DataConfidence)
            .IsRequired();

        builder.Property(f => f.VariabilityNote)
            .HasMaxLength(1000);

        builder.HasIndex(f => f.FoodCategory);
        builder.HasIndex(f => f.IsAffordableLow);
        builder.HasIndex(f => f.IsAffordableMid);
    }
}
