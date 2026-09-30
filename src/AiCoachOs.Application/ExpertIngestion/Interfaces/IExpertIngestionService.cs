using AiCoachOs.Application.ExpertIngestion.Dtos;

namespace AiCoachOs.Application.ExpertIngestion.Interfaces;

public interface IExpertIngestionService
{
    Task<ExpertContentIngestionSummaryDto> SubmitIngestionAsync(
        Guid coachId, 
        SubmitIngestionRequestDto request, 
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpertContentIngestionSummaryDto>> GetIngestionsAsync(
        Guid coachId, 
        AiCoachOs.Domain.ExpertIngestion.IngestionStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<ExpertContentIngestionDto> GetIngestionByIdAsync(
        Guid coachId, 
        Guid ingestionId, 
        CancellationToken cancellationToken = default);

    Task<ExpertClaimDto> ReviewClaimAsync(
        Guid coachId, 
        Guid ingestionId, 
        Guid claimId, 
        ReviewClaimRequestDto request, 
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpertSourceDto>> GetSourcesAsync(CancellationToken cancellationToken = default);
    
    Task<ExpertSourceDto> CreateSourceAsync(
        CreateExpertSourceDto request, 
        CancellationToken cancellationToken = default);

    Task ExecuteIngestionPipelineAsync(
        Guid ingestionId, 
        CancellationToken cancellationToken = default);
}
