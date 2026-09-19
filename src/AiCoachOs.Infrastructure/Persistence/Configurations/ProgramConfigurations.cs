using System.Text.Json;
using AiCoachOs.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class ProgramConfiguration : IEntityTypeConfiguration<Program>
{
    public void Configure(EntityTypeBuilder<Program> builder)
    {
        builder.ToTable("programs");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.ClientId).HasColumnName("client_id").IsRequired();
        builder.Property(p => p.CoachId).HasColumnName("coach_id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(p => p.RationaleSummary).HasColumnName("rationale_summary").HasMaxLength(2000).IsRequired();
        builder.Property(p => p.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(p => p.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.OwnsOne(p => p.GoalSnapshot, gb =>
        {
            gb.Property(g => g.PrimaryGoal).HasColumnName("goal_primary").HasConversion<int>().IsRequired();
            gb.Property(g => g.SecondaryGoal).HasColumnName("goal_secondary").HasConversion<int>();
            gb.Property(g => g.GoalEmphasis).HasColumnName("goal_emphasis").HasMaxLength(300);
            gb.Property(g => g.TargetTimelineWeeks).HasColumnName("goal_timeline_weeks");
        });

        builder.HasOne(p => p.Client)
            .WithMany()
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Coach)
            .WithMany()
            .HasForeignKey(p => p.CoachId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Versions)
            .WithOne(v => v.Program)
            .HasForeignKey(v => v.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.ClientId);
        builder.HasIndex(p => p.CoachId);
        builder.HasIndex(p => p.Status);
    }
}

public class ProgramVersionConfiguration : IEntityTypeConfiguration<ProgramVersion>
{
    public void Configure(EntityTypeBuilder<ProgramVersion> builder)
    {
        builder.ToTable("program_versions");

        builder.HasKey(pv => pv.Id);
        builder.Property(pv => pv.Id).HasColumnName("id");
        builder.Property(pv => pv.ProgramId).HasColumnName("program_id").IsRequired();
        builder.Property(pv => pv.VersionNumber).HasColumnName("version_number").IsRequired();
        builder.Property(pv => pv.RecoveryCapacity).HasColumnName("recovery_capacity").HasConversion<int>().IsRequired();
        builder.Property(pv => pv.ChangeReason).HasColumnName("change_reason").HasMaxLength(500);
        builder.Property(pv => pv.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(pv => pv.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(pv => pv.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasMany(pv => pv.Weeks)
            .WithOne(w => w.ProgramVersion)
            .HasForeignKey(w => w.ProgramVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pv => pv.MusclePriorities)
            .WithOne(mp => mp.ProgramVersion)
            .HasForeignKey(mp => mp.ProgramVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pv => new { pv.ProgramId, pv.VersionNumber }).IsUnique();
        builder.HasIndex(pv => pv.IsActive);
    }
}

public class ProgramMusclePriorityConfiguration : IEntityTypeConfiguration<ProgramMusclePriority>
{
    public void Configure(EntityTypeBuilder<ProgramMusclePriority> builder)
    {
        builder.ToTable("program_muscle_priorities");

        builder.HasKey(pmp => pmp.Id);
        builder.Property(pmp => pmp.Id).HasColumnName("id");
        builder.Property(pmp => pmp.ProgramVersionId).HasColumnName("program_version_id").IsRequired();
        builder.Property(pmp => pmp.MuscleId).HasColumnName("muscle_id").IsRequired();
        builder.Property(pmp => pmp.PriorityLevel).HasColumnName("priority_level").HasConversion<int>().IsRequired();
        builder.Property(pmp => pmp.Justification).HasColumnName("justification").HasMaxLength(500);
        builder.Property(pmp => pmp.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(pmp => pmp.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(pmp => pmp.Muscle)
            .WithMany()
            .HasForeignKey(pmp => pmp.MuscleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pmp => new { pmp.ProgramVersionId, pmp.MuscleId }).IsUnique();
    }
}

public class TrainingWeekConfiguration : IEntityTypeConfiguration<TrainingWeek>
{
    public void Configure(EntityTypeBuilder<TrainingWeek> builder)
    {
        builder.ToTable("training_weeks");

        builder.HasKey(tw => tw.Id);
        builder.Property(tw => tw.Id).HasColumnName("id");
        builder.Property(tw => tw.ProgramVersionId).HasColumnName("program_version_id").IsRequired();
        builder.Property(tw => tw.WeekNumber).HasColumnName("week_number").IsRequired();
        builder.Property(tw => tw.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(tw => tw.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasMany(tw => tw.Sessions)
            .WithOne(s => s.TrainingWeek)
            .HasForeignKey(s => s.TrainingWeekId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(tw => new { tw.ProgramVersionId, tw.WeekNumber }).IsUnique();
    }
}

public class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
{
    public void Configure(EntityTypeBuilder<TrainingSession> builder)
    {
        builder.ToTable("training_sessions");

        builder.HasKey(ts => ts.Id);
        builder.Property(ts => ts.Id).HasColumnName("id");
        builder.Property(ts => ts.TrainingWeekId).HasColumnName("training_week_id").IsRequired();
        builder.Property(ts => ts.DayNumber).HasColumnName("day_number").IsRequired();
        builder.Property(ts => ts.DayOfWeek).HasColumnName("day_of_week").HasConversion<int>();
        builder.Property(ts => ts.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(ts => ts.SessionIntent).HasColumnName("session_intent").HasMaxLength(500).IsRequired();
        builder.Property(ts => ts.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes").IsRequired();
        builder.Property(ts => ts.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(ts => ts.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasMany(ts => ts.Slots)
            .WithOne(s => s.TrainingSession)
            .HasForeignKey(s => s.TrainingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ts => new { ts.TrainingWeekId, ts.DayNumber }).IsUnique();
    }
}

public class ExerciseSlotConfiguration : IEntityTypeConfiguration<ExerciseSlot>
{
    public void Configure(EntityTypeBuilder<ExerciseSlot> builder)
    {
        builder.ToTable("exercise_slots");

        builder.HasKey(es => es.Id);
        builder.Property(es => es.Id).HasColumnName("id");
        builder.Property(es => es.TrainingSessionId).HasColumnName("training_session_id").IsRequired();
        builder.Property(es => es.ExerciseId).HasColumnName("exercise_id").IsRequired();
        builder.Property(es => es.Order).HasColumnName("order").IsRequired();
        builder.Property(es => es.TargetSets).HasColumnName("target_sets").IsRequired();
        builder.Property(es => es.TargetRepRange).HasColumnName("target_rep_range").HasMaxLength(50).IsRequired();
        builder.Property(es => es.EffortGuideline).HasColumnName("effort_guideline").HasMaxLength(100).IsRequired();
        builder.Property(es => es.RestSeconds).HasColumnName("rest_seconds").IsRequired();
        builder.Property(es => es.SelectionRationale).HasColumnName("selection_rationale").HasMaxLength(1000).IsRequired();
        builder.Property(es => es.CoachingNote).HasColumnName("coaching_note").HasMaxLength(500);
        builder.Property(es => es.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(es => es.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.OwnsOne(es => es.ProgressionRule, pb =>
        {
            pb.Property(p => p.Type).HasColumnName("progression_type").HasConversion<int>().IsRequired();
            pb.Property(p => p.CurrentTarget).HasColumnName("progression_target").HasMaxLength(100).IsRequired();
            pb.Property(p => p.IncrementValue).HasColumnName("progression_increment").HasMaxLength(50).IsRequired();
            pb.Property(p => p.IncrementCondition).HasColumnName("progression_condition").HasMaxLength(250);
        });

        builder.HasOne(es => es.Exercise)
            .WithMany()
            .HasForeignKey(es => es.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(es => new { es.TrainingSessionId, es.Order }).IsUnique();
        builder.HasIndex(es => es.ExerciseId);
    }
}
