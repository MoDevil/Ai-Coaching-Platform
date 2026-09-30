export enum ExpertSourceType {
  YouTubeChannel = 1,
  Podcast = 2,
  Blog = 3,
  ResearchGroup = 4,
  Other = 5
}

export const ExpertSourceTypeLabels: Record<ExpertSourceType, string> = {
  [ExpertSourceType.YouTubeChannel]: 'YouTube Channel',
  [ExpertSourceType.Podcast]: 'Podcast',
  [ExpertSourceType.Blog]: 'Blog',
  [ExpertSourceType.ResearchGroup]: 'Research Group',
  [ExpertSourceType.Other]: 'Other'
};

export enum IngestionSourceType {
  YouTubeVideo = 1,
  Article = 2,
  PodcastEpisode = 3,
  Other = 4
}

export const IngestionSourceTypeLabels: Record<IngestionSourceType, string> = {
  [IngestionSourceType.YouTubeVideo]: 'YouTube Video',
  [IngestionSourceType.Article]: 'Web Article / Blog',
  [IngestionSourceType.PodcastEpisode]: 'Podcast Episode',
  [IngestionSourceType.Other]: 'Other'
};

export enum IngestionStatus {
  Processing = 1,
  PendingReview = 2,
  PartiallyApproved = 3,
  Completed = 4,
  Failed = 5
}

export const IngestionStatusLabels: Record<IngestionStatus, string> = {
  [IngestionStatus.Processing]: 'Processing',
  [IngestionStatus.PendingReview]: 'Pending Review',
  [IngestionStatus.PartiallyApproved]: 'Partially Approved',
  [IngestionStatus.Completed]: 'Completed',
  [IngestionStatus.Failed]: 'Failed'
};

export enum ClaimCategory {
  TrainingVolume = 1,
  Frequency = 2,
  Intensity = 3,
  Nutrition = 4,
  Recovery = 5,
  Supplementation = 6,
  Biomechanics = 7,
  General = 8
}

export const ClaimCategoryLabels: Record<ClaimCategory, string> = {
  [ClaimCategory.TrainingVolume]: 'Training Volume',
  [ClaimCategory.Frequency]: 'Frequency',
  [ClaimCategory.Intensity]: 'Intensity',
  [ClaimCategory.Nutrition]: 'Nutrition',
  [ClaimCategory.Recovery]: 'Recovery',
  [ClaimCategory.Supplementation]: 'Supplementation',
  [ClaimCategory.Biomechanics]: 'Biomechanics',
  [ClaimCategory.General]: 'General'
};

export enum EvidenceClassification {
  OpinionOnly = 1,
  InterpretationOfResearch = 2,
  CitesConcreteSources = 3,
  ContradictsCurrentEvidence = 4,
  AgreesWithCurrentEvidence = 5,
  Uncertain = 6
}

export const EvidenceClassificationLabels: Record<EvidenceClassification, string> = {
  [EvidenceClassification.OpinionOnly]: 'Opinion Only',
  [EvidenceClassification.InterpretationOfResearch]: 'Interpretation of Research',
  [EvidenceClassification.CitesConcreteSources]: 'Cites Concrete Sources',
  [EvidenceClassification.ContradictsCurrentEvidence]: 'Contradicts Current Evidence',
  [EvidenceClassification.AgreesWithCurrentEvidence]: 'Agrees with Current Evidence',
  [EvidenceClassification.Uncertain]: 'Uncertain'
};

export enum CoachReviewStatus {
  PendingReview = 1,
  Approved = 2,
  Rejected = 3,
  Deferred = 4
}

export const CoachReviewStatusLabels: Record<CoachReviewStatus, string> = {
  [CoachReviewStatus.PendingReview]: 'Pending Review',
  [CoachReviewStatus.Approved]: 'Approved',
  [CoachReviewStatus.Rejected]: 'Rejected',
  [CoachReviewStatus.Deferred]: 'Deferred'
};

export enum CreatorConfidence {
  Low = 1,
  Medium = 2,
  High = 3
}

export const CreatorConfidenceLabels: Record<CreatorConfidence, string> = {
  [CreatorConfidence.Low]: 'Low',
  [CreatorConfidence.Medium]: 'Medium',
  [CreatorConfidence.High]: 'High'
};

export interface ExpertSourceDto {
  id: string;
  name: string;
  sourceType: ExpertSourceType;
  url: string;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface CreateExpertSourceDto {
  name: string;
  sourceType: ExpertSourceType;
  url: string;
}

export interface SubmitIngestionRequestDto {
  sourceUrl: string;
  expertSourceId?: string | null;
  sourceTitle?: string | null;
  sourceType?: IngestionSourceType | null;
  publishedAt?: string | null;
}

export interface ExpertClaimDto {
  id: string;
  ingestionId: string;
  claimText: string;
  claimCategory: ClaimCategory;
  evidenceClassification: EvidenceClassification;
  creatorConfidence: CreatorConfidence;
  directQuote: boolean;
  sourceContext?: string | null;
  supportingClaimId?: string | null;
  supportingClaimText?: string | null;
  conflictingClaimId?: string | null;
  conflictingClaimText?: string | null;
  coachReviewStatus: CoachReviewStatus;
  coachReviewedAt?: string | null;
  coachNote?: string | null;
  approvedKnowledgeClaimId?: string | null;
  reviewedByCoachId?: string | null;
}

export interface ExpertContentIngestionSummaryDto {
  id: string;
  coachId: string;
  expertSourceId?: string | null;
  sourceName?: string | null;
  sourceUrl: string;
  sourceTitle: string;
  sourceType: IngestionSourceType;
  publishedAt?: string | null;
  extractedTextLength: number;
  wasTruncated: boolean;
  status: IngestionStatus;
  failureReason?: string | null;
  containsMedicalClaims: boolean;
  claimCount: number;
  submittedAtUtc: string;
  processedAtUtc?: string | null;
}

export interface ExpertContentIngestionDto {
  id: string;
  coachId: string;
  expertSourceId?: string | null;
  sourceName?: string | null;
  sourceUrl: string;
  sourceTitle: string;
  sourceType: IngestionSourceType;
  publishedAt?: string | null;
  extractedTextLength: number;
  wasTruncated: boolean;
  status: IngestionStatus;
  failureReason?: string | null;
  containsMedicalClaims: boolean;
  claims: ExpertClaimDto[];
  submittedAtUtc: string;
  processedAtUtc?: string | null;
}

export interface ReviewClaimRequestDto {
  decision: CoachReviewStatus;
  notes?: string | null;
  existingKnowledgeClaimIdToLink?: string | null;
  createNewKnowledgeClaim?: boolean;
  newClaimQuestion?: string | null;
  egyptSpecificNotes?: string | null;
  practitionerNotes?: string | null;
}
