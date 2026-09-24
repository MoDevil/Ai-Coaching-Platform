using AiCoachOs.Application.Gyms.Engine;
using AiCoachOs.Domain.Gyms;
using FluentAssertions;
using Xunit;

namespace AiCoachOs.UnitTests.Application;

public class GymEquipmentResolverTests
{
    private readonly GymEquipmentResolver _resolver = new();

    [Fact]
    public void GetDefaultEquipmentForTier_MinimalTier_ReturnsDumbbellAndBodyweight_NoBarbellNoRacks()
    {
        // Act
        var equipment = _resolver.GetDefaultEquipmentForTier(EquipmentTier.Minimal);

        // Assert
        equipment.Should().HaveCount(2);
        equipment.Should().Contain(GymEquipmentResolver.Dumbbell);
        equipment.Should().Contain(GymEquipmentResolver.Bodyweight);
        equipment.Should().NotContain(GymEquipmentResolver.Barbell);
        equipment.Should().NotContain(GymEquipmentResolver.SquatRack);
        equipment.Should().NotContain(GymEquipmentResolver.Cable);
        equipment.Should().NotContain(GymEquipmentResolver.Machine);
    }

    [Fact]
    public void GetDefaultEquipmentForTier_BasicTier_ReturnsDumbbellBenchMachine_StrictlyNoBarbellNoSquatRack()
    {
        // Act
        var equipment = _resolver.GetDefaultEquipmentForTier(EquipmentTier.Basic);

        // Assert
        equipment.Should().HaveCount(4);
        equipment.Should().Contain(GymEquipmentResolver.Dumbbell);
        equipment.Should().Contain(GymEquipmentResolver.Bench);
        equipment.Should().Contain(GymEquipmentResolver.Machine);
        equipment.Should().Contain(GymEquipmentResolver.Bodyweight);
        equipment.Should().NotContain(GymEquipmentResolver.Barbell);
        equipment.Should().NotContain(GymEquipmentResolver.SquatRack);
        equipment.Should().NotContain(GymEquipmentResolver.Cable);
    }

    [Fact]
    public void GetDefaultEquipmentForTier_CommercialAndPremiumTiers_SatisfyFullFreeWeightsCablesAndMachines()
    {
        // Act
        var commercial = _resolver.GetDefaultEquipmentForTier(EquipmentTier.Commercial);
        var premium = _resolver.GetDefaultEquipmentForTier(EquipmentTier.Premium);

        // Assert
        commercial.Should().HaveCount(7);
        commercial.Should().Contain(GymEquipmentResolver.Barbell);
        commercial.Should().Contain(GymEquipmentResolver.Dumbbell);
        commercial.Should().Contain(GymEquipmentResolver.Bench);
        commercial.Should().Contain(GymEquipmentResolver.SquatRack);
        commercial.Should().Contain(GymEquipmentResolver.Cable);
        commercial.Should().Contain(GymEquipmentResolver.Machine);
        commercial.Should().Contain(GymEquipmentResolver.Bodyweight);

        premium.Should().BeEquivalentTo(commercial);
    }

    [Fact]
    public void ResolveAvailableEquipment_NoExplicitInventory_UsesTierDefaults()
    {
        // Arrange
        var basicGym = new GymProfile(
            id: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            name: "Basic Gym Without Explicit Items",
            tier: EquipmentTier.Basic,
            location: "Giza",
            explicitEquipmentIds: Array.Empty<Guid>(),
            isInventoryAuthoritative: false);

        // Act
        var resolved = _resolver.ResolveAvailableEquipment(basicGym);

        // Assert
        resolved.Should().HaveCount(4);
        resolved.Should().Contain(GymEquipmentResolver.Dumbbell);
        resolved.Should().Contain(GymEquipmentResolver.Bench);
        resolved.Should().Contain(GymEquipmentResolver.Machine);
        resolved.Should().Contain(GymEquipmentResolver.Bodyweight);
        resolved.Should().NotContain(GymEquipmentResolver.Barbell);
    }

