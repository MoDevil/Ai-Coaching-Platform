using System.Text;
using AiCoachOs.Application.Auth.Services;
using AiCoachOs.Application.Clients.Services;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Infrastructure.Authentication;
using AiCoachOs.Infrastructure.Identity;
using AiCoachOs.Infrastructure.Persistence;
using AiCoachOs.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AiCoachOs.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// The providers that derive from <see cref="Ai.OpenAiCompatibleChatProvider"/>, in the order
    /// they are offered to the router. The router itself orders by the Priority in configuration,
    /// so this order only decides which providers are registered at all.
    /// </summary>
    /// <remarks>
    /// Anthropic and Gemini are deliberately absent: they speak their own request and response
    /// shapes and are registered separately.
    /// </remarks>
    private static readonly (string Name, Type ProviderType, string DefaultKeyEnvVar)[] OpenAiCompatibleProviders =
    {
        ("Groq", typeof(AiCoachOs.Infrastructure.Ai.GroqAiProvider), "GROQ_API_KEYS"),
        ("OpenRouter", typeof(AiCoachOs.Infrastructure.Ai.OpenRouterAiProvider), "OPENROUTER_API_KEYS"),
        ("Cerebras", typeof(AiCoachOs.Infrastructure.Ai.CerebrasAiProvider), "CEREBRAS_API_KEYS"),
        ("SambaNova", typeof(AiCoachOs.Infrastructure.Ai.SambaNovaAiProvider), "SAMBANOVA_API_KEYS"),
        ("xAI", typeof(AiCoachOs.Infrastructure.Ai.XAiProvider), "XAI_API_KEYS"),
        ("HuggingFace", typeof(AiCoachOs.Infrastructure.Ai.HuggingFaceAiProvider), "HUGGINGFACE_API_KEYS"),
    };

    private static readonly string[] OpenAiCompatibleProviderNames =
        OpenAiCompatibleProviders.Select(p => p.Name).ToArray();

    private static readonly string RecognisedProviderNames =
        "Router, Anthropic, Gemini, " + string.Join(", ", OpenAiCompatibleProviderNames) + ", Mock";

    /// <summary>
    /// Registers a provider that derives from <see cref="Ai.OpenAiCompatibleChatProvider"/> as a
    /// typed <see cref="HttpClient"/> and a scoped instance, wiring its model and key environment
    /// variable from the <c>AiProviders</c> configuration section.
    /// </summary>
    private static void RegisterOpenAiCompatibleProvider(
        IServiceCollection services,
        AiCoachOs.Application.Ai.Models.AiProviderOptions options,
        string name,
        Type providerType,
        string defaultKeyEnvVar)
    {
        services.AddScoped(providerType, sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(name);
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(providerType);
            var config = options.Providers
                .FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

            var keySelector = new AiCoachOs.Infrastructure.Ai.RotatingKeySelector(
                string.IsNullOrWhiteSpace(config?.ApiKeysEnvVar) ? defaultKeyEnvVar : config.ApiKeysEnvVar);

            // Empty rather than null so the provider falls back to its built-in default model.
            return ActivatorUtilities.CreateInstance(
                sp,
                providerType,
                httpClient,
                keySelector,
                logger,
                config?.Model ?? string.Empty);
        });
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. Set it via " +
                "ConnectionStrings__DefaultConnection (user-secrets, environment variable, or appsettings).");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, b =>
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Identity Core
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT Configuration
        // Validated at startup so a missing or weak signing key fails the host rather than
        // silently minting tokens that anybody holding the repository can forge.
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.SecretKey),
                "Jwt:SecretKey is not configured. Set it via user-secrets or the Jwt__SecretKey environment variable. " +
                "Generate one with: openssl rand -base64 48")
            .Validate(s => s.SecretKey.Length >= JwtSettings.MinimumSecretKeyLength,
                $"Jwt:SecretKey must be at least {JwtSettings.MinimumSecretKeyLength} characters.")
            .Validate(s => s.ExpirationMinutes > 0, "Jwt:ExpirationMinutes must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer();

        // Resolved lazily from IOptionsMonitor so configuration overrides applied after
        // service registration (host builders, integration test factories) are honoured.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptionsMonitor<JwtSettings>>((options, jwt) =>
            {
                var settings = jwt.CurrentValue;
                options.RequireHttpsMetadata = environment.IsDevelopment() ? false : true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });


        // Application & Domain Services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentCoachService, CurrentCoachService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<AiCoachOs.Application.Exercises.Services.IExerciseService, ExerciseService>();
        services.AddScoped<AiCoachOs.Application.TrainingProfiles.Services.ITrainingProfileService, TrainingProfileService>();
        services.AddScoped<AiCoachOs.Application.Knowledge.Services.IKnowledgeService, KnowledgeService>();
        services.AddScoped<AiCoachOs.Application.AnatomyAndBiomechanics.Services.IAnatomyService, AnatomyService>();
        services.AddScoped<AiCoachOs.Application.AnatomyAndBiomechanics.Services.IBiomechanicsService, BiomechanicsService>();

        // M5 Program Designer Engine Services
        services.AddScoped<AiCoachOs.Application.Programs.Engine.IGoalAnalyzer, AiCoachOs.Application.Programs.Engine.GoalAnalyzer>();
        services.AddScoped<AiCoachOs.Application.Programs.Engine.IConstraintAnalyzer, AiCoachOs.Application.Programs.Engine.ConstraintAnalyzer>();
        services.AddScoped<AiCoachOs.Application.Programs.Engine.IRecoveryModel, AiCoachOs.Application.Programs.Engine.RecoveryModel>();
        services.AddScoped<AiCoachOs.Application.Programs.Engine.IExerciseSelector, AiCoachOs.Application.Programs.Engine.ExerciseSelector>();
        services.AddScoped<AiCoachOs.Application.Programs.Engine.ISessionBuilder, AiCoachOs.Application.Programs.Engine.SessionBuilder>();
        services.AddScoped<AiCoachOs.Application.Programs.Engine.IProgramBuilder, AiCoachOs.Application.Programs.Engine.ProgramBuilder>();
        services.AddScoped<AiCoachOs.Application.Programs.Services.IProgramService, ProgramService>();

        // M6 Workout Logging & Progression Services
        services.AddScoped<AiCoachOs.Application.Workouts.Engine.IProgressionEvaluator, AiCoachOs.Application.Workouts.Engine.ProgressionEvaluator>();
        services.AddScoped<AiCoachOs.Application.Workouts.Services.IWorkoutService, WorkoutService>();

        // M7 Adaptive Coaching Services
        services.AddScoped<AiCoachOs.Application.Adaptations.Engine.IAdaptationAnalyzer, AiCoachOs.Application.Adaptations.Engine.AdaptationAnalyzer>();
        services.AddScoped<AiCoachOs.Application.Adaptations.Services.IAdaptationService, AdaptationService>();

        // M8 Medical Awareness + Safety Services
        services.AddScoped<AiCoachOs.Application.Safety.Engine.ISafetyScreener, AiCoachOs.Application.Safety.Engine.SafetyScreener>();
        services.AddScoped<AiCoachOs.Application.Safety.Interfaces.ISafetyService, SafetyService>();

        // M9 Rehab Awareness Services
        services.AddScoped<AiCoachOs.Application.Rehab.Engine.IRehabAwarenessEngine, AiCoachOs.Application.Rehab.Engine.RehabAwarenessEngine>();
        services.AddScoped<AiCoachOs.Application.Rehab.Interfaces.IRehabService, RehabService>();

        // M10 Nutrition Foundation Services
        services.AddScoped<AiCoachOs.Application.Nutrition.Engine.INutritionCalculator, AiCoachOs.Application.Nutrition.Engine.NutritionCalculator>();
        services.AddScoped<AiCoachOs.Application.Nutrition.Interfaces.INutritionService, NutritionService>();

        // M11 Egypt Localization Services
        services.AddScoped<AiCoachOs.Application.Gyms.Engine.IGymEquipmentResolver, AiCoachOs.Application.Gyms.Engine.GymEquipmentResolver>();
        services.AddScoped<AiCoachOs.Application.Gyms.Interfaces.IGymService, GymService>();
        services.AddScoped<AiCoachOs.Application.Nutrition.Engine.IFoodSuggestionEngine, AiCoachOs.Application.Nutrition.Engine.FoodSuggestionEngine>();

        // M12 Substances & Safety Awareness Services
        services.AddScoped<AiCoachOs.Application.Substances.Engine.ISubstanceSafetyEvaluator, AiCoachOs.Application.Substances.Engine.SubstanceSafetyEvaluator>();
        services.AddScoped<AiCoachOs.Application.Substances.Interfaces.ISubstanceService, AiCoachOs.Infrastructure.Services.SubstanceService>();

        // M13 Client Memory Services
        services.AddScoped<AiCoachOs.Application.Memory.Engine.IClientMemoryConflictDetector, AiCoachOs.Application.Memory.Engine.ClientMemoryConflictDetector>();
        services.AddScoped<AiCoachOs.Application.Memory.Interfaces.IClientMemoryService, AiCoachOs.Infrastructure.Services.ClientMemoryService>();

        // M14 / M19 AI Provider & Reasoning Layer
        var aiSettings = new AiCoachOs.Application.Ai.Models.AiSettings();
        configuration.GetSection(AiCoachOs.Application.Ai.Models.AiSettings.SectionName).Bind(aiSettings);
        services.Configure<AiCoachOs.Application.Ai.Models.AiSettings>(configuration.GetSection(AiCoachOs.Application.Ai.Models.AiSettings.SectionName));

        var aiProviderOptions = new AiCoachOs.Application.Ai.Models.AiProviderOptions();
        configuration.GetSection(AiCoachOs.Application.Ai.Models.AiProviderOptions.SectionName).Bind(aiProviderOptions);
        services.Configure<AiCoachOs.Application.Ai.Models.AiProviderOptions>(configuration.GetSection(AiCoachOs.Application.Ai.Models.AiProviderOptions.SectionName));

        services.AddHttpClient<AiCoachOs.Infrastructure.Ai.AnthropicAiProvider>();
        services.AddHttpClient<AiCoachOs.Infrastructure.Ai.GeminiAiProvider>();

        // Every OpenAI-compatible provider shares one HttpClient registration and one factory
        // shape; only the name and key environment variable differ.
        foreach (var openAiCompatibleProvider in OpenAiCompatibleProviderNames)
        {
            services.AddHttpClient(openAiCompatibleProvider);
        }

        services.AddScoped<AiCoachOs.Infrastructure.Ai.MockAiProvider>();
        services.AddScoped<AiCoachOs.Infrastructure.Ai.AnthropicAiProvider>();
        services.AddScoped<AiCoachOs.Infrastructure.Ai.GeminiAiProvider>(sp =>
        {
            var http = sp.GetRequiredService<HttpClient>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AiCoachOs.Infrastructure.Ai.GeminiAiProvider>>();
            var cfg = aiProviderOptions.Providers.FirstOrDefault(p => string.Equals(p.Name, "Gemini", StringComparison.OrdinalIgnoreCase));
            var keySelector = new AiCoachOs.Infrastructure.Ai.RotatingKeySelector(cfg?.ApiKeysEnvVar ?? "GEMINI_API_KEYS");
            return new AiCoachOs.Infrastructure.Ai.GeminiAiProvider(http, keySelector, logger, cfg?.Model);
        });

        // Name to (concrete type, default key environment variable). Adding a provider means adding
        // one entry here plus the matching appsettings block; the fallback chain in
        // AiProviderRouter picks it up automatically.
        foreach (var (name, providerType, defaultKeyEnvVar) in OpenAiCompatibleProviders)
        {
            RegisterOpenAiCompatibleProvider(services, aiProviderOptions, name, providerType, defaultKeyEnvVar);
        }

        services.AddScoped<AiCoachOs.Infrastructure.Ai.AiProviderRouter>(sp =>
        {
            var providers = new List<AiCoachOs.Application.Ai.Interfaces.IAiProvider>
            {
                sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.AnthropicAiProvider>(),
                sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.GeminiAiProvider>()
            };

            foreach (var (_, providerType, _) in OpenAiCompatibleProviders)
            {
                providers.Add((AiCoachOs.Application.Ai.Interfaces.IAiProvider)sp.GetRequiredService(providerType));
            }

            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiCoachOs.Application.Ai.Models.AiProviderOptions>>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AiCoachOs.Infrastructure.Ai.AiProviderRouter>>();
            return new AiCoachOs.Infrastructure.Ai.AiProviderRouter(providers, options, logger);
        });

        // Provider selection is explicit. An unrecognised name is a configuration error and
        // must fail the host rather than silently downgrade to the mock provider, which
        // returns fabricated, evidence-citing recommendations.
        var configuredProvider = aiSettings.Provider?.Trim();
        if (string.IsNullOrWhiteSpace(configuredProvider))
        {
            throw new InvalidOperationException(
                "AiSettings:Provider is not configured. Set it to one of: " +
                RecognisedProviderNames + ", or Mock (Mock is permitted in Development only).");
        }

        switch (configuredProvider.ToLowerInvariant())
        {
            case "router":
                services.AddScoped<AiCoachOs.Application.Ai.Interfaces.IAiProvider>(sp => sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.AiProviderRouter>());
                break;
            case "anthropic":
                services.AddScoped<AiCoachOs.Application.Ai.Interfaces.IAiProvider>(sp => sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.AnthropicAiProvider>());
                break;
            case "gemini":
                services.AddScoped<AiCoachOs.Application.Ai.Interfaces.IAiProvider>(sp => sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.GeminiAiProvider>());
                break;
            case "mock":
                var allowMock = configuration.GetValue("AiSettings:AllowMockProvider", false)
                                || environment.IsDevelopment();
                if (!allowMock)
                {
                    throw new InvalidOperationException(
                        "AiSettings:Provider is 'Mock' but mock output is not permitted in this environment. " +
                        "MockAiProvider fabricates coaching recommendations; it must only run in Development. " +
                        "Set AiSettings:Provider to a real provider, or set AiSettings:AllowMockProvider=true " +
                        "for an isolated test run.");
                }
                services.AddScoped<AiCoachOs.Application.Ai.Interfaces.IAiProvider>(sp => sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.MockAiProvider>());
                break;
            default:
                // OpenAI-compatible providers are selected by name from the shared table so a
                // provider cannot be registered for the router yet be missing from the switch.
                var openAiMatch = OpenAiCompatibleProviders
                    .FirstOrDefault(p => string.Equals(p.Name, configuredProvider, StringComparison.OrdinalIgnoreCase));

                if (openAiMatch.ProviderType is null)
                {
                    throw new InvalidOperationException(
                        $"AiSettings:Provider '{configuredProvider}' is not a recognised provider. " +
                        $"Expected one of: {RecognisedProviderNames}, Mock.");
                }

                services.AddScoped<AiCoachOs.Application.Ai.Interfaces.IAiProvider>(
                    sp => (AiCoachOs.Application.Ai.Interfaces.IAiProvider)sp.GetRequiredService(openAiMatch.ProviderType));
                break;
        }

        services.AddScoped<AiCoachOs.Application.Ai.Interfaces.IAiReasoningService, AiCoachOs.Infrastructure.Ai.AiReasoningService>();

        // M15 Photo Vision Architecture Services
        services.AddScoped<AiCoachOs.Application.Photos.Interfaces.IPhotoStorageService, AiCoachOs.Infrastructure.Photos.LocalStorageService>();
        services.AddScoped<AiCoachOs.Application.Photos.Interfaces.IPhotoVisionService, AiCoachOs.Infrastructure.Photos.PhotoVisionService>();

        // M16 Video Analysis Architecture Services
        services.AddScoped<AiCoachOs.Application.Videos.Interfaces.IVideoProcessingService, AiCoachOs.Infrastructure.Videos.FFmpegVideoProcessingService>();
        services.AddSingleton<AiCoachOs.Application.Videos.Interfaces.IVideoAnalysisJobService, AiCoachOs.Infrastructure.Videos.VideoAnalysisJobService>();
        services.AddScoped<AiCoachOs.Application.Videos.Interfaces.IVideoAnalysisService, AiCoachOs.Infrastructure.Videos.VideoAnalysisService>();

        // M17 Expert Content Ingestion Services
        services.AddHttpClient<AiCoachOs.Application.ExpertIngestion.Interfaces.IContentFetcherService, AiCoachOs.Infrastructure.ExpertIngestion.ContentFetcherService>();
        services.AddScoped<AiCoachOs.Application.ExpertIngestion.Interfaces.IClaimExtractionService, AiCoachOs.Infrastructure.ExpertIngestion.ClaimExtractionService>();
        services.AddScoped<AiCoachOs.Application.ExpertIngestion.Interfaces.IConflictDetectionService, AiCoachOs.Infrastructure.ExpertIngestion.ConflictDetectionService>();
        services.AddScoped<AiCoachOs.Application.ExpertIngestion.Interfaces.IExpertIngestionService, AiCoachOs.Infrastructure.ExpertIngestion.ExpertIngestionService>();

        return services;
    }
}
