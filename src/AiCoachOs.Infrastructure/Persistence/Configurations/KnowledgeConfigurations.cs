using AiCoachOs.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class KnowledgeSourceConfiguration : IEntityTypeConfiguration<KnowledgeSource>
{
    public void Configure(EntityTypeBuilder<KnowledgeSource> builder)
    {
        builder.ToTable("knowledge_sources");

        builder.HasKey(ks => ks.Id);
        builder.Property(ks => ks.Id).HasColumnName("id");
        builder.Property(ks => ks.SourceType).HasColumnName("source_type").HasConversion<int>().IsRequired();
        builder.Property(ks => ks.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
        builder.Property(ks => ks.Authors).HasColumnName("authors").HasMaxLength(500).IsRequired();
        builder.Property(ks => ks.Year).HasColumnName("year").IsRequired();
        builder.Property(ks => ks.EvidenceLevel).HasColumnName("evidence_level").HasConversion<int>().IsRequired();
        builder.Property(ks => ks.Doi).HasColumnName("doi").HasMaxLength(150);
        builder.Property(ks => ks.Url).HasColumnName("url").HasMaxLength(1000);
        builder.Property(ks => ks.Notes).HasColumnName("notes");
        builder.Property(ks => ks.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(ks => ks.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(ks => ks.SourceType);
        builder.HasIndex(ks => ks.EvidenceLevel);
    }
}

public class KnowledgeClaimConfiguration : IEntityTypeConfiguration<KnowledgeClaim>
{
    public void Configure(EntityTypeBuilder<KnowledgeClaim> builder)
    {
        builder.ToTable("knowledge_claims");

        builder.HasKey(kc => kc.Id);
        builder.Property(kc => kc.Id).HasColumnName("id");
        builder.Property(kc => kc.Topic).HasColumnName("topic").HasMaxLength(100).IsRequired();
        builder.Property(kc => kc.Question).HasColumnName("question").HasMaxLength(500).IsRequired();
        builder.Property(kc => kc.ClaimText).HasColumnName("claim_text").IsRequired();
        builder.Property(kc => kc.EvidenceLevel).HasColumnName("evidence_level").HasConversion<int>().IsRequired();
        builder.Property(kc => kc.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(kc => kc.Population).HasColumnName("population").HasMaxLength(500);
        builder.Property(kc => kc.Limitations).HasColumnName("limitations");
        builder.Property(kc => kc.PracticalApplication).HasColumnName("practical_application");
        builder.Property(kc => kc.ExpertConsensus).HasColumnName("expert_consensus");
        builder.Property(kc => kc.ExpertDisagreements).HasColumnName("expert_disagreements");
        builder.Property(kc => kc.PractitionerNotes).HasColumnName("practitioner_notes");
        builder.Property(kc => kc.EgyptSpecificNotes).HasColumnName("egypt_specific_notes");
        builder.Property(kc => kc.ExerciseId).HasColumnName("exercise_id");
        builder.Property(kc => kc.ReviewedAtUtc).HasColumnName("reviewed_at_utc");
        builder.Property(kc => kc.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(200);
        builder.Property(kc => kc.SupersededByClaimId).HasColumnName("superseded_by_claim_id");
        builder.Property(kc => kc.SupersededAtUtc).HasColumnName("superseded_at_utc");
        builder.Property(kc => kc.SupersessionReason).HasColumnName("supersession_reason").HasMaxLength(1000);
        builder.Property(kc => kc.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(kc => kc.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(kc => kc.Exercise)
            .WithMany()
            .HasForeignKey(kc => kc.ExerciseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(kc => kc.SupersededByClaim)
            .WithMany()
            .HasForeignKey(kc => kc.SupersededByClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(kc => kc.Sources)
            .WithOne(kcs => kcs.Claim)
            .HasForeignKey(kcs => kcs.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(kc => kc.Topic);
        builder.HasIndex(kc => kc.Status);
        builder.HasIndex(kc => kc.ExerciseId);
    }
}

public class KnowledgeClaimSourceConfiguration : IEntityTypeConfiguration<KnowledgeClaimSource>
{
    public void Configure(EntityTypeBuilder<KnowledgeClaimSource> builder)
    {
        builder.ToTable("knowledge_claim_sources");

        builder.HasKey(kcs => new { kcs.ClaimId, kcs.SourceId });
        builder.Property(kcs => kcs.ClaimId).HasColumnName("claim_id").IsRequired();
        builder.Property(kcs => kcs.SourceId).HasColumnName("source_id").IsRequired();
        builder.Property(kcs => kcs.RelevanceNote).HasColumnName("relevance_note").HasMaxLength(1000);
        builder.Property(kcs => kcs.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasOne(kcs => kcs.Source)
            .WithMany(ks => ks.ClaimSources)
            .HasForeignKey(kcs => kcs.SourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
