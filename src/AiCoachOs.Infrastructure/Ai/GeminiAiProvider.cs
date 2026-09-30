using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Ai.Models;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Ai;

public class GeminiAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly RotatingKeySelector _keySelector;
    private readonly ILogger<GeminiAiProvider> _logger;
    private readonly string _model;

    public string ProviderName => "Gemini";
    public string DefaultModelName => _model;

    public GeminiAiProvider(
        HttpClient httpClient,
        RotatingKeySelector keySelector,
        ILogger<GeminiAiProvider> logger,
        string? model = null)
    {
        _httpClient = httpClient;
        _keySelector = keySelector;
        _logger = logger;
        _model = string.IsNullOrWhiteSpace(model) ? "gemini-2.0-flash" : model.Trim();
    }

    public async Task<AiCompletionResponse> GenerateStructuredAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_keySelector.IsAvailable())
        {
            _logger.LogWarning("Gemini provider is temporarily unavailable (all keys exhausted).");
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = _model,
                ErrorMessage = "Gemini provider temporarily unavailable."
            };
        }

        var apiKey = _keySelector.GetCurrentOrNextKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured.");
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = _model,
                ErrorMessage = "Gemini API key is missing."
            };
        }

        var temperature = request.Temperature ?? 0.2;
        var maxTokens = request.MaxTokens ?? 2048;

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = request.UserPrompt }
                    }
                }
            },
            systemInstruction = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : new
            {
                parts = new object[]
                {
                    new { text = request.SystemPrompt }
                }
            },
            generationConfig = new
            {
                temperature,
                maxOutputTokens = maxTokens,
                responseMimeType = "application/json"
            }
        };

        return await ExecuteWithRetryAsync(requestBody, apiKey, cancellationToken);
    }

    public async Task<AiCompletionResponse> AnalyzeImageAsync(
        AiImageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_keySelector.IsAvailable())
        {
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = _model,
                ErrorMessage = "Gemini provider temporarily unavailable."
            };
        }

        var apiKey = _keySelector.GetCurrentOrNextKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = _model,
                ErrorMessage = "Gemini API key is missing."
            };
        }

        var parts = new List<object>();
        foreach (var img in request.Images)
        {
            parts.Add(new
            {
                inlineData = new
                {
                    mimeType = img.MimeType,
                    data = Convert.ToBase64String(img.ImageData)
                }
            });
        }
        parts.Add(new { text = request.UserPrompt });

        var requestBody = new
        {
            contents = new[]
            {
                new { role = "user", parts = parts.ToArray() }
            },
            systemInstruction = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : new
            {
                parts = new object[]
                {
                    new { text = request.SystemPrompt }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                maxOutputTokens = request.MaxTokens ?? 2048,
                responseMimeType = "application/json"
            }
        };

        return await ExecuteWithRetryAsync(requestBody, apiKey, cancellationToken);
    }

    public async Task<AiCompletionResponse> AnalyzeVideoAsync(
        AiVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_keySelector.IsAvailable())
        {
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = _model,
                ErrorMessage = "Gemini provider temporarily unavailable."
            };
        }

        var apiKey = _keySelector.GetCurrentOrNextKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = _model,
                ErrorMessage = "Gemini API key is missing."
            };
        }

        var parts = new List<object>();
        foreach (var frame in request.Frames.OrderBy(f => f.FrameIndex))
        {
            parts.Add(new
            {
                inlineData = new
                {
                    mimeType = frame.MimeType,
                    data = Convert.ToBase64String(frame.ImageData)
                }
            });
        }
        parts.Add(new { text = request.UserPrompt });

        var requestBody = new
        {
            contents = new[]
            {
                new { role = "user", parts = parts.ToArray() }
            },
            systemInstruction = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : new
            {
                parts = new object[]
                {
                    new { text = request.SystemPrompt }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                maxOutputTokens = request.MaxTokens ?? 2048,
                responseMimeType = "application/json"
            }
        };

        return await ExecuteWithRetryAsync(requestBody, apiKey, cancellationToken);
    }

    private async Task<AiCompletionResponse> ExecuteWithRetryAsync(
        object requestBody,
        string currentApiKey,
        CancellationToken cancellationToken)
    {
        var activeKey = currentApiKey;

        for (int attempt = 1; attempt <= 2; attempt++)
        {
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={activeKey}";

            try
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
                httpRequest.Content = JsonContent.Create(requestBody);

                var responseMessage = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                    return ParseGeminiResponse(responseBody);
                }

                if ((int)responseMessage.StatusCode == 429)
                {
                    _logger.LogWarning("Gemini API returned 429 Rate Limit on attempt {Attempt}.", attempt);
                    var nextKey = _keySelector.RotateToNextKey();

                    if (attempt == 1 && !string.IsNullOrWhiteSpace(nextKey) && nextKey != activeKey)
                    {
                        activeKey = nextKey;
                        continue; // retry once with next key
                    }

                    _logger.LogWarning("All Gemini API keys exhausted or rate limited. Marking unavailable for 60 seconds.");
                    _keySelector.MarkTemporarilyUnavailable(TimeSpan.FromSeconds(60));

                    return new AiCompletionResponse
                    {
                        IsSuccess = false,
                        ProviderName = ProviderName,
                        ModelName = _model,
                        ErrorMessage = "Gemini API rate limit exceeded (429)."
                    };
                }

                var errorBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API error {StatusCode}: {ErrorBody}", responseMessage.StatusCode, errorBody);

                return new AiCompletionResponse
                {
                    IsSuccess = false,
                    ProviderName = ProviderName,
                    ModelName = _model,
                    ErrorMessage = $"Gemini API error: {responseMessage.StatusCode} - {errorBody}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini API request failed on attempt {Attempt}", attempt);
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
                    ErrorMessage = $"Gemini request failed: {ex.Message}"
                };
            }
        }

        return new AiCompletionResponse
        {
            IsSuccess = false,
            ProviderName = ProviderName,
            ModelName = _model,
            ErrorMessage = "Gemini API request failed after retries."
        };
    }

    private AiCompletionResponse ParseGeminiResponse(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            int? totalTokens = null;
            if (root.TryGetProperty("usageMetadata", out var usage) && usage.TryGetProperty("totalTokenCount", out var countProp))
            {
                totalTokens = countProp.GetInt32();
            }

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                return new AiCompletionResponse
                {
                    IsSuccess = false,
                    ProviderName = ProviderName,
                    ModelName = _model,
                    ErrorMessage = "Gemini returned no candidates in response."
                };
            }

            var firstCandidate = candidates[0];
            var content = firstCandidate.GetProperty("content");
            var parts = content.GetProperty("parts");

            var rawText = string.Empty;
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var textProp))
                {
                    rawText += textProp.GetString();
                }
            }

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
                    RationaleText = "Rationale parsed from unstructured Gemini output.",
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
            _logger.LogError(ex, "Failed to parse Gemini response JSON: {Json}", responseJson);
            return new AiCompletionResponse
            {
                IsSuccess = false,
                ProviderName = ProviderName,
                ModelName = _model,
                ErrorMessage = $"Failed to parse Gemini response: {ex.Message}"
            };
        }
    }
}
