using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Coaches;

public class Coach : Entity<Guid>
{
    public string IdentityUserId { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    // Parameterless constructor for EF Core
    private Coach() { }

    public Coach(Guid id, string identityUserId, string fullName, string email) : base(id)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new ArgumentException("Identity user ID is required.", nameof(identityUserId));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        IdentityUserId = identityUserId;
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
    }

    public void UpdateProfile(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));

        FullName = fullName.Trim();
        MarkUpdated();
    }
}
