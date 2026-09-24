using AiCoachOs.Application.Rehab.Dtos;
using AiCoachOs.Domain.Rehab;

namespace AiCoachOs.Application.Rehab.Interfaces;

public interface IRehabService
{
    Task<TrainingLimitationDto> CreateLimitationAsync(
        Guid coachId,
        CreateTrainingLimitationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<TrainingLimitationDto> ActivateLimitationAsync(
        Guid coachId,
        Guid limitationId,
        ActivateLimitationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<TrainingLimitationDto> UpdateLimitationStatusAsync(
        Guid coachId,
        Guid limitationId,
        UpdateLimitationStatusRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RehabAwarenessConsiderationDto>> GenerateConsiderationsAsync(
        Guid coachId,
        Guid limitationId,
        GenerateConsiderationsRequestDto request,
        CancellationToken cancellationToken = default);

    Task<RehabAwarenessConsiderationDto> RecordDecisionAsync(
        Guid coachId,
        Guid considerationId,
        RecordConsiderationDecisionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrainingLimitationDto>> GetClientLimitationsAsync(
        Guid coachId,
        Guid clientId,
        CancellationToken cancellationToken = default);
}
