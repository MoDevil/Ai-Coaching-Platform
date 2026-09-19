using AiCoachOs.Domain.Clients;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Domain;

public class ConsentRecordTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var clientId = Guid.NewGuid();

        var consent = new ConsentRecord(
            id: Guid.NewGuid(),
            clientId: clientId,
            consentType: "DataProcessing",
            isGranted: true,
            notes: "Client signed initial consent."
        );

        consent.Should().NotBeNull();
        consent.ClientId.Should().Be(clientId);
        consent.ConsentType.Should().Be("DataProcessing");
        consent.IsGranted.Should().BeTrue();
        consent.Notes.Should().Be("Client signed initial consent.");
        consent.GrantedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithEmptyClientId_ShouldThrowArgumentException()
    {
        Action act = () => new ConsentRecord(
            id: Guid.NewGuid(),
            clientId: Guid.Empty,
            consentType: "DataProcessing",
            isGranted: true
        );

        act.Should().Throw<ArgumentException>()
           .WithParameterName("clientId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyConsentType_ShouldThrowArgumentException(string? consentType)
    {
        Action act = () => new ConsentRecord(
            id: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            consentType: consentType!,
            isGranted: true
        );

        act.Should().Throw<ArgumentException>()
           .WithParameterName("consentType");
    }

    [Fact]
    public void UpdateConsent_ShouldModifyStatusAndSetUpdatedAtUtc()
    {
        var consent = new ConsentRecord(Guid.NewGuid(), Guid.NewGuid(), "DataProcessing", true);

        consent.UpdateConsent(false, "Client revoked consent");

        consent.IsGranted.Should().BeFalse();
        consent.Notes.Should().Be("Client revoked consent");
        consent.UpdatedAtUtc.Should().NotBeNull();
    }
}
