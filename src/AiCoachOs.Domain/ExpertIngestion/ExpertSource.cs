using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.ExpertIngestion;

/// <summary>
/// Represents a recognized fitness/strength/physique domain expert or publisher.
/// </summary>
public class ExpertSource : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public ExpertSourceType SourceType { get; private set; }
    public string Url { get; private set; } = null!;

    private ExpertSource() { } // EF Core

    public ExpertSource(
        Guid id,
        string name,
        ExpertSourceType sourceType,
        string url) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Expert source name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Expert source URL cannot be empty.", nameof(url));

        Name = name.Trim();
        SourceType = sourceType;
        Url = url.Trim();
    }

    public void Update(
        string name,
        ExpertSourceType sourceType,
        string url)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Expert source name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Expert source URL cannot be empty.", nameof(url));

        Name = name.Trim();
        SourceType = sourceType;
        Url = url.Trim();
        MarkUpdated();
    }
}
