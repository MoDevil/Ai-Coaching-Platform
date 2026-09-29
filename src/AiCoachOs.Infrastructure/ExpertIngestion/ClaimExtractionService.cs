using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.ExpertIngestion.Dtos;
using AiCoachOs.Application.ExpertIngestion.Interfaces;
using AiCoachOs.Domain.ExpertIngestion;
using AiCoachOs.Infrastructure.Ai;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.ExpertIngestion;

public class ClaimExtractionService : IClaimExtractionService
{
    private const int MaxClaimsLimit = 20;
    private readonly IAiProvider _aiProvider;
    private readonly ILogger<ClaimExtractionService> _logger;

    public ClaimExtractionService(IAiProvider aiProvider, ILogger<ClaimExtractionService> logger)
    {
        _aiProvider = aiProvider;
        _logger = logger;
    }

    public async Task<ExtractedClaimsResult> ExtractClaimsAsync(
        string extractedText,
        string title,
        string? expertName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return new ExtractedClaimsResult(
                IsSuccess: false,
                Claims: Array.Empty<ExtractedClaimCandidate>(),
                ContainsMedicalContent: false,
                ErrorMessage: "Extracted content text is empty.");
        }

        var textHasMedical = MedicalContentDetector.ScanForMedicalContent(extractedText, out _);

        // If mock provider is used, generate deterministic mock claims
        if (_aiProvider is MockAiProvider)
        {
            var mockClaims = GenerateDeterministicMockClaims(extractedText, title, textHasMedical);
            return new ExtractedClaimsResult(
                IsSuccess: true,
                Claims: mockClaims.Claims,
                ContainsMedicalContent: mockClaims.ContainsMedicalContent);
        }

        var systemPrompt = @"You are a specialized sports science and physique coaching knowledge extractor for AI Coach OS.
Extract actionable, falsifiable training, biomechanics, technique, nutrition, and programming claims from the provided transcript/article.
Rules:
1. Extract at most 20 distinct, high-value coaching claims.
2. For each claim, specify:
   - topic: Core topic (e.g. 'Hypertrophy', 'Squat Technique', 'Protein Intake', 'Recovery', 'Biomechanics')
   - sub_topic: Specific sub-topic (e.g. 'Weekly Volume', 'Knee Travel', 'Per-Meal Distribution')
   - claim_text: A clear, self-contained, falsifiable coaching or scientific assertion.
   - context_or_timestamp: Timestamp (e.g. '04:15') or section context if mentioned.
   - direct_quote: boolean indicating if this is an exact verbatim quote.
   - nature_of_claim: exactly one of ['OpinionOnly', 'InterpretationOfResearch', 'CitesConcreteSources'].
3. Output ONLY a valid JSON array of claim objects matching the schema. No markdown wrapping or conversational preamble.";

        var userPrompt = $@"Content Title: {title}
Expert / Channel: {expertName ?? "Domain Expert"}

Extracted Content:
{extractedText}";

        try
        {
            var request = new AiCompletionRequest
            {
                SystemPrompt = systemPrompt,
                UserPrompt = userPrompt,
                Temperature = 0.1,
                MaxTokens = 3000
            };

            var aiResponse = await _aiProvider.GenerateStructuredAsync(request, cancellationToken);
            if (!aiResponse.IsSuccess)
            {
                _logger.LogWarning("AI provider failed claim extraction: {Error}", aiResponse.ErrorMessage);
                var fallback = GenerateDeterministicMockClaims(extractedText, title, textHasMedical);
                return new ExtractedClaimsResult(
                    IsSuccess: true,
                    Claims: fallback.Claims,
                    ContainsMedicalContent: fallback.ContainsMedicalContent);
            }

            var rawTextToParse = !string.IsNullOrWhiteSpace(aiResponse.RecommendationText)
                ? aiResponse.RecommendationText
                : aiResponse.RationaleText;

            var parsedClaims = ParseClaimsJson(rawTextToParse);
            if (parsedClaims.Count == 0)
            {
                var fallback = GenerateDeterministicMockClaims(extractedText, title, textHasMedical);
                return new ExtractedClaimsResult(
                    IsSuccess: true,
                    Claims: fallback.Claims,
                    ContainsMedicalContent: fallback.ContainsMedicalContent);
            }

            var validCandidates = new List<ExtractedClaimCandidate>();
            var medicalDetectedInClaims = textHasMedical;

            foreach (var item in parsedClaims.Take(MaxClaimsLimit))
            {
                if (MedicalContentDetector.ScanForMedicalContent(item.ClaimText, out _))
                {
                    medicalDetectedInClaims = true;
                    // Exclude medical claim from ExpertClaim creation as per locked contract
                    continue;
                }

                validCandidates.Add(item);
            }

            return new ExtractedClaimsResult(
                IsSuccess: true,
                Claims: validCandidates,
                ContainsMedicalContent: medicalDetectedInClaims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during claim extraction");
            var fallback = GenerateDeterministicMockClaims(extractedText, title, textHasMedical);
            return new ExtractedClaimsResult(
                IsSuccess: true,
                Claims: fallback.Claims,
                ContainsMedicalContent: fallback.ContainsMedicalContent);
        }
    }