    [Fact]
    public void ResolveAvailableEquipment_AuthoritativeExplicitInventory_StrictlyOverridesTierDefaults()
    {
        // Arrange: Commercial gym where coach explicitly audited and confirmed only Dumbbell + Bench exist
        var gym = new GymProfile(
            id: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            name: "Audited Studio",
            tier: EquipmentTier.Commercial,
            location: "Cairo",
            explicitEquipmentIds: new[] { GymEquipmentResolver.Dumbbell, GymEquipmentResolver.Bench },
            isInventoryAuthoritative: true);

        // Act
        var resolved = _resolver.ResolveAvailableEquipment(gym);

        // Assert: Strictly Dumbbell + Bench + Bodyweight, NOT Barbell or Cable or Machine
        resolved.Should().HaveCount(3);
        resolved.Should().Contain(GymEquipmentResolver.Dumbbell);
        resolved.Should().Contain(GymEquipmentResolver.Bench);
        resolved.Should().Contain(GymEquipmentResolver.Bodyweight);
        resolved.Should().NotContain(GymEquipmentResolver.Barbell);
        resolved.Should().NotContain(GymEquipmentResolver.SquatRack);
        resolved.Should().NotContain(GymEquipmentResolver.Cable);
        resolved.Should().NotContain(GymEquipmentResolver.Machine);
    }

    [Fact]
    public void ResolveAvailableEquipment_IncompletePartialInventory_FallsBackToTierDefaultsPlusExplicitItems()
    {
        // Arrange: Commercial gym with partial inventory (e.g. coach only added a custom specialty machine)
        var customSpecialtyEquipmentId = Guid.NewGuid();
        var gym = new GymProfile(
            id: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            name: "Commercial Gym With Partial Additions",
            tier: EquipmentTier.Commercial,
            location: "Cairo",
            explicitEquipmentIds: new[] { customSpecialtyEquipmentId },
            isInventoryAuthoritative: false); // Incomplete / non-authoritative

        // Act
        var resolved = _resolver.ResolveAvailableEquipment(gym);

        // Assert: Commercial tier capabilities are preserved AND specialty item is available
        resolved.Should().HaveCount(8);
        resolved.Should().Contain(GymEquipmentResolver.Barbell);
        resolved.Should().Contain(GymEquipmentResolver.SquatRack);
        resolved.Should().Contain(GymEquipmentResolver.Cable);
        resolved.Should().Contain(GymEquipmentResolver.Machine);
        resolved.Should().Contain(customSpecialtyEquipmentId);
    }

    [Fact]
    public void ResolveAvailableEquipment_BasicTierWithIncompleteInventory_CannotSatisfyBarbellRequirement()
    {
        // Arrange: Basic gym with partial inventory of dumbbells
        var gym = new GymProfile(
            id: Guid.NewGuid(),
            coachId: Guid.NewGuid(),
            name: "Basic Gym Partial",
            tier: EquipmentTier.Basic,
            location: "Alexandria",
            explicitEquipmentIds: new[] { GymEquipmentResolver.Dumbbell },
            isInventoryAuthoritative: false);

        // Act
        var resolved = _resolver.ResolveAvailableEquipment(gym);

        // Assert: Barbell and SquatRack remain unavailable
        resolved.Should().NotContain(GymEquipmentResolver.Barbell);
        resolved.Should().NotContain(GymEquipmentResolver.SquatRack);
    }

    [Fact]
    public void ResolveAvailableEquipment_NullGymProfile_ReturnsBodyweightOrFallback()
    {
        // Act
        var resolvedNull = _resolver.ResolveAvailableEquipment((GymProfile?)null);
        var resolvedFallback = _resolver.ResolveAvailableEquipment((GymProfile?)null, new[] { GymEquipmentResolver.Dumbbell });

        // Assert
        resolvedNull.Should().HaveCount(1);
        resolvedNull.Should().Contain(GymEquipmentResolver.Bodyweight);

        resolvedFallback.Should().HaveCount(2);
        resolvedFallback.Should().Contain(GymEquipmentResolver.Dumbbell);
        resolvedFallback.Should().Contain(GymEquipmentResolver.Bodyweight);
    }
}
