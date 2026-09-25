using AiCoachOs.Domain.Coaches;
using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents a logged substance safety evaluation and escalation decision.
/// Coach-owned decision-support log. DOES NOT link to Client entities (no ClientId)
/// to maintain strict privacy and safety architecture.
/// Locked contract: Id, CoachId, SubstanceRecordId, ReportedSignals, TriggeredFlagIds, EscalationLevel,
/// CoachNote, SummaryRationale, RecommendedAction, Disclaimer, CreatedAtUtc.
/// </summary>
public class SubstanceEscalationRecord : Entity<Guid>
{
    private readonly List<string> _reportedSignals = new();
    private readonly List<string> _triggeredFlagIds = new();

    public Guid CoachId { get; private set; }
    public Coach? Coach { get; private set; }

    public Guid? SubstanceRecordId { get; private set; }
    public SubstanceRecord? SubstanceRecord { get; private set; }

    public EscalationLevel EscalationLevel { get; private set; }
    public string? CoachNote { get; private set; }
    public string SummaryRationale { get; private set; } = string.Empty;
    public string RecommendedAction { get; private set; } = string.Empty;
    public string Disclaimer { get; private set; } = "MEDICAL DECISION SUPPORT DISCLAIMER: AI Coach OS does not provide medical diagnoses or treatment prescriptions. Any flagged severe or emergency symptoms require immediate referral to qualified medical professionals.";

    public IReadOnlyCollection<string> ReportedSignals => _reportedSignals;
    public IReadOnlyCollection<string> TriggeredFlagIds => _triggeredFlagIds;

    private SubstanceEscalationRecord() { } // EF Core

    public SubstanceEscalationRecord(
        Guid id,
        Guid coachId,
        EscalationLevel escalationLevel,
        string summaryRationale,
        string recommendedAction,
        Guid? substanceRecordId = null,
        string? coachNote = null,
        DateTime? createdAtUtc = null,
        IEnumerable<string>? reportedSignals = null,
        IEnumerable<string>? triggeredFlagIds = null) : base(id)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (string.IsNullOrWhiteSpace(summaryRationale))
            throw new ArgumentException("Summary rationale cannot be empty.", nameof(summaryRationale));
        if (string.IsNullOrWhiteSpace(recommendedAction))
            throw new ArgumentException("Recommended action cannot be empty.", nameof(recommendedAction));

        CoachId = coachId;
        EscalationLevel = escalationLevel;
        SummaryRationale = summaryRationale.Trim();
        RecommendedAction = recommendedAction.Trim();
        SubstanceRecordId = substanceRecordId;
        CoachNote = string.IsNullOrWhiteSpace(coachNote) ? null : coachNote.Trim();
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;

        if (reportedSignals != null)
        {
            foreach (var sig in reportedSignals)
            {
                if (!string.IsNullOrWhiteSpace(sig))
                    _reportedSignals.Add(sig.Trim());
            }
        }

        if (triggeredFlagIds != null)
        {
            foreach (var flagId in triggeredFlagIds)
            {
                if (!string.IsNullOrWhiteSpace(flagId))
                    _triggeredFlagIds.Add(flagId.Trim());
            }
        }
    }

    public void AddReportedSignal(string signal)
    {
        if (!string.IsNullOrWhiteSpace(signal))
            _reportedSignals.Add(signal.Trim());
    }

    public void AddTriggeredFlagId(string flagId)
    {
        if (!string.IsNullOrWhiteSpace(flagId))
            _triggeredFlagIds.Add(flagId.Trim());
    }
}
