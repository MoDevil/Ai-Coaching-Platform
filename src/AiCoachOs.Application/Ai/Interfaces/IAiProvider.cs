using AiCoachOs.Application.Ai.Dtos;

namespace AiCoachOs.Application.Ai.Interfaces;

public interface IAiProvider
{
    string ProviderName { get; }
    string DefaultModelName { get; }
    Task<AiCompletionResponse> GenerateCompletionAsync(
        AiCompletionRequest request, 
        CancellationToken cancellationToken = default);
}
