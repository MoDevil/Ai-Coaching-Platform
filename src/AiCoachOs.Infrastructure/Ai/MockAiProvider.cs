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
        var citedGuids = matches.Select(m => Guid.Parse(m.Value)).Distinct().ToList();

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
        AiVideoRequest request, 
        CancellationToken cancellationToken = default)
    {
        var techniqueResult = new AiCoachOs.Application.Videos.Dtos.VideoObservationResult
        {
            MovementExecutionNotes = "Bar path remains visual and balanced throughout the movement sequence.",
            JointAlignmentNotes = "Knee tracking and hip hinge visual alignment observed across frames.",
            RangeOfMotionNotes = "Full movement excursion observable across extracted sequential frames.",
            TempoAndControlNotes = "Controlled eccentric tempo and stable concentric turnaround visual across frames.",
            LimitationsStatement = "Visual observations from video frames are qualitative movement cues and do not constitute biomechanical lab measurement or medical diagnosis.",
            CoachActionRequired = true,
            ConfidenceStatement = "Qualitative technique observation completed based on available 2D video frames."
        };

        var response = new AiCompletionResponse
        {
            IsSuccess = true,
            ProviderName = ProviderName,
            ModelName = DefaultModelName,
            TokensUsed = 450,
            RecommendationText = System.Text.Json.JsonSerializer.Serialize(techniqueResult),
            RationaleText = "Deterministic mock technique observation completed across video frames.",
            ConfidenceStatement = techniqueResult.ConfidenceStatement
        };

        return Task.FromResult(response);
    }
}
