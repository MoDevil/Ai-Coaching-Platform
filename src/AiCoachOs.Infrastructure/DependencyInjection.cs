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
using Microsoft.IdentityModel.Tokens;

namespace AiCoachOs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Port=5432;Database=aicoachos;Username=postgres;Password=;";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, b =>
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Identity Core
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT Configuration
        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
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
        services.AddHttpClient<AiCoachOs.Infrastructure.Ai.GroqAiProvider>();
        services.AddHttpClient<AiCoachOs.Infrastructure.Ai.OpenRouterAiProvider>();

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
        services.AddScoped<AiCoachOs.Infrastructure.Ai.GroqAiProvider>(sp =>
        {
            var http = sp.GetRequiredService<HttpClient>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AiCoachOs.Infrastructure.Ai.GroqAiProvider>>();
            var cfg = aiProviderOptions.Providers.FirstOrDefault(p => string.Equals(p.Name, "Groq", StringComparison.OrdinalIgnoreCase));
            var keySelector = new AiCoachOs.Infrastructure.Ai.RotatingKeySelector(cfg?.ApiKeysEnvVar ?? "GROQ_API_KEYS");
            return new AiCoachOs.Infrastructure.Ai.GroqAiProvider(http, keySelector, logger, cfg?.Model);
        });
        services.AddScoped<AiCoachOs.Infrastructure.Ai.OpenRouterAiProvider>(sp =>
        {
            var http = sp.GetRequiredService<HttpClient>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AiCoachOs.Infrastructure.Ai.OpenRouterAiProvider>>();
            var cfg = aiProviderOptions.Providers.FirstOrDefault(p => string.Equals(p.Name, "OpenRouter", StringComparison.OrdinalIgnoreCase));
            var keySelector = new AiCoachOs.Infrastructure.Ai.RotatingKeySelector(cfg?.ApiKeysEnvVar ?? "OPENROUTER_API_KEYS");
            return new AiCoachOs.Infrastructure.Ai.OpenRouterAiProvider(http, keySelector, logger, cfg?.Model);
        });

        services.AddScoped<AiCoachOs.Infrastructure.Ai.AiProviderRouter>(sp =>
        {
            var providers = new List<AiCoachOs.Application.Ai.Interfaces.IAiProvider>
            {
                sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.AnthropicAiProvider>(),
                sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.GeminiAiProvider>(),
                sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.GroqAiProvider>(),
                sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.OpenRouterAiProvider>()
            };
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiCoachOs.Application.Ai.Models.AiProviderOptions>>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AiCoachOs.Infrastructure.Ai.AiProviderRouter>>();
            return new AiCoachOs.Infrastructure.Ai.AiProviderRouter(providers, options, logger);
        });

        if (string.Equals(aiSettings.Provider, "Router", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<AiCoachOs.Application.Ai.Interfaces.IAiProvider>(sp => sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.AiProviderRouter>());
        }
        else if (string.Equals(aiSettings.Provider, "Anthropic", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<AiCoachOs.Application.Ai.Interfaces.IAiProvider>(sp => sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.AnthropicAiProvider>());
        }
        else
        {
            services.AddScoped<AiCoachOs.Application.Ai.Interfaces.IAiProvider>(sp => sp.GetRequiredService<AiCoachOs.Infrastructure.Ai.MockAiProvider>());
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
