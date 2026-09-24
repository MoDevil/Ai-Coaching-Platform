using System.Text.Json;
using AiCoachOs.Domain.TrainingProfiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class ClientTrainingProfileConfiguration : IEntityTypeConfiguration<ClientTrainingProfile>
{
    public void Configure(EntityTypeBuilder<ClientTrainingProfile> builder)
    {
        builder.ToTable("client_training_profiles");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.ClientId).HasColumnName("client_id").IsRequired();
        builder.HasIndex(p => p.ClientId).IsUnique();

        builder.Property(p => p.GymProfileId).HasColumnName("gym_profile_id");
        builder.HasIndex(p => p.GymProfileId);

        builder.Property(p => p.ExperienceLevel).HasColumnName("experience_level").HasConversion<int>().IsRequired();
        builder.Property(p => p.SessionDurationMinMinutes).HasColumnName("session_duration_min_minutes");
        builder.Property(p => p.SessionDurationTargetMinutes).HasColumnName("session_duration_target_minutes");
        builder.Property(p => p.SessionDurationMaxMinutes).HasColumnName("session_duration_max_minutes");
        builder.Property(p => p.ExercisePreferences).HasColumnName("exercise_preferences").HasMaxLength(2000);
        builder.Property(p => p.ExerciseConstraints).HasColumnName("exercise_constraints").HasMaxLength(2000);
        builder.Property(p => p.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(p => p.UpdatedAtUtc).HasColumnName("updated_at_utc");

        var guidListComparer = new ValueComparer<List<Guid>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        var dayOfWeekComparer = new ValueComparer<IReadOnlyList<DayOfWeek>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property<List<Guid>>("_availableEquipmentIds")
            .HasColumnName("available_equipment_ids")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>()
            )
            .Metadata.SetValueComparer(guidListComparer);

        // WeeklyAvailability owned value object
        builder.OwnsOne(p => p.WeeklyAvailability, b =>
        {
            b.Property(a => a.SessionsPerWeek).HasColumnName("sessions_per_week").IsRequired();

            b.Property(a => a.AvailableDays)
                .HasColumnName("available_days")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions?)null) ?? new List<DayOfWeek>()
                )
                .Metadata.SetValueComparer(dayOfWeekComparer);

            b.Property(a => a.PreferredDays)
                .HasColumnName("preferred_days")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<DayOfWeek>>(v, (JsonSerializerOptions?)null) ?? new List<DayOfWeek>()
                )
                .Metadata.SetValueComparer(dayOfWeekComparer);
        });

        builder.HasMany(p => p.Priorities)
            .WithOne()
            .HasForeignKey(pr => pr.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ClientTrainingPriorityConfiguration : IEntityTypeConfiguration<ClientTrainingPriority>
{
    public void Configure(EntityTypeBuilder<ClientTrainingPriority> builder)
    {
        builder.ToTable("client_training_priorities");
        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Id).HasColumnName("id");
        builder.Property(pr => pr.ProfileId).HasColumnName("profile_id").IsRequired();
        builder.Property(pr => pr.Order).HasColumnName("order").IsRequired();
        builder.Property(pr => pr.FocusArea).HasColumnName("focus_area").HasMaxLength(100).IsRequired();
        builder.Property(pr => pr.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(pr => pr.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(pr => pr.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(pr => new { pr.ProfileId, pr.Order });
    }
}
