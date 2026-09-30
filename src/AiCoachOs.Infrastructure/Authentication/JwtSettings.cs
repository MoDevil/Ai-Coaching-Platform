namespace AiCoachOs.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Minimum accepted length for the HMAC-SHA256 signing key.</summary>
    public const int MinimumSecretKeyLength = 32;

    public string Issuer { get; set; } = "AiCoachOs";
    public string Audience { get; set; } = "AiCoachOsApp";
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 120;
}
