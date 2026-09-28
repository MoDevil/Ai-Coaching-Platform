using System.Text.Json;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.Memory.Dtos;
using AiCoachOs.Application.Memory.Interfaces;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Memory;
using AiCoachOs.Domain.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Ai;

public class AiReasoningService : IAiReasoningService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAiProvider _aiProvider;
    private readonly IClientMemoryService _memoryService;
    private readonly ILogger<AiReasoningService> _logger;

    public AiReasoningService(
        IApplicationDbContext dbContext,
        IAiProvider aiProvider,
        IClientMemoryService memoryService,
        ILogger<AiReasoningService> logger)
    {
        _dbContext = dbContext;
        _aiProvider = aiProvider;
        _memoryService = memoryService;
        _logger = logger;
    }

    public async Task<AIRecommendationRecordDto> GenerateReasoningAsync(
        Guid coachId, 
        GenerateReasoningRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!Enum.IsDefined(typeof(AIRecommendationCategory), request.Category))
        {
            throw new ArgumentException($"Invalid AI recommendation category: {request.Category}", nameof(request));
        }

        var client = await _dbContext.Clients
            .FirstOrDefaultAsync(c => c.Id == request.ClientId, cancellationToken);

        if (client == null)
        {
            throw new KeyNotFoundException($"Client {request.ClientId} not found.");
        }

        if (client.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach does not own this client record.");
        }

        // 1. Deterministic Safety Gate Check
        var safetyScreenings = await _dbContext.SafetyScreenings
            .Where(s => s.ClientId == client.Id)
            .OrderByDescending(s => s.GeneratedAtUtc)
            .Take(5)
            .ToListAsync(cancellationToken);

        var hasActiveMedicalEscalation = safetyScreenings.Any(s =>
            s.ScreeningResult == SafetyCategory.ReferToHealthcareProfessional ||
            s.ScreeningResult == SafetyCategory.UrgentMedicalAttention ||
            s.RecommendedAction == SafetyActionType.ReferToHealthcareProfessional ||
            s.RecommendedAction == SafetyActionType.UrgentMedicalAttention);

        // Optional training profile for richer deterministic context
        var trainingProfile = await _dbContext.ClientTrainingProfiles
            .FirstOrDefaultAsync(tp => tp.ClientId == client.Id, cancellationToken);

        // 2. Fetch or Generate Deterministic Memory Snapshot
        var snapshotDto = await _memoryService.GetLatestSnapshotAsync(client.Id, cancellationToken);
        if (snapshotDto == null)
        {
            snapshotDto = await _memoryService.GenerateSnapshotAsync(client.Id, new GenerateClientMemorySnapshotRequestDto
            {
                Trigger = SnapshotGenerationTrigger.Manual
            }, cancellationToken);
        }

        // 3. Query Active, Eligible Knowledge Claims
        var eligibleClaims = await _dbContext.KnowledgeClaims
            .Where(k => k.Status == ClaimStatus.Active)
            .Take(20)
            .ToListAsync(cancellationToken);

        var eligibleClaimIds = eligibleClaims.Select(c => c.Id).ToHashSet();

        // 4. Assemble Prompts
        var systemPrompt = BuildSystemPrompt(hasActiveMedicalEscalation);
        var userPrompt = BuildUserPrompt(client, trainingProfile, request, snapshotDto, eligibleClaims, hasActiveMedicalEscalation);

        // 5. Invoke AI Provider
        var completionRequest = new AiCompletionRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt
        };

        var aiResponse = await _aiProvider.GenerateCompletionAsync(completionRequest, cancellationToken);

        if (!aiResponse.IsSuccess)
        {
            _logger.LogWarning("AI Provider failed: {ErrorMessage}. Applying deterministic fallback recommendation.", aiResponse.ErrorMessage);
            aiResponse = BuildDeterministicFallback(request.Category, hasActiveMedicalEscalation, eligibleClaims);
        }

        // 6. Validate Evidence References against Eligible Claims
        var validatedClaimRefs = aiResponse.EvidenceClaimRefs
            .Where(id => eligibleClaimIds.Contains(id))
            .ToList();

        // If no valid claims were cited by the model, provide the top matching eligible claim as baseline evidence
        if (validatedClaimRefs.Count == 0 && eligibleClaims.Count > 0)
        {
            validatedClaimRefs.Add(eligibleClaims[0].Id);
        }

        // 7. Enforce Safety Disclaimer if Medical Escalation Active
        var finalRecommendation = aiResponse.RecommendationText;
        if (hasActiveMedicalEscalation && !finalRecommendation.Contains("medical evaluation", StringComparison.OrdinalIgnoreCase))
        {
            finalRecommendation = $"[SAFETY ESCALATION] Client has active safety flags. Recommend medical clearance prior to progressing. {finalRecommendation}";
        }

        // 8. Persist AIRecommendationRecord in PendingReview Status
        var recommendationRecord = new AIRecommendationRecord(
            id: Guid.NewGuid(),
            clientId: client.Id,
            coachId: coachId,
            recommendationCategory: request.Category,
            recommendationText: finalRecommendation,
            rationaleText: aiResponse.RationaleText,
            confidenceStatement: aiResponse.ConfidenceStatement,
            aiProvider: aiResponse.ProviderName,
            aiModel: aiResponse.ModelName,
            knowledgeClaimRefs: validatedClaimRefs,
            generatedAt: DateTime.UtcNow);

        await _dbContext.AddAIRecommendationRecordAsync(recommendationRecord, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(recommendationRecord);
    }

    public async Task<AIRecommendationRecordDto> GetReasoningByIdAsync(
        Guid coachId, 
        Guid recommendationId, 
        CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.FindAIRecommendationRecordByIdAsync(recommendationId, cancellationToken);
        if (record == null)
        {
            throw new KeyNotFoundException($"Recommendation record {recommendationId} not found.");
        }

        if (record.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach does not own this recommendation record.");
        }

        return MapToDto(record);
    }

    private static string BuildSystemPrompt(bool hasActiveMedicalEscalation)
    {
        return $$"""
You are the evidence-based AI Coaching Reasoning Engine for AI Coach OS.
Your objective is to provide structured, multidisciplinary coaching decision support for human gym coaches.

CORE RULES:
1. Always output valid JSON conforming to the schema:
{
  "recommendation": "string (clear, actionable coaching recommendation)",
  "rationale": "string (physiological, biomechanical, or behavioral reasoning)",
  "confidence_statement": "string (explicit statement of certainty and key limitations)",
  "evidence_claim_ids": ["string GUIDs of relevant claims provided in context"]
}
2. Never diagnose medical conditions or prescribe medications/PEDs.
3. Only cite evidence claim IDs provided in the scientific knowledge context.
{{(hasActiveMedicalEscalation ? "4. CRITICAL: Client has active safety flags. Emphasize conservative modification and recommend formal medical evaluation." : "")}}
""";
    }

    private static string BuildUserPrompt(
        Domain.Clients.Client client,
        Domain.TrainingProfiles.ClientTrainingProfile? trainingProfile,
        GenerateReasoningRequestDto request,
        ClientMemorySnapshotDto snapshot,
        List<KnowledgeClaim> eligibleClaims,
        bool hasActiveMedicalEscalation)
    {
        var claimsSummary = eligibleClaims.Select(c => new
        {
            c.Id,
            c.Topic,
            c.ClaimText,
            EvidenceLevel = c.EvidenceLevel.ToString()
        });

        int? age = null;
        if (client.DateOfBirth.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            var dob = client.DateOfBirth.Value.Date;
            age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age.Value)) age--;
        }

        var payload = new
        {
            ClientContext = new
            {
                client.Id,
                client.FirstName,
                client.LastName,
                Age = age,
                Gender = client.Gender?.ToString() ?? "NotSpecified",
                TrainingExperience = trainingProfile?.ExperienceLevel.ToString() ?? "Intermediate",
                Goal = client.Goal?.ToString() ?? "GeneralFitness",
                AvailableSessionsPerWeek = trainingProfile?.WeeklyAvailability?.SessionsPerWeek
            },
            Category = request.Category.ToString(),
            GuidanceNote = request.GuidanceNote,
            HasActiveMedicalEscalation = hasActiveMedicalEscalation,
            MemorySnapshot = snapshot.SnapshotContentJson,
            EligibleKnowledgeClaims = claimsSummary
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    private static AiCompletionResponse BuildDeterministicFallback(
        AIRecommendationCategory category,
        bool hasActiveMedicalEscalation,
        List<KnowledgeClaim> eligibleClaims)
    {
        var citedGuids = eligibleClaims.Take(1).Select(c => c.Id).ToList();

        var recText = category switch
        {
            AIRecommendationCategory.ProgramDesign => "Structure program with primary movement patterns balanced across available training days.",
            AIRecommendationCategory.ExerciseSelection => "Select exercises providing stable resistance profile matching client joint comfort.",
            AIRecommendationCategory.VolumeAdjustment => "Maintain current weekly set volume within recovery threshold.",
            AIRecommendationCategory.NutritionTarget => "Calibrate caloric intake with adequate protein distribution across daily meals.",
            AIRecommendationCategory.RecoveryStrategy => "Prioritize sleep hygiene and stress management to match training stimulus.",
            _ => "Review client progress and adjust training variables progressively."
        };

        if (hasActiveMedicalEscalation)
        {
            recText = $"[SAFETY NOTICE] Conservative modification advised. Clinical evaluation recommended. {recText}";
        }

        return new AiCompletionResponse
        {
            IsSuccess = true,
            ProviderName = "DeterministicEngine",
            ModelName = "deterministic-fallback-v1",
            TokensUsed = 100,
            RecommendationText = recText,
            RationaleText = "Deterministic evidence-backed baseline applied based on client constraints and recovery boundaries.",
            ConfidenceStatement = "Baseline confidence based on deterministic rules and structured knowledge claims.",
            EvidenceClaimRefs = citedGuids
        };
    }

    private static AIRecommendationRecordDto MapToDto(AIRecommendationRecord r)
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
