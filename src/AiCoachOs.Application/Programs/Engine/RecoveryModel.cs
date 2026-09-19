using AiCoachOs.Domain.Clients;
using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.TrainingProfiles;

namespace AiCoachOs.Application.Programs.Engine;

public interface IRecoveryModel
{
    RecoveryCapacity EvaluateRecoveryCapacity(Client client, ClientTrainingProfile profile);
}

public class RecoveryModel : IRecoveryModel
{
    public RecoveryCapacity EvaluateRecoveryCapacity(Client client, ClientTrainingProfile profile)
    {
        // Deterministic qualitative classification:
        // Person characteristics + training profile constraints
        var notes = $"{client.IntakeNotes ?? string.Empty} {profile.ExerciseConstraints ?? string.Empty}".ToLowerInvariant();

        // High fatigue/stress/poor recovery indicators:
        bool hasHighFatigueContext = notes.Contains("poor sleep") ||
                                     notes.Contains("high stress") ||
                                     notes.Contains("shift work") ||
                                     notes.Contains("chronic fatigue") ||
                                     notes.Contains("injury prone") ||
                                     notes.Contains("joint pain") ||
                                     notes.Contains("lower back pain") ||
                                     notes.Contains("recovering from injury");

        bool hasFavorableContext = notes.Contains("great sleep") ||
                                   notes.Contains("optimal recovery") ||
                                   notes.Contains("young athlete") ||
                                   notes.Contains("low stress");

        int age = 30; // default adult
        if (client.DateOfBirth.HasValue)
        {
            var today = DateTime.UtcNow;
            age = today.Year - client.DateOfBirth.Value.Year;
            if (client.DateOfBirth.Value.Date > today.AddYears(-age)) age--;
        }

        // Beginner clients have low work capacity/tolerance to high volumes, but recover fast between exposures.
        // Advanced athletes have high work capacity but higher systemic fatigue per heavy session.
        if (hasHighFatigueContext || age >= 55)
        {
            return RecoveryCapacity.Low;
        }

        if (hasFavorableContext && profile.ExperienceLevel == TrainingExperienceLevel.Advanced && age < 40)
        {
            return RecoveryCapacity.High;
        }

        if (profile.ExperienceLevel == TrainingExperienceLevel.Beginner)
        {
            // Beginners need moderate overall volume and lower systemic fatigue
            return RecoveryCapacity.Moderate;
        }

        if (profile.WeeklyAvailability.SessionsPerWeek >= 5 && (profile.SessionDurationTargetMinutes ?? 60) >= 75)
        {
            return RecoveryCapacity.High;
        }

        return RecoveryCapacity.Moderate;
    }
}
