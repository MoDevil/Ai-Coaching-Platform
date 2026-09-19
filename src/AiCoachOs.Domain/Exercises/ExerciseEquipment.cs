namespace AiCoachOs.Domain.Exercises;

public class ExerciseEquipment
{
    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;

    public Guid EquipmentId { get; private set; }
    public Equipment Equipment { get; private set; } = null!;

    public bool IsRequired { get; private set; }

    private ExerciseEquipment() { } // EF Core

    public ExerciseEquipment(Guid exerciseId, Guid equipmentId, bool isRequired = true)
    {
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("ExerciseId cannot be empty.", nameof(exerciseId));
        if (equipmentId == Guid.Empty)
            throw new ArgumentException("EquipmentId cannot be empty.", nameof(equipmentId));

        ExerciseId = exerciseId;
        EquipmentId = equipmentId;
        IsRequired = isRequired;
    }
}
