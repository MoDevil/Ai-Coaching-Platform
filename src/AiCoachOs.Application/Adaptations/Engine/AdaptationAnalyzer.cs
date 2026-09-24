using System.Text.RegularExpressions;
using AiCoachOs.Application.Programs.Engine;
using AiCoachOs.Application.Workouts.Engine;
using AiCoachOs.Domain.Adaptations;
using AiCoachOs.Domain.Exercises;
using AiCoachOs.Domain.Programs;
using AiCoachOs.Domain.TrainingProfiles;
using AiCoachOs.Domain.Workouts;

namespace AiCoachOs.Application.Adaptations.Engine;

public interface IAdaptationAnalyzer
{
    AdaptationAssessment Analyze(
        ProgramVersion programVersion,
        IReadOnlyList<WorkoutSession> workouts,
        IReadOnlyList<Exercise> allExercises,
        ClientTrainingProfile? profile,
        IExerciseSelector exerciseSelector,
        IConstraintAnalyzer constraintAnalyzer);
}

public class AdaptationAnalyzer : IAdaptationAnalyzer
{
    private readonly IProgressionEvaluator _progressionEvaluator;

    private static readonly string[] PainKeywords = new[]
    {
        "pain", "sharp pain", "hurt", "hurting", "injury", "injured", "tweak", "strain", "ache", "soreness in joint"
    };

    public AdaptationAnalyzer(IProgressionEvaluator progressionEvaluator)
    {
        _progressionEvaluator = progressionEvaluator;
    }

