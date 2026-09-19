using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Programs;

namespace AiCoachOs.Domain.Workouts;

public class WorkoutExercise : Entity<Guid>
{
    private readonly List<WorkoutSet> _sets = new();

    public Guid WorkoutSessionId { get; private set; }
    public WorkoutSession WorkoutSession { get; private set; } = null!;

    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;

    public Guid? ExerciseSlotId { get; private set; }
    public ExerciseSlot? ExerciseSlot { get; private set; }

    public int OrderInSession { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<WorkoutSet> Sets => _sets.OrderBy(s => s.SetNumber).ToList().AsReadOnly();

    private WorkoutExercise() { } // EF Core

    public WorkoutExercise(
        Guid id,
        Guid workoutSessionId,
        Guid exerciseId,
        int orderInSession,
        Guid? exerciseSlotId = null,
        string? notes = null) : base(id)
    {
        if (workoutSessionId == Guid.Empty)
            throw new ArgumentException("WorkoutSessionId cannot be empty.", nameof(workoutSessionId));
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (orderInSession < 1)
            throw new ArgumentOutOfRangeException(nameof(orderInSession), "Order in session must be at least 1.");

        WorkoutSessionId = workoutSessionId;
        ExerciseId = exerciseId;
        OrderInSession = orderInSession;
        ExerciseSlotId = exerciseSlotId;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void AddSet(WorkoutSet set)
    {
        ArgumentNullException.ThrowIfNull(set);
        if (_sets.Any(s => s.Id == set.Id))
            return;

        _sets.Add(set);
        MarkUpdated();
    }

    public void RemoveSet(Guid setId)
    {
        var set = _sets.FirstOrDefault(s => s.Id == setId);
        if (set != null)
        {
            _sets.Remove(set);
            MarkUpdated();
        }
    }
}
