using AiCoachOs.Domain.Gyms;

namespace AiCoachOs.Application.Gyms.Engine;

public interface IGymEquipmentResolver
{
    IReadOnlySet<Guid> GetDefaultEquipmentForTier(EquipmentTier tier);
    IReadOnlySet<Guid> ResolveAvailableEquipment(GymProfile? gym, IEnumerable<Guid>? fallbackEquipmentIds = null);
}

public class GymEquipmentResolver : IGymEquipmentResolver
{
    // Deterministic M2 Equipment IDs
    public static readonly Guid Barbell = new("22222222-2222-2222-2222-222222222201");
    public static readonly Guid Dumbbell = new("22222222-2222-2222-2222-222222222202");
    public static readonly Guid Cable = new("22222222-2222-2222-2222-222222222203");
    public static readonly Guid Machine = new("22222222-2222-2222-2222-222222222204");
    public static readonly Guid Bench = new("22222222-2222-2222-2222-222222222205");
    public static readonly Guid SquatRack = new("22222222-2222-2222-2222-222222222206");
    public static readonly Guid Bodyweight = new("22222222-2222-2222-2222-222222222207");

    public IReadOnlySet<Guid> GetDefaultEquipmentForTier(EquipmentTier tier)
    {
        var set = new HashSet<Guid> { Bodyweight };

        switch (tier)
        {
            case EquipmentTier.Minimal:
                set.Add(Dumbbell);
                break;

            case EquipmentTier.Basic:
                // Basic: dumbbells, some machines, NO BARBELL, NO SQUAT RACK
                set.Add(Dumbbell);
                set.Add(Bench);
                set.Add(Machine);
                break;

            case EquipmentTier.Commercial:
            case EquipmentTier.Premium:
            default:
                // Commercial / Premium: full free weights, cables, standard machines
                set.Add(Dumbbell);
                set.Add(Barbell);
                set.Add(Bench);
                set.Add(SquatRack);
                set.Add(Cable);
                set.Add(Machine);
                break;
        }

        return set;
    }

    public IReadOnlySet<Guid> ResolveAvailableEquipment(GymProfile? gym, IEnumerable<Guid>? fallbackEquipmentIds = null)
    {
        if (gym != null)
        {
            var tierDefaults = GetDefaultEquipmentForTier(gym.Tier);

            // Explicit inventory is authoritative -> strictly use explicit inventory + Bodyweight
            if (gym.IsInventoryAuthoritative && gym.ExplicitEquipmentIds.Count > 0)
            {
                var authoritativeInventory = gym.ExplicitEquipmentIds.ToHashSet();
                authoritativeInventory.Add(Bodyweight);
                return authoritativeInventory;
            }

            // Explicit inventory is provided but incomplete/non-authoritative -> fall back to tier defaults supplemented with explicit items
            if (gym.ExplicitEquipmentIds.Count > 0)
            {
                var combined = tierDefaults.ToHashSet();
                foreach (var id in gym.ExplicitEquipmentIds)
                {
                    combined.Add(id);
                }
                return combined;
            }

            // No explicit inventory -> fall back to tier capability defaults
            return tierDefaults;
        }

        if (fallbackEquipmentIds != null && fallbackEquipmentIds.Any())
        {
            var fallback = fallbackEquipmentIds.ToHashSet();
            fallback.Add(Bodyweight);
            return fallback;
        }

        return new HashSet<Guid> { Bodyweight };
    }
}
