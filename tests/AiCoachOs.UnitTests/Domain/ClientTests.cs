using AiCoachOs.Domain.Clients;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain;

public class ClientTests
{
    private readonly Guid _coachId = Guid.NewGuid();

    [Fact]
    public void Create_WithRequiredFieldsOnly_ShouldSucceed()
    {
        // Act
        var client = new Client(
            id: Guid.NewGuid(),
            coachId: _coachId,
            firstName: "Ahmed",
            lastName: "Hassan"
        );

        // Assert
        client.Should().NotBeNull();
        client.CoachId.Should().Be(_coachId);
        client.FirstName.Should().Be("Ahmed");
        client.LastName.Should().Be("Hassan");
        client.Email.Should().BeNull();
        client.Phone.Should().BeNull();
        client.DateOfBirth.Should().BeNull();
        client.Gender.Should().BeNull();
        client.Goal.Should().BeNull();
        client.IntakeNotes.Should().BeNull();
        client.Status.Should().Be(ClientStatus.Active);
        client.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        client.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllOptionalFields_ShouldSucceed()
    {
        // Arrange
        var dob = new DateTime(1995, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var goal = new ClientGoal("Hypertrophy", 12, "Focus on delts and upper chest");

        // Act
        var client = new Client(
            id: Guid.NewGuid(),
            coachId: _coachId,
            firstName: "Mahmoud",
            lastName: "Ali",
            email: "Mahmoud@example.com",
            phone: "+201001234567",
            dateOfBirth: dob,
            gender: Gender.Male,
            goal: goal,
            intakeNotes: "Trains at local gym in Cairo, 4 days available."
        );

        // Assert
        client.FirstName.Should().Be("Mahmoud");
        client.LastName.Should().Be("Ali");
        client.Email.Should().Be("mahmoud@example.com"); // Normalized to lowercase
        client.Phone.Should().Be("+201001234567");
        client.DateOfBirth.Should().Be(dob);
        client.Gender.Should().Be(Gender.Male);
        client.Goal.Should().NotBeNull();
        client.Goal!.PrimaryGoal.Should().Be("Hypertrophy");
        client.Goal.TargetTimelineWeeks.Should().Be(12);
        client.IntakeNotes.Should().Be("Trains at local gym in Cairo, 4 days available.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyFirstName_ShouldThrowArgumentException(string? firstName)
    {
        Action act = () => new Client(
            id: Guid.NewGuid(),
            coachId: _coachId,
            firstName: firstName!,
            lastName: "Ali"
        );

        act.Should().Throw<ArgumentException>()
           .WithParameterName("firstName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyLastName_ShouldThrowArgumentException(string? lastName)
    {
        Action act = () => new Client(
            id: Guid.NewGuid(),
            coachId: _coachId,
            firstName: "Omar",
            lastName: lastName!
        );

        act.Should().Throw<ArgumentException>()
           .WithParameterName("lastName");
    }

    [Fact]
    public void Create_WithEmptyCoachId_ShouldThrowArgumentException()
    {
        Action act = () => new Client(
            id: Guid.NewGuid(),
            coachId: Guid.Empty,
            firstName: "Omar",
            lastName: "Khaled"
        );

        act.Should().Throw<ArgumentException>()
           .WithParameterName("coachId");
    }

    [Fact]
    public void UpdateProfile_ShouldModifyFieldsAndSetUpdatedAtUtc()
    {
        // Arrange
        var client = new Client(Guid.NewGuid(), _coachId, "Tarek", "Nabil");

        // Act
        client.UpdateProfile("Tarek", "El-Sayed", "tarek@gym.eg", "+201112223334", null, Gender.Male, "Updated intake");

        // Assert
        client.LastName.Should().Be("El-Sayed");
        client.Email.Should().Be("tarek@gym.eg");
        client.Phone.Should().Be("+201112223334");
        client.Gender.Should().Be(Gender.Male);
        client.IntakeNotes.Should().Be("Updated intake");
        client.UpdatedAtUtc.Should().NotBeNull();
        client.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SetGoal_ShouldUpdateGoalAndMarkUpdated()
    {
        // Arrange
        var client = new Client(Guid.NewGuid(), _coachId, "Kareem", "Adel");
        var goal = new ClientGoal("Strength", 16, "Aiming for 140kg squat");

        // Act
        client.SetGoal(goal);

        // Assert
        client.Goal.Should().Be(goal);
        client.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Archive_ShouldSetStatusToArchived()
    {
        // Arrange
        var client = new Client(Guid.NewGuid(), _coachId, "Sara", "Ibrahim");
        client.Status.Should().Be(ClientStatus.Active);

        // Act
        client.Archive();

        // Assert
        client.Status.Should().Be(ClientStatus.Archived);
        client.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Activate_ShouldSetStatusToActive()
    {
        // Arrange
        var client = new Client(Guid.NewGuid(), _coachId, "Sara", "Ibrahim");
        client.Archive();
        client.Status.Should().Be(ClientStatus.Archived);

        // Act
        client.Activate();

        // Assert
        client.Status.Should().Be(ClientStatus.Active);
        client.UpdatedAtUtc.Should().NotBeNull();
    }
}
