using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Nutrition;

public class NutritionCalibrationRecord : Entity<Guid>
{
    private readonly List<decimal> _weeklyWeightAverages = new();

    public Guid ClientNutritionProfileId { get; private set; }
    public ClientNutritionProfile ClientNutritionProfile { get; private set; } = null!;

    public DateTime RecordedAtUtc { get; private set; }
    public decimal WeightKg { get; private set; }
    public int? EstimatedTDEE { get; private set; }
    public AdjustmentRecommendation AdjustmentRecommendation { get; private set; }
    public int? AdjustmentKcal { get; private set; }
    public int WeeksObserved { get; private set; }
    public CalibrationDecision CoachDecision { get; private set; }
    public string? CoachNote { get; private set; }

    public IReadOnlyCollection<decimal> WeeklyWeightAverages => _weeklyWeightAverages.AsReadOnly();

    private NutritionCalibrationRecord() { } // EF Core

    public NutritionCalibrationRecord(
        Guid id,
        Guid clientNutritionProfileId,
        DateTime recordedAtUtc,
        decimal weightKg,
        AdjustmentRecommendation adjustmentRecommendation,
        int weeksObserved,
        IEnumerable<decimal>? weeklyWeightAverages = null,
        int? estimatedTDEE = null,
        int? adjustmentKcal = null,
        CalibrationDecision coachDecision = CalibrationDecision.Pending,
        string? coachNote = null) : base(id)
    {
        if (clientNutritionProfileId == Guid.Empty)
            throw new ArgumentException("ClientNutritionProfileId cannot be empty.", nameof(clientNutritionProfileId));
        if (weightKg <= 0)
            throw new ArgumentException("Weight must be greater than zero.", nameof(weightKg));
        if (weeksObserved < 0)
            throw new ArgumentException("Weeks observed cannot be negative.", nameof(weeksObserved));

        ClientNutritionProfileId = clientNutritionProfileId;
        RecordedAtUtc = recordedAtUtc;
        WeightKg = weightKg;
        AdjustmentRecommendation = adjustmentRecommendation;
        WeeksObserved = weeksObserved;
        EstimatedTDEE = estimatedTDEE;
        AdjustmentKcal = adjustmentKcal;
        CoachDecision = coachDecision;
        CoachNote = coachNote?.Trim();

        if (weeklyWeightAverages != null)
        {
            _weeklyWeightAverages.AddRange(weeklyWeightAverages);
        }
    }

    public void ApplyDecision(CalibrationDecision decision, string? note = null)
    {
        CoachDecision = decision;
        CoachNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        MarkUpdated();
    }
}
