using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Programs;

public class ProgramVersion : Entity<Guid>
{
    private readonly List<TrainingWeek> _weeks = new();
    private readonly List<ProgramMusclePriority> _musclePriorities = new();

    public Guid ProgramId { get; private set; }
    public Program Program { get; private set; } = null!;

    public int VersionNumber { get; private set; }
    public string? ChangeReason { get; private set; }
    public bool IsActive { get; private set; }
    public RecoveryCapacity RecoveryCapacity { get; private set; }

    public IReadOnlyCollection<TrainingWeek> Weeks => _weeks.OrderBy(w => w.WeekNumber).ToList().AsReadOnly();
    public IReadOnlyCollection<ProgramMusclePriority> MusclePriorities => _musclePriorities.AsReadOnly();

    private ProgramVersion() { } // EF Core

    public ProgramVersion(
        Guid id,
        Guid programId,
        int versionNumber,
        RecoveryCapacity recoveryCapacity,
        string? changeReason = null,
        bool isActive = true) : base(id)
    {
        if (programId == Guid.Empty)
            throw new ArgumentException("ProgramId cannot be empty.", nameof(programId));
        if (versionNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(versionNumber), "Version number must be at least 1.");

        ProgramId = programId;
        VersionNumber = versionNumber;
        RecoveryCapacity = recoveryCapacity;
        ChangeReason = string.IsNullOrWhiteSpace(changeReason) ? null : changeReason.Trim();
        IsActive = isActive;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        MarkUpdated();
    }

    public void AddWeek(TrainingWeek week)
    {
        ArgumentNullException.ThrowIfNull(week);
        if (_weeks.Any(w => w.Id == week.Id))
            return;
        _weeks.Add(week);
        MarkUpdated();
    }

    public void AddMusclePriority(ProgramMusclePriority priority)
    {
        ArgumentNullException.ThrowIfNull(priority);
        if (_musclePriorities.Any(p => p.MuscleId == priority.MuscleId))
            return;
        _musclePriorities.Add(priority);
        MarkUpdated();
    }
}
