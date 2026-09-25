export enum SubstanceCategory {
  Supplement = 1,
  Hormone = 2,
  PED = 3
}

export enum SupplementEvidenceStatus {
  StrongEvidence = 1,
  ModerateEvidence = 2,
  Preliminary = 3,
  InsufficientEvidence = 4,
  Disproven = 5
}

export enum EffectMagnitude {
  None = 0,
  Small = 1,
  Moderate = 2,
  Large = 3,
  Unclear = 4
}

export enum SafetyFlagCategory {
  Contraindication = 1,
  AdverseInteraction = 2,
  HighDoseToxicity = 3,
  SpecialPopulationPrecaution = 4,
  OrganStressPrecaution = 5
}

export enum HormoneCategory {
  Androgen = 1,
  Glucocorticoid = 2,
  Thyroid = 3,
  PeptideGrowth = 4,
  MetabolicEnergy = 5,
  EstrogenProgestin = 6
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

export enum RiskCategory {
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

export enum EscalationLevel {
  None = 0,
  CoachAwareness = 1,
  HealthcareProfessionalReferral = 2,
  UrgentMedicalAttention = 3
}

export interface SubstanceSafetyFlag {
  category: SafetyFlagCategory;
  description: string;
  affectedPopulation?: string;
  sourceClaimId?: string;
  escalationLevel: EscalationLevel;
  coachNote: string;
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
  commonAliases: string[];
  primaryClaimedBenefit: string;
  evidenceStatus: SupplementEvidenceStatus;
  effectMagnitude: EffectMagnitude;
  description: string;
  isEgyptianMarketAvailable: boolean;
  isProvisional: boolean;
  requiresClinicalReview: boolean;
  safetyFlagCount: number;
  lastReviewedAtUtc?: string;
  reviewDueAtUtc?: string;
}

export interface SupplementKnowledge {
  id: string;
  name: string;
  commonAliases: string[];
  substanceCategory: SubstanceCategory;
  primaryClaimedBenefit: string;
  efficacyClaim?: string;
  evidenceStatus: SupplementEvidenceStatus;
  effectMagnitude: EffectMagnitude;
  populationNote?: string;
  description: string;
  uncertaintyStatement: string;
  typicalDoseRangeMin?: number;
  typicalDoseRangeMax?: number;
  doseUnit?: string;
  doseSourceClaimId?: string;
  timingNote?: string;
  commonForms?: string;
  interactionsAndNotes?: string;
  isEgyptianMarketAvailable: boolean;
  isProvisional: boolean;
  requiresClinicalReview: boolean;
  claimStatus: number;
  lastReviewedAtUtc?: string;
  reviewDueAtUtc?: string;
  reviewedBy?: string;
  isActive: boolean;
  safetyFlags: SubstanceSafetyFlag[];
  evidenceClaim?: EvidenceCitation;
}

export interface HormoneKnowledgeSummary {
  id: string;
  name: string;
  commonAliases: string[];
  hormoneCategory: HormoneCategory;
  physiologicalRole: string;
  description: string;
  isProvisional: boolean;
  requiresClinicalReview: boolean;
  lastReviewedAtUtc?: string;
  reviewDueAtUtc?: string;
}

export interface HormoneKnowledge {
  id: string;
  name: string;
  commonAliases: string[];
  substanceCategory: SubstanceCategory;
  hormoneCategory: HormoneCategory;
  description: string;
  physiologicalRole: string;
  trainingRelevance: string;
  uncertaintyStatement: string;
  biomarkerReferenceNotes?: string;
  evidenceClaimIds: string[];
  medicalEvaluationTriggers: string[];
  isProvisional: boolean;
  requiresClinicalReview: boolean;
  claimStatus: number;
  lastReviewedAtUtc?: string;
  reviewDueAtUtc?: string;
  reviewedBy?: string;
  isActive: boolean;
  safetyFlags: SubstanceSafetyFlag[];
  evidenceClaim?: EvidenceCitation;
}

export interface PEDRiskRecord {
  id: string;
  pedSafetyRecordId: string;
  riskCategory: RiskCategory;
  severity: PEDRiskSeverity;
  description: string;
  evidenceLevel: number;
  reversibilityNotes?: string;
  evidenceClaimId?: string;
}

export interface PEDSafetyRecordSummary {
  id: string;
  name: string;
  commonAliases: string[];
  pedCategory: PEDCategory;
  description: string;
  riskCount: number;
  isProvisional: boolean;
  requiresClinicalReview: boolean;
  lastReviewedAtUtc?: string;
  reviewDueAtUtc?: string;
}

export interface PEDSafetyRecord {
  id: string;
  name: string;
  commonAliases: string[];
  substanceCategory: SubstanceCategory;
  pedCategory: PEDCategory;
  description: string;
  mechanismSummary: string;
  safetyDisclaimer: string;
  monitoringConcepts: string[];
  isProvisional: boolean;
  requiresClinicalReview: boolean;
  claimStatus: number;
  lastReviewedAtUtc?: string;
  reviewDueAtUtc?: string;
  reviewedBy?: string;
  isActive: boolean;
  risks: PEDRiskRecord[];
  safetyFlags: SubstanceSafetyFlag[];
  evidenceClaim?: EvidenceCitation;
}

export interface PEDRedFlagRule {
  id: string;
  name: string;
  pedCategory?: PEDCategory;
  description: string;
  signalPattern: string;
  escalationLevel: EscalationLevel;
  requiresClinicalReview: boolean;
  recommendedAction: string;
  evidenceBasis: string;
  sourceClaimId?: string;
  isActive: boolean;
}

export interface EvaluateSubstanceSafetyRequest {
  substanceRecordId?: string;
  reportedSignals: string[];
  coachNote?: string;
}

export interface SubstanceSafetyEvaluationResult {
  escalationRecordId?: string;
  escalationLevel: EscalationLevel;
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
  escalationLevel: EscalationLevel;
  coachNote?: string;
  summaryRationale: string;
  recommendedAction: string;
  disclaimer: string;
  createdAtUtc: string;
  reportedSignals: string[];
  triggeredFlagIds: string[];
}
