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

        return services;
    }
}
