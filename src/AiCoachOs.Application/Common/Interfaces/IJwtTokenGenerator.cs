using AiCoachOs.Domain.Coaches;

namespace AiCoachOs.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(Coach coach);
}
