namespace AiCoachOs.Infrastructure.Persistence;

/// <summary>
/// M12 Substance Knowledge Database Seeder.
/// Critical Seed Rule: Leaves production knowledge tables with 0 active unconfirmed records on startup.
/// Test suites use isolated synthetic in-memory fixtures.
/// </summary>
public static class SubstanceSeeder
{
    public static Task SeedAsync(ApplicationDbContext context)
    {
        // Production seeder leaves substance knowledge tables with 0 active records.
        // Synthetic test fixtures are used in unit/integration test suites.
        return Task.CompletedTask;
    }
}
