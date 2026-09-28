using System.Text.Json;
using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class ClientMemoryRecordConfiguration : IEntityTypeConfiguration<ClientMemoryRecord>
{
    public void Configure(EntityTypeBuilder<ClientMemoryRecord> builder)
    {
        builder.ToTable("ClientMemoryRecords");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ClientId)
            .IsRequired();

        builder.Property(m => m.CoachId)
            .IsRequired();

        builder.Property(m => m.MemoryCategory)
            .IsRequired();

        builder.Property(m => m.RecordedAt)
            .IsRequired();

        builder.Property(m => m.ObservedAt);

        builder.Property(m => m.SourceType)
            .IsRequired();

        builder.Property(m => m.SourceReference)
            .HasMaxLength(500);

        builder.Property(m => m.SourceDescription)
            .HasMaxLength(1000);

        builder.Property(m => m.ConfidenceLevel)
            .IsRequired();

        builder.Property(m => m.RecordStatus)
            .IsRequired();

        builder.Property(m => m.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(m => m.SupersededAt);

        builder.Property(m => m.SupersessionReason)
            .HasMaxLength(1000);

        builder.Property(m => m.IsConflicted)
            .IsRequired();

        builder.Property(m => m.ConflictNotes)
            .HasMaxLength(2000);

        builder.Property(m => m.CoachCorrectionNote)
            .HasMaxLength(2000);

        builder.Property(m => m.CorrectedAt);

        builder.Property(m => m.IsAnonymized)
            .IsRequired();

        builder.Property(m => m.AnonymizedAt);

        builder.Property(m => m.CreatedAtUtc)
            .IsRequired();

        builder.Property(m => m.UpdatedAtUtc);

        // Foreign keys with ON DELETE RESTRICT
        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(m => m.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing FK for supersession: OLD.SupersededById = NEW.Id
        builder.HasOne(m => m.SupersededBy)
            .WithMany()
            .HasForeignKey(m => m.SupersededById)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for fast querying
        builder.HasIndex(m => m.ClientId);
        builder.HasIndex(m => m.CoachId);
        builder.HasIndex(m => new { m.ClientId, m.RecordStatus });
        builder.HasIndex(m => new { m.ClientId, m.MemoryCategory });
        builder.HasIndex(m => new { m.ClientId, m.RecordedAt });
        builder.HasIndex(m => m.IsConflicted);
        builder.HasIndex(m => m.IsAnonymized);
    }
}

public class ClientMemoryConflictConfiguration : IEntityTypeConfiguration<ClientMemoryConflict>
{
    public void Configure(EntityTypeBuilder<ClientMemoryConflict> builder)
    {
        builder.ToTable("ClientMemoryConflicts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClientId)
            .IsRequired();

        builder.Property(c => c.RecordAId)
            .IsRequired();

        builder.Property(c => c.RecordBId)
            .IsRequired();

        builder.Property(c => c.ConflictDescription)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(c => c.DetectedAtUtc)
            .IsRequired();

        builder.Property(c => c.IsAutoDetected)
            .IsRequired();

        builder.Property(c => c.IsResolved)
            .IsRequired();

        builder.Property(c => c.ResolvedAtUtc);
        builder.Property(c => c.ResolvedByCoachId);

        builder.Property(c => c.ResolutionNote)
            .HasMaxLength(2000);

        builder.Property(c => c.WinningRecordId);

        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.RecordA)
            .WithMany()
            .HasForeignKey(c => c.RecordAId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.RecordB)
            .WithMany()
            .HasForeignKey(c => c.RecordBId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.ClientId, c.IsResolved });
    }
}

public class ClientMemorySnapshotConfiguration : IEntityTypeConfiguration<ClientMemorySnapshot>
{
    public void Configure(EntityTypeBuilder<ClientMemorySnapshot> builder)
    {
        builder.ToTable("ClientMemorySnapshots");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ClientId)
            .IsRequired();

        builder.Property(s => s.CoachId)
            .IsRequired();

        builder.Property(s => s.GeneratedAtUtc)
            .IsRequired();

        builder.Property(s => s.GenerationTrigger)
            .IsRequired();

        builder.Property(s => s.SnapshotContentJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(s => s.IsStale)
            .IsRequired();

        var guidsComparer = new ValueComparer<IReadOnlyCollection<Guid>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        builder.Property(s => s.IncludedRecordIds)
            .HasConversion(
                ids => JsonSerializer.Serialize(ids, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<Guid>)new List<Guid>()
                    : JsonSerializer.Deserialize<List<Guid>>(json, (JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnName("IncludedRecordIdsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(guidsComparer);

        builder.Property(s => s.ExcludedConflictIds)
            .HasConversion(
                ids => JsonSerializer.Serialize(ids, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<Guid>)new List<Guid>()
                    : JsonSerializer.Deserialize<List<Guid>>(json, (JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnName("ExcludedConflictIdsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(guidsComparer);

        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.ClientId, s.GeneratedAtUtc });
    }
}

public class AIRecommendationRecordConfiguration : IEntityTypeConfiguration<AIRecommendationRecord>
{
    public void Configure(EntityTypeBuilder<AIRecommendationRecord> builder)
    {
        builder.ToTable("AIRecommendationRecords");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ClientId)
            .IsRequired();

        builder.Property(r => r.CoachId)
            .IsRequired();

        builder.Property(r => r.RecommendationCategory)
            .IsRequired();

        builder.Property(r => r.RecommendationText)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(r => r.RationaleText)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(r => r.ConfidenceStatement)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(r => r.GeneratedAt)
            .IsRequired();

        builder.Property(r => r.AIProvider)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.AIModel)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.ReviewStatus)
            .IsRequired();

        builder.Property(r => r.CoachDecision);

        builder.Property(r => r.CoachDecisionNote)
            .HasMaxLength(2000);

        builder.Property(r => r.CoachDecisionAt);

        builder.Property(r => r.FinalImplementedPlan)
            .HasMaxLength(4000);

        var guidsComparer = new ValueComparer<IReadOnlyCollection<Guid>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly());

        builder.Property(r => r.KnowledgeClaimRefs)
            .HasConversion(
                ids => JsonSerializer.Serialize(ids, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? (IReadOnlyCollection<Guid>)new List<Guid>()
                    : JsonSerializer.Deserialize<List<Guid>>(json, (JsonSerializerOptions?)null) ?? new List<Guid>())
            .HasColumnName("KnowledgeClaimRefsJson")
            .HasColumnType("text")
            .Metadata.SetValueComparer(guidsComparer);

        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.ClientId, r.ReviewStatus });
    }
}

public class ClientAnonymizationLogConfiguration : IEntityTypeConfiguration<ClientAnonymizationLog>
{
    public void Configure(EntityTypeBuilder<ClientAnonymizationLog> builder)
    {
        builder.ToTable("ClientAnonymizationLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.ClientId)
            .IsRequired();

        builder.Property(l => l.RequestedByCoachId)
            .IsRequired();

        builder.Property(l => l.AnonymizedAt)
            .IsRequired();

        builder.Property(l => l.RecordsAnonymized)
            .IsRequired();

        builder.Property(l => l.AnonymizationReason)
            .HasMaxLength(1000);

        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(l => l.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.ClientId, l.AnonymizedAt });
    }
}
