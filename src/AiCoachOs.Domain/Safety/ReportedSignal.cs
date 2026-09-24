namespace AiCoachOs.Domain.Safety;

public class ReportedSignal
{
    public string BodyRegion { get; private set; } = string.Empty;
    public SignalType SignalType { get; private set; }
    public SignalOnset Onset { get; private set; }
    public SignalTiming Timing { get; private set; }
    public SignalSeverity Severity { get; private set; }
    public bool? Worsening { get; private set; }
    public SignalDuration Duration { get; private set; }
    public Guid? AssociatedWithExerciseId { get; private set; }
    public string? FreeText { get; private set; }

    private ReportedSignal() { } // EF Core

    public ReportedSignal(
        string bodyRegion,
        SignalType signalType,
        SignalOnset onset = SignalOnset.Unknown,
        SignalTiming timing = SignalTiming.Unknown,
        SignalSeverity severity = SignalSeverity.Unknown,
        bool? worsening = null,
        SignalDuration duration = SignalDuration.Unknown,
        Guid? associatedWithExerciseId = null,
        string? freeText = null)
    {
        if (string.IsNullOrWhiteSpace(bodyRegion))
            throw new ArgumentException("Body region cannot be empty.", nameof(bodyRegion));

        BodyRegion = bodyRegion.Trim();
        SignalType = signalType;
        Onset = onset;
        Timing = timing;
        Severity = severity;
        Worsening = worsening;
        Duration = duration;
        AssociatedWithExerciseId = associatedWithExerciseId;
        FreeText = string.IsNullOrWhiteSpace(freeText) ? null : freeText.Trim();
    }
}
