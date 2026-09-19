using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Exercises;

public class Equipment : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Category { get; private set; }

    private Equipment() { } // EF Core

    public Equipment(Guid id, string name, string? category = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name cannot be empty.", nameof(name));

        Name = name.Trim();
        Category = category?.Trim();
    }
}
