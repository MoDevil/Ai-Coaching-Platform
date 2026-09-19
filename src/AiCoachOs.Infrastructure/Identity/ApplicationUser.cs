using Microsoft.AspNetCore.Identity;

namespace AiCoachOs.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public Guid CoachId { get; set; }
}
