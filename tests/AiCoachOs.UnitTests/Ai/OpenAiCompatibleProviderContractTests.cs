using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Infrastructure.Ai;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AiCoachOs.UnitTests.Ai;

/// <summary>
/// Contract tests for the OpenAI-compatible providers.
/// </summary>
/// <remarks>
/// These assert the wire contract of each provider (endpoint, auth header, request body, response
/// parsing) and the key-rotation behaviour, all against a stubbed <see cref="HttpMessageHandler"/>
/// so no API key or network access is required. That matters because every provider derives from
/// one base class: a defect in the shared request or parsing path would otherwise stay invisible
/// until a live call failed in production.
/// </remarks>
public class OpenAiCompatibleProviderContractTests
{
    private static readonly Guid ClaimId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid SecondClaimId = Guid.Parse("99999999-8888-7777-6666-555555555555");

    /// <summary>
    /// One case per provider, carrying the expected endpoint, model, and a factory so each test
    /// can construct the provider against a stubbed client.
    /// </summary>
    public sealed record ProviderCase(
        string Name,
        string ExpectedEndpoint,
        string ExpectedModel,
        Func<HttpClient, RotatingKeySelector, string?, IAiProvider> Factory);

    public static TheoryData<ProviderCase> AllProviders => new()
    {
        new ProviderCase(
            "Groq",
            "https://api.groq.com/openai/v1/chat/completions",
            "llama-3.3-70b-versatile",
            (http, keys, model) => new GroqAiProvider(http, keys, NullLogger<GroqAiProvider>.Instance, model)),
        new ProviderCase(
            "OpenRouter",
            "https://openrouter.ai/api/v1/chat/completions",
            "openai/gpt-4o-mini",
            (http, keys, model) => new OpenRouterAiProvider(http, keys, NullLogger<OpenRouterAiProvider>.Instance, model)),
        new ProviderCase(
            "Cerebras",
            "https://api.cerebras.ai/v1/chat/completions",
            "qwen-3.8-27b",
            (http, keys, model) => new CerebrasAiProvider(http, keys, NullLogger<CerebrasAiProvider>.Instance, model)),
        new ProviderCase(
            "SambaNova",
            "https://api.sambanova.ai/v1/chat/completions",
            "gpt-oss-120b",
            (http, keys, model) => new SambaNovaAiProvider(http, keys, NullLogger<SambaNovaAiProvider>.Instance, model)),
        new ProviderCase(
            "xAI",
            "https://api.x.ai/v1/chat/completions",
            "grok-4.6",
            (http, keys, model) => new XAiProvider(http, keys, NullLogger<XAiProvider>.Instance, model)),
        new ProviderCase(
            "HuggingFace",
            "https://router.huggingface.co/v1/chat/completions",
            "openai/gpt-oss-120b",
            (http, keys, model) => new HuggingFaceAiProvider(http, keys, NullLogger<HuggingFaceAiProvider>.Instance, model)),
    };

