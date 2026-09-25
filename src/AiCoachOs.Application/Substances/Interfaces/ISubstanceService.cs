using AiCoachOs.Application.Substances.Dtos;
using AiCoachOs.Domain.Substances;

namespace AiCoachOs.Application.Substances.Interfaces;

public interface ISubstanceService
{
    Task<IReadOnlyList<SupplementKnowledgeSummaryDto>> GetSupplementsAsync(
        string? name = null,
        SupplementEvidenceStatus? evidenceStatus = null,
        bool includeProvisional = false,
        CancellationToken cancellationToken = default);

    Task<SupplementKnowledgeDto?> GetSupplementByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HormoneKnowledgeSummaryDto>> GetHormonesAsync(
        HormoneCategory? category = null,
        string? name = null,
        CancellationToken cancellationToken = default);

    Task<HormoneKnowledgeDto?> GetHormoneByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PEDSafetyRecordSummaryDto>> GetPEDSafetyRecordsAsync(
        PEDCategory? category = null,
        CancellationToken cancellationToken = default);

    Task<PEDSafetyRecordDto?> GetPEDSafetyRecordByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SubstanceSafetyEvaluationResultDto> EvaluateSafetyAsync(
        Guid coachId,
        EvaluateSubstanceSafetyRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubstanceEscalationRecordDto>> GetCoachEscalationsAsync(
        Guid coachId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PEDRedFlagRuleDto>> GetActivePEDRedFlagRulesAsync(
        CancellationToken cancellationToken = default);
}
