using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.TrainingProfiles;

public class ClientTrainingPriority : Entity<Guid>
{
    public Guid ProfileId { get; private set; }
    public int Order { get; private set; }
    public string FocusArea { get; private set; } = null!;
    public string? Notes { get; private set; }

    private ClientTrainingPriority() { } // EF Core

    public ClientTrainingPriority(Guid id, Guid profileId, int order, string focusArea, string? notes = null) : base(id)
    {
        if (profileId == Guid.Empty)
            throw new ArgumentException("ProfileId cannot be empty.", nameof(profileId));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be at least 1.");
        if (string.IsNullOrWhiteSpace(focusArea))
            throw new ArgumentException("Focus area cannot be empty.", nameof(focusArea));

        ProfileId = profileId;
        Order = order;
        FocusArea = focusArea.Trim();
        Notes = notes?.Trim();
    }

    public void Update(int order, string focusArea, string? notes = null)
    {
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be at least 1.");
        if (string.IsNullOrWhiteSpace(focusArea))
            throw new ArgumentException("Focus area cannot be empty.", nameof(focusArea));

        Order = order;
        FocusArea = focusArea.Trim();
        Notes = notes?.Trim();
    }
}
