using AiCoachOs.Application.ExpertIngestion.Dtos;

namespace AiCoachOs.Application.ExpertIngestion.Interfaces;

public interface IClaimExtractionService
{
    Task<ExtractedClaimsResult> ExtractClaimsAsync(
        string extractedText, 
        string title, 
        string? expertName = null, 
        CancellationToken cancellationToken = default);
}
