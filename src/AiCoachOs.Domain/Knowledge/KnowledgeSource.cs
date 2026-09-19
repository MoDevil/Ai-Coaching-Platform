using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Knowledge;

/// <summary>
/// Represents an external evidence source (peer-reviewed paper, position stand, textbook, consensus document).
/// Preserves the identity and bibliographic metadata of the actual publication.
/// </summary>
public class KnowledgeSource : Entity<Guid>
{
    private readonly List<KnowledgeClaimSource> _claimSources = new();

    public KnowledgeSourceType SourceType { get; private set; }
    public string Title { get; private set; } = null!;
    public string Authors { get; private set; } = null!;
    public int Year { get; private set; }
    public string? Doi { get; private set; }
    public string? Url { get; private set; }
    public EvidenceLevel EvidenceLevel { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<KnowledgeClaimSource> ClaimSources => _claimSources;

    private KnowledgeSource() { } // EF Core

    public KnowledgeSource(
        Guid id,
        KnowledgeSourceType sourceType,
        string title,
        string authors,
        int year,
        EvidenceLevel evidenceLevel,
        string? doi = null,
        string? url = null,
        string? notes = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Source title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(authors))
            throw new ArgumentException("Authors cannot be empty.", nameof(authors));
        if (year < 1800 || year > DateTime.UtcNow.Year + 1)
            throw new ArgumentException($"Publication year must be between 1800 and {DateTime.UtcNow.Year + 1}.", nameof(year));

        SourceType = sourceType;
        Title = title.Trim();
        Authors = authors.Trim();
        Year = year;
        EvidenceLevel = evidenceLevel;
        Doi = doi?.Trim();
        Url = url?.Trim();
        Notes = notes?.Trim();
    }
}
