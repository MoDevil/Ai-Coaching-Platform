namespace AiCoachOs.Domain.Programs;

public record ProgressionRule
{
    public ProgressionRuleType Type { get; init; }
    public string CurrentTarget { get; init; } = string.Empty;
    public string IncrementValue { get; init; } = string.Empty;
    public string? IncrementCondition { get; init; }

    public ProgressionRule() { }

    public ProgressionRule(
        ProgressionRuleType type,
        string currentTarget,
        string incrementValue,
        string? incrementCondition = null)
    {
        if (string.IsNullOrWhiteSpace(currentTarget))
            throw new ArgumentException("Current target cannot be empty.", nameof(currentTarget));
        if (string.IsNullOrWhiteSpace(incrementValue))
            throw new ArgumentException("Increment value cannot be empty.", nameof(incrementValue));

        Type = type;
        CurrentTarget = currentTarget.Trim();
        IncrementValue = incrementValue.Trim();
        IncrementCondition = string.IsNullOrWhiteSpace(incrementCondition) ? null : incrementCondition.Trim();
    }
}
