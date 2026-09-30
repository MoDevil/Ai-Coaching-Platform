using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.ExpertIngestion.Dtos;
using AiCoachOs.Application.ExpertIngestion.Interfaces;
using AiCoachOs.Domain.ExpertIngestion;
using AiCoachOs.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.ExpertIngestion;

public class ExpertIngestionService : IExpertIngestionService
{
    private readonly IApplicationDbContext _context;
    private readonly IContentFetcherService _contentFetcherService;
    private readonly IClaimExtractionService _claimExtractionService;
    private readonly IConflictDetectionService _conflictDetectionService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ExpertIngestionService> _logger;

    public ExpertIngestionService(
        IApplicationDbContext context,
        IContentFetcherService contentFetcherService,
        IClaimExtractionService claimExtractionService,
        IConflictDetectionService conflictDetectionService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ExpertIngestionService> logger)
    {
        _context = context;
        _contentFetcherService = contentFetcherService;
        _claimExtractionService = claimExtractionService;
        _conflictDetectionService = conflictDetectionService;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<ExpertContentIngestionSummaryDto> SubmitIngestionAsync(
        Guid coachId,
        SubmitIngestionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (string.IsNullOrWhiteSpace(request.SourceUrl))
            throw new ValidationException("SourceUrl", "Source URL is required.");

        var normalizedUrl = request.SourceUrl.Trim();

        // Duplicate prevention per coach
        var existing = await _context.ExpertContentIngestions
            .FirstOrDefaultAsync(i => i.CoachId == coachId && i.SourceUrl == normalizedUrl, cancellationToken);

        if (existing != null)
        {
            throw new ConflictException($"An ingestion for URL '{normalizedUrl}' already exists for this coach (Id: {existing.Id}).");
        }

        ExpertSource? expertSource = null;
        if (request.ExpertSourceId.HasValue && request.ExpertSourceId.Value != Guid.Empty)
        {
            expertSource = await _context.FindExpertSourceByIdAsync(request.ExpertSourceId.Value, cancellationToken);
            if (expertSource == null)
            {
                throw new NotFoundException(nameof(ExpertSource), request.ExpertSourceId.Value);
            }
        }

        var determinedType = request.SourceType ?? (
            normalizedUrl.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
            normalizedUrl.Contains("youtu.be", StringComparison.OrdinalIgnoreCase)
                ? IngestionSourceType.YouTubeVideo
                : IngestionSourceType.Article);

        var title = !string.IsNullOrWhiteSpace(request.SourceTitle)
            ? request.SourceTitle.Trim()
            : $"Pending Extraction: {normalizedUrl}";

        var ingestion = new ExpertContentIngestion(
            Guid.NewGuid(),
            coachId,
            normalizedUrl,
            title,
            determinedType,
            request.ExpertSourceId,
            request.PublishedAt);

        await _context.AddExpertContentIngestionAsync(ingestion, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var ingestionId = ingestion.Id;

        // Launch background processing
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var scopedService = scope.ServiceProvider.GetRequiredService<IExpertIngestionService>();
                await scopedService.ExecuteIngestionPipelineAsync(ingestionId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background ingestion pipeline failed for IngestionId {Id}", ingestionId);
            }
        });

        return new ExpertContentIngestionSummaryDto(
            Id: ingestion.Id,
            CoachId: ingestion.CoachId,
            ExpertSourceId: ingestion.ExpertSourceId,
            SourceName: expertSource?.Name,
            SourceUrl: ingestion.SourceUrl,
            SourceTitle: ingestion.SourceTitle,
            SourceType: ingestion.SourceType,
            PublishedAt: ingestion.PublishedAt,
            ExtractedTextLength: ingestion.ExtractedTextLength,
            WasTruncated: ingestion.WasTruncated,
            Status: ingestion.Status,
            FailureReason: ingestion.FailureReason,
            ContainsMedicalClaims: ingestion.ContainsMedicalClaims,
            ClaimCount: 0,
            SubmittedAtUtc: ingestion.SubmittedAtUtc,
            ProcessedAtUtc: ingestion.ProcessedAtUtc);
    }

