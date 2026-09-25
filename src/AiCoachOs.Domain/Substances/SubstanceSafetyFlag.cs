using System.Text.Json.Serialization;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Represents a structured safety flag, contraindication, or precaution associated with a substance.
/// Locked contract: Category, Description, AffectedPopulation, SourceClaimId, EscalationLevel, CoachNote.
/// </summary>
public class SubstanceSafetyFlag
{
    public SafetyFlagCategory Category { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? AffectedPopulation { get; private set; }
    public Guid? SourceClaimId { get; private set; }
    public EscalationLevel EscalationLevel { get; private set; }
    public string CoachNote { get; private set; } = string.Empty;

    private SubstanceSafetyFlag() { } // EF Core

    [JsonConstructor]
    public SubstanceSafetyFlag(
        SafetyFlagCategory category,
        string description,
        EscalationLevel escalationLevel,
        string coachNote,
        string? affectedPopulation = null,
        Guid? sourceClaimId = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Safety flag description cannot be empty.", nameof(description));
        if (string.IsNullOrWhiteSpace(coachNote))
            throw new ArgumentException("Coach note cannot be empty.", nameof(coachNote));

        Category = category;
        Description = description.Trim();
        EscalationLevel = escalationLevel;
        CoachNote = coachNote.Trim();
        AffectedPopulation = string.IsNullOrWhiteSpace(affectedPopulation) ? null : affectedPopulation.Trim();
        SourceClaimId = sourceClaimId;
    }
}
