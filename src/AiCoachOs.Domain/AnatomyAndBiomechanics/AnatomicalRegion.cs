using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.AnatomyAndBiomechanics;

/// <summary>
/// Broad anatomical region of the human body for coaching and structural reference
/// (e.g., Shoulder, Spine, Hip, Knee, Ankle, Elbow, Wrist, Trunk).
/// </summary>
public class AnatomicalRegion : Entity<Guid>
{
    private readonly List<Joint> _joints = new();

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    public IReadOnlyCollection<Joint> Joints => _joints;

    private AnatomicalRegion() { } // EF Core

    public AnatomicalRegion(Guid id, string name, string? description = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Anatomical region name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
    }
}
