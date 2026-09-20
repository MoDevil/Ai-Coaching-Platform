using AiCoachOs.Domain.Adaptations;

namespace AiCoachOs.Application.Adaptations.DTOs;

public record AdaptationAssessmentDto(
    Guid Id,
    Guid ProgramVersionId,
    int VersionNumber,
    Guid ProgramId,
    string ProgramName,
    Guid ClientId,
    string ClientName,
    DateTime AssessedAt,
    DateTime ObservationStartDate,
    DateTime ObservationEndDate,
    int TotalExposures,
    int CompletedExposures,
    decimal AdherenceRate,
    AdaptationOverallStatus OverallStatus,
    string? CoachNotes,
    IReadOnlyList<ExerciseAdaptationRecordDto> ExerciseRecords,
    IReadOnlyList<AdaptationRecommendationDto> Recommendations);

public record ExerciseAdaptationRecordDto(
    Guid Id,
    Guid ExerciseSlotId,
    Guid ExerciseId,
    string ExerciseName,
    int ExposureCount,
    int ProgressionMetCount,
    EffortAlignmentStatus EffortAlignmentStatus,
    PerformanceTrend PerformanceTrend,
    bool PlateauConfirmed,
    decimal AdherenceToExercise);

public record AdaptationRecommendationDto(
    Guid Id,
    Guid? ExerciseAdaptationRecordId,
    Guid? TargetSlotId,
    string? ExerciseName,
    AdaptationActionType ActionType,
    string? SuggestedChangeDetail,
    string Rationale,
    RecommendationConfidence Confidence,
    RecommendationStatus Status,
    DateTime? CoachDecisionAt,
    string? CoachDecisionNote);

public record CoachRecommendationDecisionDto(
    bool Approve,
    string? CoachDecisionNote,
    string? CustomChangeDetail = null);
