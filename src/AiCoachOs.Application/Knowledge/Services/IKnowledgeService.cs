using AiCoachOs.Application.Knowledge.DTOs;

namespace AiCoachOs.Application.Knowledge.Services;

public interface IKnowledgeService
{
    Task<KnowledgeSourceDto> CreateSourceAsync(CreateKnowledgeSourceDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<KnowledgeSourceSummaryDto>> GetSourcesAsync(CancellationToken ct = default);
    Task<KnowledgeSourceDto> GetSourceByIdAsync(Guid id, CancellationToken ct = default);

    Task<KnowledgeClaimDto> CreateClaimAsync(CreateKnowledgeClaimDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<KnowledgeClaimSummaryDto>> GetClaimsAsync(KnowledgeFilterDto filter, CancellationToken ct = default);
    Task<KnowledgeClaimDto> GetClaimByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<KnowledgeClaimSummaryDto>> GetClaimsByExerciseIdAsync(Guid exerciseId, CancellationToken ct = default);

    Task<KnowledgeClaimDto> AddSourceToClaimAsync(Guid claimId, AddClaimSourceDto dto, CancellationToken ct = default);
    Task<KnowledgeClaimDto> SupersedeClaimAsync(Guid claimId, SupersedeClaimDto dto, CancellationToken ct = default);
}
