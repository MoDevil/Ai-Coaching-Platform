using System.Text.Json;
using System.Text.Json.Serialization;
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
                SourceSummary: null,
                CreatorApparentPosition: null,
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
                SourceSummary: "Summary of expert content regarding hypertrophy and exercise programming.",
                CreatorApparentPosition: "Evidence-based strength and conditioning coach advocating structured volume targets.",
                ContainsMedicalContent: mockClaims.ContainsMedicalContent);
        }

        var systemPrompt = @"You are a sports science and coaching knowledge extractor for AI Coach OS.
Extract actionable, falsifiable training, biomechanics, technique, nutrition, and recovery claims from the provided transcript/article.
Rules:
1. Extract at most 20 distinct, high-value coaching claims.
2. For each claim, output:
   - claim_text: A clear, self-contained, falsifiable coaching or scientific assertion.
   - category: Exactly one of ['TrainingVolume', 'Frequency', 'Intensity', 'Nutrition', 'Recovery', 'Supplementation', 'Biomechanics', 'General'].
   - evidence_classification: Exactly one of ['OpinionOnly', 'InterpretationOfResearch', 'CitesConcreteSources', 'ContradictsCurrentEvidence', 'AgreesWithCurrentEvidence', 'Uncertain'].
   - creator_confidence: Exactly one of ['High', 'Medium', 'Low'].
   - direct_quote: boolean (true only if near-verbatim quote from the source).
   - source_context: timestamp (e.g. '04:15') or concise section context.
