using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.ExpertIngestion;

/// <summary>
/// Represents an expert content ingestion job and container for extracted claims.
/// </summary>
public class ExpertContentIngestion : Entity<Guid>
{
    private readonly List<ExpertClaim> _claims = new();

    public Guid CoachId { get; private set; }
    public Guid? SourceId { get; private set; }
    public ExpertSource? Source { get; private set; }

    public string SourceUrl { get; private set; } = null!;
    public IngestionContentType ContentType { get; private set; }
    public string Title { get; private set; } = null!;
    public string? RawExtractedTextSnippet { get; private set; }
    public int WordCount { get; private set; }
    public bool WasTruncated { get; private set; }

    public IngestionStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public bool ContainsMedicalClaims { get; private set; }
    public bool MedicalWarningAcknowledged { get; private set; }

    public DateTime SubmittedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public IReadOnlyCollection<ExpertClaim> Claims => _claims;

    private ExpertContentIngestion() { } // EF Core

    public ExpertContentIngestion(
        Guid id,
        Guid coachId,
        string sourceUrl,
        IngestionContentType contentType,
        string title,
        Guid? sourceId = null) : base(id)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (string.IsNullOrWhiteSpace(sourceUrl))
            throw new ArgumentException("SourceUrl cannot be empty.", nameof(sourceUrl));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        CoachId = coachId;
        SourceUrl = sourceUrl.Trim();
        ContentType = contentType;
        Title = title.Trim();
        SourceId = sourceId;
        Status = IngestionStatus.Processing;
        SubmittedAtUtc = DateTime.UtcNow;
    }

    public void SetExtractedContent(string snippet, int wordCount, bool wasTruncated)
    {
        RawExtractedTextSnippet = snippet?.Trim();
        WordCount = wordCount;
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
        CompletedAtUtc = DateTime.UtcNow;
        FailureReason = null;
        MarkUpdated();
    }

    public void SetFailed(string reason)
    {
        Status = IngestionStatus.Failed;
        FailureReason = reason?.Trim();
        CompletedAtUtc = DateTime.UtcNow;
        MarkUpdated();
    }

    public void AcknowledgeMedicalWarning()
    {
        MedicalWarningAcknowledged = true;
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

        var allApproved = _claims.All(c => c.ReviewStatus == ExpertClaimReviewStatus.Approved);
        var anyApproved = _claims.Any(c => c.ReviewStatus == ExpertClaimReviewStatus.Approved);
        var allDecided = _claims.All(c => c.ReviewStatus != ExpertClaimReviewStatus.Pending);

        if (allApproved)
        {
            Status = IngestionStatus.FullyApproved;
        }
        else if (anyApproved)
        {
            Status = IngestionStatus.PartiallyApproved;
        }
        else if (allDecided && _claims.All(c => c.ReviewStatus == ExpertClaimReviewStatus.Rejected))
        {
            Status = IngestionStatus.Rejected;
        }
        else
        {
            Status = IngestionStatus.PendingReview;
        }

        MarkUpdated();
    }
}
