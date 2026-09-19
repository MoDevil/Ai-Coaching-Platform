using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Clients;

public class ConsentRecord : Entity<Guid>
{
    public Guid ClientId { get; private set; }
    public string ConsentType { get; private set; } = string.Empty;
    public bool IsGranted { get; private set; }
    public DateTime GrantedAtUtc { get; private set; }
    public string? Notes { get; private set; }

    // Parameterless constructor for EF Core
    private ConsentRecord() { }

    public ConsentRecord(Guid id, Guid clientId, string consentType, bool isGranted, string? notes = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("Client ID must not be empty.", nameof(clientId));

        if (string.IsNullOrWhiteSpace(consentType))
            throw new ArgumentException("Consent type cannot be empty.", nameof(consentType));

        ClientId = clientId;
        ConsentType = consentType.Trim();
        IsGranted = isGranted;
        GrantedAtUtc = DateTime.UtcNow;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void UpdateConsent(bool isGranted, string? notes = null)
    {
        IsGranted = isGranted;
        GrantedAtUtc = DateTime.UtcNow;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        MarkUpdated();
    }
}
