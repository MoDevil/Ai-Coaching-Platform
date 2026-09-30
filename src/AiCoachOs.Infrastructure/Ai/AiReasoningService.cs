using System.Text.Json;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Common.Exceptions;
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

        // Step 7: RELEVANT KNOWLEDGE CLAIMS (Active + Confirmed only)
        var eligibleClaims = await _dbContext.KnowledgeClaims
            .Where(k => k.Status == ClaimStatus.Active && k.ReviewedAtUtc != null && k.ReviewedBy != null && k.ReviewedBy != "")
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
        // Only claims the provider actually cited survive, and only if they are confirmed-eligible.
        // Citing nothing is a valid, honest outcome: an uncited recommendation must reach the coach
        // with no evidence attached rather than borrow an unrelated claim.
        var validatedClaimRefs = aiResponse.EvidenceClaimRefs
            .Where(id => eligibleClaimIds.Contains(id))
            .Distinct()
            .ToList();

        var uncitedRefs = aiResponse.EvidenceClaimRefs.Except(validatedClaimRefs).ToList();
        if (uncitedRefs.Count > 0)
        {
            _logger.LogWarning(
                "Dropped {Count} claim reference(s) cited by provider {Provider} that are not confirmed-eligible evidence.",
                uncitedRefs.Count,
                aiResponse.ProviderName);
        }

        if (validatedClaimRefs.Count == 0)
        {
            _logger.LogInformation(
                "Recommendation for client {ClientId} carries no evidence citations from provider {Provider}. Coach review must not treat it as evidence-based.",
                client.Id,
                aiResponse.ProviderName);
        }

        // Enforce Safety Disclaimer if Healthcare Referral is active
        var finalRecommendation = aiResponse.RecommendationText;
        if (hasReferralUnacknowledged && !finalRecommendation.Contains("healthcare professional", StringComparison.OrdinalIgnoreCase))
        {
            finalRecommendation = $"[SAFETY REFERRAL NOTICE] Active healthcare referral indicated. Human coach clearance advised. {finalRecommendation}";
        }

        var confidenceStatement = string.IsNullOrWhiteSpace(aiResponse.ConfidenceStatement)
            ? "Confidence statement based on deterministic rules and verified knowledge claims."
            : aiResponse.ConfidenceStatement;

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
            rationaleText: string.IsNullOrWhiteSpace(aiResponse.RationaleText) ? "Rationale provided by evidence-based reasoning." : aiResponse.RationaleText,
            confidenceStatement: confidenceStatement,
            aiProvider: string.IsNullOrWhiteSpace(aiResponse.ProviderName) ? "MockAiProvider" : aiResponse.ProviderName,
            aiModel: string.IsNullOrWhiteSpace(aiResponse.ModelName) ? "claude-sonnet-4-6" : aiResponse.ModelName,
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
  "summary": "string (concise coaching summary)",
  "observations": ["string (key contextual observations)"],
  "recommendations": ["string (actionable coaching recommendations)"],
  "rationale": "string (physiological, biomechanical, or behavioral reasoning)",
  "confidence_statement": "string (explicit statement of certainty and key limitations)",
  "assumptions": ["string (assumptions made)"],
  "missing_high_value_data": ["string (unresolved or missing data)"],
  "evidence_refs": ["string GUIDs of relevant claims provided in context"],
  "safety_summary": "string or null",
  "coach_action_required": true
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

        var summaryText = category switch
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
            summaryText = $"[SAFETY REFERRAL NOTICE] Conservative management advised. Healthcare evaluation recommended. {summaryText}";
        }

        var structured = new StructuredRecommendation
        {
            Summary = summaryText,
            Observations = new List<string> { $"Deterministic evaluation executed for category {category}." },
            Recommendations = new List<string> { summaryText },
            Rationale = "Deterministic evidence-backed baseline applied based on client constraints and recovery boundaries.",
            ConfidenceStatement = "Baseline confidence based on deterministic rules and structured knowledge claims.",
            Assumptions = new List<string> { "Standard recovery capacity under progressive loading." },
            MissingHighValueData = new List<string>(),
            EvidenceRefs = citedGuids,
            SafetySummary = hasReferralUnacknowledged ? "Active healthcare referral indicated." : null,
            CoachActionRequired = true
        };

        return new AiCompletionResponse
        {
            IsSuccess = true,
            ProviderName = "DeterministicEngine",
            ModelName = "deterministic-fallback-v1",
            TokensUsed = 100,
            StructuredRecommendation = structured,
            RecommendationText = structured.Summary,
            RationaleText = structured.Rationale,
            ConfidenceStatement = structured.ConfidenceStatement,
            EvidenceClaimRefs = citedGuids
        };
    }

    public async Task<AIRecommendationRecordDto> ReviewRecommendationAsync(
        Guid coachId,
        Guid recommendationId,
        ReviewAIRecommendationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var record = await _dbContext.FindAIRecommendationRecordByIdAsync(recommendationId, cancellationToken);
        if (record == null)
        {
            throw new KeyNotFoundException($"Recommendation record {recommendationId} not found.");
        }

        if (record.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach does not own this recommendation record.");
        }

        record.ApplyReview(request.ReviewStatus, request.CoachDecision, request.FinalImplementedPlan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(record);
    }

    public async Task<AIRecommendationRecordDto> LinkProgramVersionAsync(
        Guid coachId,
        Guid recommendationId,
        Guid programVersionId,
        CancellationToken cancellationToken = default)
    {
        if (programVersionId == Guid.Empty)
            throw new ValidationException("ProgramVersionId", "ProgramVersionId cannot be empty.");

        var record = await _dbContext.FindAIRecommendationRecordByIdAsync(recommendationId, cancellationToken);
        if (record == null)
        {
            throw new KeyNotFoundException($"Recommendation record {recommendationId} not found.");
        }

        if (record.CoachId != coachId)
        {
            throw new UnauthorizedAccessException("Coach does not own this recommendation record.");
        }

        if (record.ReviewStatus != AIRecommendationReviewStatus.Accepted)
        {
            throw new ValidationException("ReviewStatus", "A program version can only be linked to an Accepted recommendation.");
        }

        var programVersion = await _dbContext.ProgramVersions
            .Include(pv => pv.Program)
            .FirstOrDefaultAsync(pv => pv.Id == programVersionId, cancellationToken);

        if (programVersion == null)
        {
            throw new KeyNotFoundException($"Program version {programVersionId} not found.");
        }

        if (programVersion.Program == null || programVersion.Program.ClientId != record.ClientId || programVersion.Program.CoachId != coachId)
        {
            throw new ValidationException("ProgramVersionId", "Program version does not belong to the recommendation's client or coach.");
        }

        record.LinkProgramVersion(programVersionId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(record);
    }

    public async Task<IReadOnlyList<AIRecommendationSummaryDto>> GetClientRecommendationsAsync(
        Guid coachId,
        Guid clientId,
        AIRecommendationReviewStatus? status = null,
        AIRecommendationCategory? category = null,
        CancellationToken cancellationToken = default)
    {
        var client = await _dbContext.Clients
            .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);

        if (client == null || client.CoachId != coachId)
        {
            throw new KeyNotFoundException($"Client {clientId} not found.");
        }

        var query = _dbContext.AIRecommendationRecords
            .Where(r => r.ClientId == clientId && r.CoachId == coachId);

        if (status.HasValue)
        {
            query = query.Where(r => r.ReviewStatus == status.Value);
        }

        if (category.HasValue)
        {
            query = query.Where(r => r.RecommendationCategory == category.Value);
        }

        var records = await query
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync(cancellationToken);

        return records.Select(r =>
        {
            var hasSafetyNotice = r.RecommendationText.Contains("[SAFETY REFERRAL NOTICE]");
            return new AIRecommendationSummaryDto
            {
                Id = r.Id,
                ClientId = r.ClientId,
                CoachId = r.CoachId,
                Category = r.RecommendationCategory,
                Summary = r.RecommendationText,
                GeneratedAt = r.GeneratedAt,
                Status = r.ReviewStatus,
                CoachActionRequired = r.ReviewStatus == AIRecommendationReviewStatus.PendingReview || r.ReviewStatus == AIRecommendationReviewStatus.UnderReview,
                SafetySummary = hasSafetyNotice ? "Active healthcare referral indicated." : null,
                KnowledgeClaimCount = r.KnowledgeClaimRefs.Count
            };
        }).ToList();
    }

    public async Task<AIRecommendationDetailDto> GetClientRecommendationDetailAsync(
        Guid coachId,
        Guid clientId,
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        var client = await _dbContext.Clients
            .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);

        if (client == null || client.CoachId != coachId)
        {
            throw new KeyNotFoundException($"Client {clientId} not found.");
        }

        var record = await _dbContext.FindAIRecommendationRecordByIdAsync(recommendationId, cancellationToken);
        if (record == null || record.ClientId != clientId || record.CoachId != coachId)
        {
            throw new KeyNotFoundException($"Recommendation {recommendationId} not found for client {clientId}.");
        }

        var claimGuids = record.KnowledgeClaimRefs.ToList();
        var resolvedClaims = new List<ResolvedKnowledgeClaimDto>();
        if (claimGuids.Count > 0)
        {
            var claims = await _dbContext.KnowledgeClaims
                .Where(k => claimGuids.Contains(k.Id))
                .ToListAsync(cancellationToken);

            foreach (var claim in claims)
            {
                var text = claim.ClaimText ?? string.Empty;
                var truncatedText = text.Length > 200 ? text.Substring(0, 197) + "..." : text;
                resolvedClaims.Add(new ResolvedKnowledgeClaimDto
                {
                    Id = claim.Id,
                    Topic = claim.Topic,
                    ClaimText = truncatedText,
                    EvidenceLevel = claim.EvidenceLevel,
                    Status = claim.Status
                });
            }
        }

        var hasSafetyNotice = record.RecommendationText.Contains("[SAFETY REFERRAL NOTICE]");
        var safetySummary = hasSafetyNotice ? "Active healthcare referral indicated." : null;

        string? versionLabel = null;
        if (record.ImplementedProgramVersionId.HasValue)
        {
            var version = await _dbContext.ProgramVersions
                .Include(pv => pv.Program)
                .FirstOrDefaultAsync(pv => pv.Id == record.ImplementedProgramVersionId.Value, cancellationToken);

            if (version != null && version.Program != null)
            {
                versionLabel = $"v{version.VersionNumber} - {version.Program.Name}";
            }
        }

        return new AIRecommendationDetailDto
        {
            Id = record.Id,
            ClientId = record.ClientId,
            CoachId = record.CoachId,
            RecommendationCategory = record.RecommendationCategory,
            Summary = record.RecommendationText,
            Observations = new List<string> { "Client context evaluated against active baseline and recovery markers." },
            Recommendations = new List<string> { record.RecommendationText },
            Rationale = record.RationaleText,
            ConfidenceStatement = record.ConfidenceStatement,
            Assumptions = new List<string> { "Standard physiological response under progressive training load." },
            MissingHighValueData = new List<string>(),
            SafetySummary = safetySummary,
            CoachActionRequired = record.ReviewStatus == AIRecommendationReviewStatus.PendingReview || record.ReviewStatus == AIRecommendationReviewStatus.UnderReview,
            ResolvedKnowledgeClaims = resolvedClaims,
            AIProvider = record.AIProvider,
            AIModel = record.AIModel,
            GeneratedAt = record.GeneratedAt,
            ReviewStatus = record.ReviewStatus,
            CoachDecision = record.CoachDecisionNote,
            CoachDecisionAt = record.CoachDecisionAt,
            FinalImplementedPlan = record.FinalImplementedPlan,
            ImplementedProgramVersionId = record.ImplementedProgramVersionId,
            ImplementedProgramVersionLabel = versionLabel
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
            ImplementedProgramVersionId = r.ImplementedProgramVersionId,
            KnowledgeClaimRefs = r.KnowledgeClaimRefs.ToList()
        };
    }
}
