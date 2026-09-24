using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Gyms;

public enum EquipmentTier
{
    Minimal = 1,
    Basic = 2,
    Commercial = 3,
    Premium = 4
}

public class GymProfile : Entity<Guid>
{
    private readonly List<Guid> _explicitEquipmentIds = new();

    public Guid CoachId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public EquipmentTier Tier { get; private set; } = EquipmentTier.Commercial;
    public bool IsInventoryAuthoritative { get; private set; }

    public IReadOnlyCollection<Guid> ExplicitEquipmentIds => _explicitEquipmentIds;

    private GymProfile() { } // EF Core

    public GymProfile(
        Guid id,
        Guid coachId,
        string name,
        EquipmentTier tier,
        string? location = null,
        IEnumerable<Guid>? explicitEquipmentIds = null,
        bool isInventoryAuthoritative = false) : base(id)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Gym name cannot be empty.", nameof(name));

        CoachId = coachId;
        Name = name.Trim();
        Tier = tier;
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        IsInventoryAuthoritative = isInventoryAuthoritative;

        if (explicitEquipmentIds != null)
        {
            _explicitEquipmentIds.AddRange(explicitEquipmentIds.Distinct());
        }
    }

    public void UpdateProfile(
        string name,
        EquipmentTier tier,
        string? location,
        IEnumerable<Guid>? explicitEquipmentIds,
        bool isInventoryAuthoritative = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Gym name cannot be empty.", nameof(name));

        Name = name.Trim();
        Tier = tier;
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        IsInventoryAuthoritative = isInventoryAuthoritative;

        _explicitEquipmentIds.Clear();
        if (explicitEquipmentIds != null)
        {
            _explicitEquipmentIds.AddRange(explicitEquipmentIds.Distinct());
        }
    }
}
