using AiCoachOs.Domain.Gyms;

namespace AiCoachOs.Application.Gyms.Dtos;

public record GymProfileDto(
    Guid Id,
    Guid CoachId,
    string Name,
    EquipmentTier Tier,
    string? Location,
    IReadOnlyList<Guid> ExplicitEquipmentIds,
    IReadOnlyList<string> AvailableEquipmentNames,
    bool HasExplicitInventory,
    bool IsInventoryAuthoritative);

public record CreateGymProfileRequestDto(
    string Name,
    EquipmentTier Tier,
    string? Location = null,
    List<Guid>? ExplicitEquipmentIds = null,
    bool IsInventoryAuthoritative = false);

public record UpdateGymProfileRequestDto(
    string Name,
    EquipmentTier Tier,
    string? Location = null,
    List<Guid>? ExplicitEquipmentIds = null,
    bool IsInventoryAuthoritative = false);

public record AssignClientGymRequestDto(
    Guid ClientId,
    Guid? GymProfileId);

public record EquipmentOptionDto(
    Guid Id,
    string Name,
    string? Category,
    bool IsAvailable);
