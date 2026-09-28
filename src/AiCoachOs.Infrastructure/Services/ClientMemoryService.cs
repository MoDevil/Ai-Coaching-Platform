using System.Text.Json;
using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Memory.Dtos;
using AiCoachOs.Application.Memory.Engine;
using AiCoachOs.Application.Memory.Interfaces;
using AiCoachOs.Domain.Memory;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Services;

public class ClientMemoryService : IClientMemoryService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentCoachService _currentCoachService;
    private readonly IClientMemoryConflictDetector _conflictDetector;

    public ClientMemoryService(
        IApplicationDbContext dbContext,
        ICurrentCoachService currentCoachService,
        IClientMemoryConflictDetector conflictDetector)
    {
        _dbContext = dbContext;
        _currentCoachService = currentCoachService;
        _conflictDetector = conflictDetector;
    }

    public async Task<IReadOnlyList<ClientMemoryRecordDto>> GetClientMemoriesAsync(
        Guid clientId,
        MemoryCategory? category = null,
        bool includeAnonymized = false,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var query = _dbContext.ClientMemoryRecords
            .Where(m => m.ClientId == clientId && m.CoachId == coachId);

        if (!includeAnonymized)
        {
            query = query.Where(m => m.RecordStatus != MemoryRecordStatus.Anonymized && !m.IsAnonymized);
        }

        if (category.HasValue)
        {
            query = query.Where(m => m.MemoryCategory == category.Value);
        }

        var records = await query
            .OrderByDescending(m => m.RecordedAt)
            .ToListAsync(cancellationToken);

        return records.Select(MapToDto).ToList();
    }

    public async Task<ClientMemoryRecordDto> GetMemoryByIdAsync(
        Guid clientId,
        Guid recordId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var record = await _dbContext.FindClientMemoryRecordByIdAsync(recordId, cancellationToken);
        if (record == null || record.ClientId != clientId || record.CoachId != coachId)
        {
            throw new NotFoundException("ClientMemoryRecord", recordId);
        }

        return MapToDto(record);
    }

    public async Task<IReadOnlyList<ClientMemoryRecordDto>> GetMemoryHistoryAsync(
        Guid clientId,
        Guid recordId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var target = await _dbContext.FindClientMemoryRecordByIdAsync(recordId, cancellationToken);
        if (target == null || target.ClientId != clientId || target.CoachId != coachId)
        {
            throw new NotFoundException("ClientMemoryRecord", recordId);
        }

        // Retrieve all records for this client in the same category
        var allCategoryRecords = await _dbContext.ClientMemoryRecords
            .Where(m => m.ClientId == clientId && m.CoachId == coachId && m.MemoryCategory == target.MemoryCategory)
            .OrderBy(m => m.RecordedAt)
            .ToListAsync(cancellationToken);

        // Build connected supersession history chain
        var chain = new HashSet<Guid> { target.Id };
        bool addedAny;
        do
        {
            addedAny = false;
            foreach (var r in allCategoryRecords)
            {
                if (r.SupersededById.HasValue && chain.Contains(r.SupersededById.Value) && chain.Add(r.Id))
                {
                    addedAny = true;
                }
                if (chain.Contains(r.Id) && r.SupersededById.HasValue && chain.Add(r.SupersededById.Value))
                {
                    addedAny = true;
                }
            }
        } while (addedAny);

        var result = allCategoryRecords
            .Where(r => chain.Contains(r.Id))
            .OrderBy(r => r.RecordedAt)
            .Select(MapToDto)
            .ToList();

        return result;
    }

    public async Task<ClientMemoryRecordDto> CreateMemoryAsync(
        Guid clientId,
        CreateClientMemoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var newRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: request.MemoryCategory,
            sourceType: request.SourceType,
            content: request.Content,
            observedAt: request.ObservedAt,
            sourceReference: request.SourceReference,
            sourceDescription: request.SourceDescription,
            explicitConfidence: request.ExplicitConfidence);

        // Deterministic conflict detection against active memory records
        var activeRecords = await _dbContext.ClientMemoryRecords
            .Where(m => m.ClientId == clientId && m.RecordStatus == MemoryRecordStatus.Active && !m.IsAnonymized)
            .ToListAsync(cancellationToken);

        var conflictResult = _conflictDetector.DetectConflict(newRecord, activeRecords);
        if (conflictResult.HasConflict && conflictResult.ConflictedWithRecord != null)
        {
            var trackedConflicted = await _dbContext.FindClientMemoryRecordByIdAsync(conflictResult.ConflictedWithRecord.Id, cancellationToken);
            if (trackedConflicted != null)
            {
                trackedConflicted.FlagConflicted(conflictResult.ConflictDescription!);
            }
            newRecord.FlagConflicted(conflictResult.ConflictDescription!);

            var conflict = new ClientMemoryConflict(
                id: Guid.NewGuid(),
                clientId: clientId,
                recordAId: conflictResult.ConflictedWithRecord.Id,
                recordBId: newRecord.Id,
                conflictDescription: conflictResult.ConflictDescription!,
                isAutoDetected: true);

            await _dbContext.AddClientMemoryConflictAsync(conflict, cancellationToken);
        }

        await _dbContext.AddClientMemoryRecordAsync(newRecord, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(newRecord);
    }

    public async Task<ClientMemoryRecordDto> CorrectMemoryAsync(
        Guid clientId,
        Guid recordId,
        CorrectClientMemoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var oldRecord = await _dbContext.FindClientMemoryRecordByIdAsync(recordId, cancellationToken);
        if (oldRecord == null || oldRecord.ClientId != clientId || oldRecord.CoachId != coachId)
        {
            throw new NotFoundException("ClientMemoryRecord", recordId);
        }

        if (oldRecord.IsConflicted || oldRecord.RecordStatus == MemoryRecordStatus.Conflicted)
        {
            throw new InvalidOperationException("Cannot correct a conflicted memory record. Resolve the conflict first.");
        }

        if (oldRecord.IsAnonymized || oldRecord.RecordStatus == MemoryRecordStatus.Anonymized)
        {
            throw new InvalidOperationException("Cannot correct an anonymized memory record.");
        }

        var newRecord = new ClientMemoryRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            memoryCategory: oldRecord.MemoryCategory,
            sourceType: MemorySourceType.CoachCorrected,
            content: request.Content,
            observedAt: request.ObservedAt ?? oldRecord.ObservedAt,
            sourceReference: request.SourceReference ?? oldRecord.SourceReference,
            sourceDescription: request.SourceDescription ?? oldRecord.SourceDescription,
            explicitConfidence: MemoryConfidenceLevel.Confirmed,
            coachCorrectionNote: request.Reason);

        // Forward supersession pointer: OLD.SupersededById = NEW.Id
        oldRecord.Supersede(newRecord.Id, request.Reason);

        // Conflict check for new corrected record
        var activeRecords = await _dbContext.ClientMemoryRecords
            .Where(m => m.ClientId == clientId && m.Id != oldRecord.Id && m.RecordStatus == MemoryRecordStatus.Active && !m.IsAnonymized)
            .ToListAsync(cancellationToken);

        var conflictResult = _conflictDetector.DetectConflict(newRecord, activeRecords);
        if (conflictResult.HasConflict && conflictResult.ConflictedWithRecord != null)
        {
            var trackedConflicted = await _dbContext.FindClientMemoryRecordByIdAsync(conflictResult.ConflictedWithRecord.Id, cancellationToken);
            if (trackedConflicted != null)
            {
                trackedConflicted.FlagConflicted(conflictResult.ConflictDescription!);
            }
            newRecord.FlagConflicted(conflictResult.ConflictDescription!);

            var conflict = new ClientMemoryConflict(
                id: Guid.NewGuid(),
                clientId: clientId,
                recordAId: conflictResult.ConflictedWithRecord.Id,
                recordBId: newRecord.Id,
                conflictDescription: conflictResult.ConflictDescription!,
                isAutoDetected: true);

            await _dbContext.AddClientMemoryConflictAsync(conflict, cancellationToken);
        }

        await _dbContext.AddClientMemoryRecordAsync(newRecord, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(newRecord);
    }

    public async Task<ClientMemoryRecordDto> ArchiveMemoryAsync(
        Guid clientId,
        Guid recordId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var record = await _dbContext.FindClientMemoryRecordByIdAsync(recordId, cancellationToken);
        if (record == null || record.ClientId != clientId || record.CoachId != coachId)
        {
            throw new NotFoundException("ClientMemoryRecord", recordId);
        }

        record.Archive();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(record);
    }

    public async Task<ClientMemoryRecordDto> FlagUncertainAsync(
        Guid clientId,
        Guid recordId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var record = await _dbContext.FindClientMemoryRecordByIdAsync(recordId, cancellationToken);
        if (record == null || record.ClientId != clientId || record.CoachId != coachId)
        {
            throw new NotFoundException("ClientMemoryRecord", recordId);
        }

        record.FlagUncertain();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(record);
    }

    public async Task<IReadOnlyList<ClientMemoryConflictDto>> GetConflictsAsync(
        Guid clientId,
        bool unresolvedOnly = false,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var query = _dbContext.ClientMemoryConflicts
            .Include(c => c.RecordA)
            .Include(c => c.RecordB)
            .Where(c => c.ClientId == clientId);

        if (unresolvedOnly)
        {
            query = query.Where(c => !c.IsResolved);
        }

        var conflicts = await query
            .OrderByDescending(c => c.DetectedAtUtc)
            .ToListAsync(cancellationToken);

        return conflicts.Select(MapConflictToDto).ToList();
    }

    public async Task<ClientMemoryConflictDto> CreateConflictAsync(
        Guid clientId,
        CreateClientMemoryConflictRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var recordA = await _dbContext.FindClientMemoryRecordByIdAsync(request.RecordAId, cancellationToken);
        var recordB = await _dbContext.FindClientMemoryRecordByIdAsync(request.RecordBId, cancellationToken);

        if (recordA == null || recordA.ClientId != clientId || recordA.CoachId != coachId)
            throw new NotFoundException("ClientMemoryRecord (RecordA)", request.RecordAId);
        if (recordB == null || recordB.ClientId != clientId || recordB.CoachId != coachId)
            throw new NotFoundException("ClientMemoryRecord (RecordB)", request.RecordBId);

        if (recordA.Id == recordB.Id)
            throw new InvalidOperationException("Cannot create a conflict between a record and itself.");

        recordA.FlagConflicted(request.ConflictDescription);
        recordB.FlagConflicted(request.ConflictDescription);

        var conflict = new ClientMemoryConflict(
            id: Guid.NewGuid(),
            clientId: clientId,
            recordAId: recordA.Id,
            recordBId: recordB.Id,
            conflictDescription: request.ConflictDescription,
            isAutoDetected: false);

        await _dbContext.AddClientMemoryConflictAsync(conflict, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapConflictToDto(conflict);
    }

    public async Task<ClientMemoryConflictDto> ResolveConflictAsync(
        Guid clientId,
        Guid conflictId,
        ResolveClientMemoryConflictRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var conflict = await _dbContext.FindClientMemoryConflictByIdAsync(conflictId, cancellationToken);
        if (conflict == null || conflict.ClientId != clientId)
        {
            throw new NotFoundException("ClientMemoryConflict", conflictId);
        }

        if (conflict.IsResolved)
        {
            throw new InvalidOperationException("Conflict is already resolved.");
        }

        var recordA = conflict.RecordA ?? await _dbContext.FindClientMemoryRecordByIdAsync(conflict.RecordAId, cancellationToken);
        var recordB = conflict.RecordB ?? await _dbContext.FindClientMemoryRecordByIdAsync(conflict.RecordBId, cancellationToken);

        if (recordA == null || recordB == null)
        {
            throw new InvalidOperationException("Involved conflict records could not be found.");
        }

        if (request.WinningRecordId != recordA.Id && request.WinningRecordId != recordB.Id)
        {
            throw new ArgumentException("WinningRecordId must match either RecordA or RecordB of the conflict.", nameof(request.WinningRecordId));
        }

        var winningRecord = request.WinningRecordId == recordA.Id ? recordA : recordB;
        var losingRecord = request.WinningRecordId == recordA.Id ? recordB : recordA;

        // Winning record: IsConflicted = false, Status = Active, Confidence restored to pre-conflict value
        winningRecord.ResolveConflict();

        // Losing record: Status = Superseded, Confidence = Superseded, SupersededById = winningRecord.Id, SupersessionReason = resolutionNote
        losingRecord.Supersede(winningRecord.Id, request.ResolutionNote);

        conflict.Resolve(coachId, request.ResolutionNote, winningRecordId: winningRecord.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapConflictToDto(conflict);
    }

    public async Task<ClientMemorySnapshotDto?> GetLatestSnapshotAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var snapshot = await _dbContext.ClientMemorySnapshots
            .Where(s => s.ClientId == clientId && s.CoachId == coachId)
            .OrderByDescending(s => s.GeneratedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return snapshot == null ? null : MapSnapshotToDto(snapshot);
    }

    public async Task<ClientMemorySnapshotDto> GenerateSnapshotAsync(
        Guid clientId,
        GenerateClientMemorySnapshotRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        // Fetch active, non-anonymized records (deterministic snapshot retrieval)
        var activeRecords = await _dbContext.ClientMemoryRecords
            .Where(m => m.ClientId == clientId && m.CoachId == coachId && m.RecordStatus == MemoryRecordStatus.Active && !m.IsAnonymized)
            .OrderBy(m => m.MemoryCategory)
            .ThenByDescending(m => m.RecordedAt)
            .ToListAsync(cancellationToken);

        var unresolvedConflicts = await _dbContext.ClientMemoryConflicts
            .Where(c => c.ClientId == clientId && !c.IsResolved)
            .ToListAsync(cancellationToken);

        // Build deterministic structured view
        var snapshotData = new
        {
            ClientId = clientId,
            GeneratedAtUtc = DateTime.UtcNow,
            Trigger = request.Trigger.ToString(),
            ActiveRecordCount = activeRecords.Count,
            Categories = activeRecords
                .GroupBy(r => r.MemoryCategory.ToString())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => new
                    {
                        r.Id,
                        r.RecordedAt,
                        r.ObservedAt,
                        r.ConfidenceLevel,
                        r.Content,
                        r.SourceType
                    }).ToList())
        };

        var snapshotJson = JsonSerializer.Serialize(snapshotData);

        // Mark prior snapshots stale
        var priorSnapshotIds = await _dbContext.ClientMemorySnapshots
            .Where(s => s.ClientId == clientId && !s.IsStale)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        foreach (var priorId in priorSnapshotIds)
        {
            var trackedSnapshot = await _dbContext.FindClientMemorySnapshotByIdAsync(priorId, cancellationToken);
            if (trackedSnapshot != null && !trackedSnapshot.IsStale)
            {
                trackedSnapshot.MarkStale();
            }
        }

        var snapshot = new ClientMemorySnapshot(
            id: Guid.NewGuid(),
            clientId: clientId,
            coachId: coachId,
            generationTrigger: request.Trigger,
            snapshotContentJson: snapshotJson,
            includedRecordIds: activeRecords.Select(r => r.Id),
            excludedConflictIds: unresolvedConflicts.Select(c => c.Id));

        await _dbContext.AddClientMemorySnapshotAsync(snapshot, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapSnapshotToDto(snapshot);
    }

    public async Task<IReadOnlyList<UnresolvedQuestionDto>> GetUnresolvedQuestionsAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var unresolvedQuestions = await _dbContext.ClientMemoryRecords
            .Where(m => m.ClientId == clientId && m.CoachId == coachId &&
                        !m.IsAnonymized &&
                        m.RecordStatus != MemoryRecordStatus.Anonymized &&
                        m.MemoryCategory == MemoryCategory.UnresolvedQuestion)
            .OrderByDescending(m => m.RecordedAt)
            .ToListAsync(cancellationToken);

        return unresolvedQuestions.Select(r => new UnresolvedQuestionDto
        {
            RecordId = r.Id,
            Category = r.MemoryCategory,
            Content = r.Content,
            ConfidenceLevel = r.ConfidenceLevel,
            RecordStatus = r.RecordStatus,
            QuestionContext = "Unresolved question requiring client/coach follow-up",
            RecordedAt = r.RecordedAt
        }).ToList();
    }

    public async Task<AnonymizeClientMemoryResultDto> AnonymizeClientMemoriesAsync(
        Guid clientId,
        AnonymizeClientMemoryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var recordIds = await _dbContext.ClientMemoryRecords
            .Where(m => m.ClientId == clientId && m.CoachId == coachId && !m.IsAnonymized)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        int count = 0;
        foreach (var id in recordIds)
        {
            var tracked = await _dbContext.FindClientMemoryRecordByIdAsync(id, cancellationToken);
            if (tracked != null && !tracked.IsAnonymized)
            {
                tracked.Anonymize();
                count++;
            }
        }

        var log = new ClientAnonymizationLog(
            id: Guid.NewGuid(),
            clientId: clientId,
            requestedByCoachId: coachId,
            recordsAnonymized: count,
            anonymizationReason: request.AnonymizationReason);

        await _dbContext.AddClientAnonymizationLogAsync(log, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AnonymizeClientMemoryResultDto
        {
            ClientId = clientId,
            RecordsAnonymized = count,
            AnonymizedAtUtc = log.AnonymizedAt
        };
    }

    public async Task<IReadOnlyList<AIRecommendationRecordDto>> GetAIRecommendationsAsync(
        Guid clientId,
        AIRecommendationCategory? category = null,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var query = _dbContext.AIRecommendationRecords
            .Where(r => r.ClientId == clientId && r.CoachId == coachId);

        if (category.HasValue)
        {
            query = query.Where(r => r.RecommendationCategory == category.Value);
        }

        var list = await query
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync(cancellationToken);

        return list.Select(MapRecommendationToDto).ToList();
    }

    public async Task<AIRecommendationRecordDto> DecideAIRecommendationAsync(
        Guid clientId,
        Guid recommendationId,
        DecideAIRecommendationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var coachId = await _currentCoachService.GetRequiredCoachIdAsync(cancellationToken);
        await EnsureClientAccessAsync(clientId, coachId, cancellationToken);

        var rec = await _dbContext.FindAIRecommendationRecordByIdAsync(recommendationId, cancellationToken);
        if (rec == null || rec.ClientId != clientId || rec.CoachId != coachId)
        {
            throw new NotFoundException("AIRecommendationRecord", recommendationId);
        }

        rec.RecordDecision(request.Decision, request.DecisionNote, request.FinalImplementedPlan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapRecommendationToDto(rec);
    }

    private async Task EnsureClientAccessAsync(Guid clientId, Guid coachId, CancellationToken cancellationToken)
    {
        var client = await _dbContext.FindClientByIdAsync(clientId, cancellationToken);
        if (client == null || client.CoachId != coachId)
        {
            // Strictly do not leak existence of another coach's client
            throw new NotFoundException("Client", clientId);
        }
    }

    private static ClientMemoryRecordDto MapToDto(ClientMemoryRecord r)
    {
        return new ClientMemoryRecordDto
        {
            Id = r.Id,
            ClientId = r.ClientId,
            CoachId = r.CoachId,
            MemoryCategory = r.MemoryCategory,
            RecordedAt = r.RecordedAt,
            ObservedAt = r.ObservedAt,
            SourceType = r.SourceType,
            SourceReference = r.SourceReference,
            SourceDescription = r.SourceDescription,
            ConfidenceLevel = r.ConfidenceLevel,
            RecordStatus = r.RecordStatus,
            Content = r.Content,
            SupersededById = r.SupersededById,
            SupersededAt = r.SupersededAt,
            SupersessionReason = r.SupersessionReason,
            IsConflicted = r.IsConflicted,
            ConflictNotes = r.ConflictNotes,
            CoachCorrectionNote = r.CoachCorrectionNote,
            CorrectedAt = r.CorrectedAt,
            IsAnonymized = r.IsAnonymized,
            AnonymizedAt = r.AnonymizedAt,
            CreatedAtUtc = r.CreatedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc
        };
    }

    private static ClientMemoryConflictDto MapConflictToDto(ClientMemoryConflict c)
    {
        return new ClientMemoryConflictDto
        {
            Id = c.Id,
            ClientId = c.ClientId,
            RecordAId = c.RecordAId,
            RecordA = c.RecordA != null ? MapToDto(c.RecordA) : null,
            RecordBId = c.RecordBId,
            RecordB = c.RecordB != null ? MapToDto(c.RecordB) : null,
            ConflictDescription = c.ConflictDescription,
            DetectedAtUtc = c.DetectedAtUtc,
            IsAutoDetected = c.IsAutoDetected,
            IsResolved = c.IsResolved,
            ResolvedAtUtc = c.ResolvedAtUtc,
            ResolvedByCoachId = c.ResolvedByCoachId,
            ResolutionNote = c.ResolutionNote,
            WinningRecordId = c.WinningRecordId
        };
    }

    private static ClientMemorySnapshotDto MapSnapshotToDto(ClientMemorySnapshot s)
    {
        return new ClientMemorySnapshotDto
        {
            Id = s.Id,
            ClientId = s.ClientId,
            CoachId = s.CoachId,
            GeneratedAtUtc = s.GeneratedAtUtc,
            GenerationTrigger = s.GenerationTrigger,
            SnapshotContentJson = s.SnapshotContentJson,
            IsStale = s.IsStale,
            IncludedRecordIds = s.IncludedRecordIds.ToList(),
            ExcludedConflictIds = s.ExcludedConflictIds.ToList()
        };
    }

    private static AIRecommendationRecordDto MapRecommendationToDto(AIRecommendationRecord r)
    {
        return new AIRecommendationRecordDto
        {
            Id = r.Id,
            ClientId = r.ClientId,
            CoachId = r.CoachId,
            RecommendationCategory = r.RecommendationCategory,
            RecommendationText = r.RecommendationText,
            RationaleText = r.RationaleText,
            ConfidenceStatement = r.ConfidenceStatement,
            GeneratedAt = r.GeneratedAt,
            AIProvider = r.AIProvider,
            AIModel = r.AIModel,
            ReviewStatus = r.ReviewStatus,
            CoachDecision = r.CoachDecision,
            CoachDecisionNote = r.CoachDecisionNote,
            CoachDecisionAt = r.CoachDecisionAt,
            FinalImplementedPlan = r.FinalImplementedPlan,
            LinkedMemoryRecordId = r.LinkedMemoryRecordId,
            KnowledgeClaimRefs = r.KnowledgeClaimRefs.ToList()
        };
    }
}