    public async Task ExecuteIngestionPipelineAsync(
        Guid ingestionId,
        CancellationToken cancellationToken = default)
    {
        var ingestion = await _context.FindExpertContentIngestionByIdAsync(ingestionId, cancellationToken);
        if (ingestion == null)
        {
            _logger.LogWarning("Ingestion {Id} was not found during background execution.", ingestionId);
            return;
        }

        try
        {
            // 1. Fetch content
            var fetchResult = await _contentFetcherService.FetchContentAsync(
                ingestion.SourceUrl,
                ingestion.SourceType,
                cancellationToken);

            if (!fetchResult.IsSuccess)
            {
                ingestion.SetFailed(fetchResult.ErrorMessage ?? "Failed to fetch content.");
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            // Record transient stats (no raw text persisted to DB)
            ingestion.SetExtractedStats(fetchResult.ExtractedTextLength, fetchResult.WasTruncated);

            // 2. Extract claims
            var expertName = ingestion.ExpertSource?.Name;
            var extractionResult = await _claimExtractionService.ExtractClaimsAsync(
                fetchResult.RawText,
                fetchResult.Title,
                expertName,
                cancellationToken);

            if (!extractionResult.IsSuccess)
            {
                ingestion.SetFailed(extractionResult.ErrorMessage ?? "Failed to extract claims.");
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            // 3. Deterministic conflict & support matching against M3 (zero LLM calls)
            var conflictMatches = await _conflictDetectionService.DetectM3ConflictsAsync(
                extractionResult.Claims,
                cancellationToken);

            foreach (var match in conflictMatches)
            {
                var expertClaim = new ExpertClaim(
                    Guid.NewGuid(),
                    ingestion.Id,
                    match.Candidate.ClaimText,
                    match.Candidate.Category,
                    match.Candidate.EvidenceClassification,
                    match.Candidate.CreatorConfidence,
                    match.Candidate.DirectQuote,
                    match.Candidate.SourceContext);

                expertClaim.SetDeterministicM3Comparison(match.SupportingClaimId, match.ConflictingClaimId);
                await _context.AddExpertClaimAsync(expertClaim, cancellationToken);
                ingestion.AddClaim(expertClaim);
            }

            // 4. Mark completed and flag medical warnings if detected
            ingestion.SetCompleted(extractionResult.ContainsMedicalContent);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ingestion pipeline for IngestionId {Id}", ingestionId);
            ingestion.SetFailed($"Unexpected ingestion error: {ex.Message}");
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<ExpertContentIngestionSummaryDto>> GetIngestionsAsync(
        Guid coachId,
        IngestionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ExpertContentIngestions
            .Include(i => i.ExpertSource)
            .Include(i => i.Claims)
            .Where(i => i.CoachId == coachId);

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        var ingestions = await query
            .OrderByDescending(i => i.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        return ingestions.Select(i => new ExpertContentIngestionSummaryDto(
            Id: i.Id,
            CoachId: i.CoachId,
            ExpertSourceId: i.ExpertSourceId,
            SourceName: i.ExpertSource?.Name,
            SourceUrl: i.SourceUrl,
            SourceTitle: i.SourceTitle,
            SourceType: i.SourceType,
            PublishedAt: i.PublishedAt,
            ExtractedTextLength: i.ExtractedTextLength,
            WasTruncated: i.WasTruncated,
            Status: i.Status,
            FailureReason: i.FailureReason,
            ContainsMedicalClaims: i.ContainsMedicalClaims,
            ClaimCount: i.Claims.Count,
            SubmittedAtUtc: i.SubmittedAtUtc,
            ProcessedAtUtc: i.ProcessedAtUtc)).ToList();
    }

    public async Task<ExpertContentIngestionDto> GetIngestionByIdAsync(
        Guid coachId,
        Guid ingestionId,
        CancellationToken cancellationToken = default)
    {
        var ingestion = await _context.FindExpertContentIngestionByIdAsync(ingestionId, cancellationToken);
        if (ingestion == null)
        {
            throw new NotFoundException(nameof(ExpertContentIngestion), ingestionId);
        }

        if (ingestion.CoachId != coachId)
        {
            throw new ForbiddenException("You do not have access to this ingestion.");
        }

        var claims = await _context.ExpertClaims
            .Include(c => c.SupportingClaim)
            .Include(c => c.ConflictingClaim)
            .Where(c => c.IngestionId == ingestionId)
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var claimDtos = claims.Select(c => new ExpertClaimDto(
            Id: c.Id,
            IngestionId: c.IngestionId,
            ClaimText: c.ClaimText,
            ClaimCategory: c.ClaimCategory,
            EvidenceClassification: c.EvidenceClassification,
            CreatorConfidence: c.CreatorConfidence,
            DirectQuote: c.DirectQuote,
            SourceContext: c.SourceContext,
            SupportingClaimId: c.SupportingClaimId,
            SupportingClaimText: c.SupportingClaim?.ClaimText,
            ConflictingClaimId: c.ConflictingClaimId,
            ConflictingClaimText: c.ConflictingClaim?.ClaimText,
            CoachReviewStatus: c.CoachReviewStatus,
            CoachReviewedAt: c.CoachReviewedAt,
            CoachNote: c.CoachNote,
            ApprovedKnowledgeClaimId: c.ApprovedKnowledgeClaimId,
            ReviewedByCoachId: c.ReviewedByCoachId)).ToList();

        return new ExpertContentIngestionDto(
            Id: ingestion.Id,
            CoachId: ingestion.CoachId,
            ExpertSourceId: ingestion.ExpertSourceId,
            SourceName: ingestion.ExpertSource?.Name,
            SourceUrl: ingestion.SourceUrl,
            SourceTitle: ingestion.SourceTitle,
            SourceType: ingestion.SourceType,
            PublishedAt: ingestion.PublishedAt,
            ExtractedTextLength: ingestion.ExtractedTextLength,
            WasTruncated: ingestion.WasTruncated,
            Status: ingestion.Status,
            FailureReason: ingestion.FailureReason,
            ContainsMedicalClaims: ingestion.ContainsMedicalClaims,
            Claims: claimDtos,
            SubmittedAtUtc: ingestion.SubmittedAtUtc,
            ProcessedAtUtc: ingestion.ProcessedAtUtc);
    }

    public async Task<ExpertClaimDto> ReviewClaimAsync(
        Guid coachId,
        Guid ingestionId,
        Guid claimId,
        ReviewClaimRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var claim = await _context.FindExpertClaimByIdAsync(claimId, cancellationToken);
        if (claim == null)
        {
            throw new NotFoundException(nameof(ExpertClaim), claimId);
        }

        if (claim.IngestionId != ingestionId)
        {
            throw new NotFoundException("Claim does not belong to the specified ingestion.");
        }

        var ingestion = await _context.FindExpertContentIngestionByIdAsync(ingestionId, cancellationToken);
        if (ingestion == null || ingestion.CoachId != coachId)
        {
            throw new ForbiddenException("You do not have access to review this claim.");
        }

        if (request.Decision == CoachReviewStatus.Approved)
        {
            KnowledgeClaim? targetKnowledgeClaim = null;

            if (request.ExistingKnowledgeClaimIdToLink.HasValue)
            {
                targetKnowledgeClaim = await _context.FindKnowledgeClaimByIdAsync(
                    request.ExistingKnowledgeClaimIdToLink.Value, cancellationToken);

                if (targetKnowledgeClaim == null)
                {
                    throw new NotFoundException(nameof(KnowledgeClaim), request.ExistingKnowledgeClaimIdToLink.Value);
                }

                if (!string.IsNullOrWhiteSpace(request.EgyptSpecificNotes) || !string.IsNullOrWhiteSpace(request.PractitionerNotes))
                {
                    targetKnowledgeClaim.UpdateExpertNuances(
                        expertConsensus: null,
                        expertDisagreements: null,
                        practitionerNotes: request.PractitionerNotes,
                        egyptSpecificNotes: request.EgyptSpecificNotes);
                }
            }
            else if (request.CreateNewKnowledgeClaim || claim.SupportingClaimId == null)
            {
                // Create new KnowledgeClaim in Provisional status (zero automatic activation)
                var question = !string.IsNullOrWhiteSpace(request.NewClaimQuestion)
                    ? request.NewClaimQuestion.Trim()
                    : $"What does expert consensus assert regarding {claim.ClaimCategory}?";

                targetKnowledgeClaim = new KnowledgeClaim(
                    Guid.NewGuid(),
                    claim.ClaimCategory.ToString(),
                    question,
                    claim.ClaimText,
                    EvidenceLevel.ExpertConsensus,
                    ClaimStatus.Provisional,
                    exerciseId: null,
                    population: null,
                    limitations: null,
                    practicalApplication: request.Notes);

                targetKnowledgeClaim.UpdateExpertNuances(
                    expertConsensus: claim.ClaimText,
                    expertDisagreements: null,
                    practitionerNotes: request.PractitionerNotes,
                    egyptSpecificNotes: request.EgyptSpecificNotes);

                await _context.AddKnowledgeClaimAsync(targetKnowledgeClaim, cancellationToken);
            }
            else
            {
                // Link to supporting claim
                targetKnowledgeClaim = await _context.FindKnowledgeClaimByIdAsync(
                    claim.SupportingClaimId.Value, cancellationToken);

                if (targetKnowledgeClaim != null &&
                    (!string.IsNullOrWhiteSpace(request.EgyptSpecificNotes) || !string.IsNullOrWhiteSpace(request.PractitionerNotes)))
                {
                    targetKnowledgeClaim.UpdateExpertNuances(
                        expertConsensus: null,
                        expertDisagreements: null,
                        practitionerNotes: request.PractitionerNotes,
                        egyptSpecificNotes: request.EgyptSpecificNotes);
                }
            }

            // Ensure M3 KnowledgeSource provenance is linked
            var sourceUrl = ingestion.SourceUrl;
            var existingKnowledgeSource = await _context.KnowledgeSources
                .FirstOrDefaultAsync(s => s.Url == sourceUrl, cancellationToken);

            if (existingKnowledgeSource == null)
            {
                var authorName = ingestion.ExpertSource?.Name ?? "Expert Content Creator";
                var pubYear = ingestion.PublishedAt?.Year ?? DateTime.UtcNow.Year;
                existingKnowledgeSource = new KnowledgeSource(
                    Guid.NewGuid(),
                    KnowledgeSourceType.ExpertConsensus,
                    ingestion.SourceTitle,
                    authorName,
                    pubYear,
                    EvidenceLevel.ExpertConsensus,
                    doi: null,
                    url: sourceUrl,
                    notes: $"Ingested via AI Coach OS Expert Content Ingestion (ID: {ingestion.Id})");

                await _context.AddKnowledgeSourceAsync(existingKnowledgeSource, cancellationToken);
            }

            if (targetKnowledgeClaim != null)
            {
                targetKnowledgeClaim.AddSource(existingKnowledgeSource.Id, $"Ingested claim (ID: {claim.Id})");
            }

            var targetClaimId = targetKnowledgeClaim?.Id ?? Guid.NewGuid();
            claim.Approve(targetClaimId, coachId, request.Notes);
        }
        else if (request.Decision == CoachReviewStatus.Rejected)
        {
            claim.Reject(coachId, request.Notes);
        }
        else if (request.Decision == CoachReviewStatus.Deferred)
        {
            claim.Defer(coachId, request.Notes);
        }

        ingestion.RecalculateOverallStatus();
        await _context.SaveChangesAsync(cancellationToken);

        return new ExpertClaimDto(
            Id: claim.Id,
            IngestionId: claim.IngestionId,
            ClaimText: claim.ClaimText,
            ClaimCategory: claim.ClaimCategory,
            EvidenceClassification: claim.EvidenceClassification,
            CreatorConfidence: claim.CreatorConfidence,
            DirectQuote: claim.DirectQuote,
            SourceContext: claim.SourceContext,
            SupportingClaimId: claim.SupportingClaimId,
            SupportingClaimText: claim.SupportingClaim?.ClaimText,
            ConflictingClaimId: claim.ConflictingClaimId,
            ConflictingClaimText: claim.ConflictingClaim?.ClaimText,
            CoachReviewStatus: claim.CoachReviewStatus,
            CoachReviewedAt: claim.CoachReviewedAt,
            CoachNote: claim.CoachNote,
            ApprovedKnowledgeClaimId: claim.ApprovedKnowledgeClaimId,
            ReviewedByCoachId: claim.ReviewedByCoachId);
    }

    public async Task<IReadOnlyList<ExpertSourceDto>> GetSourcesAsync(CancellationToken cancellationToken = default)
    {
        var sources = await _context.ExpertSources
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return sources.Select(s => new ExpertSourceDto(
            Id: s.Id,
            Name: s.Name,
            SourceType: s.SourceType,
            Url: s.Url,
            CreatedAtUtc: s.CreatedAtUtc,
            UpdatedAtUtc: s.UpdatedAtUtc)).ToList();
    }

    public async Task<ExpertSourceDto> CreateSourceAsync(
        CreateExpertSourceDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name", "Expert source name is required.");
        if (string.IsNullOrWhiteSpace(request.Url))
            throw new ValidationException("Url", "Expert source URL is required.");

        var source = new ExpertSource(
            Guid.NewGuid(),
            request.Name,
            request.SourceType,
            request.Url);

        await _context.AddExpertSourceAsync(source, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ExpertSourceDto(
            Id: source.Id,
            Name: source.Name,
            SourceType: source.SourceType,
            Url: source.Url,
            CreatedAtUtc: source.CreatedAtUtc,
            UpdatedAtUtc: source.UpdatedAtUtc);
    }
}
