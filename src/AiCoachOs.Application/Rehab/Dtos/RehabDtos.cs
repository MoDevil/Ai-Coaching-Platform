using AiCoachOs.Domain.Rehab;

namespace AiCoachOs.Application.Rehab.Dtos;

public record RehabAwarenessConsiderationDto(
    Guid Id,
    Guid TrainingLimitationId,
    Guid? ExerciseId,
    string? ExerciseName,
    ConsiderationType ConsiderationType,
    string ConsiderationText,
    Guid? KnowledgeClaimId,
    string? EvidenceBasis,
    string Disclaimer,
    ConsiderationStatus Status,
    DateTime GeneratedAtUtc,
    DateTime? CoachDecisionAtUtc,
    string? CoachDecisionNote);

public record TrainingLimitationDto(
    Guid Id,
    Guid ClientId,
    Guid? SafetyScreeningId,
    string AffectedBodyRegion,
    LimitationSource LimitationSource,
    LimitationStatus Status,
    string? Description,
    DateTime ReportedAtUtc,
    DateTime? CoachActivatedM9AtUtc,
    string? CoachActivationNote,
    DateTime? ResolvedAtUtc,
    IReadOnlyList<RehabAwarenessConsiderationDto> Considerations);

public record CreateTrainingLimitationRequestDto(
    Guid ClientId,
    string AffectedBodyRegion,
    LimitationSource LimitationSource,
    Guid? SafetyScreeningId = null,
    string? Description = null);

public record ActivateLimitationRequestDto(
    string CoachNote);

public record UpdateLimitationStatusRequestDto(
    LimitationStatus Status);

public record RecordConsiderationDecisionRequestDto(
    ConsiderationStatus Decision,
    string? Note = null);

public record GenerateConsiderationsRequestDto(
    Guid? ExerciseId = null);
