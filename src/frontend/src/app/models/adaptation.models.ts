export enum AdaptationOverallStatus {
  Stable = 1,
  ReviewRecommended = 2,
  CautionNeedsAttention = 3
}

export const AdaptationOverallStatusLabels: Record<AdaptationOverallStatus, string> = {
  [AdaptationOverallStatus.Stable]: 'Stable — Progressing Normally',
  [AdaptationOverallStatus.ReviewRecommended]: 'Review Recommended',
  [AdaptationOverallStatus.CautionNeedsAttention]: 'Caution — Needs Attention'
};

export enum EffortAlignmentStatus {
  UnderShooting = 1,
  Aligned = 2,
  OverShooting = 3,
  Inconsistent = 4
}

export const EffortAlignmentStatusLabels: Record<EffortAlignmentStatus, string> = {
  [EffortAlignmentStatus.UnderShooting]: 'Undershooting Target Effort',
  [EffortAlignmentStatus.Aligned]: 'Aligned With Effort Targets',
  [EffortAlignmentStatus.OverShooting]: 'Overshooting Target Effort',
  [EffortAlignmentStatus.Inconsistent]: 'Inconsistent Effort Reporting'
};

export enum PerformanceTrend {
  Progressing = 1,
  Plateaued = 2,
  Regressing = 3,
  InsufficientData = 4
}

export const PerformanceTrendLabels: Record<PerformanceTrend, string> = {
  [PerformanceTrend.Progressing]: 'Progressing',
  [PerformanceTrend.Plateaued]: 'Plateaued',
  [PerformanceTrend.Regressing]: 'Regressing',
  [PerformanceTrend.InsufficientData]: 'Insufficient Data'
};

export enum AdaptationActionType {
  NoChange = 1,
  ChangeExercise = 2,
  ModifySets = 3,
  ModifyEffortGuideline = 4,
  ReferToM8M9 = 5
}

export const AdaptationActionTypeLabels: Record<AdaptationActionType, string> = {
  [AdaptationActionType.NoChange]: 'No Change / Continue',
  [AdaptationActionType.ChangeExercise]: 'Substitute Exercise',
  [AdaptationActionType.ModifySets]: 'Adjust Target Sets',
  [AdaptationActionType.ModifyEffortGuideline]: 'Adjust Effort Target (RIR)',
  [AdaptationActionType.ReferToM8M9]: 'Flag for Pain / Clinical Review'
};

export enum RecommendationConfidence {
  Low = 1,
  Medium = 2,
  High = 3
}

export const RecommendationConfidenceLabels: Record<RecommendationConfidence, string> = {
  [RecommendationConfidence.Low]: 'Low',
  [RecommendationConfidence.Medium]: 'Medium',
  [RecommendationConfidence.High]: 'High'
};

export enum RecommendationStatus {
  PendingReview = 1,
  Approved = 2,
  Rejected = 3,
  Applied = 4
}

export const RecommendationStatusLabels: Record<RecommendationStatus, string> = {
  [RecommendationStatus.PendingReview]: 'Pending Coach Review',
  [RecommendationStatus.Approved]: 'Approved',
  [RecommendationStatus.Rejected]: 'Rejected',
  [RecommendationStatus.Applied]: 'Applied to Program'
};

export interface ExerciseAdaptationRecord {
  id: string;
  exerciseId: string;
  exerciseName?: string;
  totalExposures: number;
  progressionMetCount: number;
  progressionNotMetCount: number;
  effortAlignment: EffortAlignmentStatus;
  trend: PerformanceTrend;
  summaryRationale: string;
}

export interface AdaptationRecommendation {
  id: string;
  exerciseAdaptationRecordId?: string;
  targetSlotId?: string;
  exerciseName?: string;
  actionType: AdaptationActionType;
  suggestedChangeDetail?: string;
  rationale: string;
  confidence: RecommendationConfidence;
  status: RecommendationStatus;
  coachDecisionAt?: string;
  coachDecisionNote?: string;
}

export interface AdaptationAssessment {
  id: string;
  programVersionId: string;
  programName?: string;
  clientName?: string;
  overallStatus: AdaptationOverallStatus;
  totalExposures: number;
  completedExposures: number;
  adherenceRate: number;
  assessedAtUtc: string;
  narrativeSummary: string;
  exerciseRecords: ExerciseAdaptationRecord[];
  recommendations: AdaptationRecommendation[];
}

export interface CoachRecommendationDecision {
  approve: boolean;
  coachDecisionNote?: string;
}
