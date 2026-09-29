export enum IngestionContentType {
  YouTube = 1,
  Article = 2,
  Podcast = 3
}

export const IngestionContentTypeLabels: Record<IngestionContentType, string> = {
  [IngestionContentType.YouTube]: 'YouTube Video',
  [IngestionContentType.Article]: 'Web Article / Transcript',
  [IngestionContentType.Podcast]: 'Podcast Transcript'
};

export enum IngestionStatus {
  Processing = 1,
  PendingReview = 2,
  PartiallyApproved = 3,
  FullyApproved = 4,
  Rejected = 5,
  Failed = 6
}

export const IngestionStatusLabels: Record<IngestionStatus, string> = {
  [IngestionStatus.Processing]: 'Processing',
  [IngestionStatus.PendingReview]: 'Pending Review',
  [IngestionStatus.PartiallyApproved]: 'Partially Approved',
  [IngestionStatus.FullyApproved]: 'Fully Approved',
  [IngestionStatus.Rejected]: 'Rejected',
  [IngestionStatus.Failed]: 'Failed'
};

export enum ClaimNature {
  OpinionOnly = 1,
  InterpretationOfResearch = 2,
  CitesConcreteSources = 3
}

export const ClaimNatureLabels: Record<ClaimNature, string> = {
  [ClaimNature.OpinionOnly]: 'Opinion Only',
  [ClaimNature.InterpretationOfResearch]: 'Interpretation of Research',
  [ClaimNature.CitesConcreteSources]: 'Cites Concrete Sources'
};

export enum ExpertClaimReviewStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  Deferred = 4
}

export const ExpertClaimReviewStatusLabels: Record<ExpertClaimReviewStatus, string> = {
  [ExpertClaimReviewStatus.Pending]: 'Pending',
  [ExpertClaimReviewStatus.Approved]: 'Approved',
  [ExpertClaimReviewStatus.Rejected]: 'Rejected',
  [ExpertClaimReviewStatus.Deferred]: 'Deferred'
};

export enum CredibilityTier {
  High = 1,
  Medium = 2,
  Low = 3,
  Practitioner = 4
}

export const CredibilityTierLabels: Record<CredibilityTier, string> = {
  [CredibilityTier.High]: 'High (Peer-Reviewed / Elite)',
  [CredibilityTier.Medium]: 'Medium (Established Coach)',
  [CredibilityTier.Low]: 'Low / Unverified',
  [CredibilityTier.Practitioner]: 'Practitioner / Field Expert'
};

export enum ExpertPlatform {
  YouTube = 1,
  Article = 2,
  Podcast = 3,
  Book = 4
}

export const ExpertPlatformLabels: Record<ExpertPlatform, string> = {
  [ExpertPlatform.YouTube]: 'YouTube',
  [ExpertPlatform.Article]: 'Web Article',
  [ExpertPlatform.Podcast]: 'Podcast',
  [ExpertPlatform.Book]: 'Book'
};

export interface ExpertSourceDto {
  id: string;
  name: string;
  channelOrPublication: string;
  platform: ExpertPlatform;
  primaryDomain: string;
  credibilityTier: CredibilityTier;
  bio?: string | null;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface CreateExpertSourceDto {
  name: string;
  channelOrPublication: string;
  platform: ExpertPlatform;
  primaryDomain: string;
  credibilityTier: CredibilityTier;
  bio?: string | null;
}

export interface SubmitIngestionRequestDto {
  sourceUrl: string;
  sourceId?: string | null;
  title?: string | null;
  contentType?: IngestionContentType | null;
}

export interface ExpertClaimDto {
  id: string;
  ingestionId: string;
  topic: string;
  subTopic?: string | null;
  claimText: string;
  contextOrTimestamp?: string | null;
  directQuote: boolean;
  natureOfClaim: ClaimNature;
  supportingClaimId?: string | null;
  supportingClaimText?: string | null;
  conflictingClaimId?: string | null;
  conflictingClaimText?: string | null;
  reviewStatus: ExpertClaimReviewStatus;
  coachNotes?: string | null;
  approvedKnowledgeClaimId?: string | null;
  reviewedAtUtc?: string | null;
  reviewedByCoachId?: string | null;
}

export interface ExpertContentIngestionSummaryDto {
  id: string;
  coachId: string;
  sourceId?: string | null;
  sourceName?: string | null;
  sourceUrl: string;
  contentType: IngestionContentType;
  title: string;
  wordCount: number;
  wasTruncated: boolean;
  status: IngestionStatus;
  failureReason?: string | null;
  containsMedicalClaims: boolean;
  medicalWarningAcknowledged: boolean;
  claimCount: number;
  submittedAtUtc: string;
  completedAtUtc?: string | null;
}

export interface ExpertContentIngestionDto {
  id: string;
  coachId: string;
  sourceId?: string | null;
  sourceName?: string | null;
  sourceUrl: string;
  contentType: IngestionContentType;
  title: string;
  rawExtractedTextSnippet?: string | null;
  wordCount: number;
  wasTruncated: boolean;
  status: IngestionStatus;
  failureReason?: string | null;
  containsMedicalClaims: boolean;
  medicalWarningAcknowledged: boolean;
  claims: ExpertClaimDto[];
  submittedAtUtc: string;
  completedAtUtc?: string | null;
}

export interface ReviewClaimRequestDto {
  decision: ExpertClaimReviewStatus;
  notes?: string | null;
  existingKnowledgeClaimIdToLink?: string | null;
  createNewKnowledgeClaim?: boolean;
  newClaimQuestion?: string | null;
  egyptSpecificNotes?: string | null;
  practitionerNotes?: string | null;
}
