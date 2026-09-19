using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AiCoachOs.Application.Common.Interfaces;
using AiCoachOs.Domain.Coaches;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AiCoachOs.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(Coach coach)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, coach.IdentityUserId),
            new(JwtRegisteredClaimNames.Email, coach.Email),
            new(JwtRegisteredClaimNames.Name, coach.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("coach_id", coach.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
