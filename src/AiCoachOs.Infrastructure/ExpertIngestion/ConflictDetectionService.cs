using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Application.ExpertIngestion.Dtos;
using AiCoachOs.Application.ExpertIngestion.Interfaces;
using AiCoachOs.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.ExpertIngestion;

/// <summary>
/// Deterministic conflict and support detector against M3 KnowledgeClaims (zero LLM calls).
/// </summary>
public class ConflictDetectionService : IConflictDetectionService
{
    private readonly IApplicationDbContext _context;

    public ConflictDetectionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ExpertClaimConflictMatch>> DetectM3ConflictsAsync(
        IReadOnlyList<ExtractedClaimCandidate> candidates,
        CancellationToken cancellationToken = default)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return Array.Empty<ExpertClaimConflictMatch>();
        }

        var activeClaims = await _context.KnowledgeClaims
            .Where(c => c.Status == ClaimStatus.Active)
            .ToListAsync(cancellationToken);

        var results = new List<ExpertClaimConflictMatch>();

        foreach (var candidate in candidates)
        {
            var (supportingId, conflictingId) = FindDeterministicMatch(candidate, activeClaims);
            results.Add(new ExpertClaimConflictMatch(candidate, supportingId, conflictingId));
        }

        return results;
    }

    private static (Guid? SupportingId, Guid? ConflictingId) FindDeterministicMatch(
        ExtractedClaimCandidate candidate,
        List<KnowledgeClaim> existingClaims)
    {
        if (existingClaims.Count == 0)
        {
            return (null, null);
        }

        var candidateTopic = candidate.Topic.ToLowerInvariant();
        var candidateSubTopic = (candidate.SubTopic ?? string.Empty).ToLowerInvariant();
        var candidateText = candidate.ClaimText.ToLowerInvariant();

        KnowledgeClaim? bestSupporting = null;
        KnowledgeClaim? bestConflicting = null;
        double bestSupportScore = 0;
        double bestConflictScore = 0;

        foreach (var claim in existingClaims)
        {
            var claimTopic = claim.Topic.ToLowerInvariant();
            var claimQuestion = claim.Question.ToLowerInvariant();
            var claimText = claim.ClaimText.ToLowerInvariant();

            // Check topic relevance
            var topicOverlap = CalculateTopicOverlap(candidateTopic, candidateSubTopic, claimTopic, claimQuestion);
            if (topicOverlap < 0.25)
            {
                continue;
            }

            var textOverlap = CalculateWordOverlap(candidateText, claimText);
            var isOpposing = DetectContradictoryPolarity(candidateText, claimText);

            if (isOpposing)
            {
                var conflictScore = (topicOverlap * 0.6) + (textOverlap * 0.4);
                if (conflictScore > bestConflictScore && conflictScore > 0.3)
                {
                    bestConflictScore = conflictScore;
                    bestConflicting = claim;
                }
            }
            else
            {
                var supportScore = (topicOverlap * 0.5) + (textOverlap * 0.5);
                if (supportScore > bestSupportScore && supportScore > 0.35)
                {
                    bestSupportScore = supportScore;
                    bestSupporting = claim;
                }
            }
        }

        return (bestSupporting?.Id, bestConflicting?.Id);
    }

    private static double CalculateTopicOverlap(string candTopic, string candSub, string existingTopic, string existingQuestion)
    {
        var candTokens = candTopic.Split(new[] { ' ', '-', '/', '_' }, StringSplitOptions.RemoveEmptyEntries)
            .Concat(candSub.Split(new[] { ' ', '-', '/', '_' }, StringSplitOptions.RemoveEmptyEntries))
            .ToHashSet();

        var existingTokens = existingTopic.Split(new[] { ' ', '-', '/', '_' }, StringSplitOptions.RemoveEmptyEntries)
            .Concat(existingQuestion.Split(new[] { ' ', '-', '/', '_' }, StringSplitOptions.RemoveEmptyEntries))
            .ToHashSet();

        if (candTokens.Count == 0 || existingTokens.Count == 0) return 0;

        var intersection = candTokens.Intersect(existingTokens).Count();
        return (double)intersection / Math.Min(candTokens.Count, existingTokens.Count);
    }

    private static double CalculateWordOverlap(string text1, string text2)
    {
        var stopWords = new HashSet<string> { "the", "a", "an", "is", "are", "and", "or", "in", "on", "to", "for", "with", "of", "by", "that", "this" };

        var words1 = text1.Split(new[] { ' ', '.', ',', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !stopWords.Contains(w))
            .ToHashSet();

        var words2 = text2.Split(new[] { ' ', '.', ',', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !stopWords.Contains(w))
            .ToHashSet();

        if (words1.Count == 0 || words2.Count == 0) return 0;

        var intersection = words1.Intersect(words2).Count();
        return (double)intersection / Math.Max(words1.Count, words2.Count);
    }

    private static bool DetectContradictoryPolarity(string text1, string text2)
    {
        // Polarity indicators
        var positiveTerms = new[] { "increases", "effective", "optimal", "superior", "beneficial", "necessary", "required", "improves", "essential", "high volume" };
        var negativeTerms = new[] { "reduces", "ineffective", "suboptimal", "inferior", "harmful", "unnecessary", "impairs", "detrimental", "myth", "false", "low volume", "not required" };

        var text1HasPositive = positiveTerms.Any(text1.Contains);
        var text1HasNegative = negativeTerms.Any(text1.Contains);

        var text2HasPositive = positiveTerms.Any(text2.Contains);
        var text2HasNegative = negativeTerms.Any(text2.Contains);

        // One text asserts positive benefit while the other asserts negative/unnecessary
        if ((text1HasPositive && text2HasNegative) || (text1HasNegative && text2HasPositive))
        {
            return true;
        }

        // Explicit negation check
        if ((text1.Contains("never") || text1.Contains("avoid") || text1.Contains("do not")) &&
            (text2.Contains("always") || text2.Contains("prioritize") || text2.Contains("should")))
        {
            return true;
        }

        return false;
    }
}
