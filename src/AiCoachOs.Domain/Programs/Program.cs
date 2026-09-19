using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Coaches;
using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Programs;

public class Program : Entity<Guid>
{
    private readonly List<ProgramVersion> _versions = new();

    public Guid ClientId { get; private set; }
    public Client Client { get; private set; } = null!;

    public Guid CoachId { get; private set; }
    public Coach Coach { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;
    public GoalSnapshot GoalSnapshot { get; private set; } = null!;
    public ProgramStatus Status { get; private set; } = ProgramStatus.Draft;
    public string RationaleSummary { get; private set; } = string.Empty;

    public IReadOnlyCollection<ProgramVersion> Versions => _versions.OrderByDescending(v => v.VersionNumber).ToList().AsReadOnly();

    private Program() { } // EF Core

    public Program(
        Guid id,
        Guid clientId,
        Guid coachId,
        string name,
        GoalSnapshot goalSnapshot,
        string rationaleSummary,
        ProgramStatus status = ProgramStatus.Draft) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Program name cannot be empty.", nameof(name));
        ArgumentNullException.ThrowIfNull(goalSnapshot);
        if (string.IsNullOrWhiteSpace(rationaleSummary))
            throw new ArgumentException("Rationale summary cannot be empty.", nameof(rationaleSummary));

        ClientId = clientId;
        CoachId = coachId;
        Name = name.Trim();
        GoalSnapshot = goalSnapshot;
        RationaleSummary = rationaleSummary.Trim();
        Status = status;
    }

    public void UpdateStatus(ProgramStatus status)
    {
        Status = status;
        MarkUpdated();
    }

    public void AddVersion(ProgramVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);
        if (_versions.Any(v => v.Id == version.Id))
            return;

        if (version.IsActive)
        {
            foreach (var v in _versions)
            {
                v.SetActive(false);
            }
        }

        _versions.Add(version);
        MarkUpdated();
    }

    public ProgramVersion? GetActiveVersion()
    {
        return _versions.FirstOrDefault(v => v.IsActive) ?? _versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
    }
}
