using AiCoachOs.Domain.Photos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class ClientPhotoConfiguration : IEntityTypeConfiguration<ClientPhoto>
{
    public void Configure(EntityTypeBuilder<ClientPhoto> builder)
    {
        builder.ToTable("client_photos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.ClientId).HasColumnName("client_id").IsRequired();
        builder.Property(p => p.CoachId).HasColumnName("coach_id").IsRequired();
        builder.Property(p => p.PhotoSetType).HasColumnName("photo_set_type").HasConversion<int>().IsRequired();
        builder.Property(p => p.StorageKey).HasColumnName("storage_key").HasMaxLength(250).IsRequired();
        builder.Property(p => p.MimeType).HasColumnName("mime_type").HasMaxLength(50).IsRequired();
        builder.Property(p => p.FileSizeBytes).HasColumnName("file_size_bytes").IsRequired();
        builder.Property(p => p.TakenAt).HasColumnName("taken_at").IsRequired();
        builder.Property(p => p.UploadedAt).HasColumnName("uploaded_at").IsRequired();
        builder.Property(p => p.Notes).HasColumnName("notes").HasMaxLength(500);
        builder.Property(p => p.ObservationRecordId).HasColumnName("observation_record_id");
        builder.Property(p => p.IsAnonymized).HasColumnName("is_anonymized").IsRequired();
        builder.Property(p => p.AnonymizedAt).HasColumnName("anonymized_at");
        builder.Property(p => p.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(p => p.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(p => p.Client)
            .WithMany()
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ObservationRecord)
            .WithMany()
            .HasForeignKey(p => p.ObservationRecordId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.ClientId);
        builder.HasIndex(p => p.CoachId);
        builder.HasIndex(p => p.TakenAt);
    }
}
