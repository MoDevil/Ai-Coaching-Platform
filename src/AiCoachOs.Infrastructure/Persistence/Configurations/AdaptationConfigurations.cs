using AiCoachOs.Domain.Adaptations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCoachOs.Infrastructure.Persistence.Configurations;

public class AdaptationAssessmentConfiguration : IEntityTypeConfiguration<AdaptationAssessment>
{
    public void Configure(EntityTypeBuilder<AdaptationAssessment> builder)
    {
        builder.ToTable("adaptation_assessments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.ProgramVersionId).HasColumnName("program_version_id").IsRequired();
        builder.Property(a => a.AssessedAt).HasColumnName("assessed_at").IsRequired();
        builder.Property(a => a.ObservationStartDate).HasColumnName("observation_start_date").IsRequired();
        builder.Property(a => a.ObservationEndDate).HasColumnName("observation_end_date").IsRequired();
        builder.Property(a => a.TotalExposures).HasColumnName("total_exposures").IsRequired();
        builder.Property(a => a.CompletedExposures).HasColumnName("completed_exposures").IsRequired();
        builder.Property(a => a.AdherenceRate).HasColumnName("adherence_rate").HasPrecision(5, 2).IsRequired();
        builder.Property(a => a.OverallStatus).HasColumnName("overall_status").HasConversion<int>().IsRequired();
        builder.Property(a => a.CoachNotes).HasColumnName("coach_notes").HasMaxLength(1000);
        builder.Property(a => a.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(a => a.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(a => a.ProgramVersion)
            .WithMany()
            .HasForeignKey(a => a.ProgramVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.ExerciseRecords)
            .WithOne(r => r.AdaptationAssessment)
            .HasForeignKey(r => r.AdaptationAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.ExerciseRecords)
            .HasField("_exerciseRecords")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(a => a.Recommendations)
            .WithOne(r => r.AdaptationAssessment)
            .HasForeignKey(r => r.AdaptationAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Recommendations)
            .HasField("_recommendations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(a => a.ProgramVersionId);
        builder.HasIndex(a => a.AssessedAt);
    }
}

public class ExerciseAdaptationRecordConfiguration : IEntityTypeConfiguration<ExerciseAdaptationRecord>
{
    public void Configure(EntityTypeBuilder<ExerciseAdaptationRecord> builder)
    {
        builder.ToTable("exercise_adaptation_records");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.AdaptationAssessmentId).HasColumnName("adaptation_assessment_id").IsRequired();
        builder.Property(r => r.ExerciseSlotId).HasColumnName("exercise_slot_id").IsRequired();
        builder.Property(r => r.ExerciseId).HasColumnName("exercise_id").IsRequired();
        builder.Property(r => r.ExposureCount).HasColumnName("exposure_count").IsRequired();
        builder.Property(r => r.ProgressionMetCount).HasColumnName("progression_met_count").IsRequired();
        builder.Property(r => r.EffortAlignmentStatus).HasColumnName("effort_alignment_status").HasConversion<int>().IsRequired();
        builder.Property(r => r.PerformanceTrend).HasColumnName("performance_trend").HasConversion<int>().IsRequired();
        builder.Property(r => r.PlateauConfirmed).HasColumnName("plateau_confirmed").IsRequired();
        builder.Property(r => r.AdherenceToExercise).HasColumnName("adherence_to_exercise").HasPrecision(5, 2).IsRequired();
        builder.Property(r => r.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(r => r.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(r => r.ExerciseSlot)
            .WithMany()
            .HasForeignKey(r => r.ExerciseSlotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Exercise)
            .WithMany()
            .HasForeignKey(r => r.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.AdaptationAssessmentId);
        builder.HasIndex(r => r.ExerciseSlotId);
    }
}

public class AdaptationRecommendationConfiguration : IEntityTypeConfiguration<AdaptationRecommendation>
{
    public void Configure(EntityTypeBuilder<AdaptationRecommendation> builder)
    {
        builder.ToTable("adaptation_recommendations");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.AdaptationAssessmentId).HasColumnName("adaptation_assessment_id").IsRequired();
        builder.Property(r => r.ExerciseAdaptationRecordId).HasColumnName("exercise_adaptation_record_id");
        builder.Property(r => r.ActionType).HasColumnName("action_type").HasConversion<int>().IsRequired();
        builder.Property(r => r.TargetSlotId).HasColumnName("target_slot_id");
        builder.Property(r => r.SuggestedChangeDetail).HasColumnName("suggested_change_detail").HasMaxLength(500);
        builder.Property(r => r.Rationale).HasColumnName("rationale").HasMaxLength(1000).IsRequired();
        builder.Property(r => r.Confidence).HasColumnName("confidence").HasConversion<int>().IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(r => r.CoachDecisionAt).HasColumnName("coach_decision_at");
        builder.Property(r => r.CoachDecisionNote).HasColumnName("coach_decision_note").HasMaxLength(500);
        builder.Property(r => r.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(r => r.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne(r => r.ExerciseAdaptationRecord)
            .WithMany()
            .HasForeignKey(r => r.ExerciseAdaptationRecordId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.AdaptationAssessmentId);
        builder.HasIndex(r => r.Status);
    }
}
