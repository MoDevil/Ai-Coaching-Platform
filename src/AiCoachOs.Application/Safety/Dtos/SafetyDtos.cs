using AiCoachOs.Domain.Safety;

namespace AiCoachOs.Application.Safety.Dtos;

public record ReportedSignalDto(
    string BodyRegion,
    SignalType SignalType,
    SignalOnset Onset = SignalOnset.Unknown,
    SignalTiming Timing = SignalTiming.Unknown,
    SignalSeverity Severity = SignalSeverity.Unknown,
    bool? Worsening = null,
    SignalDuration Duration = SignalDuration.Unknown,
    Guid? AssociatedWithExerciseId = null,
    string? FreeText = null);

public record SafetyScreeningDto(
    Guid Id,
    Guid ClientId,
    TriggeredByType TriggeredByType,
    Guid? TriggeredByEntityId,
    SafetyCategory ScreeningResult,
    SafetyActionType RecommendedAction,
    string SummaryRationale,
    string Disclaimer,
    DateTime GeneratedAtUtc,
    bool RequiresCoachAcknowledgment,
    DateTime? CoachAcknowledgedAtUtc,
    string? CoachNote,
    IReadOnlyList<ReportedSignalDto> ReportedSignals,
    IReadOnlyList<string> RedFlagsMatched);

public record CreateSafetyReportRequestDto(
    Guid ClientId,
    TriggeredByType TriggeredByType,
    IReadOnlyList<ReportedSignalDto> Signals,
    Guid? TriggeredByEntityId = null);

public record AcknowledgeSafetyScreeningRequestDto(
    string? CoachNote = null);

public record RedFlagRuleDto(
    Guid Id,
    string Name,
    string Description,
    string SignalPattern,
    SafetyCategory SafetyCategoryTriggered,
    SafetyActionType RecommendedAction,
    string EvidenceBasis,
    bool RequiresClinicalReview,
    bool IsActive,
    DateTime? LastReviewedAtUtc,
    string? ReviewedBy,
    Guid? KnowledgeClaimId);
