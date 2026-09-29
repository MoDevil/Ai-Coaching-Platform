using AiCoachOs.Application.Photos.Dtos;

namespace AiCoachOs.Application.Photos.Interfaces;

public interface IPhotoVisionService
{
    Task<ClientPhotoDto> UploadPhotoAsync(
        Guid coachId, 
        Guid clientId, 
        UploadPhotoRequestDto request, 
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientPhotoSummaryDto>> GetClientPhotosAsync(
        Guid coachId, 
        Guid clientId, 
        CancellationToken cancellationToken = default);

    Task<SignedPhotoUrlDto> GetSignedPhotoUrlAsync(
        Guid coachId, 
        Guid clientId, 
        Guid photoId, 
        CancellationToken cancellationToken = default);

    Task DeletePhotoAsync(
        Guid coachId, 
        Guid clientId, 
        Guid photoId, 
        CancellationToken cancellationToken = default);

    Task<PhysiqueObservationResultDto> AnalyzePhotoAsync(
        Guid coachId, 
        Guid clientId, 
        Guid photoId, 
        AnalyzePhotoRequestDto? request = null, 
        CancellationToken cancellationToken = default);

    Task<PhysiqueObservationResultDto?> GetPhotoObservationAsync(
        Guid coachId, 
        Guid clientId, 
        Guid photoId, 
        CancellationToken cancellationToken = default);
}
