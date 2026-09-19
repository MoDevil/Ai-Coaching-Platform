using AiCoachOs.Domain.AnatomyAndBiomechanics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class AnatomicalRegionConfiguration : IEntityTypeConfiguration<AnatomicalRegion>
{
    public void Configure(EntityTypeBuilder<AnatomicalRegion> builder)
    {
        builder.ToTable("anatomical_regions");

        builder.HasKey(ar => ar.Id);
        builder.Property(ar => ar.Id).HasColumnName("id");
        builder.Property(ar => ar.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(ar => ar.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(ar => ar.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(ar => ar.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasMany(ar => ar.Joints)
            .WithOne(j => j.Region)
            .HasForeignKey(j => j.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ar => ar.Name).IsUnique();
    }
}

public class JointConfiguration : IEntityTypeConfiguration<Joint>
{
    public void Configure(EntityTypeBuilder<Joint> builder)
    {
        builder.ToTable("joints");

        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id).HasColumnName("id");
        builder.Property(j => j.RegionId).HasColumnName("region_id").IsRequired();
        builder.Property(j => j.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(j => j.CommonName).HasColumnName("common_name").HasMaxLength(150);
        builder.Property(j => j.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(j => j.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(j => j.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(j => j.RegionId);
        builder.HasIndex(j => j.Name);
    }
}

public class JointActionConfiguration : IEntityTypeConfiguration<JointAction>
{
    public void Configure(EntityTypeBuilder<JointAction> builder)
    {
        builder.ToTable("joint_actions");

        builder.HasKey(ja => ja.Id);
        builder.Property(ja => ja.Id).HasColumnName("id");
        builder.Property(ja => ja.JointId).HasColumnName("joint_id").IsRequired();
        builder.Property(ja => ja.ActionType).HasColumnName("action_type").HasConversion<int>().IsRequired();
        builder.Property(ja => ja.PlaneOfMotion).HasColumnName("plane_of_motion").HasConversion<int>().IsRequired();
        builder.Property(ja => ja.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(ja => ja.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(ja => ja.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(ja => ja.Joint)
            .WithMany(j => j.Actions)
            .HasForeignKey(ja => ja.JointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ja => new { ja.JointId, ja.ActionType }).IsUnique();
    }
}

public class MuscleJointActionConfiguration : IEntityTypeConfiguration<MuscleJointAction>
{
    public void Configure(EntityTypeBuilder<MuscleJointAction> builder)
    {
        builder.ToTable("muscle_joint_actions");

        builder.HasKey(mja => new { mja.MuscleId, mja.JointActionId });
        builder.Property(mja => mja.MuscleId).HasColumnName("muscle_id").IsRequired();
        builder.Property(mja => mja.JointActionId).HasColumnName("joint_action_id").IsRequired();
        builder.Property(mja => mja.IsPrimaryAction).HasColumnName("is_primary_action").IsRequired();

        builder.HasOne(mja => mja.Muscle)
            .WithMany()
            .HasForeignKey(mja => mja.MuscleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mja => mja.JointAction)
            .WithMany(ja => ja.Muscles)
            .HasForeignKey(mja => mja.JointActionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ExerciseJointActionConfiguration : IEntityTypeConfiguration<ExerciseJointAction>
{
    public void Configure(EntityTypeBuilder<ExerciseJointAction> builder)
    {
        builder.ToTable("exercise_joint_actions");

        builder.HasKey(eja => new { eja.ExerciseId, eja.JointActionId });
        builder.Property(eja => eja.ExerciseId).HasColumnName("exercise_id").IsRequired();
        builder.Property(eja => eja.JointActionId).HasColumnName("joint_action_id").IsRequired();
        builder.Property(eja => eja.Role).HasColumnName("role").HasConversion<int>().IsRequired();

        builder.HasOne(eja => eja.Exercise)
            .WithMany()
            .HasForeignKey(eja => eja.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(eja => eja.JointAction)
            .WithMany(ja => ja.Exercises)
            .HasForeignKey(eja => eja.JointActionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BiomechanicalConsiderationConfiguration : IEntityTypeConfiguration<BiomechanicalConsideration>
{
    public void Configure(EntityTypeBuilder<BiomechanicalConsideration> builder)
    {
        builder.ToTable("biomechanical_considerations");

        builder.HasKey(bc => bc.Id);
        builder.Property(bc => bc.Id).HasColumnName("id");
        builder.Property(bc => bc.ExerciseId).HasColumnName("exercise_id").IsRequired();
        builder.Property(bc => bc.Aspect).HasColumnName("aspect").HasConversion<int>().IsRequired();
        builder.Property(bc => bc.Certainty).HasColumnName("certainty").HasConversion<int>().IsRequired();
        builder.Property(bc => bc.Summary).HasColumnName("summary").HasMaxLength(300).IsRequired();
        builder.Property(bc => bc.Explanation).HasColumnName("explanation").HasMaxLength(2000).IsRequired();
        builder.Property(bc => bc.PracticalCues).HasColumnName("practical_cues").HasMaxLength(1000);
        builder.Property(bc => bc.KnowledgeClaimId).HasColumnName("knowledge_claim_id");
        builder.Property(bc => bc.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(bc => bc.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(bc => bc.Exercise)
            .WithMany()
            .HasForeignKey(bc => bc.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bc => bc.KnowledgeClaim)
            .WithMany()
            .HasForeignKey(bc => bc.KnowledgeClaimId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(bc => bc.ExerciseId);
        builder.HasIndex(bc => bc.Aspect);
        builder.HasIndex(bc => bc.Certainty);
        builder.HasIndex(bc => bc.KnowledgeClaimId);
    }
}
