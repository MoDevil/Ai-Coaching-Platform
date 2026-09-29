using AiCoachOs.Domain.ExpertIngestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class ExpertSourceConfiguration : IEntityTypeConfiguration<ExpertSource>
{
    public void Configure(EntityTypeBuilder<ExpertSource> builder)
    {
        builder.ToTable("expert_sources");

        builder.HasKey(es => es.Id);
        builder.Property(es => es.Id).HasColumnName("id");
        builder.Property(es => es.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(es => es.ChannelOrPublication).HasColumnName("channel_or_publication").HasMaxLength(200).IsRequired();
        builder.Property(es => es.Platform).HasColumnName("platform").HasConversion<int>().IsRequired();
        builder.Property(es => es.PrimaryDomain).HasColumnName("primary_domain").HasMaxLength(200).IsRequired();
        builder.Property(es => es.CredibilityTier).HasColumnName("credibility_tier").HasConversion<int>().IsRequired();
        builder.Property(es => es.Bio).HasColumnName("bio").HasMaxLength(2000);
        builder.Property(es => es.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(es => es.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(es => es.Platform);
        builder.HasIndex(es => es.CredibilityTier);
    }
}

public class ExpertContentIngestionConfiguration : IEntityTypeConfiguration<ExpertContentIngestion>
{
    public void Configure(EntityTypeBuilder<ExpertContentIngestion> builder)
    {
        builder.ToTable("expert_content_ingestions");

        builder.HasKey(eci => eci.Id);
        builder.Property(eci => eci.Id).HasColumnName("id");
        builder.Property(eci => eci.CoachId).HasColumnName("coach_id").IsRequired();
        builder.Property(eci => eci.SourceId).HasColumnName("source_id");
        builder.Property(eci => eci.SourceUrl).HasColumnName("source_url").HasMaxLength(1000).IsRequired();
        builder.Property(eci => eci.ContentType).HasColumnName("content_type").HasConversion<int>().IsRequired();
        builder.Property(eci => eci.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
        builder.Property(eci => eci.RawExtractedTextSnippet).HasColumnName("raw_extracted_text_snippet");
        builder.Property(eci => eci.WordCount).HasColumnName("word_count").IsRequired();
        builder.Property(eci => eci.WasTruncated).HasColumnName("was_truncated").IsRequired();
        builder.Property(eci => eci.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(eci => eci.FailureReason).HasColumnName("failure_reason").HasMaxLength(1000);
        builder.Property(eci => eci.ContainsMedicalClaims).HasColumnName("contains_medical_claims").IsRequired();
        builder.Property(eci => eci.MedicalWarningAcknowledged).HasColumnName("medical_warning_acknowledged").IsRequired();
        builder.Property(eci => eci.SubmittedAtUtc).HasColumnName("submitted_at_utc").IsRequired();
        builder.Property(eci => eci.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(eci => eci.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(eci => eci.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne<AiCoachOs.Domain.Coaches.Coach>()
            .WithMany()
            .HasForeignKey(eci => eci.CoachId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(eci => eci.Source)
            .WithMany()
            .HasForeignKey(eci => eci.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(eci => eci.Claims)
            .WithOne(c => c.Ingestion)
            .HasForeignKey(c => c.IngestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(eci => eci.CoachId);
        builder.HasIndex(eci => eci.SourceId);
        builder.HasIndex(eci => eci.Status);
        builder.HasIndex(eci => eci.SubmittedAtUtc);
        builder.HasIndex(eci => new { eci.CoachId, eci.SourceUrl });
    }
}

public class ExpertClaimConfiguration : IEntityTypeConfiguration<ExpertClaim>
{
    public void Configure(EntityTypeBuilder<ExpertClaim> builder)
    {
        builder.ToTable("expert_claims");

        builder.HasKey(ec => ec.Id);
        builder.Property(ec => ec.Id).HasColumnName("id");
        builder.Property(ec => ec.IngestionId).HasColumnName("ingestion_id").IsRequired();
        builder.Property(ec => ec.Topic).HasColumnName("topic").HasMaxLength(100).IsRequired();
        builder.Property(ec => ec.SubTopic).HasColumnName("sub_topic").HasMaxLength(100);
        builder.Property(ec => ec.ClaimText).HasColumnName("claim_text").IsRequired();
        builder.Property(ec => ec.ContextOrTimestamp).HasColumnName("context_or_timestamp").HasMaxLength(200);
        builder.Property(ec => ec.DirectQuote).HasColumnName("direct_quote").IsRequired();
        builder.Property(ec => ec.NatureOfClaim).HasColumnName("nature_of_claim").HasConversion<int>().IsRequired();
        builder.Property(ec => ec.ReviewStatus).HasColumnName("review_status").HasConversion<int>().IsRequired();
        builder.Property(ec => ec.CoachNotes).HasColumnName("coach_notes").HasMaxLength(1000);
        builder.Property(ec => ec.SupportingClaimId).HasColumnName("supporting_claim_id");
        builder.Property(ec => ec.ConflictingClaimId).HasColumnName("conflicting_claim_id");
        builder.Property(ec => ec.ApprovedKnowledgeClaimId).HasColumnName("approved_knowledge_claim_id");
        builder.Property(ec => ec.ReviewedAtUtc).HasColumnName("reviewed_at_utc");
        builder.Property(ec => ec.ReviewedByCoachId).HasColumnName("reviewed_by_coach_id");
        builder.Property(ec => ec.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(ec => ec.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(ec => ec.Ingestion)
            .WithMany(i => i.Claims)
            .HasForeignKey(ec => ec.IngestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ec => ec.SupportingClaim)
            .WithMany()
            .HasForeignKey(ec => ec.SupportingClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ec => ec.ConflictingClaim)
            .WithMany()
            .HasForeignKey(ec => ec.ConflictingClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ec => ec.ApprovedKnowledgeClaim)
            .WithMany()
            .HasForeignKey(ec => ec.ApprovedKnowledgeClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AiCoachOs.Domain.Coaches.Coach>()
            .WithMany()
            .HasForeignKey(ec => ec.ReviewedByCoachId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ec => ec.IngestionId);
        builder.HasIndex(ec => ec.Topic);
        builder.HasIndex(ec => ec.ReviewStatus);
        builder.HasIndex(ec => ec.SupportingClaimId);
        builder.HasIndex(ec => ec.ConflictingClaimId);
    }
}