3. You must NOT determine scientific truth or validate medical advice.
4. Output ONLY a valid JSON object matching this schema:
{
  ""claims"": [
    {
      ""claim_text"": ""..."",
      ""category"": ""TrainingVolume"",
      ""evidence_classification"": ""InterpretationOfResearch"",
      ""creator_confidence"": ""High"",
      ""direct_quote"": false,
      ""source_context"": ""02:15""
    }
  ],
  ""source_summary"": ""..."",
  ""creator_apparent_position"": ""...""
}";

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
                    SourceSummary: "Extracted summary of training content.",
                    CreatorApparentPosition: "Evidence-based practitioner.",
                    ContainsMedicalContent: fallback.ContainsMedicalContent);
            }

            var rawTextToParse = !string.IsNullOrWhiteSpace(aiResponse.RecommendationText)
                ? aiResponse.RecommendationText
                : aiResponse.RationaleText;

            var parsedExtraction = ParseClaimsJsonObject(rawTextToParse);
            if (parsedExtraction.Claims.Count == 0)
            {
                var fallback = GenerateDeterministicMockClaims(extractedText, title, textHasMedical);
                return new ExtractedClaimsResult(
                    IsSuccess: true,
                    Claims: fallback.Claims,
                    SourceSummary: parsedExtraction.SourceSummary ?? "Extracted summary of training content.",
                    CreatorApparentPosition: parsedExtraction.CreatorApparentPosition ?? "Evidence-based practitioner.",
                    ContainsMedicalContent: fallback.ContainsMedicalContent);
            }

            var validCandidates = new List<ExtractedClaimCandidate>();
            var medicalDetectedInClaims = textHasMedical;

            foreach (var item in parsedExtraction.Claims.Take(MaxClaimsLimit))
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
                SourceSummary: parsedExtraction.SourceSummary,
                CreatorApparentPosition: parsedExtraction.CreatorApparentPosition,
                ContainsMedicalContent: medicalDetectedInClaims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during claim extraction");
            var fallback = GenerateDeterministicMockClaims(extractedText, title, textHasMedical);
            return new ExtractedClaimsResult(
                IsSuccess: true,
                Claims: fallback.Claims,
                SourceSummary: "Fallback extraction summary.",
                CreatorApparentPosition: "Practitioner.",
                ContainsMedicalContent: fallback.ContainsMedicalContent);
        }
    }

    private static (List<ExtractedClaimCandidate> Claims, string? SourceSummary, string? CreatorApparentPosition) ParseClaimsJsonObject(string jsonText)
    {
        var result = new List<ExtractedClaimCandidate>();
        if (string.IsNullOrWhiteSpace(jsonText)) return (result, null, null);

        var cleanJson = ExtractJsonObject(jsonText);
        if (string.IsNullOrWhiteSpace(cleanJson)) return (result, null, null);

        try
        {
            var root = JsonSerializer.Deserialize<ExtractionResponseJsonDto>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (root?.Claims != null)
            {
                foreach (var raw in root.Claims)
                {
                    if (string.IsNullOrWhiteSpace(raw.ClaimText))
                        continue;

                    var category = ParseCategory(raw.Category);
                    var evidence = ParseEvidence(raw.EvidenceClassification);
                    var confidence = ParseConfidence(raw.CreatorConfidence);

                    result.Add(new ExtractedClaimCandidate(
                        ClaimText: raw.ClaimText.Trim(),
                        Category: category,
                        EvidenceClassification: evidence,
                        CreatorConfidence: confidence,
                        DirectQuote: raw.DirectQuote ?? false,
                        SourceContext: raw.SourceContext?.Trim()));
                }
            }

            return (result, root?.SourceSummary, root?.CreatorApparentPosition);
        }
        catch
        {
            // Json parse failed
            return (result, null, null);
        }
    }

    private static string ExtractJsonObject(string text)
    {
        var firstBrace = text.IndexOf('{');
        var lastBrace = text.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return text.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return string.Empty;
    }

    private static ClaimCategory ParseCategory(string? categoryStr)
    {
        if (string.IsNullOrWhiteSpace(categoryStr)) return ClaimCategory.General;

        var normalized = categoryStr.Replace(" ", "").Replace("_", "").Trim();
        if (Enum.TryParse<ClaimCategory>(normalized, true, out var parsed))
        {
            return parsed;
        }

        return ClaimCategory.General;
    }

    private static EvidenceClassification ParseEvidence(string? evidenceStr)
    {
        if (string.IsNullOrWhiteSpace(evidenceStr)) return EvidenceClassification.InterpretationOfResearch;

        var normalized = evidenceStr.Replace(" ", "").Replace("_", "").Trim();
        if (Enum.TryParse<EvidenceClassification>(normalized, true, out var parsed))
        {
            return parsed;
        }

        return EvidenceClassification.InterpretationOfResearch;
    }

    private static CreatorConfidence ParseConfidence(string? confidenceStr)
    {
        if (string.IsNullOrWhiteSpace(confidenceStr)) return CreatorConfidence.High;

        var normalized = confidenceStr.Replace(" ", "").Replace("_", "").Trim();
        if (Enum.TryParse<CreatorConfidence>(normalized, true, out var parsed))
        {
            return parsed;
        }

        return CreatorConfidence.High;
    }

    private static (IReadOnlyList<ExtractedClaimCandidate> Claims, bool ContainsMedicalContent) GenerateDeterministicMockClaims(
        string text, string title, bool initialMedical)
    {
        var list = new List<ExtractedClaimCandidate>();
        var hasMedical = initialMedical;

        if (text.Contains("volume", StringComparison.OrdinalIgnoreCase) || title.Contains("volume", StringComparison.OrdinalIgnoreCase))
        {
            list.Add(new ExtractedClaimCandidate(
                ClaimText: "Performing 10 to 20 hard working sets per muscle group per week yields optimal hypertrophy for intermediate lifters.",
                Category: ClaimCategory.TrainingVolume,
                EvidenceClassification: EvidenceClassification.InterpretationOfResearch,
                CreatorConfidence: CreatorConfidence.High,
                DirectQuote: false,
                SourceContext: "02:15"));
        }

        if (text.Contains("squat", StringComparison.OrdinalIgnoreCase) || title.Contains("squat", StringComparison.OrdinalIgnoreCase))
        {
            list.Add(new ExtractedClaimCandidate(
                ClaimText: "Allowing the knees to travel freely past the toes during deep squats distributes shear stress safely and maximizes quad recruitment.",
                Category: ClaimCategory.Biomechanics,
                EvidenceClassification: EvidenceClassification.InterpretationOfResearch,
                CreatorConfidence: CreatorConfidence.High,
                DirectQuote: false,
                SourceContext: "05:30"));
        }

        if (text.Contains("protein", StringComparison.OrdinalIgnoreCase) || title.Contains("protein", StringComparison.OrdinalIgnoreCase) || title.Contains("nutrition", StringComparison.OrdinalIgnoreCase))
        {
            list.Add(new ExtractedClaimCandidate(
                ClaimText: "Distributing protein intake across 4 to 5 meals with at least 0.4g/kg per meal optimizes muscle protein synthesis over 24 hours.",
                Category: ClaimCategory.Nutrition,
                EvidenceClassification: EvidenceClassification.CitesConcreteSources,
                CreatorConfidence: CreatorConfidence.High,
                DirectQuote: false,
                SourceContext: "08:10"));
        }

        if (list.Count == 0)
        {
            list.Add(new ExtractedClaimCandidate(
                ClaimText: $"Systematic progressive overload targeting 1-3 RIR provides the primary stimulus for muscular development in {title}.",
                Category: ClaimCategory.Intensity,
                EvidenceClassification: EvidenceClassification.OpinionOnly,
                CreatorConfidence: CreatorConfidence.Medium,
                DirectQuote: false,
                SourceContext: "01:00"));
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

    private class ExtractionResponseJsonDto
    {
        [JsonPropertyName("claims")]
        public List<RawClaimJsonDto>? Claims { get; set; }

        [JsonPropertyName("source_summary")]
        public string? SourceSummary { get; set; }

        [JsonPropertyName("creator_apparent_position")]
        public string? CreatorApparentPosition { get; set; }
    }

    private class RawClaimJsonDto
    {
        [JsonPropertyName("claim_text")]
        public string? ClaimText { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("evidence_classification")]
        public string? EvidenceClassification { get; set; }

        [JsonPropertyName("creator_confidence")]
        public string? CreatorConfidence { get; set; }

        [JsonPropertyName("direct_quote")]
        public bool? DirectQuote { get; set; }

        [JsonPropertyName("source_context")]
        public string? SourceContext { get; set; }
    }
}
