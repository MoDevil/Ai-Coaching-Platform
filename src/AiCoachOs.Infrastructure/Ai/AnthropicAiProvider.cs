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
    public string DefaultModelName => string.IsNullOrWhiteSpace(_settings.Model) ? "claude-sonnet-4-6" : _settings.Model;

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

    public async Task<AiCompletionResponse> GenerateStructuredAsync(
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

    public async Task<AiCompletionResponse> AnalyzeImageAsync(
        AiImageRequest request, 
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

        var model = request.Model ?? DefaultModelName;
        var maxTokens = request.MaxTokens ?? _settings.MaxTokens;

        var contentBlocks = new List<object>();

        foreach (var img in request.Images)
        {
            contentBlocks.Add(new
            {
                type = "image",
                source = new
                {
                    type = "base64",
                    media_type = img.MimeType,
                    data = Convert.ToBase64String(img.ImageData)
                }
            });
        }

        contentBlocks.Add(new
        {
            type = "text",
            text = request.UserPrompt
        });

        var requestBody = new
        {
            model,
            max_tokens = maxTokens,
            temperature = 0.2,
            system = request.SystemPrompt,
            messages = new[]
            {
                new { role = "user", content = contentBlocks }
            }
        };

        const string endpoint = "https://api.anthropic.com/v1/messages";
        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
            httpRequest.Headers.Add("x-api-key", _settings.AnthropicApiKey);
            httpRequest.Headers.Add("anthropic-version", "2023-06-01");
            httpRequest.Content = JsonContent.Create(requestBody);

            var responseMessage = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var errorBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Anthropic Vision API error {StatusCode}: {ErrorBody}", responseMessage.StatusCode, errorBody);
                return new AiCompletionResponse
                {
                    IsSuccess = false,
                    ProviderName = ProviderName,
                    ModelName = model,
                    ErrorMessage = $"Anthropic Vision API error: {responseMessage.StatusCode} - {errorBody}"
                };
            }

            var responseJson = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            return ParseAnthropicResponse(responseJson, model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed calling Anthropic Vision API.");
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = model,
                ErrorMessage = $"Anthropic Vision API connection error: {ex.Message}"
            };
        }
    }

    public async Task<AiCompletionResponse> AnalyzeVideoAsync(
        AiVideoRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.AnthropicApiKey))
        {
            _logger.LogError("Anthropic API key is not configured for video frame analysis.");
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = DefaultModelName,
                ErrorMessage = "Anthropic API key is missing or not configured."
            };
        }

        var model = request.Model ?? DefaultModelName;
        var maxTokens = request.MaxTokens ?? _settings.MaxTokens;

        var contentBlocks = new List<object>();

        foreach (var frame in request.Frames.OrderBy(f => f.FrameIndex))
        {
            contentBlocks.Add(new
            {
                type = "image",
                source = new
                {
                    type = "base64",
                    media_type = frame.MimeType,
                    data = Convert.ToBase64String(frame.ImageData)
                }
            });

            contentBlocks.Add(new
            {
                type = "text",
                text = $"[Frame {frame.FrameIndex} at {frame.TimestampSeconds:F2}s]"
            });
        }

        contentBlocks.Add(new
        {
            type = "text",
            text = request.UserPrompt
        });

        var requestBody = new
        {
            model,
            max_tokens = maxTokens,
            temperature = 0.2,
            system = request.SystemPrompt,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = contentBlocks
                }
            }
        };

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            httpRequest.Headers.Add("x-api-key", _settings.AnthropicApiKey);
            httpRequest.Headers.Add("anthropic-version", "2023-06-01");
            httpRequest.Content = JsonContent.Create(requestBody);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Anthropic Vision API returned {StatusCode}: {ResponseJson}", response.StatusCode, responseJson);
                return new AiCompletionResponse
                {
                    IsSuccess = false,
                    ProviderName = ProviderName,
                    ModelName = model,
                    ErrorMessage = $"Anthropic Vision API returned error: {response.StatusCode} - {responseJson}"
                };
            }

            return ParseAnthropicResponse(responseJson, model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Anthropic Vision API for video analysis.");
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = model,
                ErrorMessage = $"Anthropic Vision API connection error: {ex.Message}"
            };
        }
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

            if (structured != null)
            {
                var evidenceGuids = new List<Guid>();
                var rawIds = structured.EvidenceRefs ?? structured.EvidenceClaimIds;
                if (rawIds != null)
                {
                    foreach (var idStr in rawIds)
                    {
                        if (Guid.TryParse(idStr, out var parsedGuid))
                        {
                            evidenceGuids.Add(parsedGuid);
                        }
                    }
                }

                var summary = !string.IsNullOrWhiteSpace(structured.Summary)
                    ? structured.Summary.Trim()
                    : (!string.IsNullOrWhiteSpace(structured.LegacyRecommendation) ? structured.LegacyRecommendation.Trim() : text.Trim());

                var rationale = !string.IsNullOrWhiteSpace(structured.Rationale)
                    ? structured.Rationale.Trim()
                    : "Extracted from AI response";

                var confidence = !string.IsNullOrWhiteSpace(structured.ConfidenceStatement)
                    ? structured.ConfidenceStatement.Trim()
                    : "Evaluated by AI reasoning provider";

                var structuredRec = new StructuredRecommendation
                {
                    Summary = summary,
                    Observations = structured.Observations ?? new List<string>(),
                    Recommendations = structured.Recommendations ?? new List<string>(),
                    Rationale = rationale,
                    ConfidenceStatement = confidence,
                    Assumptions = structured.Assumptions ?? new List<string>(),
                    MissingHighValueData = structured.MissingHighValueData ?? new List<string>(),
                    EvidenceRefs = evidenceGuids,
                    SafetySummary = structured.SafetySummary,
                    CoachActionRequired = structured.CoachActionRequired ?? true
                };

                return new AiCompletionResponse
                {
                    IsSuccess = true,
                    ProviderName = ProviderName,
                    ModelName = model,
                    TokensUsed = tokens,
                    StructuredRecommendation = structuredRec,
                    RecommendationText = summary,
                    RationaleText = rationale,
                    ConfidenceStatement = confidence,
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
