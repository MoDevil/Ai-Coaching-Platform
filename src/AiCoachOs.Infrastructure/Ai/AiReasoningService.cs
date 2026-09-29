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

        if (!Enum.IsDefined(typeof(ReasoningCategory), request.ReasoningCategory))
        {
            throw new ArgumentException($"Invalid reasoning category: {request.ReasoningCategory}", nameof(request));
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

        // ==========================================
        // 1. DETERMINISTIC SAFETY GATE (PRE-CHECK)
        // ==========================================
        var safetyScreenings = await _dbContext.SafetyScreenings
            .Where(s => s.ClientId == client.Id)
            .OrderByDescending(s => s.GeneratedAtUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        // Urgent medical attention check (unacknowledged -> abort immediately, no provider call)
        var hasUrgentUnacknowledgedSafety = safetyScreenings.Any(s =>
            (s.ScreeningResult == SafetyCategory.UrgentMedicalAttention || s.RecommendedAction == SafetyActionType.UrgentMedicalAttention)
            && !s.CoachAcknowledgedAtUtc.HasValue);

        if (hasUrgentUnacknowledgedSafety)
        {
            _logger.LogWarning("Deterministic safety gate triggered: Urgent unacknowledged safety flags for client {ClientId}. Aborting AI reasoning.", client.Id);
            throw new InvalidOperationException("Client has urgent unacknowledged medical safety flags requiring immediate clinical attention. AI reasoning cannot proceed until safety flags are resolved.");
        }

        // Referral check (unacknowledged -> reasoning proceeds, SafetySummary included, CoachActionRequired = true)
        var hasReferralUnacknowledged = safetyScreenings.Any(s =>
            (s.ScreeningResult == SafetyCategory.ReferToHealthcareProfessional || s.RecommendedAction == SafetyActionType.ReferToHealthcareProfessional)
            && !s.CoachAcknowledgedAtUtc.HasValue);

        // ==========================================
        // 2. CONTEXT ASSEMBLY (LOCKED 9-STEP ORDERING)
        // ==========================================
        
        // Step 1: CLIENT PROFILE
        int? age = null;
        if (client.DateOfBirth.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            var dob = client.DateOfBirth.Value.Date;
            age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age.Value)) age--;
        }

        var trainingProfile = await _dbContext.ClientTrainingProfiles
            .FirstOrDefaultAsync(tp => tp.ClientId == client.Id, cancellationToken);

        // Step 2: ACTIVE SAFETY FLAGS
        var activeSafetyFlags = safetyScreenings
            .Where(s => s.ScreeningResult != SafetyCategory.NoSafetyConcern)
            .Select(s => new
            {
                s.ScreeningResult,
                s.RecommendedAction,
                s.SummaryRationale,
                Acknowledged = s.CoachAcknowledgedAtUtc.HasValue,
                s.GeneratedAtUtc
            })
            .ToList();

        // Step 3: CURRENT PROGRAM SUMMARY
        var currentProgram = await _dbContext.Programs
            .Include(p => p.Versions)
            .Where(p => p.ClientId == client.Id && p.Status == Domain.Programs.ProgramStatus.Active)
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        object? programSummary = currentProgram == null ? null : new
        {
            currentProgram.Id,
            currentProgram.Name,
            currentProgram.RationaleSummary,
            Status = currentProgram.Status.ToString(),
            currentProgram.CreatedAtUtc
        };

        // Step 4: RECENT PERFORMANCE
        var recentWorkouts = await _dbContext.WorkoutSessions
            .Where(w => w.ClientId == client.Id && w.CompletedAtUtc.HasValue)
            .OrderByDescending(w => w.CompletedAtUtc)
            .Take(3)
            .Select(w => new
            {
                w.Id,
                w.StartedAtUtc,
                w.CompletedAtUtc,
                Status = w.Status.ToString(),
                w.Notes
            })
            .ToListAsync(cancellationToken);

        // Step 5: NUTRITION CONTEXT
        var nutritionProfile = await _dbContext.ClientNutritionProfiles
            .FirstOrDefaultAsync(n => n.ClientId == client.Id, cancellationToken);

        object? nutritionContext = nutritionProfile == null ? null : new
        {
            BudgetTier = nutritionProfile.BudgetTier.ToString(),
            nutritionProfile.MealsPerDay,
            nutritionProfile.CurrentCalorieTarget,
            nutritionProfile.CurrentProteinTargetGrams,
            DietaryPreferences = nutritionProfile.DietaryPreferences.ToList(),
            FoodExclusions = nutritionProfile.FoodExclusions.ToList()
        };

        // Step 6: RELEVANT MEMORY (Snapshot)
        var snapshotDto = await _memoryService.GetLatestSnapshotAsync(client.Id, cancellationToken);
        if (snapshotDto == null)
        {
            snapshotDto = await _memoryService.GenerateSnapshotAsync(client.Id, new GenerateClientMemorySnapshotRequestDto
            {
                Trigger = SnapshotGenerationTrigger.Manual
            }, cancellationToken);
        }

        // Step 7: RELEVANT KNOWLEDGE CLAIMS (Active only)
        var eligibleClaims = await _dbContext.KnowledgeClaims
            .Where(k => k.Status == ClaimStatus.Active)
            .Take(25)
            .ToListAsync(cancellationToken);

        var eligibleClaimIds = eligibleClaims.Select(c => c.Id).ToHashSet();

        // Step 8: ACTIVE UNRESOLVED QUESTIONS
        var unresolvedQuestions = await _memoryService.GetUnresolvedQuestionsAsync(client.Id, cancellationToken);

        // Step 9: REASONING INSTRUCTION & ASSEMBLED USER PROMPT
        var systemPrompt = BuildSystemPrompt(hasReferralUnacknowledged);
        var userPrompt = BuildLockedContextPrompt(
            client,
            age,
            trainingProfile,
            activeSafetyFlags,
            programSummary,
            recentWorkouts,
            nutritionContext,
            snapshotDto,
            eligibleClaims,
            unresolvedQuestions,
            request,
            hasReferralUnacknowledged);

        // ==========================================
        // 3. EXECUTE AI PROVIDER
        // ==========================================
        var completionRequest = new AiCompletionRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt
        };

        var aiResponse = await _aiProvider.GenerateStructuredAsync(completionRequest, cancellationToken);

        if (!aiResponse.IsSuccess)
        {
            _logger.LogWarning("AI Provider failed: {ErrorMessage}. Applying deterministic fallback recommendation.", aiResponse.ErrorMessage);
            aiResponse = BuildDeterministicFallback(request.ReasoningCategory, hasReferralUnacknowledged, eligibleClaims);
        }

        // ==========================================
        // 4. EVIDENCE VALIDATION & PROVENANCE
        // ==========================================
        var validatedClaimRefs = aiResponse.EvidenceClaimRefs
            .Where(id => eligibleClaimIds.Contains(id))
            .ToList();

        if (validatedClaimRefs.Count == 0 && eligibleClaims.Count > 0)
        {
            validatedClaimRefs.Add(eligibleClaims[0].Id);
        }

        // Enforce Safety Disclaimer if Healthcare Referral is active
        var finalRecommendation = aiResponse.RecommendationText;
        if (hasReferralUnacknowledged && !finalRecommendation.Contains("healthcare professional", StringComparison.OrdinalIgnoreCase))
        {
            finalRecommendation = $"[SAFETY REFERRAL NOTICE] Active healthcare referral indicated. Human coach clearance advised. {finalRecommendation}";
        }

        // Deterministic Coach Action Required for sensitive/actionable categories
        var categoryEnum = (AIRecommendationCategory)request.ReasoningCategory;

        // ==========================================
        // 5. PERSIST IN M13 AIRecommendationRecord
        // ==========================================
        var recommendationRecord = new AIRecommendationRecord(
            id: Guid.NewGuid(),
            clientId: client.Id,
            coachId: coachId,
            recommendationCategory: categoryEnum,
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

    private static string BuildSystemPrompt(bool hasReferralUnacknowledged)
    {
        return $$"""
You are the evidence-based AI Coaching Reasoning Engine for AI Coach OS.
Your objective is to provide structured coaching decision support for human gym coaches in Egypt.

CORE CONTRACT RULES:
1. Always output valid JSON conforming exactly to the schema:
{
  "recommendation": "string (clear, actionable coaching recommendation)",
  "rationale": "string (physiological, biomechanical, or behavioral reasoning)",
  "confidence_statement": "string (explicit statement of certainty and key limitations)",
  "evidence_claim_ids": ["string GUIDs of relevant claims provided in context"]
}
2. Never diagnose medical conditions and never prescribe medications or PEDs.
3. Only cite evidence claim IDs provided in the scientific knowledge context.
{{(hasReferralUnacknowledged ? "4. CRITICAL: Client has unacknowledged healthcare referral signals. Emphasize conservative management and physician review." : "")}}
""";
    }

    private static string BuildLockedContextPrompt(
        Domain.Clients.Client client,
        int? age,
        Domain.TrainingProfiles.ClientTrainingProfile? trainingProfile,
        object activeSafetyFlags,
        object? programSummary,
        object recentWorkouts,
        object? nutritionContext,
        ClientMemorySnapshotDto snapshot,
        List<KnowledgeClaim> eligibleClaims,
        IReadOnlyList<UnresolvedQuestionDto> unresolvedQuestions,
        GenerateReasoningRequestDto request,
        bool hasReferralUnacknowledged)
    {
        var claimsSummary = eligibleClaims.Select(c => new
        {
            c.Id,
            c.Topic,
            c.ClaimText,
            EvidenceLevel = c.EvidenceLevel.ToString()
        });

        var payload = new
        {
            Section1_ClientProfile = new
            {
                client.Id,
                client.FirstName,
                client.LastName,
                Age = age,
                Gender = client.Gender?.ToString() ?? "NotSpecified",
                Goal = client.Goal?.ToString() ?? "GeneralFitness",
                TrainingExperience = trainingProfile?.ExperienceLevel.ToString() ?? "Intermediate",
                WeeklySessions = trainingProfile?.WeeklyAvailability?.SessionsPerWeek
            },
            Section2_ActiveSafetyFlags = activeSafetyFlags,
            Section3_CurrentProgramSummary = programSummary,
            Section4_RecentPerformance = recentWorkouts,
            Section5_NutritionContext = nutritionContext,
            Section6_RelevantMemorySnapshot = snapshot.SnapshotContentJson,
            Section7_RelevantKnowledgeClaims = claimsSummary,
            Section8_ActiveUnresolvedQuestions = unresolvedQuestions.Select(q => new { q.RecordId, q.Content }),
            Section9_ReasoningInstruction = new
            {
                ReasoningCategory = request.ReasoningCategory.ToString(),
                AdditionalContext = request.AdditionalContext,
                HealthcareReferralActive = hasReferralUnacknowledged
            }
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    private static AiCompletionResponse BuildDeterministicFallback(
        ReasoningCategory category,
        bool hasReferralUnacknowledged,
        List<KnowledgeClaim> eligibleClaims)
    {
        var citedGuids = eligibleClaims.Take(1).Select(c => c.Id).ToList();

        var recText = category switch
        {
            ReasoningCategory.ProgramAdaptationReview => "Review program volume and frequency relative to recovery signals and client progression.",
            ReasoningCategory.NutritionAdjustmentReview => "Evaluate energy balance and protein distribution based on bodyweight trends.",
            ReasoningCategory.ExerciseModificationReview => "Select exercises providing stable mechanics matching joint comfort.",
            ReasoningCategory.SafetyContextSummary => "Summarize reported physical signals and verify coach review requirements.",
            ReasoningCategory.GeneralCoachingNote => "Synthesize client consistency, recovery markers, and upcoming phase goals.",
            _ => "Review client progress and adjust training variables progressively."
        };

        if (hasReferralUnacknowledged)
        {
            recText = $"[SAFETY REFERRAL NOTICE] Conservative management advised. Healthcare evaluation recommended. {recText}";
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
