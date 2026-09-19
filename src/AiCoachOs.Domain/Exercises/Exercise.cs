using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Exercises;

public class Exercise : Entity<Guid>
{
    private readonly List<ExerciseMuscle> _muscles = new();
    private readonly List<ExerciseEquipment> _equipment = new();
    private readonly List<ExerciseSubstitution> _substitutions = new();

    public string Name { get; private set; } = null!;
    public string? Aliases { get; private set; }
    public ExerciseCategory Category { get; private set; }

    public Guid MovementPatternId { get; private set; }
    public MovementPattern MovementPattern { get; private set; } = null!;

    public string? JointActions { get; private set; }

    // Qualitative coaching metadata (no arbitrary fake numbers)
    public QualitativeRating StabilityRequirement { get; private set; }
    public QualitativeRating TechnicalDemand { get; private set; }
    public QualitativeRating LocalFatigueCost { get; private set; }
    public QualitativeRating SystemicFatigueCost { get; private set; }
    public QualitativeRating StimulusPotential { get; private set; }
    public QualitativeRating ProgressionPotential { get; private set; }
    public ResistanceProfile ResistanceProfile { get; private set; }

    public Guid? SubstitutionGroupId { get; private set; }


    public IReadOnlyCollection<ExerciseMuscle> Muscles => _muscles;
    public IReadOnlyCollection<ExerciseEquipment> Equipment => _equipment;
    public IReadOnlyCollection<ExerciseSubstitution> Substitutions => _substitutions;

    private Exercise() { } // EF Core

    public Exercise(
        Guid id,
        string name,
        ExerciseCategory category,
        Guid movementPatternId,
        QualitativeRating stabilityRequirement,
        QualitativeRating technicalDemand,
        QualitativeRating localFatigueCost,
        QualitativeRating systemicFatigueCost,
        QualitativeRating stimulusPotential,
        QualitativeRating progressionPotential,
        ResistanceProfile resistanceProfile,
        string? aliases = null,
        string? jointActions = null,
        Guid? substitutionGroupId = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exercise name cannot be empty.", nameof(name));
        if (movementPatternId == Guid.Empty)
            throw new ArgumentException("MovementPatternId cannot be empty.", nameof(movementPatternId));

        Name = name.Trim();
        Category = category;
        MovementPatternId = movementPatternId;
        StabilityRequirement = stabilityRequirement;
        TechnicalDemand = technicalDemand;
        LocalFatigueCost = localFatigueCost;
        SystemicFatigueCost = systemicFatigueCost;
        StimulusPotential = stimulusPotential;
        ProgressionPotential = progressionPotential;
        ResistanceProfile = resistanceProfile;
        Aliases = aliases?.Trim();
        JointActions = jointActions?.Trim();
        SubstitutionGroupId = substitutionGroupId;
    }

    public void AddMuscle(Guid muscleId, bool isPrimary)
    {
        if (muscleId == Guid.Empty)
            throw new ArgumentException("MuscleId cannot be empty.", nameof(muscleId));

        if (_muscles.Any(m => m.MuscleId == muscleId))
            return;

        _muscles.Add(new ExerciseMuscle(Id, muscleId, isPrimary));
        MarkUpdated();
    }

    public void AddEquipment(Guid equipmentId, bool isRequired = true)
    {
        if (equipmentId == Guid.Empty)
            throw new ArgumentException("EquipmentId cannot be empty.", nameof(equipmentId));

        if (_equipment.Any(e => e.EquipmentId == equipmentId))
            return;

        _equipment.Add(new ExerciseEquipment(Id, equipmentId, isRequired));
        MarkUpdated();
    }

    public void AddSubstitution(Guid substituteExerciseId, string? intentPreservationNotes = null)
    {
        if (substituteExerciseId == Guid.Empty)
            throw new ArgumentException("SubstituteExerciseId cannot be empty.", nameof(substituteExerciseId));

        if (substituteExerciseId == Id)
            throw new ArgumentException("An exercise cannot be a substitute for itself.");

        if (_substitutions.Any(s => s.SubstituteExerciseId == substituteExerciseId))
            return;

        _substitutions.Add(new ExerciseSubstitution(Guid.NewGuid(), Id, substituteExerciseId, intentPreservationNotes));
        MarkUpdated();
    }
}
