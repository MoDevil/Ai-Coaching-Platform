using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Application.Knowledge.DTOs;

public record KnowledgeSourceSummaryDto(
    Guid Id,
    KnowledgeSourceType SourceType,
    string Title,
    string Authors,
    int Year,
    EvidenceLevel EvidenceLevel,
    string? Doi
);

public record KnowledgeSourceDto(
    Guid Id,
    KnowledgeSourceType SourceType,
    string Title,
    string Authors,
    int Year,
    EvidenceLevel EvidenceLevel,
    string? Doi,
    string? Url,
    string? Notes,
    DateTime CreatedAtUtc
);

public record CreateKnowledgeSourceDto(
    KnowledgeSourceType SourceType,
    string Title,
    string Authors,
    int Year,
    EvidenceLevel EvidenceLevel,
    string? Doi = null,
    string? Url = null,
    string? Notes = null
);

public record KnowledgeClaimSourceDto(
    Guid SourceId,
    KnowledgeSourceType SourceType,
    string Title,
    string Authors,
    int Year,
    EvidenceLevel EvidenceLevel,
    string? Doi,
    string? RelevanceNote
);

public record KnowledgeClaimSummaryDto(
    Guid Id,
    string Topic,
    string Question,
    string ClaimText,
    EvidenceLevel EvidenceLevel,
    ClaimStatus Status,
    Guid? ExerciseId,
    string? ExerciseName,
    int SupportingSourceCount,
    DateTime CreatedAtUtc
);

public record KnowledgeClaimDto(
    Guid Id,
    string Topic,
    string Question,
    string ClaimText,
    EvidenceLevel EvidenceLevel,
    ClaimStatus Status,
    string? Population,
    string? Limitations,
    string? PracticalApplication,
    Guid? ExerciseId,
    string? ExerciseName,
    DateTime? ReviewedAtUtc,
    string? ReviewedBy,
    Guid? SupersededByClaimId,
    DateTime? SupersededAtUtc,
    string? SupersessionReason,
    IReadOnlyList<KnowledgeClaimSourceDto> Sources,
    DateTime CreatedAtUtc
);

public record CreateKnowledgeClaimDto(
    string Topic,
    string Question,
    string ClaimText,
    EvidenceLevel EvidenceLevel,
    ClaimStatus Status = ClaimStatus.Active,
    Guid? ExerciseId = null,
    string? Population = null,
    string? Limitations = null,
    string? PracticalApplication = null,
    IReadOnlyList<Guid>? InitialSourceIds = null
);

public record AddClaimSourceDto(
    Guid SourceId,
    string? RelevanceNote = null
);

public record SupersedeClaimDto(
    Guid ReplacementClaimId,
    string Reason
);

public record KnowledgeFilterDto(
    string? Search = null,
    string? Topic = null,
    Guid? ExerciseId = null,
    ClaimStatus? Status = null,
    EvidenceLevel? MinEvidenceLevel = null
);
