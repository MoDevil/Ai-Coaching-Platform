using System.Text.Json;
using AiCoachOs.Domain.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class SafetyScreeningConfiguration : IEntityTypeConfiguration<SafetyScreening>
{
    public void Configure(EntityTypeBuilder<SafetyScreening> builder)
    {
        builder.ToTable("SafetyScreenings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ClientId)
            .IsRequired();

        builder.Property(s => s.TriggeredByType)
            .IsRequired();

        builder.Property(s => s.ScreeningResult)
            .IsRequired();

        builder.Property(s => s.RecommendedAction)
            .IsRequired();

        builder.Property(s => s.SummaryRationale)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(s => s.Disclaimer)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.GeneratedAtUtc)
            .IsRequired();

        builder.Property(s => s.RequiresCoachAcknowledgment)
            .IsRequired();

        builder.Property(s => s.CoachNote)
            .HasMaxLength(1000);

        builder.HasOne(s => s.Client)
            .WithMany()
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        var signalsComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyCollection<ReportedSignal>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        var stringsComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyCollection<string>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        // Store ReportedSignals value object list as JSON string
        builder.Property(s => s.ReportedSignals)
            .HasConversion(
                signals => JsonSerializer.Serialize(signals, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<ReportedSignal>)new List<ReportedSignal>()
                    : JsonSerializer.Deserialize<List<ReportedSignal>>(json, (JsonSerializerOptions?)null) ?? new List<ReportedSignal>())
            .HasColumnName("ReportedSignalsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(signalsComparer);

        // Store RedFlagsMatched list as JSON string
        builder.Property(s => s.RedFlagsMatched)
            .HasConversion(
                flags => JsonSerializer.Serialize(flags, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<string>)new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnName("RedFlagsMatchedJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(stringsComparer);

        builder.HasIndex(s => s.ClientId);
        builder.HasIndex(s => s.GeneratedAtUtc);
    }
}

public class RedFlagRuleConfiguration : IEntityTypeConfiguration<RedFlagRule>
{
    public void Configure(EntityTypeBuilder<RedFlagRule> builder)
    {
        builder.ToTable("RedFlagRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(r => r.SignalPattern)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.SafetyCategoryTriggered)
            .IsRequired();

        builder.Property(r => r.RecommendedAction)
            .IsRequired();

        builder.Property(r => r.EvidenceBasis)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(r => r.RequiresClinicalReview)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired();

        builder.Property(r => r.ReviewedBy)
            .HasMaxLength(200);

        builder.HasOne(r => r.KnowledgeClaim)
            .WithMany()
            .HasForeignKey(r => r.KnowledgeClaimId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.SignalPattern);
        builder.HasIndex(r => r.IsActive);
    }
}
