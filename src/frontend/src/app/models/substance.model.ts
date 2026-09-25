export enum SubstanceCategory {
  Supplement = 1,
  Hormone = 2,
  PED = 3
}

export enum SupplementCategory {
  Performance = 1,
  HealthAndWellness = 2,
  Recovery = 3,
  BodyComposition = 4
}

export enum HormoneAxis {
  HPTA = 1,
  Thyroid = 2,
  Adrenal = 3,
  GrowthHormone = 4,
  InsulinGlucose = 5
}

export enum PEDCategory {
  AAS = 1,
  SARM = 2,
  Peptide = 3,
  GrowthHormoneSecretagogue = 4,
  Stimulant = 5,
  Diuretic = 6,
  SERM = 7,
  AromataseInhibitor = 8,
  Other = 9
}

export enum OrganSystem {
  Cardiovascular = 1,
  Hepatic = 2,
  Renal = 3,
  Endocrine = 4,
  Psychiatric = 5,
  Dermatological = 6,
  Hematological = 7,
  Musculoskeletal = 8,
  Other = 9
}

export enum PEDRiskSeverity {
  Low = 1,
  Moderate = 2,
  High = 3,
  Critical = 4
}

export enum SubstanceEscalationLevel {
  None = 0,
  CautionCoachReview = 1,
  UrgentMedicalReferral = 2,
  EmergencyMedicalAttention = 3
}

export interface SubstanceSafetyFlag {
  flagType: string;
  severity: SubstanceEscalationLevel;
  message: string;
  evidenceBasis: string;
}

export interface EvidenceCitation {
  claimId: string;
  topic: string;
  claimText: string;
  evidenceLevel: number;
  status: number;
  sourceCitations: string[];
}

export interface SupplementKnowledgeSummary {
  id: string;
  name: string;
  supplementCategory: SupplementCategory;
  evidenceLevel: number;
  description: string;
  isEgyptianMarketAvailable: boolean;
  safetyFlagCount: number;
  lastReviewedAtUtc?: string;
}

export interface SupplementKnowledge {
  id: string;
  name: string;
  category: SubstanceCategory;
  supplementCategory: SupplementCategory;
  evidenceLevel: number;
  description: string;
  evidenceSummary: string;
  uncertaintyStatement: string;
  commonForms?: string;
  typicalDoseRange?: string;
  timingRecommendation?: string;
  interactionsAndNotes?: string;
  isEgyptianMarketAvailable: boolean;
  lastReviewedAtUtc?: string;
  reviewedBy?: string;
  isActive: boolean;
  safetyFlags: SubstanceSafetyFlag[];
  evidenceClaim?: EvidenceCitation;
}

export interface HormoneKnowledgeSummary {
  id: string;
  name: string;
  hormoneAxis: HormoneAxis;
  description: string;
  lastReviewedAtUtc?: string;
}

export interface HormoneKnowledge {
  id: string;
  name: string;
  category: SubstanceCategory;
  hormoneAxis: HormoneAxis;
  description: string;
  physiologicalRole: string;
  trainingImpactSummary: string;
  evidenceSummary: string;
  uncertaintyStatement: string;
  biomarkerReferenceNotes?: string;
  lastReviewedAtUtc?: string;
  reviewedBy?: string;
  isActive: boolean;
  safetyFlags: SubstanceSafetyFlag[];
  evidenceClaim?: EvidenceCitation;
}

export interface PEDRiskRecord {
  id: string;
  organSystem: OrganSystem;
  severity: PEDRiskSeverity;
  riskDescription: string;
  reversibilityNotes?: string;
  knowledgeClaimId?: string;
}

export interface PEDSafetyRecordSummary {
  id: string;
  name: string;
  pedCategory: PEDCategory;
  description: string;
  riskCount: number;
  lastReviewedAtUtc?: string;
}

export interface PEDSafetyRecord {
  id: string;
  name: string;
  category: SubstanceCategory;
  pedCategory: PEDCategory;
  description: string;
  mechanismSummary: string;
  healthRisksSummary: string;
  evidenceSummary: string;
  safetyDisclaimer: string;
  lastReviewedAtUtc?: string;
  reviewedBy?: string;
  isActive: boolean;
  risks: PEDRiskRecord[];
  safetyFlags: SubstanceSafetyFlag[];
  evidenceClaim?: EvidenceCitation;
}

export interface PEDRedFlagRule {
  id: string;
  name: string;
  description: string;
  signalPattern: string;
  escalationLevel: SubstanceEscalationLevel;
  recommendedAction: string;
  evidenceBasis: string;
  isActive: boolean;
}

export interface EvaluateSubstanceSafetyRequest {
  substanceRecordId?: string;
  reportedSignals: string[];
  contextNotes?: string;
}

export interface SubstanceSafetyEvaluationResult {
  escalationRecordId?: string;
  escalationLevel: SubstanceEscalationLevel;
  summaryRationale: string;
  recommendedAction: string;
  disclaimer: string;
  matchedRedFlags: string[];
  reportedSignals: string[];
  createdAtUtc: string;
}

export interface SubstanceEscalationRecord {
  id: string;
  coachId: string;
  substanceRecordId?: string;
  substanceName?: string;
  escalationLevel: SubstanceEscalationLevel;
  summaryRationale: string;
  recommendedAction: string;
  disclaimer: string;
  createdAtUtc: string;
  reportedSignals: string[];
  matchedRedFlags: string[];
}
