namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents a safety flag, contraindication, or warning associated with a substance.
/// </summary>
public class SubstanceSafetyFlag
{
    public string FlagType { get; private set; } = string.Empty;
    public SubstanceEscalationLevel Severity { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string EvidenceBasis { get; private set; } = string.Empty;

    private SubstanceSafetyFlag() { } // EF Core

    public SubstanceSafetyFlag(
        string flagType,
        SubstanceEscalationLevel severity,
        string message,
        string evidenceBasis)
    {
        if (string.IsNullOrWhiteSpace(flagType))
            throw new ArgumentException("Flag type cannot be empty.", nameof(flagType));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Safety message cannot be empty.", nameof(message));

        FlagType = flagType.Trim();
        Severity = severity;
        Message = message.Trim();
        EvidenceBasis = string.IsNullOrWhiteSpace(evidenceBasis) ? "Standard pharmacological and sports science safety evidence." : evidenceBasis.Trim();
    }
}
