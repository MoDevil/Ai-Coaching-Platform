using AiCoachOs.Application.Auth.DTOs;

namespace AiCoachOs.Application.Auth.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterCoachRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginCoachRequestDto request, CancellationToken cancellationToken = default);
}
