namespace AiCoachOs.Domain.Common;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkUpdated()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
