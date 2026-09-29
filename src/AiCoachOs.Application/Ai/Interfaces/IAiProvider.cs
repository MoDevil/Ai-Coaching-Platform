using AiCoachOs.Application.Ai.Dtos;

namespace AiCoachOs.Application.Ai.Interfaces;

public interface IAiProvider
{
    string ProviderName { get; }
    string DefaultModelName { get; }

    Task<AiCompletionResponse> GenerateStructuredAsync(
        AiCompletionRequest request, 
        CancellationToken cancellationToken = default);

    Task<AiCompletionResponse> AnalyzeImageAsync(
        AiImageRequest request, 
        CancellationToken cancellationToken = default);

    Task<AiCompletionResponse> AnalyzeVideoAsync(
        AiVideoRequest request, 
        CancellationToken cancellationToken = default);
}
