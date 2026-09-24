using AiCoachOs.Application.Safety.Dtos;

namespace AiCoachOs.Application.Safety.Interfaces;

public interface ISafetyService
{
    Task<SafetyScreeningDto> ScreenReportAsync(
        Guid coachId,
        CreateSafetyReportRequestDto request,
        CancellationToken cancellationToken = default);

    Task<SafetyScreeningDto> AcknowledgeScreeningAsync(
        Guid coachId,
        Guid screeningId,
        AcknowledgeSafetyScreeningRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SafetyScreeningDto>> GetClientScreeningsAsync(
        Guid coachId,
        Guid clientId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RedFlagRuleDto>> GetActiveRulesAsync(
        CancellationToken cancellationToken = default);
}
