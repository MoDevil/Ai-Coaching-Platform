using AiCoachOs.Application.Memory.Dtos;
using AiCoachOs.Domain.Memory;

namespace AiCoachOs.Application.Memory.Interfaces;

public interface IClientMemoryService
{
    Task<IReadOnlyList<ClientMemoryRecordDto>> GetClientMemoriesAsync(
        Guid clientId,
        MemoryCategory? category = null,
        bool includeAnonymized = false,
        CancellationToken cancellationToken = default);

    Task<ClientMemoryRecordDto> GetMemoryByIdAsync(
        Guid clientId,
        Guid recordId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientMemoryRecordDto>> GetMemoryHistoryAsync(
        Guid clientId,
        Guid recordId,
        CancellationToken cancellationToken = default);

    Task<ClientMemoryRecordDto> CreateMemoryAsync(
        Guid clientId,
        CreateClientMemoryRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ClientMemoryRecordDto> CorrectMemoryAsync(
        Guid clientId,
        Guid recordId,
        CorrectClientMemoryRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ClientMemoryRecordDto> ArchiveMemoryAsync(
        Guid clientId,
        Guid recordId,
        CancellationToken cancellationToken = default);

    Task<ClientMemoryRecordDto> FlagUncertainAsync(
        Guid clientId,
        Guid recordId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientMemoryConflictDto>> GetConflictsAsync(
        Guid clientId,
        bool unresolvedOnly = false,
        CancellationToken cancellationToken = default);

    Task<ClientMemoryConflictDto> CreateConflictAsync(
        Guid clientId,
        CreateClientMemoryConflictRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ClientMemoryConflictDto> ResolveConflictAsync(
        Guid clientId,
        Guid conflictId,
        ResolveClientMemoryConflictRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ClientMemorySnapshotDto?> GetLatestSnapshotAsync(
        Guid clientId,
        CancellationToken cancellationToken = default);

    Task<ClientMemorySnapshotDto> GenerateSnapshotAsync(
        Guid clientId,
        GenerateClientMemorySnapshotRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UnresolvedQuestionDto>> GetUnresolvedQuestionsAsync(
        Guid clientId,
        CancellationToken cancellationToken = default);

    Task<AnonymizeClientMemoryResultDto> AnonymizeClientMemoriesAsync(
        Guid clientId,
        AnonymizeClientMemoryRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AIRecommendationRecordDto>> GetAIRecommendationsAsync(
        Guid clientId,
        AIRecommendationCategory? category = null,
        CancellationToken cancellationToken = default);

    Task<AIRecommendationRecordDto> DecideAIRecommendationAsync(
        Guid clientId,
        Guid recommendationId,
        DecideAIRecommendationRequestDto request,
        CancellationToken cancellationToken = default);
}
