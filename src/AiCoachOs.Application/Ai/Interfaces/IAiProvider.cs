using AiCoachOs.Application.Ai.Dtos;

namespace AiCoachOs.Application.Ai.Interfaces;

public interface IAiProvider
{
    string ProviderName { get; }
    string DefaultModelName { get; }

    Task<AiCompletionResponse> GenerateStructuredAsync(
        AiCompletionRequest request, 
        CancellationToken cancellationToken = default);

    // Interface stubs for future milestones (M15 / M16) — Not implemented in M14
    Task<AiCompletionResponse> AnalyzeImageAsync(
        byte[] imageBytes, 
        string prompt, 
        CancellationToken cancellationToken = default);

    Task<AiCompletionResponse> AnalyzeVideoAsync(
        byte[] videoBytes, 
        string prompt, 
        CancellationToken cancellationToken = default);
}
