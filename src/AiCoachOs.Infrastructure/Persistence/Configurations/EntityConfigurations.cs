using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Coaches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class CoachConfiguration : IEntityTypeConfiguration<Coach>
{
    public void Configure(EntityTypeBuilder<Coach> builder)
    {
        builder.ToTable("coaches");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.IdentityUserId)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(c => c.IdentityUserId)
            .IsUnique();

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(c => c.Email);
    }
}

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CoachId)
            .IsRequired();

        builder.HasIndex(c => c.CoachId);

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .HasMaxLength(256);

        builder.Property(c => c.Phone)
            .HasMaxLength(30);

        builder.Property(c => c.Gender)
            .HasConversion<int>();

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.IntakeNotes)
            .HasMaxLength(2000);

        // Owned ClientGoal
        builder.OwnsOne(c => c.Goal, goalBuilder =>
        {
            goalBuilder.Property(g => g.PrimaryGoal)
                .HasColumnName("goal_primary")
                .HasMaxLength(150);

            goalBuilder.Property(g => g.TargetTimelineWeeks)
                .HasColumnName("goal_target_timeline_weeks");

            goalBuilder.Property(g => g.Notes)
                .HasColumnName("goal_notes")
                .HasMaxLength(1000);
        });
    }
}

public class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> builder)
    {
        builder.ToTable("consent_records");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClientId)
            .IsRequired();

        builder.HasIndex(c => c.ClientId);

        builder.Property(c => c.ConsentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.IsGranted)
            .IsRequired();

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);
    }
}
