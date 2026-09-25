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

        builder.HasDiscriminator<SubstanceCategory>("SubstanceCategory")
            .HasValue<SupplementKnowledge>(SubstanceCategory.Supplement)
            .HasValue<HormoneKnowledge>(SubstanceCategory.Hormone)
            .HasValue<PEDSafetyRecord>(SubstanceCategory.PED);

        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(s => s.IsProvisional)
            .IsRequired();

        builder.Property(s => s.RequiresClinicalReview)
            .IsRequired();

        builder.Property(s => s.ClaimStatus)
            .IsRequired();

        builder.Property(s => s.ReviewedBy)
            .HasMaxLength(200);

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.Property(s => s.LastReviewedAtUtc);
        builder.Property(s => s.ReviewDueAtUtc);

        builder.HasOne(s => s.PrimaryKnowledgeClaim)
            .WithMany()
            .HasForeignKey(s => s.PrimaryKnowledgeClaimId)
            .OnDelete(DeleteBehavior.SetNull);

        var stringsComparer = new ValueComparer<IReadOnlyCollection<string>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        builder.Property(s => s.CommonAliases)
            .HasConversion(
                aliases => JsonSerializer.Serialize(aliases, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<string>)new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnName("CommonAliasesJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(stringsComparer);

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
        builder.HasIndex(s => s.SubstanceCategory);
        builder.HasIndex(s => s.IsActive);
    }
}

public class SupplementKnowledgeConfiguration : IEntityTypeConfiguration<SupplementKnowledge>
{
    public void Configure(EntityTypeBuilder<SupplementKnowledge> builder)
    {
        builder.Property(s => s.PrimaryClaimedBenefit)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.EfficacyClaim)
            .HasMaxLength(2000);

        builder.Property(s => s.EvidenceStatus)
            .IsRequired();

        builder.Property(s => s.EffectMagnitude)
            .IsRequired();

        builder.Property(s => s.PopulationNote)
            .HasMaxLength(1000);

        builder.Property(s => s.UncertaintyStatement)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(s => s.TypicalDoseRangeMin)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.TypicalDoseRangeMax)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.DoseUnit)
            .HasMaxLength(50);

        builder.Property(s => s.TimingNote)
            .HasMaxLength(500);

        builder.Property(s => s.CommonForms)
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
        builder.Property(h => h.HormoneCategory)
            .IsRequired();

        builder.Property(h => h.PhysiologicalRole)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(h => h.TrainingRelevance)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(h => h.UncertaintyStatement)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(h => h.BiomarkerReferenceNotes)
            .HasMaxLength(1000);

        var guidsComparer = new ValueComparer<IReadOnlyCollection<Guid>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        builder.Property(h => h.EvidenceClaimIds)
            .HasConversion(
                ids => JsonSerializer.Serialize(ids, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<Guid>)new List<Guid>()
                    : JsonSerializer.Deserialize<List<Guid>>(json, (JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnName("EvidenceClaimIdsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(guidsComparer);

        var stringsComparer = new ValueComparer<IReadOnlyCollection<string>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        builder.Property(h => h.MedicalEvaluationTriggers)
            .HasConversion(
                triggers => JsonSerializer.Serialize(triggers, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<string>)new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnName("MedicalEvaluationTriggersJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(stringsComparer);
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

        builder.Property(p => p.SafetyDisclaimer)
            .HasMaxLength(1000)
            .IsRequired();

        var stringsComparer = new ValueComparer<IReadOnlyCollection<string>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        builder.Property(p => p.MonitoringConcepts)
            .HasConversion(
                concepts => JsonSerializer.Serialize(concepts, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<string>)new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnName("MonitoringConceptsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(stringsComparer);

        builder.HasMany(p => p.DocumentedRisks)
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

        builder.Property(r => r.RiskCategory)
            .IsRequired();

        builder.Property(r => r.Severity)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(r => r.EvidenceLevel)
            .IsRequired();

        builder.Property(r => r.ReversibilityNotes)
            .HasMaxLength(1000);

        builder.HasOne(r => r.EvidenceClaim)
            .WithMany()
            .HasForeignKey(r => r.EvidenceClaimId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.PEDSafetyRecordId);
        builder.HasIndex(r => r.RiskCategory);
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

        builder.Property(r => r.RequiresClinicalReview)
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

        builder.HasOne(r => r.SourceClaim)
            .WithMany()
            .HasForeignKey(r => r.SourceClaimId)
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

        builder.Property(e => e.CoachNote)
            .HasMaxLength(2000);

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

        builder.Property(e => e.TriggeredFlagIds)
            .HasConversion(
                flags => JsonSerializer.Serialize(flags, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<string>)new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnName("TriggeredFlagIdsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(stringsComparer);

        builder.HasIndex(e => new { e.CoachId, e.CreatedAtUtc });
    }
}
