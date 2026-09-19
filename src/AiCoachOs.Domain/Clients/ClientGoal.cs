namespace AiCoachOs.Domain.Clients;

public record ClientGoal
{
    public string PrimaryGoal { get; init; } = string.Empty;
    public int? TargetTimelineWeeks { get; init; }
    public string? Notes { get; init; }

    public ClientGoal() { }

    public ClientGoal(string primaryGoal, int? targetTimelineWeeks = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(primaryGoal))
            throw new ArgumentException("Primary goal cannot be empty.", nameof(primaryGoal));

        if (targetTimelineWeeks.HasValue && targetTimelineWeeks.Value <= 0)
            throw new ArgumentException("Target timeline weeks must be greater than zero.", nameof(targetTimelineWeeks));

        PrimaryGoal = primaryGoal.Trim();
        TargetTimelineWeeks = targetTimelineWeeks;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
