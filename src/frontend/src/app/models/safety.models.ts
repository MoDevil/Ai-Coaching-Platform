export enum SafetyCategory {
  NoSafetyConcern = 1,
  CautionCoachReview = 2,
  ReferToHealthcareProfessional = 3,
  UrgentMedicalAttention = 4
}

export enum SignalType {
  Pain = 1,
  Discomfort = 2,
  Numbness = 3,
  Swelling = 4,
  Weakness = 5,
  Other = 6
}

export enum SignalOnset {
  Unknown = 0,
  Sudden = 1,
  Gradual = 2
}

export enum SignalTiming {
  Unknown = 0,
  DuringExercise = 1,
  AfterExercise = 2,
  AtRest = 3,
  Persistent = 4
}

export enum SignalSeverity {
  Unknown = 0,
  Mild = 1,
  Moderate = 2,
  Severe = 3
}

export enum SignalDuration {
  Unknown = 0,
  Acute = 1,
  Subacute = 2,
  Chronic = 3
}

export enum SafetyActionType {
  NoActionRequired = 1,
  ContinueWithCaution = 2,
  PauseActivityPendingAssessment = 3,
  CoachReviewRequired = 4,
  ReferToHealthcareProfessional = 5,
  UrgentMedicalAttention = 6
}

export enum TriggeredByType {
  WorkoutSession = 1,
  DirectReport = 2,
  AdaptationReview = 3
}

export interface ReportedSignalDto {
  bodyRegion: string;
  signalType: SignalType;
  onset?: SignalOnset;
  timing?: SignalTiming;
  severity?: SignalSeverity;
  worsening?: boolean;
  duration?: SignalDuration;
  associatedWithExerciseId?: string;
  freeText?: string;
}

export interface SafetyScreeningDto {
  id: string;
  clientId: string;
  triggeredByType: TriggeredByType;
  triggeredByEntityId?: string;
  screeningResult: SafetyCategory;
  recommendedAction: SafetyActionType;
  summaryRationale: string;
  disclaimer: string;
  generatedAtUtc: string;
  requiresCoachAcknowledgment: boolean;
  coachAcknowledgedAtUtc?: string;
  coachNote?: string;
  reportedSignals: ReportedSignalDto[];
  redFlagsMatched: string[];
}

export interface CreateSafetyReportRequestDto {
  clientId: string;
  triggeredByType: TriggeredByType;
  signals: ReportedSignalDto[];
  triggeredByEntityId?: string;
}

export interface AcknowledgeSafetyScreeningRequestDto {
  coachNote?: string;
}

export interface RedFlagRuleDto {
  id: string;
  name: string;
  description: string;
  signalPattern: string;
  safetyCategoryTriggered: SafetyCategory;
  recommendedAction: SafetyActionType;
  evidenceBasis: string;
  requiresClinicalReview: boolean;
  isActive: boolean;
  lastReviewedAtUtc?: string;
  reviewedBy?: string;
  knowledgeClaimId?: string;
}