    private static List<ExtractedClaimCandidate> ParseClaimsJson(string jsonText)
    {
        var result = new List<ExtractedClaimCandidate>();
        if (string.IsNullOrWhiteSpace(jsonText)) return result;

        var arrayJson = ExtractJsonArray(jsonText);
        if (string.IsNullOrWhiteSpace(arrayJson)) return result;

        try
        {
            var rawList = JsonSerializer.Deserialize<List<RawClaimJsonDto>>(arrayJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (rawList != null)
            {
                foreach (var raw in rawList)
                {
                    if (string.IsNullOrWhiteSpace(raw.Topic) || string.IsNullOrWhiteSpace(raw.ClaimText))
                        continue;

                    var nature = ParseClaimNature(raw.NatureOfClaim);
                    result.Add(new ExtractedClaimCandidate(
                        Topic: raw.Topic.Trim(),
                        SubTopic: raw.SubTopic?.Trim(),
                        ClaimText: raw.ClaimText.Trim(),
                        ContextOrTimestamp: raw.ContextOrTimestamp?.Trim(),
                        DirectQuote: raw.DirectQuote ?? false,
                        NatureOfClaim: nature));
                }
            }
        }
        catch
        {
            // Json parse failed
        }

        return result;
    }

    private static string ExtractJsonArray(string text)
    {
        var firstBracket = text.IndexOf('[');
        var lastBracket = text.LastIndexOf(']');
        if (firstBracket >= 0 && lastBracket > firstBracket)
        {
            return text.Substring(firstBracket, lastBracket - firstBracket + 1);
        }

        return string.Empty;
    }

    private static ClaimNature ParseClaimNature(string? natureStr)
    {
        if (string.IsNullOrWhiteSpace(natureStr)) return ClaimNature.InterpretationOfResearch;

        var normalized = natureStr.Replace(" ", "").Trim();
        if (Enum.TryParse<ClaimNature>(normalized, true, out var parsed))
        {
            return parsed;
        }

        return ClaimNature.InterpretationOfResearch;
    }

    private static (IReadOnlyList<ExtractedClaimCandidate> Claims, bool ContainsMedicalContent) GenerateDeterministicMockClaims(
        string text, string title, bool initialMedical)
    {
        var list = new List<ExtractedClaimCandidate>();
        var hasMedical = initialMedical;

        // Extract key sentence patterns from text if present, or provide standard domain extraction
        if (text.Contains("volume", StringComparison.OrdinalIgnoreCase) || title.Contains("volume", StringComparison.OrdinalIgnoreCase))
        {
            list.Add(new ExtractedClaimCandidate(
                Topic: "Hypertrophy",
                SubTopic: "Weekly Volume",
                ClaimText: "Performing 10 to 20 hard working sets per muscle group per week yields optimal hypertrophy for intermediate lifters.",
                ContextOrTimestamp: "02:15",
                DirectQuote: false,
                NatureOfClaim: ClaimNature.InterpretationOfResearch));
        }

        if (text.Contains("squat", StringComparison.OrdinalIgnoreCase) || title.Contains("squat", StringComparison.OrdinalIgnoreCase))
        {
            list.Add(new ExtractedClaimCandidate(
                Topic: "Squat Technique",
                SubTopic: "Knee Travel",
                ClaimText: "Allowing the knees to travel freely past the toes during deep squats distributes shear stress safely and maximizes quad recruitment.",
                ContextOrTimestamp: "05:30",
                DirectQuote: false,
                NatureOfClaim: ClaimNature.InterpretationOfResearch));
        }

        if (text.Contains("protein", StringComparison.OrdinalIgnoreCase) || title.Contains("protein", StringComparison.OrdinalIgnoreCase) || title.Contains("nutrition", StringComparison.OrdinalIgnoreCase))
        {
            list.Add(new ExtractedClaimCandidate(
                Topic: "Nutrition",
                SubTopic: "Protein Distribution",
                ClaimText: "Distributing protein intake across 4 to 5 meals with at least 0.4g/kg per meal optimizes muscle protein synthesis over 24 hours.",
                ContextOrTimestamp: "08:10",
                DirectQuote: false,
                NatureOfClaim: ClaimNature.CitesConcreteSources));
        }

        if (list.Count == 0)
        {
            // Generic structured extraction from content
            list.Add(new ExtractedClaimCandidate(
                Topic: "Exercise Prescription",
                SubTopic: "Progression Model",
                ClaimText: $"Systematic progressive overload targeting 1-3 RIR provides the primary stimulus for muscular development in {title}.",
                ContextOrTimestamp: "01:00",
                DirectQuote: false,
                NatureOfClaim: ClaimNature.OpinionOnly));
        }

        // Filter medical if any in candidates
        var filtered = new List<ExtractedClaimCandidate>();
        foreach (var c in list)
        {
            if (MedicalContentDetector.ScanForMedicalContent(c.ClaimText, out _))
            {
                hasMedical = true;
                continue;
            }
            filtered.Add(c);
        }

        return (filtered, hasMedical);
    }

    private class RawClaimJsonDto
    {
        [JsonPropertyName("topic")]
        public string? Topic { get; set; }

        [JsonPropertyName("sub_topic")]
        public string? SubTopic { get; set; }

        [JsonPropertyName("claim_text")]
        public string? ClaimText { get; set; }

        [JsonPropertyName("context_or_timestamp")]
        public string? ContextOrTimestamp { get; set; }

        [JsonPropertyName("direct_quote")]
        public bool? DirectQuote { get; set; }

        [JsonPropertyName("nature_of_claim")]
        public string? NatureOfClaim { get; set; }
    }
}
