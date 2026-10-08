using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using Microsoft.Extensions.Logging;

namespace AiCoachOs.Infrastructure.Ai;

/// <summary>
/// Shared implementation for providers that expose an OpenAI-compatible
/// <c>POST {endpoint}</c> chat-completions API.
/// </summary>
/// <remarks>
/// Groq, OpenRouter, Cerebras, SambaNova, xAI, and HuggingFace all speak this dialect, and their
/// implementations differed only by endpoint, default model, and the provider name used in log
/// messages. Keeping the request construction, key rotation, and response parsing in one place
/// means a fix to the evidence-citation handling applies to every provider at once.
/// <para>
/// Subclasses supply <see cref="ProviderName"/>, <see cref="ChatCompletionsEndpoint"/>, and
/// <see cref="DefaultModelName"/>, and may add vendor-specific headers.
/// </para>
/// </remarks>
public abstract class OpenAiCompatibleChatProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly RotatingKeySelector _keySelector;
    private readonly ILogger _logger;
    private readonly string _model;

    /// <summary>How long the provider is sidelined after all of its keys are rate limited.</summary>
    protected virtual TimeSpan RateLimitBackoff => TimeSpan.FromSeconds(60);

    protected OpenAiCompatibleChatProvider(
        HttpClient httpClient,
        RotatingKeySelector keySelector,
        ILogger logger,
        string? configuredModel)
    {
        _httpClient = httpClient;
        _keySelector = keySelector;
        _logger = logger;
        _model = string.IsNullOrWhiteSpace(configuredModel) ? FallbackModelName : configuredModel.Trim();
    }

    public abstract string ProviderName { get; }

    public abstract string ChatCompletionsEndpoint { get; }

    /// <summary>
    /// Model used when configuration does not supply one. Kept separate from
    /// <see cref="DefaultModelName"/> because that property reports the resolved model, which may
    /// have come from configuration instead.
    /// </summary>
    protected abstract string FallbackModelName { get; }

    public string DefaultModelName => _model;

    public async Task<AiCompletionResponse> GenerateStructuredAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_keySelector.IsAvailable())
        {
            _logger.LogWarning("{Provider} provider is temporarily unavailable (all keys exhausted).", ProviderName);
            return Failure($"{ProviderName} provider temporarily unavailable.");
        }

        var apiKey = _keySelector.GetCurrentOrNextKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("{Provider} API key is not configured.", ProviderName);
            return Failure($"{ProviderName} API key is missing.");
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
        // These chat endpoints are text-only. Reporting this honestly lets AiProviderRouter fall
        // through to the next provider instead of the caller assuming an image was analysed.
        return Task.FromResult(Failure($"Image analysis is not supported on the {ProviderName} chat endpoint."));
    }

    public Task<AiCompletionResponse> AnalyzeVideoAsync(
        AiVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Failure($"Video analysis is not supported on the {ProviderName} chat endpoint."));
    }

    /// <summary>
    /// Adds vendor-specific headers to the outgoing request. The default adds none.
    /// </summary>
    protected virtual void ConfigureRequest(HttpRequestMessage httpRequest)
    {
    }

    private async Task<AiCompletionResponse> ExecuteWithRetryAsync(
        object requestBody,
        string currentApiKey,
        CancellationToken cancellationToken)
    {
        var activeKey = currentApiKey;

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsEndpoint);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", activeKey);
                ConfigureRequest(httpRequest);
                httpRequest.Content = JsonContent.Create(requestBody);

                var responseMessage = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                    return ParseOpenAiCompatibleResponse(responseBody);
                }

                if (responseMessage.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("{Provider} API returned 429 Rate Limit on attempt {Attempt}.", ProviderName, attempt);
                    var nextKey = _keySelector.RotateToNextKey();

                    if (attempt == 1 && !string.IsNullOrWhiteSpace(nextKey) && nextKey != activeKey)
                    {
                        activeKey = nextKey;
                        continue;
                    }

                    _logger.LogWarning(
                        "All {Provider} API keys exhausted or rate limited. Marking unavailable for {Backoff}.",
                        ProviderName, RateLimitBackoff);
                    _keySelector.MarkTemporarilyUnavailable(RateLimitBackoff);

                    return Failure($"{ProviderName} API rate limit exceeded (429).");
                }

                var errorBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("{Provider} API error {StatusCode}: {ErrorBody}", ProviderName, responseMessage.StatusCode, errorBody);

                return Failure($"{ProviderName} API error: {responseMessage.StatusCode} - {errorBody}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Provider} API request failed on attempt {Attempt}", ProviderName, attempt);
                if (attempt == 1)
                {
                    var nextKey = _keySelector.RotateToNextKey();
                    if (!string.IsNullOrWhiteSpace(nextKey) && nextKey != activeKey)
                    {
                        activeKey = nextKey;
                        continue;
                    }
                }

                return Failure($"{ProviderName} request failed: {ex.Message}");
            }
        }

        return Failure($"{ProviderName} API request failed after retries.");
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
                return Failure($"{ProviderName} returned no choices in response.");
            }

            var firstChoice = choices[0];
            var message = firstChoice.GetProperty("message");
            var rawText = message.GetProperty("content").GetString() ?? string.Empty;

            var jsonMatch = Regex.Match(rawText, @"\{[\s\S]*\}");
            var cleanedJson = jsonMatch.Success ? jsonMatch.Value : rawText;

            // A model can ignore response_format and answer in prose. That is not a transport
            // failure, so the text is returned as an unstructured recommendation rather than
            // failing the call and pushing the router on to the next provider for no reason.
            // Nothing structured is invented: no observations, and no evidence citations.
            var parsed = TryDeserialize(cleanedJson);

            if (parsed == null)
            {
                return new AiCompletionResponse
                {
                    IsSuccess = true,
                    ProviderName = ProviderName,
                    ModelName = _model,
                    TokensUsed = totalTokens,
                    RecommendationText = rawText,
                    RationaleText = $"Rationale parsed from unstructured {ProviderName} output.",
                    ConfidenceStatement = "Generated with standard provider confidence."
                };
            }

            var recList = parsed.Recommendations
                ?? (string.IsNullOrWhiteSpace(parsed.LegacyRecommendation)
                    ? new List<string>()
                    : new List<string> { parsed.LegacyRecommendation });
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

            // Only ids the model actually returned are carried through. Nothing is inferred, so a
            // provider cannot cite evidence that was never supplied to it.
            var claimGuids = ParseEvidenceRefs(parsed);

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
            _logger.LogError(ex, "Failed to parse {Provider} response JSON: {Json}", ProviderName, responseJson);
            return Failure($"Failed to parse {ProviderName} response: {ex.Message}");
        }
    }

    private static StructuredAiRecommendationJson? TryDeserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<StructuredAiRecommendationJson>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static List<Guid> ParseEvidenceRefs(StructuredAiRecommendationJson parsed)
    {
        var claimGuids = new List<Guid>();

        foreach (var refStr in parsed.EvidenceRefs ?? new List<string>())
        {
            if (Guid.TryParse(refStr, out var g))
            {
                claimGuids.Add(g);
            }
        }

        foreach (var refStr in parsed.EvidenceClaimIds ?? new List<string>())
        {
            if (Guid.TryParse(refStr, out var g) && !claimGuids.Contains(g))
            {
                claimGuids.Add(g);
            }
        }

        return claimGuids;
    }

    private AiCompletionResponse Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ProviderName = ProviderName,
        ModelName = _model,
        ErrorMessage = errorMessage
    };
}
