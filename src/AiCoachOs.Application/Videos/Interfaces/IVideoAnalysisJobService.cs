using AiCoachOs.Application.Videos.Dtos;

namespace AiCoachOs.Application.Videos.Interfaces;

public enum VideoAnalysisJobStateStatus
{
    Queued = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}

public class VideoAnalysisJobState
{
    public Guid JobId { get; set; }
    public Guid VideoId { get; set; }
    public Guid ClientId { get; set; }
    public Guid CoachId { get; set; }
    public VideoAnalysisJobStateStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? ObservationRecordId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
}

public interface IVideoAnalysisJobService
{
    Guid EnqueueJob(Guid videoId, Guid clientId, Guid coachId);
    VideoAnalysisJobStatusDto? GetJobStatus(Guid coachId, Guid clientId, Guid videoId);
    void UpdateJobStatus(Guid jobId, VideoAnalysisJobStateStatus status, string? errorMessage = null, Guid? observationRecordId = null);
}
