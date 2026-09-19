using AiCoachOs.Domain.AnatomyAndBiomechanics;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain.AnatomyAndBiomechanics;

public class AnatomyDomainTests
{
    [Fact]
    public void CreateAnatomicalRegion_WithValidFields_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var region = new AnatomicalRegion(id, "Shoulder", "Glenohumeral and scapulothoracic complex");

        // Assert
        region.Id.Should().Be(id);
        region.Name.Should().Be("Shoulder");
        region.Description.Should().Be("Glenohumeral and scapulothoracic complex");
        region.Joints.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateAnatomicalRegion_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act
        var act = () => new AnatomicalRegion(Guid.NewGuid(), name!, "Description");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateJoint_WithValidFields_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var regionId = Guid.NewGuid();

        // Act
        var joint = new Joint(id, regionId, "Glenohumeral Joint", "Shoulder Ball-and-Socket", "Multiaxial articulation");

        // Assert
        joint.Id.Should().Be(id);
        joint.RegionId.Should().Be(regionId);
        joint.Name.Should().Be("Glenohumeral Joint");
        joint.CommonName.Should().Be("Shoulder Ball-and-Socket");
        joint.Description.Should().Be("Multiaxial articulation");
        joint.Actions.Should().BeEmpty();
    }

    [Fact]
    public void CreateJoint_WithEmptyRegionId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new Joint(Guid.NewGuid(), Guid.Empty, "Glenohumeral Joint");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateJoint_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act
        var act = () => new Joint(Guid.NewGuid(), Guid.NewGuid(), name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateJointAction_WithValidFields_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var jointId = Guid.NewGuid();

        // Act
        var action = new JointAction(id, jointId, JointActionType.Flexion, PlaneOfMotion.Sagittal, "Elevation of arm anteriorly");

        // Assert
        action.Id.Should().Be(id);
        action.JointId.Should().Be(jointId);
        action.ActionType.Should().Be(JointActionType.Flexion);
        action.PlaneOfMotion.Should().Be(PlaneOfMotion.Sagittal);
        action.Description.Should().Be("Elevation of arm anteriorly");
        action.Muscles.Should().BeEmpty();
        action.Exercises.Should().BeEmpty();
    }

    [Fact]
    public void CreateJointAction_WithEmptyJointId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new JointAction(Guid.NewGuid(), Guid.Empty, JointActionType.Flexion, PlaneOfMotion.Sagittal);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateMuscleJointAction_WithValidFields_ShouldSucceed()
    {
        // Arrange
        var muscleId = Guid.NewGuid();
        var actionId = Guid.NewGuid();

        // Act
        var link = new MuscleJointAction(muscleId, actionId, isPrimaryAction: true);

        // Assert
        link.MuscleId.Should().Be(muscleId);
        link.JointActionId.Should().Be(actionId);
        link.IsPrimaryAction.Should().BeTrue();
    }

    [Fact]
    public void CreateMuscleJointAction_WithEmptyIds_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act1 = () => new MuscleJointAction(Guid.Empty, Guid.NewGuid());
        act1.Should().Throw<ArgumentException>();

        var act2 = () => new MuscleJointAction(Guid.NewGuid(), Guid.Empty);
        act2.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateExerciseJointAction_WithValidFields_ShouldSucceed()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var actionId = Guid.NewGuid();

        // Act
        var link = new ExerciseJointAction(exerciseId, actionId, JointActionRole.PrimaryMover);

        // Assert
        link.ExerciseId.Should().Be(exerciseId);
        link.JointActionId.Should().Be(actionId);
        link.Role.Should().Be(JointActionRole.PrimaryMover);
    }

    [Fact]
    public void CreateExerciseJointAction_WithEmptyIds_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act1 = () => new ExerciseJointAction(Guid.Empty, Guid.NewGuid());
        act1.Should().Throw<ArgumentException>();

        var act2 = () => new ExerciseJointAction(Guid.NewGuid(), Guid.Empty);
        act2.Should().Throw<ArgumentException>();
    }
}
