using AiCoachOs.Domain.Common;

namespace AiCoachOs.Domain.Clients;

public class Client : Entity<Guid>
{
    public Guid CoachId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public ClientStatus Status { get; private set; } = ClientStatus.Active;
    public ClientGoal? Goal { get; private set; }
    public string? IntakeNotes { get; private set; }

    // Parameterless constructor for EF Core
    private Client() { }

    public Client(
        Guid id,
        Guid coachId,
        string firstName,
        string lastName,
        string? email = null,
        string? phone = null,
        DateTime? dateOfBirth = null,
        Gender? gender = null,
        ClientGoal? goal = null,
        string? intakeNotes = null) : base(id)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("Coach ID cannot be empty.", nameof(coachId));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        CoachId = coachId;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Status = ClientStatus.Active;
        Goal = goal;
        IntakeNotes = string.IsNullOrWhiteSpace(intakeNotes) ? null : intakeNotes.Trim();
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        string? email,
        string? phone,
        DateTime? dateOfBirth,
        Gender? gender,
        string? intakeNotes)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        DateOfBirth = dateOfBirth;
        Gender = gender;
        IntakeNotes = string.IsNullOrWhiteSpace(intakeNotes) ? null : intakeNotes.Trim();

        MarkUpdated();
    }

    public void SetGoal(ClientGoal? goal)
    {
        Goal = goal;
        MarkUpdated();
    }

    public void Archive()
    {
        Status = ClientStatus.Archived;
        MarkUpdated();
    }

    public void Activate()
    {
        Status = ClientStatus.Active;
        MarkUpdated();
    }
}
