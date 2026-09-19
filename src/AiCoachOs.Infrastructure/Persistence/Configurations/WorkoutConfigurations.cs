using AiCoachOs.Domain.Workouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class WorkoutSessionConfiguration : IEntityTypeConfiguration<WorkoutSession>
{
    public void Configure(EntityTypeBuilder<WorkoutSession> builder)
    {
        builder.ToTable("workout_sessions");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasColumnName("id");
        builder.Property(w => w.ClientId).HasColumnName("client_id").IsRequired();
        builder.Property(w => w.CoachId).HasColumnName("coach_id").IsRequired();
        builder.Property(w => w.ProgramVersionId).HasColumnName("program_version_id");
        builder.Property(w => w.TrainingSessionId).HasColumnName("training_session_id");
        builder.Property(w => w.StartedAtUtc).HasColumnName("started_at_utc").IsRequired();
        builder.Property(w => w.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(w => w.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(w => w.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(w => w.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(w => w.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(w => w.Client)
            .WithMany()
            .HasForeignKey(w => w.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Coach)
            .WithMany()
            .HasForeignKey(w => w.CoachId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.ProgramVersion)
            .WithMany()
            .HasForeignKey(w => w.ProgramVersionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(w => w.TrainingSession)
            .WithMany()
            .HasForeignKey(w => w.TrainingSessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(w => w.Exercises)
            .WithOne(e => e.WorkoutSession)
            .HasForeignKey(e => e.WorkoutSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Exercises)
            .HasField("_exercises")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(w => w.ClientId);
        builder.HasIndex(w => w.CoachId);
        builder.HasIndex(w => w.StartedAtUtc);
        builder.HasIndex(w => w.Status);
    }
}

public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
    {
        builder.ToTable("workout_exercises");

        builder.HasKey(we => we.Id);
        builder.Property(we => we.Id).HasColumnName("id");
        builder.Property(we => we.WorkoutSessionId).HasColumnName("workout_session_id").IsRequired();
        builder.Property(we => we.ExerciseId).HasColumnName("exercise_id").IsRequired();
        builder.Property(we => we.ExerciseSlotId).HasColumnName("exercise_slot_id");
        builder.Property(we => we.OrderInSession).HasColumnName("order_in_session").IsRequired();
        builder.Property(we => we.Notes).HasColumnName("notes").HasMaxLength(500);
        builder.Property(we => we.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(we => we.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(we => we.Exercise)
            .WithMany()
            .HasForeignKey(we => we.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(we => we.ExerciseSlot)
            .WithMany()
            .HasForeignKey(we => we.ExerciseSlotId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(we => we.Sets)
            .WithOne(s => s.WorkoutExercise)
            .HasForeignKey(s => s.WorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(we => we.Sets)
            .HasField("_sets")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(we => we.WorkoutSessionId);
        builder.HasIndex(we => we.ExerciseId);
        builder.HasIndex(we => we.ExerciseSlotId);
    }
}

public class WorkoutSetConfiguration : IEntityTypeConfiguration<WorkoutSet>
{
    public void Configure(EntityTypeBuilder<WorkoutSet> builder)
    {
        builder.ToTable("workout_sets");

        builder.HasKey(ws => ws.Id);
        builder.Property(ws => ws.Id).HasColumnName("id");
        builder.Property(ws => ws.WorkoutExerciseId).HasColumnName("workout_exercise_id").IsRequired();
        builder.Property(ws => ws.SetNumber).HasColumnName("set_number").IsRequired();
        builder.Property(ws => ws.Repetitions).HasColumnName("repetitions").IsRequired();
        builder.Property(ws => ws.LoadKg).HasColumnName("load_kg").HasPrecision(6, 2).IsRequired();
        builder.Property(ws => ws.Rir).HasColumnName("rir").HasPrecision(3, 1);
        builder.Property(ws => ws.IsCompleted).HasColumnName("is_completed").IsRequired();
        builder.Property(ws => ws.Notes).HasColumnName("notes").HasMaxLength(500);
        builder.Property(ws => ws.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(ws => ws.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(ws => ws.WorkoutExerciseId);
    }
}
