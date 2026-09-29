using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.ExpertIngestion;

/// <summary>
/// Represents a recognized fitness/strength/physique domain expert or publisher.
/// </summary>
public class ExpertSource : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string ChannelOrPublication { get; private set; } = null!;
    public ExpertPlatform Platform { get; private set; }
    public string PrimaryDomain { get; private set; } = null!;
    public CredibilityTier CredibilityTier { get; private set; }
    public string? Bio { get; private set; }

    private ExpertSource() { } // EF Core

    public ExpertSource(
        Guid id,
        string name,
        string channelOrPublication,
        ExpertPlatform platform,
        string primaryDomain,
        CredibilityTier credibilityTier,
        string? bio = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Expert source name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(channelOrPublication))
            throw new ArgumentException("Channel or publication cannot be empty.", nameof(channelOrPublication));
        if (string.IsNullOrWhiteSpace(primaryDomain))
            throw new ArgumentException("Primary domain cannot be empty.", nameof(primaryDomain));

        Name = name.Trim();
        ChannelOrPublication = channelOrPublication.Trim();
        Platform = platform;
        PrimaryDomain = primaryDomain.Trim();
        CredibilityTier = credibilityTier;
        Bio = bio?.Trim();
    }

    public void Update(
        string name,
        string channelOrPublication,
        ExpertPlatform platform,
        string primaryDomain,
        CredibilityTier credibilityTier,
        string? bio)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Expert source name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(channelOrPublication))
            throw new ArgumentException("Channel or publication cannot be empty.", nameof(channelOrPublication));
        if (string.IsNullOrWhiteSpace(primaryDomain))
            throw new ArgumentException("Primary domain cannot be empty.", nameof(primaryDomain));

        Name = name.Trim();
        ChannelOrPublication = channelOrPublication.Trim();
        Platform = platform;
        PrimaryDomain = primaryDomain.Trim();
        CredibilityTier = credibilityTier;
        Bio = bio?.Trim();
        MarkUpdated();
    }
}
