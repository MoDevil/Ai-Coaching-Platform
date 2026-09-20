using AiCoachOs.Application.Adaptations.DTOs;

namespace AiCoachOs.Application.Adaptations.Services;

public interface IAdaptationService
{
    Task<AdaptationAssessmentDto> AssessProgramVersionAsync(Guid coachId, Guid programVersionId, CancellationToken cancellationToken = default);
    Task<AdaptationAssessmentDto?> GetAssessmentByIdAsync(Guid coachId, Guid assessmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdaptationAssessmentDto>> GetAssessmentsByProgramIdAsync(Guid coachId, Guid programId, CancellationToken cancellationToken = default);
    Task<AdaptationRecommendationDto> DecideRecommendationAsync(Guid coachId, Guid recommendationId, CoachRecommendationDecisionDto decision, CancellationToken cancellationToken = default);
}
