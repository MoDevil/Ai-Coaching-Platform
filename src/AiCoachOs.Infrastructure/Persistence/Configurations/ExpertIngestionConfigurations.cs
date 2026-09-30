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
        builder.Property(es => es.SourceType).HasColumnName("source_type").HasConversion<int>().IsRequired();
        builder.Property(es => es.Url).HasColumnName("url").HasMaxLength(1000).IsRequired();
        builder.Property(es => es.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(es => es.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(es => es.SourceType);
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
        builder.Property(eci => eci.ExpertSourceId).HasColumnName("expert_source_id");
        builder.Property(eci => eci.SourceUrl).HasColumnName("source_url").HasMaxLength(1000).IsRequired();
        builder.Property(eci => eci.SourceTitle).HasColumnName("source_title").HasMaxLength(500).IsRequired();
        builder.Property(eci => eci.SourceType).HasColumnName("source_type").HasConversion<int>().IsRequired();
        builder.Property(eci => eci.PublishedAt).HasColumnName("published_at");
        builder.Property(eci => eci.ExtractedTextLength).HasColumnName("extracted_text_length").IsRequired();
        builder.Property(eci => eci.WasTruncated).HasColumnName("was_truncated").IsRequired();
        builder.Property(eci => eci.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(eci => eci.FailureReason).HasColumnName("failure_reason").HasMaxLength(1000);
        builder.Property(eci => eci.ContainsMedicalClaims).HasColumnName("contains_medical_claims").IsRequired();
        builder.Property(eci => eci.SubmittedAtUtc).HasColumnName("submitted_at_utc").IsRequired();
        builder.Property(eci => eci.ProcessedAtUtc).HasColumnName("processed_at_utc");
        builder.Property(eci => eci.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(eci => eci.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne<AiCoachOs.Domain.Coaches.Coach>()
            .WithMany()
            .HasForeignKey(eci => eci.CoachId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(eci => eci.ExpertSource)
            .WithMany()
            .HasForeignKey(eci => eci.ExpertSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(eci => eci.Claims)
            .WithOne(c => c.Ingestion)
            .HasForeignKey(c => c.IngestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(eci => eci.CoachId);
        builder.HasIndex(eci => eci.ExpertSourceId);
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
        builder.Property(ec => ec.ClaimText).HasColumnName("claim_text").IsRequired();
        builder.Property(ec => ec.ClaimCategory).HasColumnName("claim_category").HasConversion<int>().IsRequired();
        builder.Property(ec => ec.EvidenceClassification).HasColumnName("evidence_classification").HasConversion<int>().IsRequired();
        builder.Property(ec => ec.CreatorConfidence).HasColumnName("creator_confidence").HasConversion<int>().IsRequired();
        builder.Property(ec => ec.DirectQuote).HasColumnName("direct_quote").IsRequired();
        builder.Property(ec => ec.SourceContext).HasColumnName("source_context").HasMaxLength(500);
        builder.Property(ec => ec.ConflictingClaimId).HasColumnName("conflicting_claim_id");
        builder.Property(ec => ec.SupportingClaimId).HasColumnName("supporting_claim_id");
        builder.Property(ec => ec.CoachReviewStatus).HasColumnName("coach_review_status").HasConversion<int>().IsRequired();
        builder.Property(ec => ec.CoachReviewedAt).HasColumnName("coach_reviewed_at");
        builder.Property(ec => ec.CoachNote).HasColumnName("coach_note").HasMaxLength(1000);
        builder.Property(ec => ec.ApprovedKnowledgeClaimId).HasColumnName("approved_knowledge_claim_id");
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
        builder.HasIndex(ec => ec.ClaimCategory);
        builder.HasIndex(ec => ec.CoachReviewStatus);
        builder.HasIndex(ec => ec.SupportingClaimId);
        builder.HasIndex(ec => ec.ConflictingClaimId);
    }
}
