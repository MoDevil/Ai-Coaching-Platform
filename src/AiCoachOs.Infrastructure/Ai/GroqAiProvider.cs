using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Ai;

public class GroqAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly RotatingKeySelector _keySelector;
    private readonly ILogger<GroqAiProvider> _logger;
    private readonly string _model;

    public string ProviderName => "Groq";
    public string DefaultModelName => _model;

    public GroqAiProvider(
        HttpClient httpClient,
        RotatingKeySelector keySelector,
        ILogger<GroqAiProvider> logger,
        string? model = null)
    {
        _httpClient = httpClient;
        _keySelector = keySelector;
        _logger = logger;
        _model = string.IsNullOrWhiteSpace(model) ? "llama-3.3-70b-versatile" : model.Trim();
    }

    public async Task<AiCompletionResponse> GenerateStructuredAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_keySelector.IsAvailable())
        {
            _logger.LogWarning("Groq provider is temporarily unavailable (all keys exhausted).");
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = _model,
                ErrorMessage = "Groq provider temporarily unavailable."
            };
        }

        var apiKey = _keySelector.GetCurrentOrNextKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Groq API key is not configured.");
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = _model,
                ErrorMessage = "Groq API key is missing."
            };
        }

        var messages = new List<object>();
        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new { role = "system", content = request.SystemPrompt });
        }
        messages.Add(new { role = "user", content = request.UserPrompt });

        var requestBody = new
        {
            model = _model,
            messages = messages.ToArray(),
            temperature = request.Temperature ?? 0.2,
            max_tokens = request.MaxTokens ?? 2048,
            response_format = new { type = "json_object" }
        };

        return await ExecuteWithRetryAsync(requestBody, apiKey, cancellationToken);
    }

    public Task<AiCompletionResponse> AnalyzeImageAsync(
        AiImageRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AiCompletionResponse
        {
            IsSuccess = false,
            ProviderName = ProviderName,
            ModelName = _model,
            ErrorMessage = "Image analysis is not supported on standard Groq model."
        });
    }

    public Task<AiCompletionResponse> AnalyzeVideoAsync(
        AiVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AiCompletionResponse
        {
            IsSuccess = false,
            ProviderName = ProviderName,
            ModelName = _model,
            ErrorMessage = "Video analysis is not supported on standard Groq model."
        });
    }

    private async Task<AiCompletionResponse> ExecuteWithRetryAsync(
        object requestBody,
        string currentApiKey,
        CancellationToken cancellationToken)
    {
        var activeKey = currentApiKey;
        const string endpoint = "https://api.groq.com/openai/v1/chat/completions";

        for (int attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", activeKey);
                httpRequest.Content = JsonContent.Create(requestBody);

                var responseMessage = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                    return ParseOpenAiCompatibleResponse(responseBody);
                }

                if ((int)responseMessage.StatusCode == 429)
                {
                    _logger.LogWarning("Groq API returned 429 Rate Limit on attempt {Attempt}.", attempt);
                    var nextKey = _keySelector.RotateToNextKey();

                    if (attempt == 1 && !string.IsNullOrWhiteSpace(nextKey) && nextKey != activeKey)
                    {
                        activeKey = nextKey;
                        continue;
                    }

                    _logger.LogWarning("All Groq API keys exhausted or rate limited. Marking unavailable for 60 seconds.");
                    _keySelector.MarkTemporarilyUnavailable(TimeSpan.FromSeconds(60));

                    return new AiCompletionResponse
                    {
                        IsSuccess = false,
                        ProviderName = ProviderName,
                        ModelName = _model,
                        ErrorMessage = "Groq API rate limit exceeded (429)."
                    };
                }

                var errorBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Groq API error {StatusCode}: {ErrorBody}", responseMessage.StatusCode, errorBody);

                return new AiCompletionResponse
                {
                    IsSuccess = false,
                    ProviderName = ProviderName,
                    ModelName = _model,
                    ErrorMessage = $"Groq API error: {responseMessage.StatusCode} - {errorBody}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groq API request failed on attempt {Attempt}", attempt);
                if (attempt == 1)
                {
                    var nextKey = _keySelector.RotateToNextKey();
                    if (!string.IsNullOrWhiteSpace(nextKey) && nextKey != activeKey)
                    {
                        activeKey = nextKey;
                        continue;
                    }
                }

                return new AiCompletionResponse
                {
                    IsSuccess = false,
                    ProviderName = ProviderName,
                    ModelName = _model,
                    ErrorMessage = $"Groq request failed: {ex.Message}"
                };
            }
        }

        return new AiCompletionResponse
        {
            IsSuccess = false,
            ProviderName = ProviderName,
            ModelName = _model,
            ErrorMessage = "Groq API request failed after retries."
        };
    }

    private AiCompletionResponse ParseOpenAiCompatibleResponse(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            int? totalTokens = null;
            if (root.TryGetProperty("usage", out var usage) && usage.TryGetProperty("total_tokens", out var tokensProp))
            {
                totalTokens = tokensProp.GetInt32();
            }

            if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                return new AiCompletionResponse
                {
                    IsSuccess = false,
                    ProviderName = ProviderName,
                    ModelName = _model,
                    ErrorMessage = "Groq returned no choices in response."
                };
            }

            var firstChoice = choices[0];
            var message = firstChoice.GetProperty("message");
            var rawText = message.GetProperty("content").GetString() ?? string.Empty;

            var jsonMatch = Regex.Match(rawText, @"\{[\s\S]*\}");
            var cleanedJson = jsonMatch.Success ? jsonMatch.Value : rawText;

            var parsed = JsonSerializer.Deserialize<StructuredAiRecommendationJson>(cleanedJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed == null)
            {
                return new AiCompletionResponse
                {
                    IsSuccess = true,
                    ProviderName = ProviderName,
                    ModelName = _model,
                    TokensUsed = totalTokens,
                    RecommendationText = rawText,
                    RationaleText = "Rationale parsed from unstructured Groq output.",
                    ConfidenceStatement = "Generated with standard provider confidence."
                };
            }

            var recList = parsed.Recommendations ?? (string.IsNullOrWhiteSpace(parsed.LegacyRecommendation) ? new List<string>() : new List<string> { parsed.LegacyRecommendation });
            var summary = parsed.Summary ?? (recList.Count > 0 ? recList[0] : "Coaching recommendation generated.");
            var rationale = parsed.Rationale ?? "Evidence-based coaching rationale.";
            var confidence = parsed.ConfidenceStatement ?? "Standard confidence statement.";

            var structured = new StructuredRecommendation
            {
                Summary = summary,
                Observations = parsed.Observations ?? new List<string>(),
                Recommendations = recList,
                Rationale = rationale,
                ConfidenceStatement = confidence,
                Assumptions = parsed.Assumptions ?? new List<string>(),
                MissingHighValueData = parsed.MissingHighValueData ?? new List<string>(),
                SafetySummary = parsed.SafetySummary,
                CoachActionRequired = parsed.CoachActionRequired ?? true
            };

            var claimGuids = new List<Guid>();
            if (parsed.EvidenceRefs != null)
            {
                foreach (var refStr in parsed.EvidenceRefs)
                {
                    if (Guid.TryParse(refStr, out var g)) claimGuids.Add(g);
                }
            }

            return new AiCompletionResponse
            {
                IsSuccess = true,
                ProviderName = ProviderName,
                ModelName = _model,
                TokensUsed = totalTokens,
                StructuredRecommendation = structured,
                RecommendationText = summary,
                RationaleText = rationale,
                ConfidenceStatement = confidence,
                EvidenceClaimRefs = claimGuids
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Groq response JSON: {Json}", responseJson);
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = _model,
                ErrorMessage = $"Failed to parse Groq response: {ex.Message}"
            };
        }
    }
}
