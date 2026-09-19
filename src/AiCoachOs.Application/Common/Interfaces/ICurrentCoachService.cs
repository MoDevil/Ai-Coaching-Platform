namespace AiCoachOs.Application.Common.Interfaces;

public interface ICurrentCoachService
{
    Guid? CoachId { get; }
    string? IdentityUserId { get; }
    bool IsAuthenticated { get; }
    Task<Guid> GetRequiredCoachIdAsync(CancellationToken cancellationToken = default);
}
