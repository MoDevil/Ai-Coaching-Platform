using AiCoachOs.Domain.Substances;
using Microsoft.EntityFrameworkCore;

namespace AiCoachOs.Infrastructure.Persistence;

public static class SubstanceSeeder
{
    private static readonly SemaphoreSlim _seedLock = new(1, 1);

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await _seedLock.WaitAsync();
        try
        {
            // 1. Seed Knowledge Sources
            var seedSources = SubstanceSeedData.GetKnowledgeSources();
            foreach (var source in seedSources)
            {
                if (!await context.KnowledgeSourcesDbSet.AnyAsync(s => s.Id == source.Id))
                {
                    await context.KnowledgeSourcesDbSet.AddAsync(source);
                }
            }
            await context.SaveChangesAsync();

            // 2. Seed Knowledge Claims & Claim Sources
            var seedClaims = SubstanceSeedData.GetKnowledgeClaims();
            foreach (var claim in seedClaims)
            {
                if (!await context.KnowledgeClaimsDbSet.AnyAsync(c => c.Id == claim.Id))
                {
                    await context.KnowledgeClaimsDbSet.AddAsync(claim);
                }
            }
            await context.SaveChangesAsync();

            // 3. Seed Supplements
            var seedSupplements = SubstanceSeedData.GetSupplements();
            foreach (var supplement in seedSupplements)
            {
                if (!await context.SubstancesDbSet.AnyAsync(s => s.Id == supplement.Id))
                {
                    await context.SubstancesDbSet.AddAsync(supplement);
                }
            }
            await context.SaveChangesAsync();

            // 4. Seed Hormones
            var seedHormones = SubstanceSeedData.GetHormones();
            foreach (var hormone in seedHormones)
            {
                if (!await context.SubstancesDbSet.AnyAsync(s => s.Id == hormone.Id))
                {
                    await context.SubstancesDbSet.AddAsync(hormone);
                }
            }
            await context.SaveChangesAsync();

            // 5. Seed PED Safety Records
            var seedPEDs = SubstanceSeedData.GetPEDSafetyRecords();
            foreach (var ped in seedPEDs)
            {
                if (!await context.SubstancesDbSet.AnyAsync(s => s.Id == ped.Id))
                {
                    await context.SubstancesDbSet.AddAsync(ped);
                }
            }
            await context.SaveChangesAsync();

            // 6. Seed PED Red Flag Rules
            var seedRules = SubstanceSeedData.GetPEDRedFlagRules();
            foreach (var rule in seedRules)
            {
                if (!await context.PEDRedFlagRulesDbSet.AnyAsync(r => r.Id == rule.Id))
                {
                    await context.PEDRedFlagRulesDbSet.AddAsync(rule);
                }
            }
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Concurrent seed insertion gracefully handled in multi-host integration testing
        }
        finally
        {
            _seedLock.Release();
        }
    }
}
