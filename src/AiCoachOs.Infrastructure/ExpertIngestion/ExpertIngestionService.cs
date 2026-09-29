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
        var exists = await _context.ExpertContentIngestions
            .AnyAsync(i => i.CoachId == coachId && i.SourceUrl == normalizedUrl, cancellationToken);

        if (exists)
        {
            throw new ConflictException($"An ingestion for URL '{normalizedUrl}' already exists for this coach.");
        }

        ExpertSource? expertSource = null;
        if (request.SourceId.HasValue && request.SourceId.Value != Guid.Empty)
        {
            expertSource = await _context.FindExpertSourceByIdAsync(request.SourceId.Value, cancellationToken);
            if (expertSource == null)
            {
                throw new NotFoundException(nameof(ExpertSource), request.SourceId.Value);
            }
        }

        var determinedType = request.ContentType ?? (
            normalizedUrl.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
            normalizedUrl.Contains("youtu.be", StringComparison.OrdinalIgnoreCase)
                ? IngestionContentType.YouTube
                : IngestionContentType.Article);

        var title = !string.IsNullOrWhiteSpace(request.Title)
            ? request.Title.Trim()
            : $"Pending Extraction: {normalizedUrl}";

        var ingestion = new ExpertContentIngestion(
            Guid.NewGuid(),
            coachId,
            normalizedUrl,
            determinedType,
            title,
            request.SourceId);

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
            SourceId: ingestion.SourceId,
            SourceName: expertSource?.Name,
            SourceUrl: ingestion.SourceUrl,
            ContentType: ingestion.ContentType,
            Title: ingestion.Title,
            WordCount: ingestion.WordCount,
            WasTruncated: ingestion.WasTruncated,
            Status: ingestion.Status,
            FailureReason: ingestion.FailureReason,
            ContainsMedicalClaims: ingestion.ContainsMedicalClaims,
            MedicalWarningAcknowledged: ingestion.MedicalWarningAcknowledged,
            ClaimCount: 0,
            SubmittedAtUtc: ingestion.SubmittedAtUtc,
            CompletedAtUtc: ingestion.CompletedAtUtc);
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
                ingestion.ContentType,
                cancellationToken);

            if (!fetchResult.IsSuccess)
            {
                ingestion.SetFailed(fetchResult.ErrorMessage ?? "Failed to fetch content.");
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            var textSnippet = fetchResult.RawText.Length > 1500
                ? fetchResult.RawText.Substring(0, 1500) + "..."
                : fetchResult.RawText;

            ingestion.SetExtractedContent(textSnippet, fetchResult.WordCount, fetchResult.WasTruncated);

            // 2. Extract claims
            var expertName = ingestion.Source?.Name;
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
                    match.Candidate.Topic,
                    match.Candidate.ClaimText,
                    match.Candidate.NatureOfClaim,
                    match.Candidate.SubTopic,
                    match.Candidate.ContextOrTimestamp,
                    match.Candidate.DirectQuote);

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
        CancellationToken cancellationToken = default)
    {
        var ingestions = await _context.ExpertContentIngestions
            .Include(i => i.Source)
            .Include(i => i.Claims)
            .Where(i => i.CoachId == coachId)
            .OrderByDescending(i => i.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        return ingestions.Select(i => new ExpertContentIngestionSummaryDto(
            Id: i.Id,
            CoachId: i.CoachId,
            SourceId: i.SourceId,
            SourceName: i.Source?.Name,
            SourceUrl: i.SourceUrl,
            ContentType: i.ContentType,
            Title: i.Title,
            WordCount: i.WordCount,
            WasTruncated: i.WasTruncated,
            Status: i.Status,
            FailureReason: i.FailureReason,
            ContainsMedicalClaims: i.ContainsMedicalClaims,
            MedicalWarningAcknowledged: i.MedicalWarningAcknowledged,
            ClaimCount: i.Claims.Count,
            SubmittedAtUtc: i.SubmittedAtUtc,
            CompletedAtUtc: i.CompletedAtUtc)).ToList();
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
            Topic: c.Topic,
            SubTopic: c.SubTopic,
            ClaimText: c.ClaimText,
            ContextOrTimestamp: c.ContextOrTimestamp,
            DirectQuote: c.DirectQuote,
            NatureOfClaim: c.NatureOfClaim,
            SupportingClaimId: c.SupportingClaimId,
            SupportingClaimText: c.SupportingClaim?.ClaimText,
            ConflictingClaimId: c.ConflictingClaimId,
            ConflictingClaimText: c.ConflictingClaim?.ClaimText,
            ReviewStatus: c.ReviewStatus,
            CoachNotes: c.CoachNotes,
            ApprovedKnowledgeClaimId: c.ApprovedKnowledgeClaimId,
            ReviewedAtUtc: c.ReviewedAtUtc,
            ReviewedByCoachId: c.ReviewedByCoachId)).ToList();

        return new ExpertContentIngestionDto(
            Id: ingestion.Id,
            CoachId: ingestion.CoachId,
            SourceId: ingestion.SourceId,
            SourceName: ingestion.Source?.Name,
            SourceUrl: ingestion.SourceUrl,
            ContentType: ingestion.ContentType,
            Title: ingestion.Title,
            RawExtractedTextSnippet: ingestion.RawExtractedTextSnippet,
            WordCount: ingestion.WordCount,
            WasTruncated: ingestion.WasTruncated,
            Status: ingestion.Status,
            FailureReason: ingestion.FailureReason,
            ContainsMedicalClaims: ingestion.ContainsMedicalClaims,
            MedicalWarningAcknowledged: ingestion.MedicalWarningAcknowledged,
            Claims: claimDtos,
            SubmittedAtUtc: ingestion.SubmittedAtUtc,
            CompletedAtUtc: ingestion.CompletedAtUtc);
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

        if (request.Decision == ExpertClaimReviewStatus.Approved)
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
                // Create new KnowledgeClaim in Provisional status
                var question = !string.IsNullOrWhiteSpace(request.NewClaimQuestion)
                    ? request.NewClaimQuestion.Trim()
                    : $"What does expert consensus assert regarding {claim.Topic}?";

                targetKnowledgeClaim = new KnowledgeClaim(
                    Guid.NewGuid(),
                    claim.Topic,
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

            var targetClaimId = targetKnowledgeClaim?.Id ?? Guid.NewGuid();
            claim.Approve(targetClaimId, coachId, request.Notes);
        }
        else if (request.Decision == ExpertClaimReviewStatus.Rejected)
        {
            claim.Reject(coachId, request.Notes);
        }
        else if (request.Decision == ExpertClaimReviewStatus.Deferred)
        {
            claim.Defer(coachId, request.Notes);
        }

        ingestion.RecalculateOverallStatus();
        await _context.SaveChangesAsync(cancellationToken);

        return new ExpertClaimDto(
            Id: claim.Id,
            IngestionId: claim.IngestionId,
            Topic: claim.Topic,
            SubTopic: claim.SubTopic,
            ClaimText: claim.ClaimText,
            ContextOrTimestamp: claim.ContextOrTimestamp,
            DirectQuote: claim.DirectQuote,
            NatureOfClaim: claim.NatureOfClaim,
            SupportingClaimId: claim.SupportingClaimId,
            SupportingClaimText: claim.SupportingClaim?.ClaimText,
            ConflictingClaimId: claim.ConflictingClaimId,
            ConflictingClaimText: claim.ConflictingClaim?.ClaimText,
            ReviewStatus: claim.ReviewStatus,
            CoachNotes: claim.CoachNotes,
            ApprovedKnowledgeClaimId: claim.ApprovedKnowledgeClaimId,
            ReviewedAtUtc: claim.ReviewedAtUtc,
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
            ChannelOrPublication: s.ChannelOrPublication,
            Platform: s.Platform,
            PrimaryDomain: s.PrimaryDomain,
            CredibilityTier: s.CredibilityTier,
            Bio: s.Bio,
            CreatedAtUtc: s.CreatedAtUtc,
            UpdatedAtUtc: s.UpdatedAtUtc)).ToList();
    }

    public async Task<ExpertSourceDto> CreateSourceAsync(
        CreateExpertSourceDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name", "Expert source name is required.");
        if (string.IsNullOrWhiteSpace(request.ChannelOrPublication))
            throw new ValidationException("ChannelOrPublication", "Channel or publication is required.");
        if (string.IsNullOrWhiteSpace(request.PrimaryDomain))
            throw new ValidationException("PrimaryDomain", "Primary domain is required.");

        var source = new ExpertSource(
            Guid.NewGuid(),
            request.Name,
            request.ChannelOrPublication,
            request.Platform,
            request.PrimaryDomain,
            request.CredibilityTier,
            request.Bio);

        await _context.AddExpertSourceAsync(source, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ExpertSourceDto(
            Id: source.Id,
            Name: source.Name,
            ChannelOrPublication: source.ChannelOrPublication,
            Platform: source.Platform,
            PrimaryDomain: source.PrimaryDomain,
            CredibilityTier: source.CredibilityTier,
            Bio: source.Bio,
            CreatedAtUtc: source.CreatedAtUtc,
            UpdatedAtUtc: source.UpdatedAtUtc);
    }
}
