using System.Text.RegularExpressions;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Photos.Dtos;

namespace AiCoachOs.Infrastructure.Ai;

public class MockAiProvider : IAiProvider
{
    public string ProviderName => "Mock";
    public string DefaultModelName => "mock-reasoning-v1";

    public Task<AiCompletionResponse> GenerateStructuredAsync(
        AiCompletionRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Extract any GUIDs present in the user prompt (representing eligible knowledge claim IDs)
        var guidRegex = new Regex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
        var matches = guidRegex.Matches(request.UserPrompt);
        var citedGuids = matches.Take(2).Select(m => Guid.Parse(m.Value)).ToList();

        var structured = new StructuredRecommendation
        {
            Summary = "Recommended coaching review based on client constraints and active deterministic profile.",
            Observations = new List<string> { "Client training profile and recent session performance evaluated against recovery markers." },
            Recommendations = new List<string> { "Maintain consistent progressive overload while adhering to programmed volume boundaries." },
            Rationale = "Evidence demonstrates that individualizing stimulus and managing recovery boundaries optimizes progressive adaptation.",
            ConfidenceStatement = "High confidence based on structured scientific knowledge base and verified client memory snapshot.",
            Assumptions = new List<string> { "Adequate recovery and nutrition support baseline targets." },
            MissingHighValueData = new List<string>(),
            EvidenceRefs = citedGuids,
            SafetySummary = null,
            CoachActionRequired = true
        };

        var response = new AiCompletionResponse
        {
            IsSuccess = true,
            ProviderName = ProviderName,
            ModelName = DefaultModelName,
            TokensUsed = 350,
            StructuredRecommendation = structured,
            RecommendationText = structured.Summary,
            RationaleText = structured.Rationale,
            ConfidenceStatement = structured.ConfidenceStatement,
            EvidenceClaimRefs = citedGuids
        };

        return Task.FromResult(response);
    }

    public Task<AiCompletionResponse> AnalyzeImageAsync(
        AiImageRequest request, 
        CancellationToken cancellationToken = default)
    {
        var hasBaseline = request.Images.Count > 1;

        var observationResult = new PhysiqueObservationResult
        {
            GeneralObservations = "Client presents consistent framing and standing posture under standard lighting.",
            ApparentSymmetryNotes = "Bilateral shoulder and clavicle height appear visually aligned.",
            PostureObservations = "Standing sagittal and frontal plane alignment visually observable.",
            MuscularDevelopmentNotes = "Upper and lower torso musculature shows clear visual definition.",
            ComparisonNotes = hasBaseline
                ? "Qualitative comparison against baseline demonstrates visual progress in upper body development."
                : "Baseline comparison was unavailable.",
            LimitationsStatement = "Visual observations are qualitative estimates from 2D photos and do not constitute diagnostic or quantitative composition measurement.",
            CoachActionRequired = true,
            ConfidenceStatement = "Qualitative observational assessment based on available visual lighting and posture."
        };

        var response = new AiCompletionResponse
        {
            IsSuccess = true,
            ProviderName = ProviderName,
            ModelName = DefaultModelName,
            TokensUsed = 400,
            RecommendationText = System.Text.Json.JsonSerializer.Serialize(observationResult),
            RationaleText = "Deterministic mock vision analysis completed.",
            ConfidenceStatement = observationResult.ConfidenceStatement
        };

        return Task.FromResult(response);
    }

    public Task<AiCompletionResponse> AnalyzeVideoAsync(
        byte[] videoBytes, 
        string prompt, 
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Video analysis is reserved for future milestones (M16) and not implemented in M15.");
    }
}
