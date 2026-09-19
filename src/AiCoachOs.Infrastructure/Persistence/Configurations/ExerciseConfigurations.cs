using AiCoachOs.Domain.Exercises;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class MovementPatternConfiguration : IEntityTypeConfiguration<MovementPattern>
{
    public void Configure(EntityTypeBuilder<MovementPattern> builder)
    {
        builder.ToTable("movement_patterns");
        builder.HasKey(mp => mp.Id);
        builder.Property(mp => mp.Id).HasColumnName("id");
        builder.Property(mp => mp.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(mp => mp.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(mp => mp.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(mp => mp.UpdatedAtUtc).HasColumnName("updated_at_utc");
    }
}

public class MuscleConfiguration : IEntityTypeConfiguration<Muscle>
{
    public void Configure(EntityTypeBuilder<Muscle> builder)
    {
        builder.ToTable("muscles");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(m => m.CommonName).HasColumnName("common_name").HasMaxLength(100);
        builder.Property(m => m.BodyPart).HasColumnName("body_part").HasMaxLength(100).IsRequired();
        builder.Property(m => m.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(m => m.UpdatedAtUtc).HasColumnName("updated_at_utc");
    }
}

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("equipment");
        builder.HasKey(eq => eq.Id);
        builder.Property(eq => eq.Id).HasColumnName("id");
        builder.Property(eq => eq.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(eq => eq.Category).HasColumnName("category").HasMaxLength(100);
        builder.Property(eq => eq.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(eq => eq.UpdatedAtUtc).HasColumnName("updated_at_utc");
    }
}

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("exercises");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(e => e.Aliases).HasColumnName("aliases").HasMaxLength(500);
        builder.Property(e => e.Category).HasColumnName("category").HasConversion<int>().IsRequired();
        builder.Property(e => e.MovementPatternId).HasColumnName("movement_pattern_id").IsRequired();
        builder.Property(e => e.JointActions).HasColumnName("joint_actions").HasMaxLength(500);

        builder.Property(e => e.StabilityRequirement).HasColumnName("stability_requirement").HasConversion<int>().IsRequired();
        builder.Property(e => e.TechnicalDemand).HasColumnName("technical_demand").HasConversion<int>().IsRequired();
        builder.Property(e => e.LocalFatigueCost).HasColumnName("local_fatigue_cost").HasConversion<int>().IsRequired();
        builder.Property(e => e.SystemicFatigueCost).HasColumnName("systemic_fatigue_cost").HasConversion<int>().IsRequired();
        builder.Property(e => e.StimulusPotential).HasColumnName("stimulus_potential").HasConversion<int>().IsRequired();
        builder.Property(e => e.ProgressionPotential).HasColumnName("progression_potential").HasConversion<int>().IsRequired();
        builder.Property(e => e.ResistanceProfile).HasColumnName("resistance_profile").HasConversion<int>().IsRequired();

        builder.Property(e => e.SubstitutionGroupId).HasColumnName("substitution_group_id");
        builder.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(e => e.MovementPattern)
            .WithMany()
            .HasForeignKey(e => e.MovementPatternId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Muscles)
            .WithOne(em => em.Exercise)
            .HasForeignKey(em => em.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Equipment)
            .WithOne(ee => ee.Exercise)
            .HasForeignKey(ee => ee.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Substitutions)
            .WithOne(es => es.Exercise)
            .HasForeignKey(es => es.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ExerciseMuscleConfiguration : IEntityTypeConfiguration<ExerciseMuscle>
{
    public void Configure(EntityTypeBuilder<ExerciseMuscle> builder)
    {
        builder.ToTable("exercise_muscles");
        builder.HasKey(em => new { em.ExerciseId, em.MuscleId });
        builder.Property(em => em.ExerciseId).HasColumnName("exercise_id");
        builder.Property(em => em.MuscleId).HasColumnName("muscle_id");
        builder.Property(em => em.IsPrimary).HasColumnName("is_primary").IsRequired();

        builder.HasOne(em => em.Exercise)
            .WithMany(e => e.Muscles)
            .HasForeignKey(em => em.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(em => em.Muscle)
            .WithMany()
            .HasForeignKey(em => em.MuscleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExerciseEquipmentConfiguration : IEntityTypeConfiguration<ExerciseEquipment>
{
    public void Configure(EntityTypeBuilder<ExerciseEquipment> builder)
    {
        builder.ToTable("exercise_equipment");
        builder.HasKey(ee => new { ee.ExerciseId, ee.EquipmentId });
        builder.Property(ee => ee.ExerciseId).HasColumnName("exercise_id");
        builder.Property(ee => ee.EquipmentId).HasColumnName("equipment_id");
        builder.Property(ee => ee.IsRequired).HasColumnName("is_required").IsRequired();

        builder.HasOne(ee => ee.Exercise)
            .WithMany(e => e.Equipment)
            .HasForeignKey(ee => ee.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ee => ee.Equipment)
            .WithMany()
            .HasForeignKey(ee => ee.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExerciseSubstitutionConfiguration : IEntityTypeConfiguration<ExerciseSubstitution>
{
    public void Configure(EntityTypeBuilder<ExerciseSubstitution> builder)
    {
        builder.ToTable("exercise_substitutions");
        builder.HasKey(es => es.Id);
        builder.Property(es => es.Id).HasColumnName("id");
        builder.Property(es => es.ExerciseId).HasColumnName("exercise_id").IsRequired();
        builder.Property(es => es.SubstituteExerciseId).HasColumnName("substitute_exercise_id").IsRequired();
        builder.Property(es => es.IntentPreservationNotes).HasColumnName("intent_preservation_notes").HasMaxLength(1000);
        builder.Property(es => es.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(es => es.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(es => es.Exercise)
            .WithMany(e => e.Substitutions)
            .HasForeignKey(es => es.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(es => es.SubstituteExercise)
            .WithMany()
            .HasForeignKey(es => es.SubstituteExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
