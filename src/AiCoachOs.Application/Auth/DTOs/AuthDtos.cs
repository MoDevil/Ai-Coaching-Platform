namespace AiCoachOs.Application.Auth.DTOs;

public record RegisterCoachRequestDto(
    string FullName,
    string Email,
    string Password
);

public record LoginCoachRequestDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string Token,
    Guid CoachId,
    string FullName,
    string Email
);
