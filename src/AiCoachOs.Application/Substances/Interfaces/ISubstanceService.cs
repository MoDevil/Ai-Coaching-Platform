using AiCoachOs.Application.Substances.Dtos;

namespace AiCoachOs.Application.Substances.Interfaces;

public interface ISubstanceService
{
    Task<IReadOnlyList<SupplementKnowledgeSummaryDto>> GetSupplementsAsync(
        bool includeProvisional = false,
        CancellationToken cancellationToken = default);

    Task<SupplementKnowledgeDto?> GetSupplementByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HormoneKnowledgeSummaryDto>> GetHormonesAsync(
        CancellationToken cancellationToken = default);

    Task<HormoneKnowledgeDto?> GetHormoneByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PEDSafetyRecordSummaryDto>> GetPEDSafetyRecordsAsync(
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
