export enum EvidenceLevel {
  Anecdotal = 1,
  Mechanistic = 2,
  ExpertConsensus = 3,
  RandomizedControlledTrial = 4,
  MetaAnalysis = 5
}

export const EvidenceLevelLabels: Record<EvidenceLevel, string> = {
  [EvidenceLevel.Anecdotal]: 'Anecdotal / Observation',
  [EvidenceLevel.Mechanistic]: 'Mechanistic / Biomechanical',
  [EvidenceLevel.ExpertConsensus]: 'Expert Consensus',
  [EvidenceLevel.RandomizedControlledTrial]: 'Randomized Controlled Trial (RCT)',
  [EvidenceLevel.MetaAnalysis]: 'Systematic Review / Meta-Analysis'
};

export enum ClaimStatus {
  Provisional = 1,
  Active = 2,
  Superseded = 3,
  Rejected = 4
}

export const ClaimStatusLabels: Record<ClaimStatus, string> = {
  [ClaimStatus.Provisional]: 'Provisional',
  [ClaimStatus.Active]: 'Active',
  [ClaimStatus.Superseded]: 'Superseded',
  [ClaimStatus.Rejected]: 'Rejected'
};

export enum KnowledgeSourceType {
  ScientificPaper = 1,
  PositionStand = 2,
  ExpertConsensus = 3,
  Book = 4,
  PractitionerNote = 5
}

export const KnowledgeSourceTypeLabels: Record<KnowledgeSourceType, string> = {
  [KnowledgeSourceType.ScientificPaper]: 'Scientific Paper',
  [KnowledgeSourceType.PositionStand]: 'Position Stand',
  [KnowledgeSourceType.ExpertConsensus]: 'Consensus Document',
  [KnowledgeSourceType.Book]: 'Academic Textbook',
  [KnowledgeSourceType.PractitionerNote]: 'Practitioner Note'
};

export interface KnowledgeSourceSummary {
  id: string;
  sourceType: KnowledgeSourceType;
  title: string;
  authors: string;
  year: number;
  evidenceLevel: EvidenceLevel;
  doi?: string;
}

export interface KnowledgeSource {
  id: string;
  sourceType: KnowledgeSourceType;
  title: string;
  authors: string;
  year: number;
  evidenceLevel: EvidenceLevel;
  doi?: string;
  url?: string;
  notes?: string;
  createdAtUtc: string;
}

export interface CreateKnowledgeSource {
  sourceType: KnowledgeSourceType;
  title: string;
  authors: string;
  year: number;
  evidenceLevel: EvidenceLevel;
  doi?: string;
  url?: string;
  notes?: string;
}

export interface KnowledgeClaimSource {
  sourceId: string;
  sourceType: KnowledgeSourceType;
  title: string;
  authors: string;
  year: number;
  evidenceLevel: EvidenceLevel;
  doi?: string;
  relevanceNote?: string;
}

export interface KnowledgeClaimSummary {
  id: string;
  topic: string;
  question: string;
  claimText: string;
  evidenceLevel: EvidenceLevel;
  status: ClaimStatus;
  exerciseId?: string;
  exerciseName?: string;
  supportingSourceCount: number;
  createdAtUtc: string;
}

export interface KnowledgeClaim {
  id: string;
  topic: string;
  question: string;
  claimText: string;
  evidenceLevel: EvidenceLevel;
  status: ClaimStatus;
  population?: string;
  limitations?: string;
  practicalApplication?: string;
  exerciseId?: string;
  exerciseName?: string;
  reviewedAtUtc?: string;
  reviewedBy?: string;
  supersededByClaimId?: string;
  supersededAtUtc?: string;
  supersessionReason?: string;
  sources: KnowledgeClaimSource[];
  createdAtUtc: string;
}

export interface CreateKnowledgeClaim {
  topic: string;
  question: string;
  claimText: string;
  evidenceLevel: EvidenceLevel;
  status?: ClaimStatus;
  exerciseId?: string;
  population?: string;
  limitations?: string;
  practicalApplication?: string;
  initialSourceIds?: string[];
}

export interface AddClaimSource {
  sourceId: string;
  relevanceNote?: string;
}

export interface SupersedeClaim {
  replacementClaimId: string;
  reason: string;
}

export interface KnowledgeFilter {
  search?: string;
  topic?: string;
  exerciseId?: string;
  status?: ClaimStatus;
  minEvidenceLevel?: EvidenceLevel;
}