    public AdaptationAssessment Analyze(
        ProgramVersion programVersion,
        IReadOnlyList<WorkoutSession> workouts,
        IReadOnlyList<Exercise> allExercises,
        ClientTrainingProfile? profile,
        IExerciseSelector exerciseSelector,
        IConstraintAnalyzer constraintAnalyzer)
    {
        var assessmentId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        if (workouts.Count == 0)
        {
            var emptyAssessment = new AdaptationAssessment(
                id: assessmentId,
                programVersionId: programVersion.Id,
                assessedAt: now,
                observationStartDate: now,
                observationEndDate: now,
                totalExposures: 0,
                completedExposures: 0,
                adherenceRate: 0m,
                overallStatus: AdaptationOverallStatus.ProgramWorking,
                coachNotes: "No recorded workouts found for this program version.");

            emptyAssessment.AddRecommendation(new AdaptationRecommendation(
                id: Guid.NewGuid(),
                adaptationAssessmentId: assessmentId,
                actionType: AdaptationActionType.NoChange,
                rationale: "No training data logged yet. Continue current program prescription.",
                confidence: RecommendationConfidence.Low));

            return emptyAssessment;
        }

        var sortedWorkouts = workouts.OrderBy(w => w.StartedAtUtc).ToList();
        var obsStart = sortedWorkouts.First().StartedAtUtc;
        var obsEnd = sortedWorkouts.Last().StartedAtUtc;

        // Flatten planned exercise slots across the program version
        var plannedSlots = programVersion.Weeks
            .SelectMany(w => w.Sessions)
            .SelectMany(s => s.Slots)
            .ToList();

        int totalExposures = workouts.Count;
        int completedExposures = workouts.Count(w => w.Status == WorkoutStatus.Completed);
        decimal adherenceRate = totalExposures > 0
            ? Math.Round((decimal)completedExposures / totalExposures * 100m, 1)
            : 0m;

        // Check for pain / discomfort mention across all logged sets & notes
        bool painMentioned = CheckForPainMention(sortedWorkouts, out var painDetails);

        var recommendations = new List<AdaptationRecommendation>();
        var exerciseRecords = new List<ExerciseAdaptationRecord>();

        // If pain is mentioned, M7 Hard Boundary: Flag to M8/M9, no physiological substitution
        if (painMentioned)
        {
            var painRec = new AdaptationRecommendation(
                id: Guid.NewGuid(),
                adaptationAssessmentId: assessmentId,
                actionType: AdaptationActionType.ReferToM8M9,
                rationale: $"Client notes contain potential pain/discomfort flag: '{painDetails}'. Per clinical safety protocol, refer to M8/M9 injury & symptom review before making physiological programming adjustments.",
                confidence: RecommendationConfidence.High);

            recommendations.Add(painRec);
        }

        // Adherence Tiers (Section 7)
        // Poor adherence (< 60%): Insufficient evidence, flag adherence review / CoachReview
        if (adherenceRate < 60m)
        {
            var adherenceAssessment = new AdaptationAssessment(
                id: assessmentId,
                programVersionId: programVersion.Id,
                assessedAt: now,
                observationStartDate: obsStart,
                observationEndDate: obsEnd,
                totalExposures: totalExposures,
                completedExposures: completedExposures,
                adherenceRate: adherenceRate,
                overallStatus: AdaptationOverallStatus.ReviewRecommended,
                coachNotes: "Overall adherence is below 60%. Performance data is insufficient to justify physiological program adaptations.");

            var exercisesBySlotTemp = sortedWorkouts
                .SelectMany(w => w.Exercises)
                .Where(e => e.ExerciseSlotId.HasValue)
                .GroupBy(e => e.ExerciseSlotId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var slot in plannedSlots)
            {
                exercisesBySlotTemp.TryGetValue(slot.Id, out var loggedExList);
                int expCount = loggedExList?.Count ?? 0;
                decimal exAdh = totalExposures > 0 ? Math.Round((decimal)expCount / totalExposures * 100m, 1) : 0m;
                var rec = new ExerciseAdaptationRecord(
                    id: Guid.NewGuid(),
                    adaptationAssessmentId: assessmentId,
                    exerciseSlotId: slot.Id,
                    exerciseId: slot.ExerciseId,
                    exposureCount: expCount,
                    progressionMetCount: 0,
                    effortAlignmentStatus: EffortAlignmentStatus.InsufficientEvidence,
                    performanceTrend: PerformanceTrend.Insufficient,
                    plateauConfirmed: false,
                    adherenceToExercise: exAdh);
                adherenceAssessment.AddExerciseRecord(rec);
            }

            if (!painMentioned)
            {
                adherenceAssessment.AddRecommendation(new AdaptationRecommendation(
                    id: Guid.NewGuid(),
                    adaptationAssessmentId: assessmentId,
                    actionType: AdaptationActionType.CoachReview,
                    rationale: $"Adherence rate is {adherenceRate:F1}% (<60%). Missed training volume cannot be interpreted as physiological non-response. Coach review of client schedule and lifestyle constraints recommended.",
                    confidence: RecommendationConfidence.Moderate));
            }
            else
            {
                foreach (var r in recommendations)
                    adherenceAssessment.AddRecommendation(r);
            }

            return adherenceAssessment;
        }

        bool hasModerateAdherence = adherenceRate < 80m; // 60-79%

        // Group actual workout exercises by planned slot
        var exercisesBySlot = sortedWorkouts
            .SelectMany(w => w.Exercises)
            .Where(e => e.ExerciseSlotId.HasValue)
            .GroupBy(e => e.ExerciseSlotId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        int slotsWithVolumeReduced = 0;
        int slotsWithVolumeIncreased = 0;
        var exerciseMap = allExercises.ToDictionary(e => e.Id, e => e.Name);

        foreach (var slot in plannedSlots)
        {
            var exerciseName = slot.Exercise?.Name ?? (exerciseMap.TryGetValue(slot.ExerciseId, out var name) ? name : "Exercise");
            exercisesBySlot.TryGetValue(slot.Id, out var loggedExList);
            loggedExList ??= new List<WorkoutExercise>();

            int exposures = loggedExList.Count;
            var recordId = Guid.NewGuid();

            // Analyze progression across exposures
            int progressionMetCount = 0;
            var progressionResults = new List<ProgressionEvaluationStatus>();
            var effortAlignments = new List<EffortAlignmentStatus>();
            var avgRirDiffs = new List<decimal>();
            var setCompletionCounts = new List<int>();

            var (targetMinRir, targetMaxRir) = ParseRirGuideline(slot.EffortGuideline);

            foreach (var exLog in loggedExList)
            {
                var prog = _progressionEvaluator.Evaluate(slot, exLog.Sets);
                progressionResults.Add(prog.Status);
                if (prog.Status == ProgressionEvaluationStatus.Met)
                    progressionMetCount++;

                var validSets = exLog.Sets.Where(s => s.IsCompleted).ToList();
                setCompletionCounts.Add(validSets.Count);

                var rirSets = validSets.Where(s => s.Rir.HasValue).Select(s => s.Rir!.Value).ToList();
                if (rirSets.Count > 0)
                {
                    decimal avgRir = rirSets.Average();
                    decimal diffFromMax = avgRir - targetMaxRir;
                    avgRirDiffs.Add(diffFromMax);

                    if (avgRir > targetMaxRir + 1.0m)
                        effortAlignments.Add(EffortAlignmentStatus.HigherThanTarget);
                    else if (avgRir < targetMinRir - 1.0m)
                        effortAlignments.Add(EffortAlignmentStatus.LowerThanTarget);
                    else
                        effortAlignments.Add(EffortAlignmentStatus.Aligned);
                }
                else
                {
                    effortAlignments.Add(EffortAlignmentStatus.InsufficientEvidence);
                }
            }

            // Overall Effort Alignment
            EffortAlignmentStatus overallEffort = EffortAlignmentStatus.InsufficientEvidence;
            if (effortAlignments.Count > 0 && effortAlignments.Count(e => e != EffortAlignmentStatus.InsufficientEvidence) >= 2)
            {
                var validEfforts = effortAlignments.Where(e => e != EffortAlignmentStatus.InsufficientEvidence).ToList();
                overallEffort = validEfforts.GroupBy(e => e).OrderByDescending(g => g.Count()).First().Key;
            }

            // Performance Trend across exposures (Section 10: requires >= 4 completed exposures)
            int slotCompletedExposures = loggedExList.Count(e => e.Sets.Any(s => s.IsCompleted));
            PerformanceTrend trend = DeterminePerformanceTrend(loggedExList, slotCompletedExposures);

            // Plateau evaluation (Section 8)
            // Requirements:
            // 1. >= 4 completed exposures
            // 2. adherence to exercise >= 75%
            // 3. progression NotMet in >= 3 of the exposures
            // 4. effort confirmed: ActualRIR <= TargetRIR + 1 in >= 3 exposures
            decimal exerciseAdherence = totalExposures > 0
                ? Math.Round((decimal)exposures / totalExposures * 100m, 1)
                : 0m;

            bool plateauConfirmed = false;
            bool effortIssue = false;

            if (slotCompletedExposures >= 4 && exerciseAdherence >= 75m)
            {
                int notMetCount = progressionResults.Count(p => p == ProgressionEvaluationStatus.NotMet || p == ProgressionEvaluationStatus.Incomplete);
                int effortConfirmedCount = avgRirDiffs.Count(d => d <= 1.0m);
                int effortTooEasyCount = avgRirDiffs.Count(d => d > 1.0m);

                if (notMetCount >= 3 && effortConfirmedCount >= 3)
                {
                    plateauConfirmed = true;
                }
                else if (notMetCount >= 3 && effortTooEasyCount >= 2)
                {
                    effortIssue = true;
                }
            }

            var record = new ExerciseAdaptationRecord(
                id: recordId,
                adaptationAssessmentId: assessmentId,
                exerciseSlotId: slot.Id,
                exerciseId: slot.ExerciseId,
                exposureCount: exposures,
                progressionMetCount: progressionMetCount,
                effortAlignmentStatus: overallEffort,
                performanceTrend: trend,
                plateauConfirmed: plateauConfirmed,
                adherenceToExercise: exerciseAdherence);

            exerciseRecords.Add(record);

            // RECOMMENDATIONS GENERATION (Recommendation-only, Coach-controlled)

            // Bad Session Rules (Section 6)
            int consecutiveIncomplete = GetConsecutiveIncompleteExposures(loggedExList);

            if (consecutiveIncomplete == 2)
            {
                recommendations.Add(new AdaptationRecommendation(
                    id: Guid.NewGuid(),
                    adaptationAssessmentId: assessmentId,
                    actionType: AdaptationActionType.CoachReview,
                    rationale: $"Exercise '{exerciseName}' has 2 consecutive under-executed sessions. Awareness flag raised for coach review; no program modification generated.",
                    confidence: RecommendationConfidence.Low,
                    exerciseAdaptationRecordId: recordId,
                    targetSlotId: slot.Id));
            }
            else if (consecutiveIncomplete >= 3)
            {
                recommendations.Add(new AdaptationRecommendation(
                    id: Guid.NewGuid(),
                    adaptationAssessmentId: assessmentId,
                    actionType: AdaptationActionType.CoachReview,
                    rationale: $"Exercise '{exerciseName}' has {consecutiveIncomplete} consecutive incomplete exposures. Coach review recommended to assess execution barriers.",
                    confidence: RecommendationConfidence.Moderate,
                    exerciseAdaptationRecordId: recordId,
                    targetSlotId: slot.Id));
            }

            // Plateau Recommendation (Section 13)
            if (plateauConfirmed && !hasModerateAdherence)
            {
                // Recommend Exercise Substitution using M5 ExerciseSelector
                var candidateSubstitute = FindSubstituteExercise(
                    slot,
                    allExercises,
                    profile,
                    exerciseSelector,
                    constraintAnalyzer);

                string substituteDetail = candidateSubstitute != null
                    ? $"SubstituteExerciseId:{candidateSubstitute.Exercise.Id};Name:{candidateSubstitute.Exercise.Name}"
                    : "NoSubstituteFound";

                string rationale = candidateSubstitute != null
                    ? $"Exercise '{exerciseName}' confirmed in plateau across {exposures} exposures with verified effort. Recommended replacement: '{candidateSubstitute.Exercise.Name}' ({candidateSubstitute.Rationale}). Requires coach approval."
                    : $"Exercise '{exerciseName}' confirmed in plateau across {exposures} exposures with verified effort. Coach review recommended to select variation.";

                recommendations.Add(new AdaptationRecommendation(
                    id: Guid.NewGuid(),
                    adaptationAssessmentId: assessmentId,
                    actionType: AdaptationActionType.ChangeExercise,
                    rationale: rationale,
                    confidence: exposures >= 6 ? RecommendationConfidence.High : RecommendationConfidence.Moderate,
                    exerciseAdaptationRecordId: recordId,
                    targetSlotId: slot.Id,
                    suggestedChangeDetail: substituteDetail));
            }
            else if (effortIssue)
            {
                recommendations.Add(new AdaptationRecommendation(
                    id: Guid.NewGuid(),
                    adaptationAssessmentId: assessmentId,
                    actionType: AdaptationActionType.ModifyEffortGuideline,
                    rationale: $"Exercise '{exerciseName}' did not progress across {exposures} exposures, but recorded RIR was consistently higher than planned target ({slot.EffortGuideline}). This indicates an effort proximity issue rather than physiological plateau. Recommend reinforcing target proximity to failure.",
                    confidence: RecommendationConfidence.Moderate,
                    exerciseAdaptationRecordId: recordId,
                    targetSlotId: slot.Id));
            }

            // Volume Adaptation (Section 12: max 2 slots changed per assessment cycle)
            // Case A — Consistent under-execution: planned sets > actual sets in >= 3 sessions
            int underExecutedSessions = setCompletionCounts.Count(c => c < slot.TargetSets);
            if (underExecutedSessions >= 3 && slotsWithVolumeReduced + slotsWithVolumeIncreased < 2 && slot.TargetSets > 1)
            {
                slotsWithVolumeReduced++;
                int newSets = slot.TargetSets - 1;
                recommendations.Add(new AdaptationRecommendation(
                    id: Guid.NewGuid(),
                    adaptationAssessmentId: assessmentId,
                    actionType: AdaptationActionType.ModifySets,
                    rationale: $"Planned volume ({slot.TargetSets} sets) consistently exceeds execution capacity across {underExecutedSessions} sessions for '{exerciseName}'. Recommended adjustment: -1 set ({newSets} sets total) to match recoverable tolerance.",
                    confidence: RecommendationConfidence.Moderate,
                    exerciseAdaptationRecordId: recordId,
                    targetSlotId: slot.Id,
                    suggestedChangeDetail: $"TargetSets:{newSets}"));
            }
            // Case B — Demonstrated Capacity: completed all sets, improving trend, session room constraint
            else if (slotCompletedExposures >= 4 && setCompletionCounts.All(c => c >= slot.TargetSets) &&
                     trend == PerformanceTrend.Improving &&
                     slotsWithVolumeReduced + slotsWithVolumeIncreased < 2 &&
                     slot.TargetSets < 5)
            {
                // Check session duration capacity: additional set work (~45s) + rest interval
                int addedTimeMinutes = Math.Max(1, (45 + slot.RestSeconds) / 60);
                int currentSessionDuration = slot.TrainingSession != null && slot.TrainingSession.EstimatedDurationMinutes > 0
                    ? slot.TrainingSession.EstimatedDurationMinutes
                    : 60;
                int durationCap = profile?.SessionDurationMaxMinutes
                    ?? profile?.SessionDurationTargetMinutes
                    ?? currentSessionDuration;

                bool hasRoom = (currentSessionDuration + addedTimeMinutes) <= durationCap;

                if (hasRoom)
                {
                    slotsWithVolumeIncreased++;
                    int newSets = slot.TargetSets + 1;
                    recommendations.Add(new AdaptationRecommendation(
                        id: Guid.NewGuid(),
                        adaptationAssessmentId: assessmentId,
                        actionType: AdaptationActionType.ModifySets,
                        rationale: $"Observed performance and execution suggest the current prescription ({slot.TargetSets} sets) for '{exerciseName}' is tolerated with positive adaptation. An additional set ({newSets} sets total) may be considered. Requires coach approval.",
                        confidence: slotCompletedExposures >= 6 ? RecommendationConfidence.High : RecommendationConfidence.Moderate,
                        exerciseAdaptationRecordId: recordId,
                        targetSlotId: slot.Id,
                        suggestedChangeDetail: $"TargetSets:{newSets}"));
                }
            }
        }

        // If no specific recommendations were triggered, default to NoChange (Product principle)
        if (recommendations.Count == 0)
        {
            recommendations.Add(new AdaptationRecommendation(
                id: Guid.NewGuid(),
                adaptationAssessmentId: assessmentId,
                actionType: AdaptationActionType.NoChange,
                rationale: "All monitored exercises demonstrate adequate progression or stable overload tolerance. No evidence justifies modifying the program. Continue current plan.",
                confidence: RecommendationConfidence.Moderate));
        }

        // Determine Overall Assessment Status
        AdaptationOverallStatus overallStatus = AdaptationOverallStatus.ProgramWorking;
        if (recommendations.Any(r => r.ActionType == AdaptationActionType.ReferToM8M9 || r.ActionType == AdaptationActionType.ChangeExercise || r.ActionType == AdaptationActionType.ModifySets))
        {
            overallStatus = AdaptationOverallStatus.ActionRequired;
        }
        else if (recommendations.Any(r => r.ActionType == AdaptationActionType.CoachReview || r.ActionType == AdaptationActionType.ModifyEffortGuideline))
        {
            overallStatus = AdaptationOverallStatus.ReviewRecommended;
        }

        var assessment = new AdaptationAssessment(
            id: assessmentId,
            programVersionId: programVersion.Id,
            assessedAt: now,
            observationStartDate: obsStart,
            observationEndDate: obsEnd,
            totalExposures: totalExposures,
            completedExposures: completedExposures,
            adherenceRate: adherenceRate,
            overallStatus: overallStatus);

        foreach (var rec in exerciseRecords)
            assessment.AddExerciseRecord(rec);

        foreach (var rec in recommendations)
            assessment.AddRecommendation(rec);

        return assessment;
    }

    private static bool CheckForPainMention(IEnumerable<WorkoutSession> workouts, out string details)
    {
        foreach (var w in workouts)
        {
            if (!string.IsNullOrWhiteSpace(w.Notes) && ContainsPainKeyword(w.Notes))
            {
                details = w.Notes.Trim();
                return true;
            }

            foreach (var ex in w.Exercises)
            {
                if (!string.IsNullOrWhiteSpace(ex.Notes) && ContainsPainKeyword(ex.Notes))
                {
                    details = ex.Notes.Trim();
                    return true;
                }

                foreach (var s in ex.Sets)
                {
                    if (!string.IsNullOrWhiteSpace(s.Notes) && ContainsPainKeyword(s.Notes))
                    {
                        details = s.Notes.Trim();
                        return true;
                    }
                }
            }
        }

        details = string.Empty;
        return false;
    }

    private static bool ContainsPainKeyword(string text)
    {
        var lower = text.ToLowerInvariant();
        return PainKeywords.Any(k => lower.Contains(k));
    }

    private static PerformanceTrend DeterminePerformanceTrend(IReadOnlyList<WorkoutExercise> exercises, int completedExposureCount)
    {
        if (completedExposureCount < 4)
            return PerformanceTrend.Insufficient;

        // Calculate average estimated volume or top load per exposure
        var topLoads = new List<decimal>();
        foreach (var ex in exercises)
        {
            var validSets = ex.Sets.Where(s => s.IsCompleted && s.LoadKg > 0).ToList();
            if (validSets.Count > 0)
                topLoads.Add(validSets.Max(s => s.LoadKg));
        }

        if (topLoads.Count < 4)
            return PerformanceTrend.Insufficient;

        int increases = 0;
        int decreases = 0;
        for (int i = 1; i < topLoads.Count; i++)
        {
            if (topLoads[i] > topLoads[i - 1]) increases++;
            else if (topLoads[i] < topLoads[i - 1]) decreases++;
        }

        if (increases > decreases && increases >= topLoads.Count / 2)
            return PerformanceTrend.Improving;
        if (decreases > increases && decreases >= topLoads.Count / 2)
            return PerformanceTrend.Declining;

        return PerformanceTrend.Stable;
    }

    private static int GetConsecutiveIncompleteExposures(IReadOnlyList<WorkoutExercise> exercises)
    {
        int count = 0;
        for (int i = exercises.Count - 1; i >= 0; i--)
        {
            var ex = exercises[i];
            if (ex.Sets.Count == 0 || ex.Sets.All(s => !s.IsCompleted))
                count++;
            else
                break;
        }
        return count;
    }

    private static CandidateExercise? FindSubstituteExercise(
        ExerciseSlot slot,
        IReadOnlyList<Exercise> allExercises,
        ClientTrainingProfile? profile,
        IExerciseSelector exerciseSelector,
        IConstraintAnalyzer constraintAnalyzer)
    {
        var availableEquipment = profile != null
            ? profile.AvailableEquipmentIds.ToHashSet()
            : new HashSet<Guid>();

        var excluded = profile?.ExerciseConstraints != null
            ? profile.ExerciseConstraints.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : new List<string>();

        var preferred = profile?.ExercisePreferences != null
            ? profile.ExercisePreferences.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : new List<string>();

        // Exclude current exercise from substitution pool
        var alreadySelected = new HashSet<Guid> { slot.ExerciseId };

        var currentExercise = slot.Exercise ?? allExercises.FirstOrDefault(e => e.Id == slot.ExerciseId);
        if (currentExercise == null)
            return null;

        return exerciseSelector.SelectBestExercise(
            movementPatternId: currentExercise.MovementPatternId,
            allExercises: allExercises,
            availableEquipmentIds: availableEquipment,
            excludedKeywords: excluded,
            preferredKeywords: preferred,
            alreadySelectedExerciseIds: alreadySelected,
            musclePriorities: new Dictionary<Guid, MusclePriorityLevel>(),
            recoveryCapacity: RecoveryCapacity.Moderate,
            constraintAnalyzer: constraintAnalyzer);
    }

    private static (decimal MinRir, decimal MaxRir) ParseRirGuideline(string effort)
    {
        var match = Regex.Match(effort, @"(\d+)\s*-\s*(\d+)");
        if (match.Success)
        {
            decimal min = decimal.Parse(match.Groups[1].Value);
            decimal max = decimal.Parse(match.Groups[2].Value);
            return (min, max);
        }

        var single = Regex.Match(effort, @"\d+");
        if (single.Success)
        {
            decimal val = decimal.Parse(single.Value);
            return (val, val);
        }

        return (1.0m, 2.0m);
    }
}
