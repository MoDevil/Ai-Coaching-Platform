namespace AiCoachOs.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "AiCoachOs";
    public string Audience { get; set; } = "AiCoachOsApp";
    public string SecretKey { get; set; } = "AiCoachOs_Secure_JWT_Key_Egypt_Coaching_System_2026_Secret!";
    public int ExpirationMinutes { get; set; } = 120;
}
