using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Memory.Dtos;

namespace AiCoachOs.Application.Ai.Interfaces;

public interface IAiReasoningService
{
    Task<AIRecommendationRecordDto> GenerateReasoningAsync(
        Guid coachId, 
        GenerateReasoningRequestDto request, 
        CancellationToken cancellationToken = default);

    Task<AIRecommendationRecordDto> GetReasoningByIdAsync(
        Guid coachId, 
        Guid recommendationId, 
        CancellationToken cancellationToken = default);

    Task<AIRecommendationRecordDto> ReviewRecommendationAsync(
        Guid coachId,
        Guid recommendationId,
        ReviewAIRecommendationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AIRecommendationSummaryDto>> GetClientRecommendationsAsync(
        Guid coachId,
        Guid clientId,
        Domain.Memory.AIRecommendationReviewStatus? status = null,
        Domain.Memory.AIRecommendationCategory? category = null,
        CancellationToken cancellationToken = default);

    Task<AIRecommendationDetailDto> GetClientRecommendationDetailAsync(
        Guid coachId,
        Guid clientId,
        Guid recommendationId,
        CancellationToken cancellationToken = default);
}
