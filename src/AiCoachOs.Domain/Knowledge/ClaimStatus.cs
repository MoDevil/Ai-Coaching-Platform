namespace AiCoachOs.Domain.Knowledge;

/// <summary>
/// Lifecycle and versioning status of a scientific knowledge claim.
/// Ensures knowledge is never silently overwritten.
/// </summary>
public enum ClaimStatus
{
    Provisional = 1,
    Active = 2,
    Superseded = 3,
    Rejected = 4
}
