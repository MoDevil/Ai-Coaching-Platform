using AiCoachOs.Application.Videos.Dtos;

namespace AiCoachOs.Application.Videos.Interfaces;

public interface IVideoAnalysisService
{
    Task<ClientVideoDto> UploadVideoAsync(
        Guid coachId, 
        Guid clientId, 
        UploadVideoRequestDto request, 
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientVideoSummaryDto>> GetClientVideosAsync(
        Guid coachId, 
        Guid clientId, 
        CancellationToken cancellationToken = default);

    Task<SignedMediaUrlDto> GetSignedVideoUrlAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VideoFrameDto>> GetVideoFramesAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default);

    Task AnonymizeVideoAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default);

    Task<EnqueueVideoAnalysisResponseDto> EnqueueVideoAnalysisAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default);

    Task<VideoAnalysisJobStatusDto> GetAnalysisStatusAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default);

    Task<ExerciseTechniqueObservationResultDto?> GetVideoObservationAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExerciseTechniqueObservationResultDto>> GetVideoObservationsHistoryAsync(
        Guid coachId, 
        Guid clientId, 
        Guid videoId, 
        CancellationToken cancellationToken = default);

    Task ExecuteAnalysisJobAsync(
        Guid jobId, 
        Guid videoId, 
        Guid clientId, 
        Guid coachId, 
        CancellationToken cancellationToken = default);
}
