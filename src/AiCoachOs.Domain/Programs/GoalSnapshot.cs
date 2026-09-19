namespace AiCoachOs.Domain.Programs;

public record GoalSnapshot
{
    public PrimaryGoalType PrimaryGoal { get; init; }
    public PrimaryGoalType? SecondaryGoal { get; init; }
    public string? GoalEmphasis { get; init; }
    public int? TargetTimelineWeeks { get; init; }

    public GoalSnapshot() { }

    public GoalSnapshot(
        PrimaryGoalType primaryGoal,
        PrimaryGoalType? secondaryGoal = null,
        string? goalEmphasis = null,
        int? targetTimelineWeeks = null)
    {
        if (targetTimelineWeeks.HasValue && targetTimelineWeeks.Value <= 0)
            throw new ArgumentException("Target timeline weeks must be greater than zero.", nameof(targetTimelineWeeks));

        PrimaryGoal = primaryGoal;
        SecondaryGoal = secondaryGoal;
        GoalEmphasis = string.IsNullOrWhiteSpace(goalEmphasis) ? null : goalEmphasis.Trim();
        TargetTimelineWeeks = targetTimelineWeeks;
    }
}
