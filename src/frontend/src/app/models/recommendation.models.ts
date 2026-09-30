export enum AIRecommendationCategory {
  ProgramAdaptationReview = 'ProgramAdaptationReview',
  NutritionAdjustmentReview = 'NutritionAdjustmentReview',
  ExerciseModificationReview = 'ExerciseModificationReview',
  SafetyContextSummary = 'SafetyContextSummary',
  GeneralCoachingNote = 'GeneralCoachingNote',
  ExerciseTechniqueObservation = 'ExerciseTechniqueObservation'
}

export const AIRecommendationCategoryLabels: Record<string, string> = {
  'ProgramAdaptationReview': 'Program Adaptation',
  'NutritionAdjustmentReview': 'Nutrition Adjustment',
  'ExerciseModificationReview': 'Exercise Modification',
  'SafetyContextSummary': 'Safety Summary',
  'GeneralCoachingNote': 'General Coaching Note',
  'ExerciseTechniqueObservation': 'Technique Observation',
  '1': 'Program Adaptation',
  '2': 'Nutrition Adjustment',
  '3': 'Exercise Modification',
  '4': 'Safety Summary',
  '5': 'General Coaching Note',
  '6': 'Technique Observation'
};

export enum AIRecommendationReviewStatus {
  PendingReview = 'PendingReview',
  UnderReview = 'UnderReview',
  Accepted = 'Accepted',
  Rejected = 'Rejected',
  Archived = 'Archived'
}

export const AIRecommendationReviewStatusLabels: Record<string, string> = {
  'PendingReview': 'Pending Review',
  'UnderReview': 'Under Review',
  'Accepted': 'Accepted',
  'Rejected': 'Rejected',
  'Archived': 'Archived',
  '1': 'Pending Review',
  '2': 'Under Review',
  '3': 'Accepted',
  '4': 'Rejected',
  '5': 'Archived'
};

export interface ResolvedKnowledgeClaim {
  id: string;
  topic: string;
  claimText: string;
  evidenceLevel: string;
  status: string;
}

export interface AIRecommendationSummary {
  id: string;
  clientId: string;
  coachId: string;
  category: AIRecommendationCategory | string;
  summary: string;
  generatedAt: string;
  status: AIRecommendationReviewStatus | string;
  coachActionRequired: boolean;
  safetySummary?: string | null;
  knowledgeClaimCount: number;
}

export interface AIRecommendationDetail {
  id: string;
  clientId: string;
  coachId: string;
  recommendationCategory: AIRecommendationCategory | string;
  summary: string;
  observations: string[];
  recommendations: string[];
  rationale: string;
  confidenceStatement: string;
  assumptions: string[];
  missingHighValueData: string[];
  safetySummary?: string | null;
  coachActionRequired: boolean;
  resolvedKnowledgeClaims: ResolvedKnowledgeClaim[];
  aiProvider: string;
  aiModel: string;
  generatedAt: string;
  reviewStatus: AIRecommendationReviewStatus | string;
  coachDecision?: string | null;
  coachDecisionAt?: string | null;
  finalImplementedPlan?: string | null;
  implementedProgramVersionId?: string | null;
  implementedProgramVersionLabel?: string | null;
}

export interface ReviewAIRecommendationRequest {
  reviewStatus: AIRecommendationReviewStatus | string;
  coachDecision?: string | null;
  finalImplementedPlan?: string | null;
}

export interface LinkProgramVersionRequest {
  programVersionId: string;
}

export interface GenerateReasoningRequest {
  clientId: string;
  reasoningCategory: string;
  additionalContext?: string | null;
}
