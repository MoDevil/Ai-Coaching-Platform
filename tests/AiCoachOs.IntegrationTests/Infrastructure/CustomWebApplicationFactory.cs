using AiCoachOs.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiCoachOs.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory()
    {
        _connectionString = "Host=localhost;Port=5432;Database=aicoachos;Username=postgres;Password=;";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", _connectionString },
                { "Jwt:Issuer", "AiCoachOs" },
                { "Jwt:Audience", "AiCoachOsApp" },
                { "Jwt:SecretKey", "AiCoachOs_Secure_JWT_Key_Egypt_Coaching_System_2026_Secret!" },
                { "Jwt:ExpirationMinutes", "60" }
            };

            config.AddInMemoryCollection(inMemorySettings);
        });

        builder.ConfigureServices(services =>
        {
            // Ensure database is migrated
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        });

        builder.UseEnvironment("Development");
    }
}
