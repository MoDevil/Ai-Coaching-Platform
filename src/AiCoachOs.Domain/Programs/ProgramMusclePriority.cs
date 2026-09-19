using AiCoachOs.Domain.Common;
using AiCoachOs.Domain.Exercises;

namespace AiCoachOs.Domain.Programs;

public class ProgramMusclePriority : Entity<Guid>
{
    public Guid ProgramVersionId { get; private set; }
    public ProgramVersion ProgramVersion { get; private set; } = null!;

    public Guid MuscleId { get; private set; }
    public Muscle Muscle { get; private set; } = null!;

    public MusclePriorityLevel PriorityLevel { get; private set; }
    public string? Justification { get; private set; }

    private ProgramMusclePriority() { } // EF Core

    public ProgramMusclePriority(
        Guid id,
        Guid programVersionId,
        Guid muscleId,
        MusclePriorityLevel priorityLevel,
        string? justification = null) : base(id)
    {
        if (programVersionId == Guid.Empty)
            throw new ArgumentException("ProgramVersionId cannot be empty.", nameof(programVersionId));
        if (muscleId == Guid.Empty)
            throw new ArgumentException("MuscleId cannot be empty.", nameof(muscleId));

        ProgramVersionId = programVersionId;
        MuscleId = muscleId;
        PriorityLevel = priorityLevel;
        Justification = string.IsNullOrWhiteSpace(justification) ? null : justification.Trim();
    }
}
