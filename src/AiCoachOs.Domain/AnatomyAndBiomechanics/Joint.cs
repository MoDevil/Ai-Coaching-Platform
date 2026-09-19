using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.AnatomyAndBiomechanics;

/// <summary>
/// Anatomical articulation relevant for training, movement analysis, and biomechanical demand
/// (e.g., Glenohumeral, Scapulothoracic, Hip, Knee, Talocrural).
/// Non-diagnostic: focuses strictly on functional biomechanics and movement actions.
/// </summary>
public class Joint : Entity<Guid>
{
    private readonly List<JointAction> _actions = new();

    public Guid RegionId { get; private set; }
    public AnatomicalRegion Region { get; private set; } = null!;

    public string Name { get; private set; } = null!;
    public string? CommonName { get; private set; }
    public string? Description { get; private set; }

    public IReadOnlyCollection<JointAction> Actions => _actions;

    private Joint() { } // EF Core

    public Joint(Guid id, Guid regionId, string name, string? commonName = null, string? description = null) : base(id)
    {
        if (regionId == Guid.Empty)
            throw new ArgumentException("RegionId cannot be empty.", nameof(regionId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Joint name cannot be empty.", nameof(name));

        RegionId = regionId;
        Name = name.Trim();
        CommonName = commonName?.Trim();
        Description = description?.Trim();
    }
}
