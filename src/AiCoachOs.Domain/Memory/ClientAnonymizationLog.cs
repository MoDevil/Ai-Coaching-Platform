using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Memory;

/// <summary>
/// Audit log for client memory anonymization events.
/// Strictly does NOT store original memory content.
/// </summary>
public class ClientAnonymizationLog : Entity<Guid>
{
    public Guid ClientId { get; private set; }
    public DateTime AnonymizedAt { get; private set; }
    public Guid RequestedByCoachId { get; private set; }
    public int RecordsAnonymized { get; private set; }
    public string? AnonymizationReason { get; private set; }

    private ClientAnonymizationLog() { } // EF Core

    public ClientAnonymizationLog(
        Guid id,
        Guid clientId,
        Guid requestedByCoachId,
        int recordsAnonymized,
        string? anonymizationReason = null,
        DateTime? anonymizedAt = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        if (requestedByCoachId == Guid.Empty)
            throw new ArgumentException("RequestedByCoachId cannot be empty.", nameof(requestedByCoachId));
        if (recordsAnonymized < 0)
            throw new ArgumentOutOfRangeException(nameof(recordsAnonymized), "RecordsAnonymized cannot be negative.");

        ClientId = clientId;
        RequestedByCoachId = requestedByCoachId;
        RecordsAnonymized = recordsAnonymized;
        AnonymizationReason = string.IsNullOrWhiteSpace(anonymizationReason) ? null : anonymizationReason.Trim();
        AnonymizedAt = anonymizedAt ?? DateTime.UtcNow;
    }
}
