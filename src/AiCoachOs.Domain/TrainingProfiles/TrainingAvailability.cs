namespace AiCoachOs.Domain.TrainingProfiles;

public class TrainingAvailability
{
    public int SessionsPerWeek { get; private set; }
    public IReadOnlyList<DayOfWeek> AvailableDays { get; private set; } = Array.Empty<DayOfWeek>();
    public IReadOnlyList<DayOfWeek> PreferredDays { get; private set; } = Array.Empty<DayOfWeek>();

    private TrainingAvailability() { } // EF Core

    public TrainingAvailability(
        int sessionsPerWeek,
        IEnumerable<DayOfWeek>? availableDays = null,
        IEnumerable<DayOfWeek>? preferredDays = null)
    {
        if (sessionsPerWeek < 1 || sessionsPerWeek > 7)
            throw new ArgumentOutOfRangeException(nameof(sessionsPerWeek), "Sessions per week must be between 1 and 7.");

        SessionsPerWeek = sessionsPerWeek;
        AvailableDays = (availableDays ?? Array.Empty<DayOfWeek>()).Distinct().ToList().AsReadOnly();
        PreferredDays = (preferredDays ?? Array.Empty<DayOfWeek>()).Distinct().ToList().AsReadOnly();
    }
}
