using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Programs;

public class TrainingWeek : Entity<Guid>
{
    private readonly List<TrainingSession> _sessions = new();

    public Guid ProgramVersionId { get; private set; }
    public ProgramVersion ProgramVersion { get; private set; } = null!;

    public int WeekNumber { get; private set; }

    public IReadOnlyCollection<TrainingSession> Sessions => _sessions.OrderBy(s => s.DayNumber).ToList().AsReadOnly();

    private TrainingWeek() { } // EF Core

    public TrainingWeek(Guid id, Guid programVersionId, int weekNumber) : base(id)
    {
        if (programVersionId == Guid.Empty)
            throw new ArgumentException("ProgramVersionId cannot be empty.", nameof(programVersionId));
        if (weekNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(weekNumber), "Week number must be at least 1.");

        ProgramVersionId = programVersionId;
        WeekNumber = weekNumber;
    }

    public void AddSession(TrainingSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (_sessions.Any(s => s.Id == session.Id))
            return;
        _sessions.Add(session);
        MarkUpdated();
    }
}
