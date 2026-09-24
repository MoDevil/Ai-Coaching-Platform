using System.Text.Json;
using AiCoachOs.Domain.Gyms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class GymProfileConfiguration : IEntityTypeConfiguration<GymProfile>
{
    public void Configure(EntityTypeBuilder<GymProfile> builder)
    {
        builder.ToTable("GymProfiles");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.CoachId)
            .IsRequired();

        builder.Property(g => g.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(g => g.Location)
            .HasMaxLength(500);

        builder.Property(g => g.Tier)
            .IsRequired();

        builder.Property(g => g.IsInventoryAuthoritative)
            .IsRequired()
            .HasDefaultValue(false);

        var guidsComparer = new ValueComparer<IReadOnlyCollection<Guid>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        builder.Property(g => g.ExplicitEquipmentIds)
            .HasConversion(
                list => JsonSerializer.Serialize(list, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<Guid>)new List<Guid>()
                    : JsonSerializer.Deserialize<List<Guid>>(json, (JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnName("ExplicitEquipmentIdsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(guidsComparer);

        builder.HasIndex(g => g.CoachId);
        builder.HasIndex(g => g.Tier);
    }
}
