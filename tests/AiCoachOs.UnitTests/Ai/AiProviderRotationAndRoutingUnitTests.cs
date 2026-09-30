using System.Net;
using System.Text.Json;
using AiCoachOs.Application.Ai.Dtos;
using AiCoachOs.Application.Ai.Interfaces;
using AiCoachOs.Application.Ai.Models;
using AiCoachOs.Infrastructure.Ai;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AiCoachOs.UnitTests.Ai;

public class AiProviderRotationAndRoutingUnitTests
{
    #region RotatingKeySelector Tests

    [Fact]
    public void RotatingKeySelector_RoundRobin_CyclesThroughKeysInOrder()
    {
        var envVar = "TEST_ROTATING_KEYS_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(envVar, "key-1, key-2, key-3");

        try
        {
            var selector = new RotatingKeySelector(envVar);
            selector.GetKeys().Should().Equal("key-1", "key-2", "key-3");

            selector.GetCurrentOrNextKey().Should().Be("key-1");
            selector.GetCurrentOrNextKey().Should().Be("key-2");
            selector.GetCurrentOrNextKey().Should().Be("key-3");
            selector.GetCurrentOrNextKey().Should().Be("key-1");
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Fact]
    public void RotatingKeySelector_RotateToNextKey_AdvancesAndReturnsNext()
    {
        var envVar = "TEST_ROTATING_KEYS_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(envVar, "key-alpha,key-beta");

        try
        {
            var selector = new RotatingKeySelector(envVar);
            var first = selector.GetCurrentOrNextKey();
            first.Should().Be("key-alpha");

            var next = selector.RotateToNextKey();
            next.Should().Be("key-beta");

            var nextAgain = selector.RotateToNextKey();
            nextAgain.Should().Be("key-alpha");
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Fact]
    public void RotatingKeySelector_WhenEmptyOrMissing_ReturnsNullWithoutThrowing()
    {
        var envVar = "TEST_EMPTY_KEYS_" + Guid.NewGuid().ToString("N");
        var selector = new RotatingKeySelector(envVar);

        selector.GetKeys().Should().BeEmpty();
        selector.GetCurrentOrNextKey().Should().BeNull();
        selector.RotateToNextKey().Should().BeNull();
    }

    [Fact]
    public void RotatingKeySelector_MarkTemporarilyUnavailable_EnforcesCooldown()
    {
        var envVar = "TEST_COOLDOWN_KEYS_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(envVar, "key-1,key-2");

        try
        {
            var selector = new RotatingKeySelector(envVar);
            selector.IsAvailable().Should().BeTrue();

            selector.MarkTemporarilyUnavailable(TimeSpan.FromSeconds(60));
            selector.IsAvailable().Should().BeFalse();
            selector.GetCurrentOrNextKey().Should().BeNull();
            selector.RotateToNextKey().Should().BeNull();

            selector.ResetAvailability();
            selector.IsAvailable().Should().BeTrue();
            selector.GetCurrentOrNextKey().Should().Be("key-1");
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Fact]
    public void RotatingKeySelector_ThreadSafety_ConcurrentCallsDoNotThrow()
    {
        var envVar = "TEST_CONCURRENT_KEYS_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(envVar, "k1,k2,k3,k4,k5");

        try
        {
            var selector = new RotatingKeySelector(envVar);
            var results = new string?[500];

            Parallel.For(0, 500, i =>
            {
                results[i] = selector.GetCurrentOrNextKey();
            });

            results.Should().NotContainNulls();
            results.Distinct().Should().BeEquivalentTo(new[] { "k1", "k2", "k3", "k4", "k5" });
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    #endregion

    #region AiProviderRouter Tests

    [Fact]
    public async Task AiProviderRouter_ExecutesInPriorityOrder()
    {
        var executionLog = new List<string>();

        var provider1 = new MockTestProvider("ProviderA", "model-a", () =>
        {
            executionLog.Add("ProviderA");
            return new AiCompletionResponse { IsSuccess = false, ErrorMessage = "A failed" };
        });

        var provider2 = new MockTestProvider("ProviderB", "model-b", () =>
        {
            executionLog.Add("ProviderB");
            return new AiCompletionResponse { IsSuccess = true, RecommendationText = "Success B", ProviderName = "ProviderB" };
        });

        var provider3 = new MockTestProvider("ProviderC", "model-c", () =>
        {
            executionLog.Add("ProviderC");
            return new AiCompletionResponse { IsSuccess = true, RecommendationText = "Success C", ProviderName = "ProviderC" };
        });

        var explicitList = new List<(IAiProvider Provider, int Priority, bool Enabled)>
        {
            (provider3, 3, true),
            (provider1, 1, true),
            (provider2, 2, true)
        };

        var router = new AiProviderRouter(explicitList, NullLogger<AiProviderRouter>.Instance);

        var request = new AiCompletionRequest { UserPrompt = "Test prompt" };
        var response = await router.GenerateStructuredAsync(request);

        response.IsSuccess.Should().BeTrue();
        response.ProviderName.Should().Be("ProviderB");
        executionLog.Should().Equal("ProviderA", "ProviderB");
    }

    [Fact]
    public async Task AiProviderRouter_WhenAllProvidersFail_ReturnsExactLockedErrorMessage()
    {
        var provider1 = new MockTestProvider("P1", "m1", () => new AiCompletionResponse { IsSuccess = false, ErrorMessage = "Error 1" });
        var provider2 = new MockTestProvider("P2", "m2", () => throw new HttpRequestException("Network failure"));

        var explicitList = new List<(IAiProvider Provider, int Priority, bool Enabled)>
        {
            (provider1, 1, true),
            (provider2, 2, true)
        };

        var router = new AiProviderRouter(explicitList, NullLogger<AiProviderRouter>.Instance);

        var request = new AiCompletionRequest { UserPrompt = "Prompt" };
        var response = await router.GenerateStructuredAsync(request);

        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Be("All providers unavailable");
        response.ProviderName.Should().Be("Router");
    }

    [Fact]
    public async Task AiProviderRouter_SkipsDisabledProviders()
    {
        var executionLog = new List<string>();

        var provider1 = new MockTestProvider("DisabledProvider", "m1", () =>
        {
            executionLog.Add("DisabledProvider");
            return new AiCompletionResponse { IsSuccess = true };
        });

        var provider2 = new MockTestProvider("ActiveProvider", "m2", () =>
        {
            executionLog.Add("ActiveProvider");
            return new AiCompletionResponse { IsSuccess = true, ProviderName = "ActiveProvider" };
        });

        var explicitList = new List<(IAiProvider Provider, int Priority, bool Enabled)>
        {
            (provider1, 1, false), // Disabled
            (provider2, 2, true)
        };

        var router = new AiProviderRouter(explicitList, NullLogger<AiProviderRouter>.Instance);

        var response = await router.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Test" });

        response.IsSuccess.Should().BeTrue();
        response.ProviderName.Should().Be("ActiveProvider");
        executionLog.Should().Equal("ActiveProvider");
    }

    #endregion

    #region GeminiAiProvider Tests

    [Fact]
    public async Task GeminiAiProvider_WhenApiKeyMissing_ReturnsErrorWithoutThrowing()
    {
        var keySelector = new RotatingKeySelector("NON_EXISTENT_GEMINI_KEY");
        var provider = new GeminiAiProvider(new HttpClient(), keySelector, NullLogger<GeminiAiProvider>.Instance);

        var response = await provider.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Hello" });

        response.IsSuccess.Should().BeFalse();
        response.ErrorMessage.Should().Contain("missing");
    }

    [Fact]
    public async Task GeminiAiProvider_ParsesValidJsonResponse()
    {
        var claimId = Guid.NewGuid();
        var geminiResponseJson = $$"""
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  {
                    "text": "{\n  \"summary\": \"Maintain 4x weekly frequency\",\n  \"observations\": [\"Client recovered well\"],\n  \"recommendations\": [\"Maintain 4x weekly frequency\"],\n  \"rationale\": \"Volume threshold is optimal\",\n  \"confidence_statement\": \"High confidence\",\n  \"assumptions\": [],\n  \"missing_high_value_data\": [],\n  \"evidence_refs\": [\"{{claimId}}\"],\n  \"safety_summary\": null,\n  \"coach_action_required\": true\n}"
                  }
                ]
              }
            }
          ],
          "usageMetadata": {
            "totalTokenCount": 350
          }
        }
        """;

        var testHandler = new MockHttpMessageHandler((req) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(geminiResponseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var envVar = "GEMINI_TEST_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(envVar, "gemini-key-1");

        try
        {
            var keySelector = new RotatingKeySelector(envVar);
            var provider = new GeminiAiProvider(new HttpClient(testHandler), keySelector, NullLogger<GeminiAiProvider>.Instance);

            var response = await provider.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeTrue();
            response.ProviderName.Should().Be("Gemini");
            response.ModelName.Should().Be("gemini-2.0-flash");
            response.TokensUsed.Should().Be(350);
            response.StructuredRecommendation.Should().NotBeNull();
            response.StructuredRecommendation!.Summary.Should().Be("Maintain 4x weekly frequency");
            response.EvidenceClaimRefs.Should().Contain(claimId);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Fact]
    public async Task GeminiAiProvider_RotatesKeyOn429_AndRetries()
    {
        var envVar = "GEMINI_429_TEST_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(envVar, "key-first,key-second");

        var usedKeys = new List<string>();

        var testHandler = new MockHttpMessageHandler((req) =>
        {
            var uri = req.RequestUri?.ToString() ?? "";
            if (uri.Contains("key=key-first"))
            {
                usedKeys.Add("key-first");
                return new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                {
                    Content = new StringContent("{\"error\":\"rate_limited\"}")
                };
            }

            usedKeys.Add("key-second");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "candidates": [
                    {
                      "content": {
                        "parts": [{ "text": "{\"summary\": \"Success on retry key\"}" }]
                      }
                    }
                  ]
                }
                """)
            };
        });

        try
        {
            var keySelector = new RotatingKeySelector(envVar);
            var provider = new GeminiAiProvider(new HttpClient(testHandler), keySelector, NullLogger<GeminiAiProvider>.Instance);

            var response = await provider.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeTrue();
            usedKeys.Should().Equal("key-first", "key-second");
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    #endregion

    #region GroqAiProvider Tests

    [Fact]
    public async Task GroqAiProvider_ParsesOpenAiCompatibleResponse()
    {
        var groqResponseJson = """
        {
          "choices": [
            {
              "message": {
                "role": "assistant",
                "content": "{\"summary\": \"Increase RIR to 2 on heavy squats\", \"observations\": [], \"recommendations\": [\"Increase RIR to 2\"], \"rationale\": \"Fatigue management\", \"confidence_statement\": \"Moderate\", \"assumptions\": [], \"missing_high_value_data\": [], \"evidence_refs\": [], \"safety_summary\": null, \"coach_action_required\": true}"
              }
            }
          ],
          "usage": {
            "total_tokens": 128
          }
        }
        """;

        var testHandler = new MockHttpMessageHandler((req) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(groqResponseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var envVar = "GROQ_TEST_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(envVar, "groq-key-1");

        try
        {
            var keySelector = new RotatingKeySelector(envVar);
            var provider = new GroqAiProvider(new HttpClient(testHandler), keySelector, NullLogger<GroqAiProvider>.Instance);

            var response = await provider.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeTrue();
            response.ProviderName.Should().Be("Groq");
            response.ModelName.Should().Be("llama-3.3-70b-versatile");
            response.TokensUsed.Should().Be(128);
            response.StructuredRecommendation!.Summary.Should().Be("Increase RIR to 2 on heavy squats");
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Fact]
    public async Task GroqAiProvider_RotatesKeyOn429_AndMarksUnavailableIfExhausted()
    {
        var envVar = "GROQ_429_TEST_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(envVar, "single-groq-key");

        var testHandler = new MockHttpMessageHandler((req) =>
        {
            return new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("{\"error\":\"rate_limited\"}")
            };
        });

        try
        {
            var keySelector = new RotatingKeySelector(envVar);
            var provider = new GroqAiProvider(new HttpClient(testHandler), keySelector, NullLogger<GroqAiProvider>.Instance);

            var response = await provider.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeFalse();
            response.ErrorMessage.Should().Contain("429");
            keySelector.IsAvailable().Should().BeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    #endregion

    #region OpenRouterAiProvider Tests

    [Fact]
    public async Task OpenRouterAiProvider_ParsesResponseSuccessfully()
    {
        var openRouterResponseJson = """
        {
          "choices": [
            {
              "message": {
                "role": "assistant",
                "content": "{\"summary\": \"Deload planned for next microcycle\", \"observations\": [], \"recommendations\": [], \"rationale\": \"Proactive recovery\", \"confidence_statement\": \"High\", \"assumptions\": [], \"missing_high_value_data\": [], \"evidence_refs\": [], \"safety_summary\": null, \"coach_action_required\": true}"
              }
            }
          ],
          "usage": {
            "total_tokens": 210
          }
        }
        """;

        var testHandler = new MockHttpMessageHandler((req) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(openRouterResponseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var envVar = "OPENROUTER_TEST_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(envVar, "openrouter-key-1");

        try
        {
            var keySelector = new RotatingKeySelector(envVar);
            var provider = new OpenRouterAiProvider(new HttpClient(testHandler), keySelector, NullLogger<OpenRouterAiProvider>.Instance, "openai/gpt-4o-mini");

            var response = await provider.GenerateStructuredAsync(new AiCompletionRequest { UserPrompt = "Prompt" });

            response.IsSuccess.Should().BeTrue();
            response.ProviderName.Should().Be("OpenRouter");
            response.ModelName.Should().Be("openai/gpt-4o-mini");
            response.TokensUsed.Should().Be(210);
            response.StructuredRecommendation!.Summary.Should().Be("Deload planned for next microcycle");
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    #endregion

    #region Test Helpers

    private class MockTestProvider : IAiProvider
    {
        private readonly Func<AiCompletionResponse> _generator;

        public string ProviderName { get; }
        public string DefaultModelName { get; }

        public MockTestProvider(string name, string model, Func<AiCompletionResponse> generator)
        {
            ProviderName = name;
            DefaultModelName = model;
            _generator = generator;
        }

        public Task<AiCompletionResponse> GenerateStructuredAsync(AiCompletionRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_generator());
        }

        public Task<AiCompletionResponse> AnalyzeImageAsync(AiImageRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_generator());
        }

        public Task<AiCompletionResponse> AnalyzeVideoAsync(AiVideoRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_generator());
        }
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    #endregion
}
