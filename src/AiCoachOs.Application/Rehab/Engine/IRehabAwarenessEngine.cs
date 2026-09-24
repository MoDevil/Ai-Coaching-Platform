using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Knowledge;
using AiCoachOs.Domain.Rehab;
using AiCoachOs.Domain.Safety;

namespace AiCoachOs.Application.Rehab.Engine;

public interface IRehabAwarenessEngine
{
    IReadOnlyList<RehabAwarenessConsideration> GenerateConsiderations(
        TrainingLimitation limitation,
        SafetyScreening? safetyScreening,
        Exercise? exercise,
        IReadOnlyList<ExerciseSubstitution> substitutions,
        IReadOnlyList<KnowledgeClaim> knowledgeClaims,
        DateTime generatedAtUtc);
}
