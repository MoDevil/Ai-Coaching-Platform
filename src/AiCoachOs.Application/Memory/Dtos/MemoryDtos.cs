using AiCoachOs.Domain.Memory;

namespace AiCoachOs.Application.Memory.Dtos;

public class ClientMemoryRecordDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid CoachId { get; set; }
    public MemoryCategory MemoryCategory { get; set; }
    public DateTime RecordedAt { get; set; }
    public DateTime? ObservedAt { get; set; }
    public MemorySourceType SourceType { get; set; }
    public string? SourceReference { get; set; }
    public string? SourceDescription { get; set; }
    public MemoryConfidenceLevel ConfidenceLevel { get; set; }
    public MemoryRecordStatus RecordStatus { get; set; }
    public string Content { get; set; } = string.Empty;

    public Guid? SupersededById { get; set; }
    public DateTime? SupersededAt { get; set; }
    public string? SupersessionReason { get; set; }

    public bool IsConflicted { get; set; }
    public string? ConflictNotes { get; set; }
    public string? CoachCorrectionNote { get; set; }
    public DateTime? CorrectedAt { get; set; }

    public bool IsAnonymized { get; set; }
    public DateTime? AnonymizedAt { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class CreateClientMemoryRequestDto
{
    public MemoryCategory MemoryCategory { get; set; }
    public MemorySourceType SourceType { get; set; } = MemorySourceType.CoachRecorded;
    public string Content { get; set; } = string.Empty;
    public DateTime? ObservedAt { get; set; }
    public string? SourceReference { get; set; }
    public string? SourceDescription { get; set; }
    public MemoryConfidenceLevel? ExplicitConfidence { get; set; }
}

public class CorrectClientMemoryRequestDto
{
    public string Content { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime? ObservedAt { get; set; }
    public string? SourceReference { get; set; }
    public string? SourceDescription { get; set; }
}

public class ClientMemoryConflictDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid RecordAId { get; set; }
    public ClientMemoryRecordDto? RecordA { get; set; }
    public Guid RecordBId { get; set; }
    public ClientMemoryRecordDto? RecordB { get; set; }
    public string ConflictDescription { get; set; } = string.Empty;
    public DateTime DetectedAtUtc { get; set; }
    public bool IsAutoDetected { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public Guid? ResolvedByCoachId { get; set; }
    public string? ResolutionNote { get; set; }
    public Guid? WinningRecordId { get; set; }
}

public class CreateClientMemoryConflictRequestDto
{
    public Guid RecordAId { get; set; }
    public Guid RecordBId { get; set; }
    public string ConflictDescription { get; set; } = string.Empty;
}

public class ResolveClientMemoryConflictRequestDto
{
    public ConflictResolutionAction Action { get; set; }
    public string ResolutionNote { get; set; } = string.Empty;
    public Guid? AuthoritativeRecordId { get; set; }
    public string? NewCorrectedContent { get; set; }
}

public class ClientMemorySnapshotDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid CoachId { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public SnapshotGenerationTrigger GenerationTrigger { get; set; }
    public string SnapshotContentJson { get; set; } = string.Empty;
    public bool IsStale { get; set; }
    public IReadOnlyList<Guid> IncludedRecordIds { get; set; } = new List<Guid>();
    public IReadOnlyList<Guid> ExcludedConflictIds { get; set; } = new List<Guid>();
}

public class GenerateClientMemorySnapshotRequestDto
{
    public SnapshotGenerationTrigger Trigger { get; set; } = SnapshotGenerationTrigger.Manual;
}

public class AnonymizeClientMemoryRequestDto
{
    public string? AnonymizationReason { get; set; }
}

public class AnonymizeClientMemoryResultDto
{
    public Guid ClientId { get; set; }
    public int RecordsAnonymized { get; set; }
    public DateTime AnonymizedAtUtc { get; set; }
}

public class AIRecommendationRecordDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid CoachId { get; set; }
    public AIRecommendationCategory RecommendationCategory { get; set; }
    public string RecommendationText { get; set; } = string.Empty;
    public string RationaleText { get; set; } = string.Empty;
    public string ConfidenceStatement { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string AIProvider { get; set; } = string.Empty;
    public string AIModel { get; set; } = string.Empty;
    public AIRecommendationReviewStatus ReviewStatus { get; set; }
    public CoachDecisionOutcome? CoachDecision { get; set; }
    public string? CoachDecisionNote { get; set; }
    public DateTime? CoachDecisionAt { get; set; }
    public string? FinalImplementedPlan { get; set; }
    public Guid? LinkedMemoryRecordId { get; set; }
    public IReadOnlyList<Guid> KnowledgeClaimRefs { get; set; } = new List<Guid>();
}

public class DecideAIRecommendationRequestDto
{
    public CoachDecisionOutcome Decision { get; set; }
    public string? DecisionNote { get; set; }
    public string? FinalImplementedPlan { get; set; }
}

public class UnresolvedQuestionDto
{
    public Guid RecordId { get; set; }
    public MemoryCategory Category { get; set; }
    public string Content { get; set; } = string.Empty;
    public MemoryConfidenceLevel ConfidenceLevel { get; set; }
    public MemoryRecordStatus RecordStatus { get; set; }
    public string QuestionContext { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}
