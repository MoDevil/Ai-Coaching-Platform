using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.TrainingProfiles;

public class ClientTrainingProfile : Entity<Guid>
{
    private readonly List<ClientTrainingPriority> _priorities = new();
    private readonly List<Guid> _availableEquipmentIds = new();

    public Guid ClientId { get; private set; }
    public TrainingExperienceLevel ExperienceLevel { get; private set; }

    public int? SessionDurationMinMinutes { get; private set; }
    public int? SessionDurationTargetMinutes { get; private set; }
    public int? SessionDurationMaxMinutes { get; private set; }

    public TrainingAvailability WeeklyAvailability { get; private set; } = null!;

    public string? ExercisePreferences { get; private set; }
    public string? ExerciseConstraints { get; private set; }

    public IReadOnlyCollection<Guid> AvailableEquipmentIds => _availableEquipmentIds;
    public IReadOnlyCollection<ClientTrainingPriority> Priorities => _priorities;

    private ClientTrainingProfile() { } // EF Core

    public ClientTrainingProfile(
        Guid id,
        Guid clientId,
        TrainingExperienceLevel experienceLevel,
        TrainingAvailability weeklyAvailability,
        int? sessionDurationMinMinutes = null,
        int? sessionDurationTargetMinutes = null,
        int? sessionDurationMaxMinutes = null,
        IEnumerable<Guid>? availableEquipmentIds = null,
        string? exercisePreferences = null,
        string? exerciseConstraints = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));

        ValidateSessionDurations(sessionDurationMinMinutes, sessionDurationTargetMinutes, sessionDurationMaxMinutes);

        ClientId = clientId;
        ExperienceLevel = experienceLevel;
        WeeklyAvailability = weeklyAvailability ?? throw new ArgumentNullException(nameof(weeklyAvailability));
        SessionDurationMinMinutes = sessionDurationMinMinutes;
        SessionDurationTargetMinutes = sessionDurationTargetMinutes;
        SessionDurationMaxMinutes = sessionDurationMaxMinutes;
        ExercisePreferences = exercisePreferences?.Trim();
        ExerciseConstraints = exerciseConstraints?.Trim();

        if (availableEquipmentIds != null)
        {
            _availableEquipmentIds.AddRange(availableEquipmentIds.Distinct());
        }
    }

    public void UpdateProfile(
        TrainingExperienceLevel experienceLevel,
        int? sessionDurationMinMinutes,
        int? sessionDurationTargetMinutes,
        int? sessionDurationMaxMinutes,
        IEnumerable<Guid>? availableEquipmentIds,
        string? exercisePreferences,
        string? exerciseConstraints)
    {
        ValidateSessionDurations(sessionDurationMinMinutes, sessionDurationTargetMinutes, sessionDurationMaxMinutes);

        ExperienceLevel = experienceLevel;
        SessionDurationMinMinutes = sessionDurationMinMinutes;
        SessionDurationTargetMinutes = sessionDurationTargetMinutes;
        SessionDurationMaxMinutes = sessionDurationMaxMinutes;
        ExercisePreferences = exercisePreferences?.Trim();
        ExerciseConstraints = exerciseConstraints?.Trim();

        _availableEquipmentIds.Clear();
        if (availableEquipmentIds != null)
        {
            _availableEquipmentIds.AddRange(availableEquipmentIds.Distinct());
        }

        MarkUpdated();
    }

    public void SetAvailability(TrainingAvailability availability)
    {
        WeeklyAvailability = availability ?? throw new ArgumentNullException(nameof(availability));
        MarkUpdated();
    }

    public void SetPriorities(IEnumerable<(int Order, string FocusArea, string? Notes)> priorities)
    {
        _priorities.Clear();
        if (priorities != null)
        {
            foreach (var (order, focusArea, notes) in priorities.OrderBy(p => p.Order))
            {
                _priorities.Add(new ClientTrainingPriority(Guid.NewGuid(), Id, order, focusArea, notes));
            }
        }
        MarkUpdated();
    }

    private static void ValidateSessionDurations(int? min, int? target, int? max)
    {
        if (min.HasValue && min.Value < 15)
            throw new ArgumentException("Minimum session duration must be at least 15 minutes.", nameof(min));
        if (max.HasValue && max.Value > 240)
            throw new ArgumentException("Maximum session duration cannot exceed 240 minutes.", nameof(max));

        if (min.HasValue && target.HasValue && target.Value < min.Value)
            throw new ArgumentException("Target duration cannot be less than minimum duration.");

        if (target.HasValue && max.HasValue && max.Value < target.Value)
            throw new ArgumentException("Maximum duration cannot be less than target duration.");

        if (min.HasValue && max.HasValue && max.Value < min.Value)
            throw new ArgumentException("Maximum duration cannot be less than minimum duration.");
    }
}
