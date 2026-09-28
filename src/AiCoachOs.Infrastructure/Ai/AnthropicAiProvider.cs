using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Ai.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiCoachOs.Infrastructure.Ai;

public class AnthropicAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly AiSettings _settings;
    private readonly ILogger<AnthropicAiProvider> _logger;

    public string ProviderName => "Anthropic";
    public string DefaultModelName => string.IsNullOrWhiteSpace(_settings.Model) ? "claude-3-5-sonnet-20241022" : _settings.Model;

    public AnthropicAiProvider(
        HttpClient httpClient,
        IOptions<AiSettings> settings,
        ILogger<AnthropicAiProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds : 30);
    }

    public async Task<AiCompletionResponse> GenerateCompletionAsync(
        AiCompletionRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.AnthropicApiKey))
        {
            _logger.LogError("Anthropic API key is not configured.");
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = DefaultModelName,
                ErrorMessage = "Anthropic API key is missing or not configured."
            };
        }

        var model = DefaultModelName;
        var maxTokens = request.MaxTokens ?? _settings.MaxTokens;
        var temperature = request.Temperature ?? _settings.Temperature;

        var requestBody = new
        {
            model,
            max_tokens = maxTokens,
            temperature,
            system = request.SystemPrompt,
            messages = new[]
            {
                new { role = "user", content = request.UserPrompt }
            }
        };

        const string endpoint = "https://api.anthropic.com/v1/messages";
        HttpResponseMessage? responseMessage = null;

        // Attempt request with 1 transient retry
        for (int attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
                httpRequest.Headers.Add("x-api-key", _settings.AnthropicApiKey);
                httpRequest.Headers.Add("anthropic-version", "2023-06-01");
                httpRequest.Content = JsonContent.Create(requestBody);

                responseMessage = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (responseMessage.IsSuccessStatusCode)
                {
                    break;
                }

                if ((int)responseMessage.StatusCode < 500 && (int)responseMessage.StatusCode != 429)
                {
                    // Non-retriable client error
                    var errorBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Anthropic API returned error {StatusCode}: {ErrorBody}", responseMessage.StatusCode, errorBody);
                    return new AiCompletionResponse
                    {
                        IsSuccess = false,
                        ProviderName = ProviderName,
                        ModelName = model,
                        ErrorMessage = $"Anthropic API error: {responseMessage.StatusCode} - {errorBody}"
                    };
                }

                if (attempt == 1)
                {
                    _logger.LogWarning("Anthropic API request failed with status {StatusCode}. Retrying once...", responseMessage.StatusCode);
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (Exception ex) when (attempt == 1 && ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Transient exception on Anthropic API request attempt 1. Retrying once...");
                await Task.Delay(1000, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed calling Anthropic API after retry.");
                return new AiCompletionResponse
                {
                    IsSuccess = false,
                    ProviderName = ProviderName,
                    ModelName = model,
                    ErrorMessage = $"Anthropic connection error: {ex.Message}"
                };
            }
        }

        if (responseMessage == null || !responseMessage.IsSuccessStatusCode)
        {
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = model,
                ErrorMessage = $"Anthropic API request failed after retry with status {responseMessage?.StatusCode}"
            };
        }

        var responseJson = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        return ParseAnthropicResponse(responseJson, model);
    }

    private AiCompletionResponse ParseAnthropicResponse(string responseJson, string model)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            var contentArray = root.GetProperty("content");
            var text = string.Empty;
            foreach (var item in contentArray.EnumerateArray())
            {
                if (item.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "text")
                {
                    text += item.GetProperty("text").GetString();
                }
            }

            int? tokens = null;
            if (root.TryGetProperty("usage", out var usageProp) && usageProp.TryGetProperty("output_tokens", out var outputTokens))
            {
                tokens = outputTokens.GetInt32();
            }

            // Extract JSON snippet from markdown code blocks if wrapped
            var jsonText = ExtractJson(text);

            var structured = JsonSerializer.Deserialize<StructuredAiRecommendationJson>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (structured != null && !string.IsNullOrWhiteSpace(structured.Recommendation))
            {
                var evidenceGuids = new List<Guid>();
                if (structured.EvidenceClaimIds != null)
                {
                    foreach (var idStr in structured.EvidenceClaimIds)
                    {
                        if (Guid.TryParse(idStr, out var parsedGuid))
                        {
                            evidenceGuids.Add(parsedGuid);
                        }
                    }
                }

                return new AiCompletionResponse
                {
                    IsSuccess = true,
                    ProviderName = ProviderName,
                    ModelName = model,
                    TokensUsed = tokens,
                    RecommendationText = structured.Recommendation.Trim(),
                    RationaleText = structured.Rationale.Trim(),
                    ConfidenceStatement = structured.ConfidenceStatement?.Trim() ?? string.Empty,
                    EvidenceClaimRefs = evidenceGuids
                };
            }

            // Fallback if parsing didn't match structured schema
            return new AiCompletionResponse
            {
                IsSuccess = true,
                ProviderName = ProviderName,
                ModelName = model,
                TokensUsed = tokens,
                RecommendationText = text.Trim(),
                RationaleText = "Extracted from AI response",
                ConfidenceStatement = "Unstructured output from model"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed parsing Anthropic response: {RawJson}", responseJson);
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = model,
                ErrorMessage = $"Failed parsing response JSON: {ex.Message}"
            };
        }
    }

    private static string ExtractJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "{}";

        var match = Regex.Match(text, @"```(?:json)?\s*([\s\S]*?)\s*```");
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        var firstBrace = text.IndexOf('{');
        var lastBrace = text.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return text.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return text;
    }
}
