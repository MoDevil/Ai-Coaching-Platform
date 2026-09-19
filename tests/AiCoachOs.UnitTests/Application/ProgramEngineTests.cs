using AiCoachOs.Application.Programs.Engine;
using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.TrainingProfiles;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class ProgramEngineTests
{
    private readonly GoalAnalyzer _goalAnalyzer = new();
    private readonly ConstraintAnalyzer _constraintAnalyzer = new();
    private readonly RecoveryModel _recoveryModel = new();
    private readonly ExerciseSelector _exerciseSelector = new();
    private readonly SessionBuilder _sessionBuilder = new();

    [Theory]
    [InlineData("Build maximum muscle and hypertrophy", PrimaryGoalType.Hypertrophy, "6-10", "1-2 RIR")]
    [InlineData("Powerlifting strength focus", PrimaryGoalType.Strength, "3-6", "2-3 RIR (compounds), 1-2 RIR (isolations)")]
    [InlineData("Fat loss and preserve muscle", PrimaryGoalType.FatLoss, "8-12", "2 RIR")]
    public void GoalAnalyzer_DerivesAccurateParameters(
        string goalText,
        PrimaryGoalType expectedGoal,
        string expectedCompoundRepRange,
        string expectedEffort)
    {
        var snapshot = _goalAnalyzer.ParseGoal(goalText, 12, null);
        snapshot.PrimaryGoal.Should().Be(expectedGoal);

        var parameters = _goalAnalyzer.DeriveParameters(snapshot);
        parameters.CompoundRepRange.Should().Be(expectedCompoundRepRange);
        parameters.EffortGuideline.Should().Be(expectedEffort);
    }

    [Fact]
    public void RecoveryModel_EvaluatesHighFatigueContext_AsLowRecoveryCapacity()
    {
        var client = new Client(
            Guid.NewGuid(), Guid.NewGuid(), "Ahmed", "Ali",
            intakeNotes: "Suffers from chronic lower back pain and poor sleep due to night shift work.");

        var availability = new TrainingAvailability(3);
        var profile = new ClientTrainingProfile(Guid.NewGuid(), client.Id, TrainingExperienceLevel.Intermediate, availability);

        var capacity = _recoveryModel.EvaluateRecoveryCapacity(client, profile);

        capacity.Should().Be(RecoveryCapacity.Low);
    }

    [Fact]
    public void ConstraintAnalyzer_FiltersExercisesByEquipment()
    {
        var patternId = Guid.NewGuid();
        var barbellId = Guid.NewGuid();
        var dumbbellId = Guid.NewGuid();

        var barbellSquat = new Exercise(
            Guid.NewGuid(), "Barbell Back Squat", ExerciseCategory.Compound, patternId,
            QualitativeRating.High, QualitativeRating.High, QualitativeRating.High,
            QualitativeRating.High, QualitativeRating.High, QualitativeRating.High,
            ResistanceProfile.MidRange);
        barbellSquat.AddEquipment(barbellId, isRequired: true);

        var clientAvailableEquipment = new HashSet<Guid> { dumbbellId }; // Only dumbbells available

        bool canPerform = _constraintAnalyzer.CanPerformExercise(
            barbellSquat,
            clientAvailableEquipment,
            excludedKeywords: Array.Empty<string>());

        canPerform.Should().BeFalse("Barbell squat requires barbell which client does not possess");
    }

    [Fact]
    public void ConstraintAnalyzer_FiltersExercisesByExclusionKeywords()
    {
        var patternId = Guid.NewGuid();
        var squat = new Exercise(
            Guid.NewGuid(), "Barbell Back Squat", ExerciseCategory.Compound, patternId,
            QualitativeRating.High, QualitativeRating.High, QualitativeRating.High,
            QualitativeRating.High, QualitativeRating.High, QualitativeRating.High,
            ResistanceProfile.MidRange, aliases: "Back Squat");

        bool canPerform = _constraintAnalyzer.CanPerformExercise(
            squat,
            availableEquipmentIds: new HashSet<Guid>(),
            excludedKeywords: new[] { "squat" });

        canPerform.Should().BeFalse("Client explicitly excluded movements containing 'squat'");
    }

    [Fact]
    public void Individualization_TwoClientsWithSameGoal_ProduceDifferentProgramsBasedOnConstraints()
    {
        // Program Builder Test:
        var builder = new ProgramBuilder(_goalAnalyzer, _constraintAnalyzer, _recoveryModel, _exerciseSelector, _sessionBuilder);

        var patternSquat = new MovementPattern(Guid.NewGuid(), "Squat", "Knee dominant");
        var patternPush = new MovementPattern(Guid.NewGuid(), "Horizontal Push", "Chest press");
        var patternHinge = new MovementPattern(Guid.NewGuid(), "Hinge", "Hip dominant");
        var patternPull = new MovementPattern(Guid.NewGuid(), "Horizontal Pull", "Upper back row");

        var allPatterns = new List<MovementPattern> { patternSquat, patternPush, patternHinge, patternPull };

        var quadMuscle = new Muscle(Guid.NewGuid(), "Quadriceps", "Quads", "Legs");
        var chestMuscle = new Muscle(Guid.NewGuid(), "Pectoralis Major", "Chest", "Chest");
        var backMuscle = new Muscle(Guid.NewGuid(), "Latissimus Dorsi", "Lats", "Back");
        var allMuscles = new List<Muscle> { quadMuscle, chestMuscle, backMuscle };

        var barbellId = Guid.NewGuid();
        var dumbbellId = Guid.NewGuid();

        // Exercise 1: Barbell Squat
        var exSquat = new Exercise(
            Guid.NewGuid(), "Barbell Squat", ExerciseCategory.Compound, patternSquat.Id,
            QualitativeRating.High, QualitativeRating.High, QualitativeRating.High, QualitativeRating.High,
            QualitativeRating.High, QualitativeRating.High, ResistanceProfile.MidRange);
        exSquat.AddMuscle(quadMuscle.Id, isPrimary: true);
        exSquat.AddEquipment(barbellId, isRequired: true);

        // Exercise 2: Dumbbell Goblet Squat (Substitute)
        var exGoblet = new Exercise(
            Guid.NewGuid(), "Goblet Squat", ExerciseCategory.Compound, patternSquat.Id,
            QualitativeRating.Moderate, QualitativeRating.Moderate, QualitativeRating.Moderate, QualitativeRating.Low,
            QualitativeRating.Moderate, QualitativeRating.Moderate, ResistanceProfile.MidRange);
        exGoblet.AddMuscle(quadMuscle.Id, isPrimary: true);
        exGoblet.AddEquipment(dumbbellId, isRequired: true);

        // Setup substitution in M2
        exSquat.AddSubstitution(exGoblet.Id, "Dumbbell goblet squat preserves knee flexion mechanics");

        // Exercise 3: Dumbbell Bench Press
        var exBench = new Exercise(
            Guid.NewGuid(), "Dumbbell Bench Press", ExerciseCategory.Compound, patternPush.Id,
            QualitativeRating.Moderate, QualitativeRating.Moderate, QualitativeRating.Moderate, QualitativeRating.Moderate,
            QualitativeRating.High, QualitativeRating.Moderate, ResistanceProfile.MidRange);
        exBench.AddMuscle(chestMuscle.Id, isPrimary: true);
        exBench.AddEquipment(dumbbellId, isRequired: true);

        var allExercises = new List<Exercise> { exSquat, exGoblet, exBench };

        // Client A: 2 days/week, only Dumbbells available
        var clientA = new Client(Guid.NewGuid(), Guid.NewGuid(), "Client", "A", goal: new ClientGoal("Hypertrophy"));
        var profileA = new ClientTrainingProfile(
            Guid.NewGuid(), clientA.Id, TrainingExperienceLevel.Beginner,
            new TrainingAvailability(2),
            availableEquipmentIds: new[] { dumbbellId });

        // Client B: 4 days/week, Barbell + Dumbbells, Advanced
        var clientB = new Client(Guid.NewGuid(), Guid.NewGuid(), "Client", "B", goal: new ClientGoal("Hypertrophy"));
        var profileB = new ClientTrainingProfile(
            Guid.NewGuid(), clientB.Id, TrainingExperienceLevel.Advanced,
            new TrainingAvailability(4),
            availableEquipmentIds: new[] { barbellId, dumbbellId });

        // Act
        var (programA, versionA, volumeA) = builder.BuildProgram(clientA, profileA, allExercises, allPatterns, allMuscles, null, null, 4);
        var (programB, versionB, volumeB) = builder.BuildProgram(clientB, profileB, allExercises, allPatterns, allMuscles, null, null, 4);

        // Assert
        // Client A should have 2 sessions per week, and use Goblet Squat (due to equipment constraint substitution)
        var weekA = versionA.Weeks.First();
        weekA.Sessions.Should().HaveCount(2);
        var allSlotsA = weekA.Sessions.SelectMany(s => s.Slots).ToList();
        allSlotsA.Should().Contain(s => s.ExerciseId == exGoblet.Id);
        allSlotsA.Should().NotContain(s => s.ExerciseId == exSquat.Id);

        // Client B should have 4 sessions per week, and can use Barbell Squat
        var weekB = versionB.Weeks.First();
        weekB.Sessions.Should().HaveCount(4);
        var allSlotsB = weekB.Sessions.SelectMany(s => s.Slots).ToList();
        allSlotsB.Should().Contain(s => s.ExerciseId == exSquat.Id);

        // Total weekly volume is an output variable and differs organically
        volumeA.TotalWeeklySets.Should().NotBe(volumeB.TotalWeeklySets);
        volumeA.TotalWeeklySessions.Should().Be(2);
        volumeB.TotalWeeklySessions.Should().Be(4);
    }
}
