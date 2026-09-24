using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Safety;

public class SafetyScreening : Entity<Guid>
{
    private readonly List<ReportedSignal> _reportedSignals = new();
    private readonly List<string> _redFlagsMatched = new();

    public Guid ClientId { get; private set; }
    public Client Client { get; private set; } = null!;

    public TriggeredByType TriggeredByType { get; private set; }
    public Guid? TriggeredByEntityId { get; private set; }

    public SafetyCategory ScreeningResult { get; private set; }
    public SafetyActionType RecommendedAction { get; private set; }
    public string SummaryRationale { get; private set; } = string.Empty;
    public string Disclaimer { get; private set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; private set; }
    public bool RequiresCoachAcknowledgment { get; private set; }
    public DateTime? CoachAcknowledgedAtUtc { get; private set; }
    public string? CoachNote { get; private set; }

    public IReadOnlyCollection<ReportedSignal> ReportedSignals => _reportedSignals.AsReadOnly();
    public IReadOnlyCollection<string> RedFlagsMatched => _redFlagsMatched.AsReadOnly();

    public const string FixedDisclaimer = "AI Coach OS does not diagnose medical conditions. This assessment is based on reported signals only. Clinical evaluation is required for health concerns.";

    private SafetyScreening() { } // EF Core

    public SafetyScreening(
        Guid id,
        Guid clientId,
        TriggeredByType triggeredByType,
        SafetyCategory screeningResult,
        SafetyActionType recommendedAction,
        string summaryRationale,
        DateTime generatedAtUtc,
        Guid? triggeredByEntityId = null,
        bool requiresCoachAcknowledgment = false,
        string? disclaimer = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(summaryRationale))
            throw new ArgumentException("Summary rationale cannot be empty.", nameof(summaryRationale));

        ClientId = clientId;
        TriggeredByType = triggeredByType;
        TriggeredByEntityId = triggeredByEntityId;
        ScreeningResult = screeningResult;
        RecommendedAction = recommendedAction;
        SummaryRationale = summaryRationale.Trim();
        GeneratedAtUtc = generatedAtUtc;
        RequiresCoachAcknowledgment = requiresCoachAcknowledgment;
        Disclaimer = string.IsNullOrWhiteSpace(disclaimer) ? FixedDisclaimer : disclaimer.Trim();
    }

    public void AddReportedSignal(ReportedSignal signal)
    {
        ArgumentNullException.ThrowIfNull(signal);
        _reportedSignals.Add(signal);
    }

    public void AddMatchedRedFlag(string redFlagName)
    {
        if (string.IsNullOrWhiteSpace(redFlagName))
            return;
        if (!_redFlagsMatched.Contains(redFlagName.Trim()))
            _redFlagsMatched.Add(redFlagName.Trim());
    }

    public void AcknowledgeByCoach(DateTime acknowledgedAtUtc, string? coachNote = null)
    {
        CoachAcknowledgedAtUtc = acknowledgedAtUtc;
        CoachNote = string.IsNullOrWhiteSpace(coachNote) ? null : coachNote.Trim();
        RequiresCoachAcknowledgment = false;
        MarkUpdated();
    }
}
