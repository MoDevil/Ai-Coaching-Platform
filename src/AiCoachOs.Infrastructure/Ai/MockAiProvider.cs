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
        // Deliberately does NOT scrape claim IDs out of the prompt. Echoing prompt-supplied GUIDs
        // back as EvidenceRefs would present unverified claims as cited evidence.
        var structured = new StructuredRecommendation
        {
            Summary = "MOCK PROVIDER - deterministic placeholder response. Not a real coaching recommendation.",
            Observations = new List<string> { "No observations. This response was produced by MockAiProvider, not a language model." },
            Recommendations = new List<string> { "No recommendations. Configure AiSettings:Provider to route to a real provider." },
            Rationale = "Deterministic mock response for local development and tests only.",
            ConfidenceStatement = "None. The mock provider performs no analysis and has no evidence to draw on.",
            Assumptions = new List<string>(),
            MissingHighValueData = new List<string> { "Everything - the mock provider does not read the request." },
            EvidenceRefs = new List<Guid>(),
            SafetySummary = "Not assessed. The mock provider performs no safety evaluation.",
            CoachActionRequired = true
        };

        var response = new AiCompletionResponse
        {
            IsSuccess = true,
            ProviderName = ProviderName,
            ModelName = DefaultModelName,
            TokensUsed = 0,
            StructuredRecommendation = structured,
            RecommendationText = structured.Summary,
            RationaleText = structured.Rationale,
            ConfidenceStatement = structured.ConfidenceStatement,
            EvidenceClaimRefs = new List<Guid>()
        };

        return Task.FromResult(response);
    }

    public Task<AiCompletionResponse> AnalyzeImageAsync(
        AiImageRequest request, 
        CancellationToken cancellationToken = default)
    {
        var observationResult = new PhysiqueObservationResult
        {
            GeneralObservations = "MOCK PROVIDER - no image analysis performed.",
            ApparentSymmetryNotes = "Not assessed.",
            PostureObservations = "Not assessed.",
            MuscularDevelopmentNotes = "Not assessed.",
            ComparisonNotes = "Not assessed.",
            LimitationsStatement = "MockAiProvider does not inspect images. Configure a vision-capable provider.",
            CoachActionRequired = true,
            ConfidenceStatement = "None. The mock provider performs no analysis."
        };

        var response = new AiCompletionResponse
        {
            IsSuccess = true,
            ProviderName = ProviderName,
            ModelName = DefaultModelName,
            TokensUsed = 0,
            RecommendationText = System.Text.Json.JsonSerializer.Serialize(observationResult),
            RationaleText = "Deterministic mock vision response for local development and tests only.",
            ConfidenceStatement = observationResult.ConfidenceStatement
        };

        return Task.FromResult(response);
    }

    public Task<AiCompletionResponse> AnalyzeVideoAsync(
        AiVideoRequest request, 
        CancellationToken cancellationToken = default)
    {
        var techniqueResult = new AiCoachOs.Application.Videos.Dtos.VideoObservationResult
        {
            MovementExecutionNotes = "MOCK PROVIDER - no video analysis performed.",
            JointAlignmentNotes = "Not assessed.",
            RangeOfMotionNotes = "Not assessed.",
            TempoAndControlNotes = "Not assessed.",
            LimitationsStatement = "MockAiProvider does not inspect video frames. Configure a vision-capable provider.",
            CoachActionRequired = true,
            ConfidenceStatement = "None. The mock provider performs no analysis."
        };

        var response = new AiCompletionResponse
        {
            IsSuccess = true,
            ProviderName = ProviderName,
            ModelName = DefaultModelName,
            TokensUsed = 0,
            RecommendationText = System.Text.Json.JsonSerializer.Serialize(techniqueResult),
            RationaleText = "Deterministic mock technique response for local development and tests only.",
            ConfidenceStatement = techniqueResult.ConfidenceStatement
        };

        return Task.FromResult(response);
    }
}
