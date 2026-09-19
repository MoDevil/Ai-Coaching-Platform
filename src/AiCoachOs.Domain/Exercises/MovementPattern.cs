using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Exercises;

public class MovementPattern : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    private MovementPattern() { } // EF Core

    public MovementPattern(Guid id, string name, string? description = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Movement pattern name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
    }
}
