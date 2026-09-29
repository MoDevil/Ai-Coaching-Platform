using System.Collections.Concurrent;
using AiCoachOs.Application.Videos.Dtos;
using AiCoachOs.Application.Videos.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Videos;

public class VideoAnalysisJobService : IVideoAnalysisJobService
{
    private readonly ConcurrentDictionary<Guid, VideoAnalysisJobState> _jobs = new();
    private readonly ConcurrentDictionary<Guid, Guid> _latestJobForVideo = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VideoAnalysisJobService> _logger;

    public VideoAnalysisJobService(
        IServiceScopeFactory scopeFactory,
        ILogger<VideoAnalysisJobService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Guid EnqueueJob(Guid videoId, Guid clientId, Guid coachId)
    {
        var jobId = Guid.NewGuid();
        var state = new VideoAnalysisJobState
        {
            JobId = jobId,
            VideoId = videoId,
            ClientId = clientId,
            CoachId = coachId,
            Status = VideoAnalysisJobStateStatus.Queued,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _jobs[jobId] = state;
        _latestJobForVideo[videoId] = jobId;

        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var analysisService = scope.ServiceProvider.GetRequiredService<IVideoAnalysisService>();

                state.Status = VideoAnalysisJobStateStatus.Processing;
                _logger.LogInformation("Starting background video analysis job {JobId} for video {VideoId}", jobId, videoId);

                await analysisService.ExecuteAnalysisJobAsync(jobId, videoId, clientId, coachId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled failure in video analysis job {JobId} for video {VideoId}", jobId, videoId);
                UpdateJobStatus(jobId, VideoAnalysisJobStateStatus.Failed, ex.Message);
            }
        });

        return jobId;
    }

    public VideoAnalysisJobStatusDto? GetJobStatus(Guid coachId, Guid clientId, Guid videoId)
    {
        if (!_latestJobForVideo.TryGetValue(videoId, out var jobId))
            return null;

        if (!_jobs.TryGetValue(jobId, out var state))
            return null;

        if (state.CoachId != coachId || state.ClientId != clientId)
            return null;

        return new VideoAnalysisJobStatusDto
        {
            JobId = state.JobId,
            VideoId = state.VideoId,
            Status = state.Status.ToString(),
            ErrorMessage = state.ErrorMessage,
            ObservationRecordId = state.ObservationRecordId,
            CreatedAtUtc = state.CreatedAtUtc,
            CompletedAtUtc = state.CompletedAtUtc
        };
    }

    public void UpdateJobStatus(Guid jobId, VideoAnalysisJobStateStatus status, string? errorMessage = null, Guid? observationRecordId = null)
    {
        if (_jobs.TryGetValue(jobId, out var state))
        {
            state.Status = status;
            state.ErrorMessage = errorMessage;
            state.ObservationRecordId = observationRecordId ?? state.ObservationRecordId;

            if (status == VideoAnalysisJobStateStatus.Completed || status == VideoAnalysisJobStateStatus.Failed)
            {
                state.CompletedAtUtc = DateTimeOffset.UtcNow;
            }
        }
    }
}
