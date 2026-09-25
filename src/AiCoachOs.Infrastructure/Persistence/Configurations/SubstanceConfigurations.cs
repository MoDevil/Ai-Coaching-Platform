using System.Text.Json;
using AiCoachOs.Domain.Substances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class SubstanceRecordConfiguration : IEntityTypeConfiguration<SubstanceRecord>
{
    public void Configure(EntityTypeBuilder<SubstanceRecord> builder)
    {
        builder.ToTable("Substances");

        builder.HasKey(s => s.Id);

        builder.HasDiscriminator<SubstanceCategory>("Category")
            .HasValue<SupplementKnowledge>(SubstanceCategory.Supplement)
            .HasValue<HormoneKnowledge>(SubstanceCategory.Hormone)
            .HasValue<PEDSafetyRecord>(SubstanceCategory.PED);

        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(s => s.EvidenceSummary)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(s => s.ReviewedBy)
            .HasMaxLength(200);

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.HasOne(s => s.PrimaryKnowledgeClaim)
            .WithMany()
            .HasForeignKey(s => s.PrimaryKnowledgeClaimId)
            .OnDelete(DeleteBehavior.SetNull);

        var flagsComparer = new ValueComparer<IReadOnlyCollection<SubstanceSafetyFlag>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        builder.Property(s => s.SafetyFlags)
            .HasConversion(
                flags => JsonSerializer.Serialize(flags, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<SubstanceSafetyFlag>)new List<SubstanceSafetyFlag>()
                    : JsonSerializer.Deserialize<List<SubstanceSafetyFlag>>(json, (JsonSerializerOptions?)null) ?? new List<SubstanceSafetyFlag>())
            .HasColumnName("SafetyFlagsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(flagsComparer);

        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.Category);
        builder.HasIndex(s => s.IsActive);
    }
}

public class SupplementKnowledgeConfiguration : IEntityTypeConfiguration<SupplementKnowledge>
{
    public void Configure(EntityTypeBuilder<SupplementKnowledge> builder)
    {
        builder.Property(s => s.SupplementCategory)
            .IsRequired();

        builder.Property(s => s.EvidenceLevel)
            .IsRequired();

        builder.Property(s => s.UncertaintyStatement)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(s => s.CommonForms)
            .HasMaxLength(500);

        builder.Property(s => s.TypicalDoseRange)
            .HasMaxLength(500);

        builder.Property(s => s.TimingRecommendation)
            .HasMaxLength(500);

        builder.Property(s => s.InteractionsAndNotes)
            .HasMaxLength(1000);

        builder.Property(s => s.IsEgyptianMarketAvailable)
            .IsRequired();
    }
}

public class HormoneKnowledgeConfiguration : IEntityTypeConfiguration<HormoneKnowledge>
{
    public void Configure(EntityTypeBuilder<HormoneKnowledge> builder)
    {
        builder.Property(h => h.HormoneAxis)
            .IsRequired();

        builder.Property(h => h.PhysiologicalRole)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(h => h.TrainingImpactSummary)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(h => h.UncertaintyStatement)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(h => h.BiomarkerReferenceNotes)
            .HasMaxLength(1000);
    }
}

public class PEDSafetyRecordConfiguration : IEntityTypeConfiguration<PEDSafetyRecord>
{
    public void Configure(EntityTypeBuilder<PEDSafetyRecord> builder)
    {
        builder.Property(p => p.PEDCategory)
            .IsRequired();

        builder.Property(p => p.MechanismSummary)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(p => p.HealthRisksSummary)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(p => p.SafetyDisclaimer)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasMany(p => p.Risks)
            .WithOne(r => r.PEDSafetyRecord)
            .HasForeignKey(r => r.PEDSafetyRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PEDRiskRecordConfiguration : IEntityTypeConfiguration<PEDRiskRecord>
{
    public void Configure(EntityTypeBuilder<PEDRiskRecord> builder)
    {
        builder.ToTable("PEDRiskRecords");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.OrganSystem)
            .IsRequired();

        builder.Property(r => r.Severity)
            .IsRequired();

        builder.Property(r => r.RiskDescription)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(r => r.ReversibilityNotes)
            .HasMaxLength(1000);

        builder.HasOne(r => r.KnowledgeClaim)
            .WithMany()
            .HasForeignKey(r => r.KnowledgeClaimId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.PEDSafetyRecordId);
        builder.HasIndex(r => r.OrganSystem);
    }
}

public class PEDRedFlagRuleConfiguration : IEntityTypeConfiguration<PEDRedFlagRule>
{
    public void Configure(EntityTypeBuilder<PEDRedFlagRule> builder)
    {
        builder.ToTable("PEDRedFlagRules");

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

        builder.Property(r => r.EscalationLevel)
            .IsRequired();

        builder.Property(r => r.RecommendedAction)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(r => r.EvidenceBasis)
            .HasMaxLength(1000)
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

public class SubstanceEscalationRecordConfiguration : IEntityTypeConfiguration<SubstanceEscalationRecord>
{
    public void Configure(EntityTypeBuilder<SubstanceEscalationRecord> builder)
    {
        builder.ToTable("SubstanceEscalationRecords");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CoachId)
            .IsRequired();

        builder.Property(e => e.EscalationLevel)
            .IsRequired();

        builder.Property(e => e.SummaryRationale)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(e => e.RecommendedAction)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.Disclaimer)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(e => e.Coach)
            .WithMany()
            .HasForeignKey(e => e.CoachId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SubstanceRecord)
            .WithMany()
            .HasForeignKey(e => e.SubstanceRecordId)
            .OnDelete(DeleteBehavior.SetNull);

        var stringsComparer = new ValueComparer<IReadOnlyCollection<string>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        builder.Property(e => e.ReportedSignals)
            .HasConversion(
                signals => JsonSerializer.Serialize(signals, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<string>)new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnName("ReportedSignalsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(stringsComparer);

        builder.Property(e => e.MatchedRedFlags)
            .HasConversion(
                flags => JsonSerializer.Serialize(flags, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<string>)new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnName("MatchedRedFlagsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(stringsComparer);

        // Index on CoachId + CreatedAtUtc as required
        builder.HasIndex(e => new { e.CoachId, e.CreatedAtUtc });
    }
}
