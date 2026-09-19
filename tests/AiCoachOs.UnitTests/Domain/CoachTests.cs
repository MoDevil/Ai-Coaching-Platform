using AiCoachOs.Domain.Coaches;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain;

public class CoachTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var coach = new Coach(
            id: Guid.NewGuid(),
            identityUserId: "aspnet-user-123",
            fullName: "Captain Mohamed",
            email: "Captain.Mohamed@Coach.eg"
        );

        coach.Should().NotBeNull();
        coach.IdentityUserId.Should().Be("aspnet-user-123");
        coach.FullName.Should().Be("Captain Mohamed");
        coach.Email.Should().Be("captain.mohamed@coach.eg"); // Normalized to lowercase
        coach.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        coach.UpdatedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyIdentityUserId_ShouldThrowArgumentException(string? identityUserId)
    {
        Action act = () => new Coach(Guid.NewGuid(), identityUserId!, "Coach Name", "test@test.com");
        act.Should().Throw<ArgumentException>().WithParameterName("identityUserId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyFullName_ShouldThrowArgumentException(string? fullName)
    {
        Action act = () => new Coach(Guid.NewGuid(), "id-123", fullName!, "test@test.com");
        act.Should().Throw<ArgumentException>().WithParameterName("fullName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldThrowArgumentException(string? email)
    {
        Action act = () => new Coach(Guid.NewGuid(), "id-123", "Coach Name", email!);
        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Fact]
    public void UpdateProfile_ShouldModifyFullNameAndMarkUpdated()
    {
        var coach = new Coach(Guid.NewGuid(), "id-123", "Coach Name", "test@test.com");

        coach.UpdateProfile("Updated Coach Name");

        coach.FullName.Should().Be("Updated Coach Name");
        coach.UpdatedAtUtc.Should().NotBeNull();
    }
}
