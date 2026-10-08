using AiCoachOs.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AiCoachOs.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Integration tests run the real EF Core migrations against a real PostgreSQL instance.
    /// There is deliberately no in-memory provider in this solution, so there is nothing to fall
    /// back to when no connection string can be resolved.
    /// </summary>
    public const string NoConnectionStringMessage =
        "No PostgreSQL connection string available for integration tests. Set the " +
        "ConnectionStrings__DefaultConnection environment variable, or start PostgreSQL via " +
        "'docker compose -f docker/docker-compose.dev.yml up -d' and point " +
        "ConnectionStrings__DefaultConnection at that instance.";

    // A fresh random key per test run. Never reuse the development or production signing key here:
    // tokens minted by the fixture must not be forgeable with a value that exists in the repository.
    private static readonly string TestJwtSecretKey =
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

    private static readonly Lazy<string> ConnectionString = new(ResolveConnectionString);

    private static string ResolveConnectionString()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
            return fromEnvironment;

        var localSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.IntegrationTests.json");
        if (File.Exists(localSettingsPath))
        {
            var local = new ConfigurationBuilder()
                .AddJsonFile(localSettingsPath, optional: false)
                .Build()
                .GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(local))
                return local;
        }

        throw new InvalidOperationException(NoConnectionStringMessage);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", ConnectionString.Value },
                { "Jwt:Issuer", "AiCoachOs" },
                { "Jwt:Audience", "AiCoachOsApp" },
                { "Jwt:SecretKey", TestJwtSecretKey },
                { "Jwt:ExpirationMinutes", "60" },
                { "AiSettings:Provider", "Mock" },
                { "AiSettings:AllowMockProvider", "true" }
            });
        });
    }

    }
