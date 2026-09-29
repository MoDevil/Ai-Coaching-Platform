using AiCoachOs.Domain.Videos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class ClientVideoConfiguration : IEntityTypeConfiguration<ClientVideo>
{
    public void Configure(EntityTypeBuilder<ClientVideo> builder)
    {
        builder.ToTable("client_videos");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.ClientId).HasColumnName("client_id").IsRequired();
        builder.Property(v => v.CoachId).HasColumnName("coach_id").IsRequired();
        builder.Property(v => v.ExerciseId).HasColumnName("exercise_id");
        builder.Property(v => v.ExerciseName).HasColumnName("exercise_name").HasMaxLength(200).IsRequired();
        builder.Property(v => v.StorageKey).HasColumnName("storage_key").HasMaxLength(250).IsRequired();
        builder.Property(v => v.MimeType).HasColumnName("mime_type").HasMaxLength(50).IsRequired();
        builder.Property(v => v.FileSizeBytes).HasColumnName("file_size_bytes").IsRequired();
        builder.Property(v => v.DurationSeconds).HasColumnName("duration_seconds").IsRequired();
        builder.Property(v => v.FrameCount).HasColumnName("frame_count").IsRequired();
        builder.Property(v => v.FrameStorageKeys).HasColumnName("frame_storage_keys").HasColumnType("jsonb").IsRequired();
        builder.Property(v => v.UploadedAt).HasColumnName("uploaded_at").IsRequired();
        builder.Property(v => v.CoachNotes).HasColumnName("coach_notes").HasMaxLength(500);
        builder.Property(v => v.ObservationRecordId).HasColumnName("observation_record_id");
        builder.Property(v => v.IsAnonymized).HasColumnName("is_anonymized").IsRequired();
        builder.Property(v => v.AnonymizedAt).HasColumnName("anonymized_at");
        builder.Property(v => v.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(v => v.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(v => v.Client)
            .WithMany()
            .HasForeignKey(v => v.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Exercise)
            .WithMany()
            .HasForeignKey(v => v.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.ObservationRecord)
            .WithMany()
            .HasForeignKey(v => v.ObservationRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.ClientId);
        builder.HasIndex(v => v.CoachId);
        builder.HasIndex(v => v.ExerciseId);
        builder.HasIndex(v => v.UploadedAt);
    }
}
