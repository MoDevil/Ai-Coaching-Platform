using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.ExpertIngestion;

/// <summary>
/// Represents an expert content ingestion job and container for extracted claims.
/// </summary>
public class ExpertContentIngestion : Entity<Guid>
{
    private readonly List<ExpertClaim> _claims = new();

    public Guid CoachId { get; private set; }
    public Guid? ExpertSourceId { get; private set; }
    public ExpertSource? ExpertSource { get; private set; }

    public string SourceUrl { get; private set; } = null!;
    public string SourceTitle { get; private set; } = null!;
    public IngestionSourceType SourceType { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public int ExtractedTextLength { get; private set; }
    public bool WasTruncated { get; private set; }

    public IngestionStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime SubmittedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public bool ContainsMedicalClaims { get; private set; }

    public IReadOnlyCollection<ExpertClaim> Claims => _claims;

    private ExpertContentIngestion() { } // EF Core

    public ExpertContentIngestion(
        Guid id,
        Guid coachId,
        string sourceUrl,
        string sourceTitle,
        IngestionSourceType sourceType,
        Guid? expertSourceId = null,
        DateTime? publishedAt = null) : base(id)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (string.IsNullOrWhiteSpace(sourceUrl))
            throw new ArgumentException("SourceUrl cannot be empty.", nameof(sourceUrl));
        if (string.IsNullOrWhiteSpace(sourceTitle))
            throw new ArgumentException("SourceTitle cannot be empty.", nameof(sourceTitle));

        CoachId = coachId;
        SourceUrl = sourceUrl.Trim();
        SourceTitle = sourceTitle.Trim();
        SourceType = sourceType;
        ExpertSourceId = expertSourceId;
        PublishedAt = publishedAt;
        Status = IngestionStatus.Processing;
        SubmittedAtUtc = DateTime.UtcNow;
    }

    public void SetExtractedStats(int extractedTextLength, bool wasTruncated)
    {
        ExtractedTextLength = extractedTextLength;
        WasTruncated = wasTruncated;
        MarkUpdated();
    }

    public void AddClaim(ExpertClaim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        _claims.Add(claim);
        MarkUpdated();
    }

    public void SetCompleted(bool containsMedicalClaims)
    {
        ContainsMedicalClaims = containsMedicalClaims;
        Status = IngestionStatus.PendingReview;
        ProcessedAtUtc = DateTime.UtcNow;
        FailureReason = null;
        MarkUpdated();
    }

    public void SetFailed(string reason)
    {
        Status = IngestionStatus.Failed;
        FailureReason = reason?.Trim();
        ProcessedAtUtc = DateTime.UtcNow;
        MarkUpdated();
    }

    public void RecalculateOverallStatus()
    {
        if (Status == IngestionStatus.Failed || Status == IngestionStatus.Processing)
            return;

        if (_claims.Count == 0)
        {
            Status = IngestionStatus.PendingReview;
            return;
        }

        var allDecided = _claims.All(c => c.CoachReviewStatus != CoachReviewStatus.PendingReview);
        var anyApproved = _claims.Any(c => c.CoachReviewStatus == CoachReviewStatus.Approved);

        if (allDecided)
        {
            Status = IngestionStatus.Completed;
        }
        else if (anyApproved)
        {
            Status = IngestionStatus.PartiallyApproved;
        }
        else
        {
            Status = IngestionStatus.PendingReview;
        }

        MarkUpdated();
    }
}
