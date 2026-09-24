using AiCoachOs.Domain.Rehab;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class TrainingLimitationConfiguration : IEntityTypeConfiguration<TrainingLimitation>
{
    public void Configure(EntityTypeBuilder<TrainingLimitation> builder)
    {
        builder.ToTable("TrainingLimitations");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ClientId)
            .IsRequired();

        builder.Property(t => t.AffectedBodyRegion)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.LimitationSource)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.ReportedAtUtc)
            .IsRequired();

        builder.Property(t => t.CoachActivationNote)
            .HasMaxLength(1000);

        builder.HasOne(t => t.Client)
            .WithMany()
            .HasForeignKey(t => t.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.SafetyScreening)
            .WithMany()
            .HasForeignKey(t => t.SafetyScreeningId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Considerations)
            .WithOne(c => c.TrainingLimitation)
            .HasForeignKey(c => c.TrainingLimitationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.ClientId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.ReportedAtUtc);
    }
}

public class RehabAwarenessConsiderationConfiguration : IEntityTypeConfiguration<RehabAwarenessConsideration>
{
    public void Configure(EntityTypeBuilder<RehabAwarenessConsideration> builder)
    {
        builder.ToTable("RehabAwarenessConsiderations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TrainingLimitationId)
            .IsRequired();

        builder.Property(c => c.ConsiderationType)
            .IsRequired();

        builder.Property(c => c.ConsiderationText)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(c => c.EvidenceBasis)
            .HasMaxLength(1000);

        builder.Property(c => c.Disclaimer)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.Status)
            .IsRequired();

        builder.Property(c => c.GeneratedAtUtc)
            .IsRequired();

        builder.Property(c => c.CoachDecisionNote)
            .HasMaxLength(1000);

        builder.HasOne(c => c.Exercise)
            .WithMany()
            .HasForeignKey(c => c.ExerciseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.KnowledgeClaim)
            .WithMany()
            .HasForeignKey(c => c.KnowledgeClaimId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.TrainingLimitationId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.GeneratedAtUtc);
    }
}