    /// <summary>
    /// Builds a stubbed client plus a disposable key environment variable. The key value is a
    /// placeholder, never a real credential.
    /// </summary>
    private static (HttpClient Client, string EnvVar) CreateClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler,
        string keys = "test-key-1")
    {
        var envVar = "PROVIDER_TEST_KEYS_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(envVar, keys);
        return (new HttpClient(new StubHttpMessageHandler(handler)), envVar);
    }

    private static Task<HttpResponseMessage> JsonAsync(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

    private static string SuccessBody(string content, int tokens = 42) => $$"""
    {
      "choices": [
        { "message": { "role": "assistant", "content": {{JsonSerializer.Serialize(content)}} } }
      ],
      "usage": { "total_tokens": {{tokens}} }
    }
    """;

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task PostsToTheDocumentedEndpointWithABearerToken(ProviderCase provider)
    {
        HttpRequestMessage? captured = null;
        var (client, envVar) = CreateClient((req, _) =>
        {
            captured = req;
            return JsonAsync(SuccessBody("{\"summary\":\"ok\"}"));
        });

        try
        {
            var instance = provider.Factory(client, new RotatingKeySelector(envVar), null);
            var response = await instance.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeTrue();
            response.ProviderName.Should().Be(provider.Name);

            captured.Should().NotBeNull();
            captured!.RequestUri.Should().Be(provider.ExpectedEndpoint);
            captured.Method.Should().Be(HttpMethod.Post);
            captured.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("Bearer", "test-key-1"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task SendsTheConfiguredModelAndJsonResponseFormat(ProviderCase provider)
    {
        string? body = null;
        var (client, envVar) = CreateClient(async (req, ct) =>
        {
            body = await req.Content!.ReadAsStringAsync(ct);
            return await JsonAsync(SuccessBody("{\"summary\":\"ok\"}"));
        });

        try
        {
            var instance = provider.Factory(client, new RotatingKeySelector(envVar), null);
            await instance.GenerateStructuredAsync(new AiCompletionRequest
            {
                SystemPrompt = "You are a coach.",
                UserPrompt = "Assess the client.",
                Temperature = 0.4,
                MaxTokens = 900
            });

            body.Should().NotBeNull();
            using var json = JsonDocument.Parse(body!);
            var root = json.RootElement;

            root.GetProperty("model").GetString().Should().Be(provider.ExpectedModel);
            root.GetProperty("temperature").GetDouble().Should().Be(0.4);
            root.GetProperty("max_tokens").GetInt32().Should().Be(900);

            // Structured coaching output depends on this; without it the model may answer in prose.
            root.GetProperty("response_format").GetProperty("type").GetString().Should().Be("json_object");

            var messages = root.GetProperty("messages").EnumerateArray().ToList();
            messages.Should().HaveCount(2);
            messages[0].GetProperty("role").GetString().Should().Be("system");
            messages[0].GetProperty("content").GetString().Should().Be("You are a coach.");
            messages[1].GetProperty("role").GetString().Should().Be("user");
            messages[1].GetProperty("content").GetString().Should().Be("Assess the client.");

            // Cerebras rejects stream together with json_object, so the client must never stream.
            root.TryGetProperty("stream", out _).Should().BeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void HonoursAModelOverrideFromConfiguration(ProviderCase provider)
    {
        var (client, envVar) = CreateClient((_, _) => JsonAsync(SuccessBody("{\"summary\":\"ok\"}")));

        try
        {
            var instance = provider.Factory(client, new RotatingKeySelector(envVar), "custom/model-v2");

            instance.DefaultModelName.Should().Be("custom/model-v2");
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task ParsesAStructuredRecommendationWithEvidenceAndTokenUsage(ProviderCase provider)
    {
        var content = $$"""
        {
          "summary": "Reduce top-set RIR to 2",
          "observations": ["Client logged RIR 1 on all top sets"],
          "recommendations": ["Reduce load 5%", "Add a back-off set"],
          "rationale": "Sustained RIR 1 indicates accumulated fatigue",
          "confidence_statement": "Moderate",
          "assumptions": ["Logbook is accurate"],
          "missing_high_value_data": ["Sleep data"],
          "evidence_refs": ["{{ClaimId}}"],
          "safety_summary": "No acute risk identified",
          "coach_action_required": true
        }
        """;

        var (client, envVar) = CreateClient((_, _) => JsonAsync(SuccessBody(content, 128)));

        try
        {
            var instance = provider.Factory(client, new RotatingKeySelector(envVar), null);
            var response = await instance.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeTrue();
            response.TokensUsed.Should().Be(128);
            response.RecommendationText.Should().Be("Reduce top-set RIR to 2");
            response.RationaleText.Should().Be("Sustained RIR 1 indicates accumulated fatigue");

            response.StructuredRecommendation.Should().NotBeNull();
            var structured = response.StructuredRecommendation!;
            structured.Summary.Should().Be("Reduce top-set RIR to 2");
            structured.Recommendations.Should().Equal("Reduce load 5%", "Add a back-off set");
            structured.Observations.Should().ContainSingle().Which.Should().Contain("RIR 1");
            structured.MissingHighValueData.Should().ContainSingle().Which.Should().Be("Sleep data");
            structured.SafetySummary.Should().Be("No acute risk identified");
            structured.CoachActionRequired.Should().BeTrue();
            response.EvidenceClaimRefs.Should().Equal(ClaimId);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task CarriesOnlyEvidenceIdsTheModelActuallyReturned(ProviderCase provider)
    {
        // Regression guard for the rule that a provider must never attach a citation the model did
        // not produce. A model citing nothing must yield an empty list, not a plausible one.
        var content = """
        {
          "summary": "No evidence cited",
          "evidence_refs": ["not-a-guid", ""],
          "evidence_claim_ids": []
        }
        """;

        var (client, envVar) = CreateClient((_, _) => JsonAsync(SuccessBody(content)));

        try
        {
            var instance = provider.Factory(client, new RotatingKeySelector(envVar), null);
            var response = await instance.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeTrue();
            response.EvidenceClaimRefs.Should().BeEmpty();
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task AcceptsEvidenceUnderTheAlternativeClaimIdFieldWithoutDuplicating(ProviderCase provider)
    {
        // evidence_claim_ids is declared on the response DTO but was previously ignored entirely.
        var content = $$"""
        {
          "summary": "Cited two ways",
          "evidence_refs": ["{{ClaimId}}"],
          "evidence_claim_ids": ["{{ClaimId}}", "{{SecondClaimId}}"]
        }
        """;

        var (client, envVar) = CreateClient((_, _) => JsonAsync(SuccessBody(content)));

        try
        {
            var instance = provider.Factory(client, new RotatingKeySelector(envVar), null);
            var response = await instance.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.EvidenceClaimRefs.Should().HaveCount(2);
            response.EvidenceClaimRefs.Should().OnlyHaveUniqueItems();
            response.EvidenceClaimRefs.Should().Contain(ClaimId);
            response.EvidenceClaimRefs.Should().Contain(SecondClaimId);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task FallsBackToRawTextWhenTheModelDoesNotReturnJson(ProviderCase provider)
    {
        var (client, envVar) = CreateClient((_, _) => JsonAsync(SuccessBody("The client should deload this week.")));

        try
        {
            var instance = provider.Factory(client, new RotatingKeySelector(envVar), null);
            var response = await instance.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeTrue();
            response.RecommendationText.Should().Be("The client should deload this week.");
            response.StructuredRecommendation.Should().BeNull();
            response.EvidenceClaimRefs.Should().BeEmpty();
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task RotatesToTheNextKeyOnRateLimitAndSucceeds(ProviderCase provider)
    {
        var usedKeys = new List<string?>();
        var (client, envVar) = CreateClient((req, _) =>
        {
            var key = req.Headers.Authorization?.Parameter;
            usedKeys.Add(key);

            return key == "key-first"
                ? Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                {
                    Content = new StringContent("{\"error\":\"rate_limited\"}")
                })
                : JsonAsync(SuccessBody("{\"summary\":\"ok after rotation\"}"));
        }, keys: "key-first,key-second");

        try
        {
            var keySelector = new RotatingKeySelector(envVar);
            var instance = provider.Factory(client, keySelector, null);
            var response = await instance.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeTrue();
            usedKeys.Should().Equal("key-first", "key-second");
            keySelector.IsAvailable().Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task MarksItselfUnavailableWhenEveryKeyIsRateLimited(ProviderCase provider)
    {
        var (client, envVar) = CreateClient(
            (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("{\"error\":\"rate_limited\"}")
            }),
            keys: "only-key");

        try
        {
            var keySelector = new RotatingKeySelector(envVar);
            var instance = provider.Factory(client, keySelector, null);
            var response = await instance.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeFalse();
            response.ErrorMessage.Should().Contain("429");

            // The cooldown is what lets the router skip this provider quickly on the next request.
            keySelector.IsAvailable().Should().BeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task ReportsServerErrorsAsFailuresSoTheRouterFallsThrough(ProviderCase provider)
    {
        var (client, envVar) = CreateClient(
            (_, _) => JsonAsync("{\"error\":\"internal\"}", HttpStatusCode.InternalServerError));

        try
        {
            var instance = provider.Factory(client, new RotatingKeySelector(envVar), null);
            var response = await instance.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeFalse();
            response.ErrorMessage.Should().Contain(provider.Name);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task FailsCleanlyWhenNoKeysAreConfigured(ProviderCase provider)
    {
        // A provider with no key must report failure rather than throw, so the router can move on.
        var (client, envVar) = CreateClient((_, _) => throw new InvalidOperationException("no request expected"));
        Environment.SetEnvironmentVariable(envVar, null);

        var instance = provider.Factory(client, new RotatingKeySelector(envVar), null);
        var response = await instance.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("missing");
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public async Task ReportsImageAndVideoAnalysisAsUnsupportedRatherThanPretending(ProviderCase provider)
    {
        // These are text-only chat endpoints. Saying so honestly is what allows AiProviderRouter to
        // fall through to a provider that can actually handle the request.
        var (client, envVar) = CreateClient((_, _) => throw new InvalidOperationException("no request expected"));

        try
        {
            var instance = provider.Factory(client, new RotatingKeySelector(envVar), null);

            var image = await instance.AnalyzeImageAsync(new AiImageRequest());
            image.IsSuccess.Should().BeFalse();
            image.ErrorMessage.Should().Contain("Image analysis is not supported");

            var video = await instance.AnalyzeVideoAsync(new AiVideoRequest());
            video.IsSuccess.Should().BeFalse();
            video.ErrorMessage.Should().Contain("Video analysis is not supported");
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Theory]
    [MemberData(nameof(AllProviders))]
    public void ExposesTheNameEndpointAndModelUsedForConfiguration(ProviderCase provider)
    {
        // A provider that reports a blank model or a non-absolute endpoint would be misconfigured
        // the first time it was actually called.
        var unusedEnv = "UNUSED_" + Guid.NewGuid().ToString("N");
        var instance = (OpenAiCompatibleChatProvider)provider.Factory(
            new HttpClient(new StubHttpMessageHandler((_, _) => throw new InvalidOperationException("unused"))),
            new RotatingKeySelector(unusedEnv),
            null);

        instance.ProviderName.Should().Be(provider.Name);
        instance.DefaultModelName.Should().Be(provider.ExpectedModel);
        Uri.TryCreate(instance.ChatCompletionsEndpoint, UriKind.Absolute, out var uri).Should().BeTrue();
        uri!.Scheme.Should().Be("https");
    }

    [Fact]
    public async Task RouterFallsThroughToTheNextProviderWhenOneRunsOutOfKeys()
    {
        // The behaviour the fallback chain exists for: an exhausted provider must not end the request.
        var primaryEnv = "CHAIN_PRIMARY_" + Guid.NewGuid().ToString("N");
        var secondaryEnv = "CHAIN_SECONDARY_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(primaryEnv, "primary-placeholder-key");
        Environment.SetEnvironmentVariable(secondaryEnv, "secondary-placeholder-key");

        try
        {
            var alwaysRateLimited = new StubHttpMessageHandler(
                (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                {
                    Content = new StringContent("{\"error\":\"rate_limited\"}")
                }));

            var succeeding = new StubHttpMessageHandler(
                (_, _) => JsonAsync(SuccessBody("{\"summary\":\"from the backup provider\"}")));

            var cerebras = new CerebrasAiProvider(new HttpClient(alwaysRateLimited), new RotatingKeySelector(primaryEnv), NullLogger<CerebrasAiProvider>.Instance);
            var xai = new XAiProvider(new HttpClient(succeeding), new RotatingKeySelector(secondaryEnv), NullLogger<XAiProvider>.Instance);

            var router = new AiProviderRouter(
                new List<(IAiProvider Provider, int Priority, bool Enabled)>
                {
                    (cerebras, 1, true),
                    (xai, 2, true)
                },
                NullLogger<AiProviderRouter>.Instance);

            var response = await router.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeTrue();
            response.ProviderName.Should().Be("xAI");
            response.RecommendationText.Should().Be("from the backup provider");
        }
        finally
        {
            Environment.SetEnvironmentVariable(primaryEnv, null);
            Environment.SetEnvironmentVariable(secondaryEnv, null);
        }
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handler(request, cancellationToken);
        }
    }
}
