export enum LimitationSource {
  ReportedByClient = 1,
  CoachObserved = 2,
  PostReferral = 3
}

export enum LimitationStatus {
  Active = 1,
  Resolved = 2,
  OnHold = 3
}

export enum ConsiderationType {
  ReduceLoad = 1,
  ReduceROM = 2,
  ReduceProximityToFailure = 3,
  ReduceSets = 4,
  ReduceFrequencyOnRegion = 5,
  IncreaseRest = 6,
  ModifyTempo = 7,
  TemporaryExerciseExclusion = 8,
  AlternativeExercise = 9,
  TechniqueSetupModification = 10,
  GradedLoadingConsideration = 11
}

export enum ConsiderationStatus {
  Pending = 1,
  ApprovedByCoach = 2,
  RejectedByCoach = 3,
  Applied = 4
}

export interface RehabAwarenessConsiderationDto {
  id: string;
  trainingLimitationId: string;
  exerciseId?: string;
  exerciseName?: string;
  considerationType: ConsiderationType;
  considerationText: string;
  knowledgeClaimId?: string;
  evidenceBasis?: string;
  disclaimer: string;
  status: ConsiderationStatus;
  generatedAtUtc: string;
  coachDecisionAtUtc?: string;
  coachDecisionNote?: string;
}

export interface TrainingLimitationDto {
  id: string;
  clientId: string;
  safetyScreeningId?: string;
  affectedBodyRegion: string;
  limitationSource: LimitationSource;
  status: LimitationStatus;
  description?: string;
  reportedAtUtc: string;
  coachActivatedM9AtUtc?: string;
  coachActivationNote?: string;
  resolvedAtUtc?: string;
  considerations: RehabAwarenessConsiderationDto[];
}

export interface CreateTrainingLimitationRequestDto {
  clientId: string;
  affectedBodyRegion: string;
  limitationSource: LimitationSource;
  safetyScreeningId?: string;
  description?: string;
}

export interface ActivateLimitationRequestDto {
  coachNote: string;
}

export interface UpdateLimitationStatusRequestDto {
  status: LimitationStatus;
}

export interface RecordConsiderationDecisionRequestDto {
  decision: ConsiderationStatus;
  note?: string;
}

export interface GenerateConsiderationsRequestDto {
  exerciseId?: string;
}
