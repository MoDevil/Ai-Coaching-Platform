using AiCoachOs.Domain.ExpertIngestion;

namespace AiCoachOs.Application.ExpertIngestion.Dtos;

public record SubmitIngestionRequestDto(
    string SourceUrl,
    Guid? ExpertSourceId = null,
    string? SourceTitle = null,
    IngestionSourceType? SourceType = null,
    DateTime? PublishedAt = null
);

public record ExpertSourceDto(
    Guid Id,
    string Name,
    ExpertSourceType SourceType,
    string Url,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CreateExpertSourceDto(
    string Name,
    ExpertSourceType SourceType,
    string Url
);

public record UpdateExpertSourceDto(
    string Name,
    ExpertSourceType SourceType,
    string Url
);

public record ExpertClaimDto(
    Guid Id,
    Guid IngestionId,
    string ClaimText,
    ClaimCategory ClaimCategory,
    EvidenceClassification EvidenceClassification,
    CreatorConfidence CreatorConfidence,
    bool DirectQuote,
    string? SourceContext,
    Guid? SupportingClaimId,
    string? SupportingClaimText,
    Guid? ConflictingClaimId,
    string? ConflictingClaimText,
    CoachReviewStatus CoachReviewStatus,
    DateTime? CoachReviewedAt,
    string? CoachNote,
    Guid? ApprovedKnowledgeClaimId,
    Guid? ReviewedByCoachId
);

public record ExpertContentIngestionSummaryDto(
    Guid Id,
    Guid CoachId,
    Guid? ExpertSourceId,
    string? SourceName,
    string SourceUrl,
    string SourceTitle,
    IngestionSourceType SourceType,
    DateTime? PublishedAt,
    int ExtractedTextLength,
    bool WasTruncated,
    IngestionStatus Status,
    string? FailureReason,
    bool ContainsMedicalClaims,
    int ClaimCount,
    DateTime SubmittedAtUtc,
    DateTime? ProcessedAtUtc
);

public record ExpertContentIngestionDto(
    Guid Id,
    Guid CoachId,
    Guid? ExpertSourceId,
    string? SourceName,
    string SourceUrl,
    string SourceTitle,
    IngestionSourceType SourceType,
    DateTime? PublishedAt,
    int ExtractedTextLength,
    bool WasTruncated,
    IngestionStatus Status,
    string? FailureReason,
    bool ContainsMedicalClaims,
    IReadOnlyList<ExpertClaimDto> Claims,
    DateTime SubmittedAtUtc,
    DateTime? ProcessedAtUtc
);

public record ReviewClaimRequestDto(
    CoachReviewStatus Decision,
    string? Notes = null,
    Guid? ExistingKnowledgeClaimIdToLink = null,
    bool CreateNewKnowledgeClaim = false,
    string? NewClaimQuestion = null,
    string? EgyptSpecificNotes = null,
    string? PractitionerNotes = null
);

public record FetchedContentResult(
    string RawText,
    string Title,
    IngestionSourceType SourceType,
    int ExtractedTextLength,
    bool WasTruncated,
    bool IsSuccess,
    string? ErrorMessage = null
);

public record ExtractedClaimCandidate(
    string ClaimText,
    ClaimCategory Category,
    EvidenceClassification EvidenceClassification,
    CreatorConfidence CreatorConfidence,
    bool DirectQuote,
    string? SourceContext
);

public record ExtractedClaimsResult(
    bool IsSuccess,
    IReadOnlyList<ExtractedClaimCandidate> Claims,
    string? SourceSummary,
    string? CreatorApparentPosition,
    bool ContainsMedicalContent,
    string? ErrorMessage = null
);

public record ExpertClaimConflictMatch(
    ExtractedClaimCandidate Candidate,
    Guid? SupportingClaimId,
    Guid? ConflictingClaimId
);
