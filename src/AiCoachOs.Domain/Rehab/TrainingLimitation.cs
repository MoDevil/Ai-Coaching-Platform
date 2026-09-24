using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Safety;

namespace AiCoachOs.Domain.Rehab;

public class TrainingLimitation : Entity<Guid>
{
    private readonly List<RehabAwarenessConsideration> _considerations = new();

    public Guid ClientId { get; private set; }
    public Client Client { get; private set; } = null!;

    public Guid? SafetyScreeningId { get; private set; }
    public SafetyScreening? SafetyScreening { get; private set; }

    public string AffectedBodyRegion { get; private set; } = string.Empty;
    public LimitationSource LimitationSource { get; private set; }
    public LimitationStatus Status { get; private set; }
    public string? Description { get; private set; }
    public DateTime ReportedAtUtc { get; private set; }

    public DateTime? CoachActivatedM9AtUtc { get; private set; }
    public string? CoachActivationNote { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }

    public IReadOnlyCollection<RehabAwarenessConsideration> Considerations => _considerations.AsReadOnly();

    private TrainingLimitation() { } // EF Core

    public TrainingLimitation(
        Guid id,
        Guid clientId,
        string affectedBodyRegion,
        LimitationSource limitationSource,
        DateTime reportedAtUtc,
        Guid? safetyScreeningId = null,
        string? description = null,
        LimitationStatus status = LimitationStatus.Active) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(affectedBodyRegion))
            throw new ArgumentException("AffectedBodyRegion cannot be empty.", nameof(affectedBodyRegion));

        ClientId = clientId;
        AffectedBodyRegion = affectedBodyRegion.Trim();
        LimitationSource = limitationSource;
        ReportedAtUtc = reportedAtUtc;
        SafetyScreeningId = safetyScreeningId;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Status = status;
    }

    public void ActivateByCoach(DateTime activatedAtUtc, string coachNote)
    {
        if (string.IsNullOrWhiteSpace(coachNote))
            throw new ArgumentException("Coach activation note is required.", nameof(coachNote));

        CoachActivatedM9AtUtc = activatedAtUtc;
        CoachActivationNote = coachNote.Trim();
        MarkUpdated();
    }

    public void Resolve(DateTime resolvedAtUtc)
    {
        Status = LimitationStatus.Resolved;
        ResolvedAtUtc = resolvedAtUtc;
        MarkUpdated();
    }

    public void PutOnHold()
    {
        Status = LimitationStatus.OnHold;
        MarkUpdated();
    }

    public void Reactivate()
    {
        Status = LimitationStatus.Active;
        ResolvedAtUtc = null;
        MarkUpdated();
    }

    public void AddConsideration(RehabAwarenessConsideration consideration)
    {
        ArgumentNullException.ThrowIfNull(consideration);
        _considerations.Add(consideration);
    }
}
