using AiCoachOs.Application.Auth.DTOs;
using AiCoachOs.Application.Auth.Services;
using AiCoachOs.Application.Common.Exceptions;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Domain.Coaches;
using Microsoft.AspNetCore.Identity;

namespace AiCoachOs.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext dbContext,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterCoachRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ConflictException($"An account with email '{request.Email}' already exists.");
        }

        var coachId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = request.Email.Trim().ToLowerInvariant(),
            Email = request.Email.Trim().ToLowerInvariant(),
            FullName = request.FullName.Trim(),
            CoachId = coachId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            throw new ValidationException(new Dictionary<string, string[]> { { "Password", errors } });
        }

        var coach = new Coach(coachId, user.Id, request.FullName, request.Email);
        await _dbContext.AddCoachAsync(coach, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(coach);

        return new AuthResponseDto(token, coach.Id, coach.FullName, coach.Email);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginCoachRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var coach = await _dbContext.FindCoachByIdentityUserIdAsync(user.Id, cancellationToken);
        if (coach == null)
        {
            // Fallback to searching by CoachId
            coach = await _dbContext.FindCoachByIdAsync(user.CoachId, cancellationToken);
            if (coach == null)
            {
                throw new UnauthorizedException("Coach profile not found for this account.");
            }
        }

        var token = _jwtTokenGenerator.GenerateToken(coach);

        return new AuthResponseDto(token, coach.Id, coach.FullName, coach.Email);
    }
}
