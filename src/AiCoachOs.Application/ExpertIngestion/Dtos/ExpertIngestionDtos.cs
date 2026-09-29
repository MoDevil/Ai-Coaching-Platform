using AiCoachOs.Domain.ExpertIngestion;

namespace AiCoachOs.Application.ExpertIngestion.Dtos;

public record SubmitIngestionRequestDto(
    string SourceUrl,
    Guid? SourceId = null,
    string? Title = null,
    IngestionContentType? ContentType = null
);

public record ExpertSourceDto(
    Guid Id,
    string Name,
    string ChannelOrPublication,
    ExpertPlatform Platform,
    string PrimaryDomain,
    CredibilityTier CredibilityTier,
    string? Bio,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CreateExpertSourceDto(
    string Name,
    string ChannelOrPublication,
    ExpertPlatform Platform,
    string PrimaryDomain,
    CredibilityTier CredibilityTier,
    string? Bio = null
);

public record UpdateExpertSourceDto(
    string Name,
    string ChannelOrPublication,
    ExpertPlatform Platform,
    string PrimaryDomain,
    CredibilityTier CredibilityTier,
    string? Bio = null
);

public record ExpertClaimDto(
    Guid Id,
    Guid IngestionId,
    string Topic,
    string? SubTopic,
    string ClaimText,
    string? ContextOrTimestamp,
    bool DirectQuote,
    ClaimNature NatureOfClaim,
    Guid? SupportingClaimId,
    string? SupportingClaimText,
    Guid? ConflictingClaimId,
    string? ConflictingClaimText,
    ExpertClaimReviewStatus ReviewStatus,
    string? CoachNotes,
    Guid? ApprovedKnowledgeClaimId,
    DateTime? ReviewedAtUtc,
    Guid? ReviewedByCoachId
);

public record ExpertContentIngestionSummaryDto(
    Guid Id,
    Guid CoachId,
    Guid? SourceId,
    string? SourceName,
    string SourceUrl,
    IngestionContentType ContentType,
    string Title,
    int WordCount,
    bool WasTruncated,
    IngestionStatus Status,
    string? FailureReason,
    bool ContainsMedicalClaims,
    bool MedicalWarningAcknowledged,
    int ClaimCount,
    DateTime SubmittedAtUtc,
    DateTime? CompletedAtUtc
);

public record ExpertContentIngestionDto(
    Guid Id,
    Guid CoachId,
    Guid? SourceId,
    string? SourceName,
    string SourceUrl,
    IngestionContentType ContentType,
    string Title,
    string? RawExtractedTextSnippet,
    int WordCount,
    bool WasTruncated,
    IngestionStatus Status,
    string? FailureReason,
    bool ContainsMedicalClaims,
    bool MedicalWarningAcknowledged,
    IReadOnlyList<ExpertClaimDto> Claims,
    DateTime SubmittedAtUtc,
    DateTime? CompletedAtUtc
);

public record ReviewClaimRequestDto(
    ExpertClaimReviewStatus Decision,
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
    IngestionContentType ContentType,
    int WordCount,
    bool WasTruncated,
    bool IsSuccess,
    string? ErrorMessage = null
);

public record ExtractedClaimCandidate(
    string Topic,
    string? SubTopic,
    string ClaimText,
    string? ContextOrTimestamp,
    bool DirectQuote,
    ClaimNature NatureOfClaim
);

public record ExtractedClaimsResult(
    bool IsSuccess,
    IReadOnlyList<ExtractedClaimCandidate> Claims,
    bool ContainsMedicalContent,
    string? ErrorMessage = null
);

public record ExpertClaimConflictMatch(
    ExtractedClaimCandidate Candidate,
    Guid? SupportingClaimId,
    Guid? ConflictingClaimId
);
