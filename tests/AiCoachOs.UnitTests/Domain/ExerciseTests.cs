using AiCoachOs.Domain.Exercises;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain;

public class ExerciseTests
{
    [Fact]
    public void CreateExercise_WithValidProperties_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var patternId = Guid.NewGuid();

        // Act
        var exercise = new Exercise(
            id: id,
            name: "Barbell Back Squat",
            category: ExerciseCategory.Compound,
            movementPatternId: patternId,
            stabilityRequirement: QualitativeRating.High,
            technicalDemand: QualitativeRating.High,
            localFatigueCost: QualitativeRating.High,
            systemicFatigueCost: QualitativeRating.High,
            stimulusPotential: QualitativeRating.High,
            progressionPotential: QualitativeRating.High,
            resistanceProfile: ResistanceProfile.MidRange,
            aliases: "Back Squat",
            jointActions: "Knee extension, hip extension"
        );

        // Assert
        exercise.Id.Should().Be(id);
        exercise.Name.Should().Be("Barbell Back Squat");
        exercise.Category.Should().Be(ExerciseCategory.Compound);
        exercise.MovementPatternId.Should().Be(patternId);
        exercise.StabilityRequirement.Should().Be(QualitativeRating.High);
        exercise.TechnicalDemand.Should().Be(QualitativeRating.High);
        exercise.LocalFatigueCost.Should().Be(QualitativeRating.High);
        exercise.SystemicFatigueCost.Should().Be(QualitativeRating.High);
        exercise.StimulusPotential.Should().Be(QualitativeRating.High);
        exercise.ProgressionPotential.Should().Be(QualitativeRating.High);
        exercise.ResistanceProfile.Should().Be(ResistanceProfile.MidRange);
        exercise.Aliases.Should().Be("Back Squat");
        exercise.MetadataStatus.Should().Be(MetadataStatus.Provisional);
        exercise.Muscles.Should().BeEmpty();
        exercise.Equipment.Should().BeEmpty();
        exercise.Substitutions.Should().BeEmpty();
    }

    [Fact]
    public void CreateExercise_WithExplicitMetadataStatus_ShouldPersistStatus()
    {
        // Act
        var exercise = new Exercise(
            id: Guid.NewGuid(),
            name: "Verified Barbell Curl",
            category: ExerciseCategory.Isolation,
            movementPatternId: Guid.NewGuid(),
            stabilityRequirement: QualitativeRating.Low,
            technicalDemand: QualitativeRating.Low,
            localFatigueCost: QualitativeRating.Moderate,
            systemicFatigueCost: QualitativeRating.Low,
            stimulusPotential: QualitativeRating.High,
            progressionPotential: QualitativeRating.Moderate,
            resistanceProfile: ResistanceProfile.MidRange,
            metadataStatus: MetadataStatus.Verified
        );

        // Assert
        exercise.MetadataStatus.Should().Be(MetadataStatus.Verified);
    }

    [Fact]
    public void UpdateMetadataStatus_ShouldUpdateStatusAndMarkUpdated()
    {
        // Arrange
        var exercise = new Exercise(
            id: Guid.NewGuid(),
            name: "Provisional Press",
            category: ExerciseCategory.Compound,
            movementPatternId: Guid.NewGuid(),
            stabilityRequirement: QualitativeRating.Moderate,
            technicalDemand: QualitativeRating.Moderate,
            localFatigueCost: QualitativeRating.Moderate,
            systemicFatigueCost: QualitativeRating.Moderate,
            stimulusPotential: QualitativeRating.Moderate,
            progressionPotential: QualitativeRating.Moderate,
            resistanceProfile: ResistanceProfile.MidRange
        );

        // Act
        exercise.UpdateMetadataStatus(MetadataStatus.Verified);

        // Assert
        exercise.MetadataStatus.Should().Be(MetadataStatus.Verified);
        exercise.UpdatedAtUtc.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateExercise_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act
        var act = () => new Exercise(
            id: Guid.NewGuid(),
            name: name!,
            category: ExerciseCategory.Compound,
            movementPatternId: Guid.NewGuid(),
            stabilityRequirement: QualitativeRating.Low,
            technicalDemand: QualitativeRating.Low,
            localFatigueCost: QualitativeRating.Low,
            systemicFatigueCost: QualitativeRating.Low,
            stimulusPotential: QualitativeRating.Low,
            progressionPotential: QualitativeRating.Low,
            resistanceProfile: ResistanceProfile.Even
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddMuscle_ShouldAddAndIgnoreDuplicates()
    {
        // Arrange
        var exercise = new Exercise(
            Guid.NewGuid(), "Leg Press", ExerciseCategory.Machine, Guid.NewGuid(),
            QualitativeRating.High, QualitativeRating.Low, QualitativeRating.High, QualitativeRating.Moderate,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.MidRange
        );
        var quadId = Guid.NewGuid();

        // Act
        exercise.AddMuscle(quadId, isPrimary: true);
        exercise.AddMuscle(quadId, isPrimary: true); // duplicate

        // Assert
        exercise.Muscles.Should().HaveCount(1);
        exercise.Muscles.First().MuscleId.Should().Be(quadId);
        exercise.Muscles.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void AddEquipment_ShouldAddAndIgnoreDuplicates()
    {
        // Arrange
        var exercise = new Exercise(
            Guid.NewGuid(), "Bench Press", ExerciseCategory.Compound, Guid.NewGuid(),
            QualitativeRating.Moderate, QualitativeRating.Moderate, QualitativeRating.High, QualitativeRating.Moderate,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.MidRange
        );
        var barbellId = Guid.NewGuid();

        // Act
        exercise.AddEquipment(barbellId, isRequired: true);
        exercise.AddEquipment(barbellId, isRequired: true); // duplicate

        // Assert
        exercise.Equipment.Should().HaveCount(1);
        exercise.Equipment.First().EquipmentId.Should().Be(barbellId);
    }

    [Fact]
    public void AddSubstitution_WhenSelf_ShouldThrowArgumentException()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = new Exercise(
            exerciseId, "Deadlift", ExerciseCategory.Compound, Guid.NewGuid(),
            QualitativeRating.Low, QualitativeRating.High, QualitativeRating.High, QualitativeRating.High,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.Lengthened
        );

        // Act
        var act = () => exercise.AddSubstitution(exerciseId, "Self substitution");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddSubstitution_WithIntentNotes_ShouldRecordSuccessfully()
    {
        // Arrange
        var exercise = new Exercise(
            Guid.NewGuid(), "Barbell Back Squat", ExerciseCategory.Compound, Guid.NewGuid(),
            QualitativeRating.High, QualitativeRating.High, QualitativeRating.High, QualitativeRating.High,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.MidRange
        );
        var hackSquatId = Guid.NewGuid();

        // Act
        exercise.AddSubstitution(hackSquatId, "Machine variant preserves quad stimulus with reduced spinal shear.");

        // Assert
        exercise.Substitutions.Should().HaveCount(1);
        var sub = exercise.Substitutions.First();
        sub.SubstituteExerciseId.Should().Be(hackSquatId);
        sub.IntentPreservationNotes.Should().Contain("quad stimulus");
    }
}
