using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Programs;

public class TrainingSession : Entity<Guid>
{
    private readonly List<ExerciseSlot> _slots = new();

    public Guid TrainingWeekId { get; private set; }
    public TrainingWeek TrainingWeek { get; private set; } = null!;

    public int DayNumber { get; private set; } // 1, 2, 3...
    public DayOfWeek? DayOfWeek { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string SessionIntent { get; private set; } = string.Empty;
    public int EstimatedDurationMinutes { get; private set; }

    public IReadOnlyCollection<ExerciseSlot> Slots => _slots.OrderBy(s => s.Order).ToList().AsReadOnly();

    private TrainingSession() { } // EF Core

    public TrainingSession(
        Guid id,
        Guid trainingWeekId,
        int dayNumber,
        string name,
        string sessionIntent,
        int estimatedDurationMinutes,
        DayOfWeek? dayOfWeek = null) : base(id)
    {
        if (trainingWeekId == Guid.Empty)
            throw new ArgumentException("TrainingWeekId cannot be empty.", nameof(trainingWeekId));
        if (dayNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(dayNumber), "Day number must be at least 1.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Session name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(sessionIntent))
            throw new ArgumentException("Session intent cannot be empty.", nameof(sessionIntent));
        if (estimatedDurationMinutes < 1)
            throw new ArgumentOutOfRangeException(nameof(estimatedDurationMinutes), "Estimated duration must be positive.");

        TrainingWeekId = trainingWeekId;
        DayNumber = dayNumber;
        Name = name.Trim();
        SessionIntent = sessionIntent.Trim();
        EstimatedDurationMinutes = estimatedDurationMinutes;
        DayOfWeek = dayOfWeek;
    }

    public void AddSlot(ExerciseSlot slot)
    {
        ArgumentNullException.ThrowIfNull(slot);
        if (_slots.Any(s => s.Id == slot.Id))
            return;
        _slots.Add(slot);
        MarkUpdated();
    }
}
