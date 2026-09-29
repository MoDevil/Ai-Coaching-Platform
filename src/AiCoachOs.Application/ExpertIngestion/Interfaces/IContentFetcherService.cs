using AiCoachOs.Application.ExpertIngestion.Dtos;
using AiCoachOs.Domain.ExpertIngestion;

namespace AiCoachOs.Application.ExpertIngestion.Interfaces;

public interface IContentFetcherService
{
    Task<FetchedContentResult> FetchContentAsync(
        string sourceUrl, 
        IngestionContentType? overrideContentType = null, 
        CancellationToken cancellationToken = default);
}
