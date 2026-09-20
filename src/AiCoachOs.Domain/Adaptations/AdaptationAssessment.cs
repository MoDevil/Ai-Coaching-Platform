using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Programs;

namespace AiCoachOs.Domain.Adaptations;

public class AdaptationAssessment : Entity<Guid>
{
    private readonly List<ExerciseAdaptationRecord> _exerciseRecords = new();
    private readonly List<AdaptationRecommendation> _recommendations = new();

    public Guid ProgramVersionId { get; private set; }
    public ProgramVersion ProgramVersion { get; private set; } = null!;

    public DateTime AssessedAt { get; private set; }
    public DateTime ObservationStartDate { get; private set; }
    public DateTime ObservationEndDate { get; private set; }

    public int TotalExposures { get; private set; }
    public int CompletedExposures { get; private set; }
    public decimal AdherenceRate { get; private set; }

    public AdaptationOverallStatus OverallStatus { get; private set; }
    public string? CoachNotes { get; private set; }

    public IReadOnlyCollection<ExerciseAdaptationRecord> ExerciseRecords => _exerciseRecords.AsReadOnly();
    public IReadOnlyCollection<AdaptationRecommendation> Recommendations => _recommendations.AsReadOnly();

    private AdaptationAssessment() { } // EF Core

    public AdaptationAssessment(
        Guid id,
        Guid programVersionId,
        DateTime assessedAt,
        DateTime observationStartDate,
        DateTime observationEndDate,
        int totalExposures,
        int completedExposures,
        decimal adherenceRate,
        AdaptationOverallStatus overallStatus,
        string? coachNotes = null) : base(id)
    {
        if (programVersionId == Guid.Empty)
            throw new ArgumentException("ProgramVersionId cannot be empty.", nameof(programVersionId));
        if (totalExposures < 0)
            throw new ArgumentOutOfRangeException(nameof(totalExposures), "Total exposures cannot be negative.");
        if (completedExposures < 0)
            throw new ArgumentOutOfRangeException(nameof(completedExposures), "Completed exposures cannot be negative.");
        if (adherenceRate < 0 || adherenceRate > 100)
            throw new ArgumentOutOfRangeException(nameof(adherenceRate), "Adherence rate must be between 0 and 100 percent.");

        ProgramVersionId = programVersionId;
        AssessedAt = assessedAt;
        ObservationStartDate = observationStartDate;
        ObservationEndDate = observationEndDate;
        TotalExposures = totalExposures;
        CompletedExposures = completedExposures;
        AdherenceRate = adherenceRate;
        OverallStatus = overallStatus;
        CoachNotes = string.IsNullOrWhiteSpace(coachNotes) ? null : coachNotes.Trim();
    }

    public void AddExerciseRecord(ExerciseAdaptationRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (_exerciseRecords.Any(r => r.Id == record.Id))
            return;

        _exerciseRecords.Add(record);
        MarkUpdated();
    }

    public void AddRecommendation(AdaptationRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);
        if (_recommendations.Any(r => r.Id == recommendation.Id))
            return;

        _recommendations.Add(recommendation);
        MarkUpdated();
    }

    public void UpdateCoachNotes(string? notes)
    {
        CoachNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        MarkUpdated();
    }
}
