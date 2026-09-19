using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Coaches;
using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Programs;

namespace AiCoachOs.Domain.Workouts;

public class WorkoutSession : Entity<Guid>
{
    private readonly List<WorkoutExercise> _exercises = new();

    public Guid ClientId { get; private set; }
    public Client Client { get; private set; } = null!;

    public Guid CoachId { get; private set; }
    public Coach Coach { get; private set; } = null!;

    public Guid? ProgramVersionId { get; private set; }
    public ProgramVersion? ProgramVersion { get; private set; }

    public Guid? TrainingSessionId { get; private set; }
    public TrainingSession? TrainingSession { get; private set; }

    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public WorkoutStatus Status { get; private set; } = WorkoutStatus.InProgress;
    public string? Notes { get; private set; }

    public IReadOnlyCollection<WorkoutExercise> Exercises => _exercises.OrderBy(e => e.OrderInSession).ToList().AsReadOnly();

    private WorkoutSession() { } // EF Core

    public WorkoutSession(
        Guid id,
        Guid clientId,
        Guid coachId,
        DateTime startedAtUtc,
        Guid? programVersionId = null,
        Guid? trainingSessionId = null,
        string? notes = null) : base(id)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty.", nameof(clientId));
        if (coachId == Guid.Empty)
            throw new ArgumentException("CoachId cannot be empty.", nameof(coachId));

        ClientId = clientId;
        CoachId = coachId;
        StartedAtUtc = startedAtUtc;
        ProgramVersionId = programVersionId;
        TrainingSessionId = trainingSessionId;
        Status = WorkoutStatus.InProgress;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void AddExercise(WorkoutExercise exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        if (_exercises.Any(e => e.Id == exercise.Id))
            return;

        _exercises.Add(exercise);
        MarkUpdated();
    }

    public void Complete(DateTime completedAtUtc, string? notes = null)
    {
        if (Status != WorkoutStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete a workout that is already {Status}.");
        if (completedAtUtc < StartedAtUtc)
            throw new ArgumentException("Completion time cannot precede start time.", nameof(completedAtUtc));

        CompletedAtUtc = completedAtUtc;
        Status = WorkoutStatus.Completed;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = string.IsNullOrWhiteSpace(Notes) ? notes.Trim() : $"{Notes}\n{notes.Trim()}";
        }
        MarkUpdated();
    }

    public void Abandon(string? notes = null)
    {
        if (Status != WorkoutStatus.InProgress)
            throw new InvalidOperationException($"Cannot abandon a workout that is already {Status}.");

        CompletedAtUtc = DateTime.UtcNow;
        Status = WorkoutStatus.Abandoned;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = string.IsNullOrWhiteSpace(Notes) ? notes.Trim() : $"{Notes}\n{notes.Trim()}";
        }
        MarkUpdated();
    }
}
