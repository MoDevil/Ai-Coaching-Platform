using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Ai.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiCoachOs.Infrastructure.Ai;

public class AiProviderRouter : IAiProvider
{
    private readonly List<(IAiProvider Provider, int Priority, bool Enabled)> _configuredProviders;
    private readonly ILogger<AiProviderRouter> _logger;

    public string ProviderName => "Router";
    public string DefaultModelName => "multi-provider-router";

    public IReadOnlyList<IAiProvider> ActiveProviders => _configuredProviders
        .Where(p => p.Enabled)
        .OrderBy(p => p.Priority)
        .Select(p => p.Provider)
        .ToList();

    public AiProviderRouter(
        IEnumerable<IAiProvider> providers,
        IOptions<AiProviderOptions> options,
        ILogger<AiProviderRouter> logger)
    {
        _logger = logger;
        _configuredProviders = new List<(IAiProvider Provider, int Priority, bool Enabled)>();

        var providerList = providers.Where(p => p is not AiProviderRouter).ToList();
        var configMap = options.Value?.Providers?.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase) 
            ?? new Dictionary<string, ProviderConfig>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in providerList)
        {
            if (configMap.TryGetValue(p.ProviderName, out var cfg))
            {
                _configuredProviders.Add((p, cfg.Priority, cfg.Enabled));
            }
            else
            {
                // Unconfigured providers default to enabled with low priority
                _configuredProviders.Add((p, 999, true));
            }
        }
    }

    public AiProviderRouter(
        IEnumerable<(IAiProvider Provider, int Priority, bool Enabled)> explicitProviders,
        ILogger<AiProviderRouter> logger)
    {
        _logger = logger;
        _configuredProviders = explicitProviders.ToList();
    }

    public async Task<AiCompletionResponse> GenerateStructuredAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var providers = _configuredProviders
            .Where(p => p.Enabled)
            .OrderBy(p => p.Priority)
            .Select(p => p.Provider)
            .ToList();

        if (providers.Count == 0)
        {
            _logger.LogWarning("No AI providers configured or enabled in router.");
            return BuildAllUnavailableResponse();
        }

        foreach (var provider in providers)
        {
            try
            {
                _logger.LogInformation("Attempting AI generation with provider: {ProviderName} (Model: {Model})", provider.ProviderName, provider.DefaultModelName);
                var response = await provider.GenerateStructuredAsync(request, cancellationToken);

                if (response.IsSuccess)
                {
                    _logger.LogInformation("AI Provider {ProviderName} succeeded.", provider.ProviderName);
                    return response;
                }

                _logger.LogWarning("AI Provider {ProviderName} failed: {ErrorMessage}. Trying next provider in priority order.", provider.ProviderName, response.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception calling AI Provider {ProviderName}. Trying next provider in priority order.", provider.ProviderName);
            }
        }

        _logger.LogError("All configured AI providers failed or were unavailable.");
        return BuildAllUnavailableResponse();
    }

    public async Task<AiCompletionResponse> AnalyzeImageAsync(
        AiImageRequest request,
        CancellationToken cancellationToken = default)
    {
        var providers = _configuredProviders
            .Where(p => p.Enabled)
            .OrderBy(p => p.Priority)
            .Select(p => p.Provider)
            .ToList();

        if (providers.Count == 0)
        {
            return BuildAllUnavailableResponse();
        }

        foreach (var provider in providers)
        {
            try
            {
                var response = await provider.AnalyzeImageAsync(request, cancellationToken);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("AI Provider {ProviderName} succeeded image analysis.", provider.ProviderName);
                    return response;
                }

                _logger.LogWarning("AI Provider {ProviderName} failed image analysis: {ErrorMessage}.", provider.ProviderName, response.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception during image analysis with provider {ProviderName}.", provider.ProviderName);
            }
        }

        return BuildAllUnavailableResponse();
    }

    public async Task<AiCompletionResponse> AnalyzeVideoAsync(
        AiVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        var providers = _configuredProviders
            .Where(p => p.Enabled)
            .OrderBy(p => p.Priority)
            .Select(p => p.Provider)
            .ToList();

        if (providers.Count == 0)
        {
            return BuildAllUnavailableResponse();
        }

        foreach (var provider in providers)
        {
            try
            {
                var response = await provider.AnalyzeVideoAsync(request, cancellationToken);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("AI Provider {ProviderName} succeeded video analysis.", provider.ProviderName);
                    return response;
                }

                _logger.LogWarning("AI Provider {ProviderName} failed video analysis: {ErrorMessage}.", provider.ProviderName, response.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception during video analysis with provider {ProviderName}.", provider.ProviderName);
            }
        }

        return BuildAllUnavailableResponse();
    }

    private static AiCompletionResponse BuildAllUnavailableResponse()
    {
        return new AiCompletionResponse
        {
            IsSuccess = false,
            ProviderName = "Router",
            ModelName = "none",
            ErrorMessage = "All providers unavailable"
        };
    }
}
