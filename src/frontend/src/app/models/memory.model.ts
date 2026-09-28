export enum MemoryCategory {
  Preference = 1,
  Aversion = 2,
  PainObservation = 3,
  LifeEvent = 4,
  GoalContext = 5,
  EquipmentConstraint = 6,
  ScheduleConstraint = 7,
  NutritionHabit = 8,
  AdherenceNote = 9,
  RecoveryNote = 10,
  GeneralNote = 11,
  UnresolvedQuestion = 12
}

export enum MemorySourceType {
  SystemGenerated = 1,
  CoachRecorded = 2,
  CoachCorrected = 3,
  AIGenerated = 4
}

export enum MemoryConfidenceLevel {
  Confirmed = 1,
  Provisional = 2,
  Uncertain = 3,
  Conflicted = 4,
  Superseded = 5
}

export enum MemoryRecordStatus {
  Active = 1,
  Conflicted = 2,
  Superseded = 3,
  Archived = 4,
  Anonymized = 5
}

export interface ClientMemoryRecordDto {
  id: string;
  clientId: string;
  coachId: string;
  memoryCategory: MemoryCategory;
  recordedAt: string;
  observedAt?: string;
  sourceType: MemorySourceType;
  sourceReference?: string;
  sourceDescription?: string;
  confidenceLevel: MemoryConfidenceLevel;
  recordStatus: MemoryRecordStatus;
  content: string;
  supersededById?: string;
  supersededAt?: string;
  supersessionReason?: string;
  isConflicted: boolean;
  conflictNotes?: string;
  coachCorrectionNote?: string;
  correctedAt?: string;
  isAnonymized: boolean;
  anonymizedAt?: string;
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface CreateClientMemoryRequestDto {
  memoryCategory: MemoryCategory;
  sourceType?: MemorySourceType;
  content: string;
  observedAt?: string;
  sourceReference?: string;
  sourceDescription?: string;
  explicitConfidence?: MemoryConfidenceLevel;
}

export interface CorrectClientMemoryRequestDto {
  content: string;
  reason: string;
  observedAt?: string;
  sourceReference?: string;
  sourceDescription?: string;
}

export interface ClientMemoryConflictDto {
  id: string;
  clientId: string;
  recordAId: string;
  recordA?: ClientMemoryRecordDto;
  recordBId: string;
  recordB?: ClientMemoryRecordDto;
  conflictDescription: string;
  detectedAtUtc: string;
  isAutoDetected: boolean;
  isResolved: boolean;
  resolvedAtUtc?: string;
  resolvedByCoachId?: string;
  resolutionNote?: string;
  winningRecordId?: string;
}

export interface ResolveClientMemoryConflictRequestDto {
  winningRecordId: string;
  resolutionNote: string;
}

export interface ClientMemorySnapshotDto {
  id: string;
  clientId: string;
  coachId: string;
  generatedAtUtc: string;
  generationTrigger: number;
  snapshotContentJson: string;
  isStale: boolean;
  includedRecordIds: string[];
  excludedConflictIds: string[];
}

export interface UnresolvedQuestionDto {
  recordId: string;
  category: MemoryCategory;
  content: string;
  confidenceLevel: MemoryConfidenceLevel;
  recordStatus: MemoryRecordStatus;
  questionContext: string;
  recordedAt: string;
}

export interface AnonymizeClientMemoryResultDto {
  clientId: string;
  recordsAnonymized: number;
  anonymizedAtUtc: string;
}
