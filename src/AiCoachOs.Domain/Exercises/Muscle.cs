using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Exercises;

public class Muscle : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string? CommonName { get; private set; }
    public string BodyPart { get; private set; } = null!;

    private Muscle() { } // EF Core

    public Muscle(Guid id, string name, string? commonName, string bodyPart) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Muscle name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(bodyPart))
            throw new ArgumentException("Body part cannot be empty.", nameof(bodyPart));

        Name = name.Trim();
        CommonName = commonName?.Trim();
        BodyPart = bodyPart.Trim();
    }
}
