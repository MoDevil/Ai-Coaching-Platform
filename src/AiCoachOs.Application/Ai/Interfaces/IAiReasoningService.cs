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
}
