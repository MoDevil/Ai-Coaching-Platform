using System.Security.Claims;
using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AiCoachOs.Infrastructure.Services;

public class CurrentCoachService : ICurrentCoachService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationDbContext _dbContext;

    public CurrentCoachService(IHttpContextAccessor httpContextAccessor, IApplicationDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public string? IdentityUserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public Guid? CoachId
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User.FindFirstValue("coach_id");
            if (Guid.TryParse(claimValue, out var coachId))
            {
                return coachId;
            }
            return null;
        }
    }

    public async Task<Guid> GetRequiredCoachIdAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        if (CoachId.HasValue && CoachId.Value != Guid.Empty)
        {
            return CoachId.Value;
        }

        var identityUserId = IdentityUserId;
        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            throw new UnauthorizedException("Authenticated identity user ID is missing.");
        }

        var coach = await _dbContext.FindCoachByIdentityUserIdAsync(identityUserId, cancellationToken);
        if (coach == null)
        {
            throw new UnauthorizedException("Associated coach profile was not found for the authenticated user.");
        }

        return coach.Id;
    }
}
