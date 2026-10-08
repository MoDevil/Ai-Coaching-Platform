using AiCoachOs.Api.Filters;
using AiCoachOs.Api.Middleware;
using AiCoachOs.Application;
using AiCoachOs.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddApplicationValidationFilter();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Coach OS API",
        Version = "v1",
        Description = "Evidence-based coaching operating system for human gym coaches in Egypt."
    });

    // Configure JWT Bearer in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token. Example: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Layer Dependencies
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddHealthChecks();

// CORS for Angular frontend
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? (builder.Environment.IsDevelopment() ? new[] { "http://localhost:4200" } : Array.Empty<string>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Global Exception Handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Coach OS API v1");
    });
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

// Ensure the schema exists before seeding, which matters on a fresh database (CI, new dev
// machines, and the integration-test fixture). Serialized because integration tests start many
// hosts against one database in parallel. Production deployments apply migrations explicitly via
// `dotnet ef database update`, so this is limited to non-production to keep that contract.
if (!app.Environment.IsProduction())
{
    lock (AppStartupMigrations.Gate)
    {
        using var bootstrapScope = app.Services.CreateScope();
        var bootstrapContext = bootstrapScope.ServiceProvider
            .GetRequiredService<AiCoachOs.Infrastructure.Persistence.ApplicationDbContext>();
        bootstrapContext.Database.Migrate();
    }
}

// Seed baseline exercise library and substance safety knowledge
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AiCoachOs.Infrastructure.Persistence.ApplicationDbContext>();
    await AiCoachOs.Infrastructure.Persistence.ExerciseLibrarySeeder.SeedAsync(dbContext);
    await AiCoachOs.Infrastructure.Persistence.SubstanceSeeder.SeedAsync(dbContext);
}

app.Run();

/// <summary>
/// Integration tests start dozens of host instances against one shared database, and each host
/// reaches the startup migration at once. A single process-wide gate means the first host creates
/// the schema and the rest find it already applied, instead of racing to run the same DDL.
/// </summary>
internal static class AppStartupMigrations
{
    internal static readonly object Gate = new();
}

// Make Program class accessible to WebApplicationFactory in integration tests
public partial class Program { }
