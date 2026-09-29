using AiCoachOs.Application.ExpertIngestion.Dtos;

namespace AiCoachOs.Application.ExpertIngestion.Interfaces;

public interface IConflictDetectionService
{
    Task<IReadOnlyList<ExpertClaimConflictMatch>> DetectM3ConflictsAsync(
        IReadOnlyList<ExtractedClaimCandidate> candidates, 
        CancellationToken cancellationToken = default);
}
