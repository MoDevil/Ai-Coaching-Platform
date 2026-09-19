export enum ProgramStatus {
  Draft = 1,
  Active = 2,
  Completed = 3,
  Archived = 4
}

export const ProgramStatusLabels: Record<ProgramStatus, string> = {
  [ProgramStatus.Draft]: 'Draft',
  [ProgramStatus.Active]: 'Active',
  [ProgramStatus.Completed]: 'Completed',
  [ProgramStatus.Archived]: 'Archived'
};

export enum PrimaryGoalType {
  Hypertrophy = 1,
  Strength = 2,
  FatLoss = 3,
  Recomposition = 4,
  GeneralFitness = 5
}

export const PrimaryGoalTypeLabels: Record<PrimaryGoalType, string> = {
  [PrimaryGoalType.Hypertrophy]: 'Hypertrophy',
  [PrimaryGoalType.Strength]: 'Strength',
  [PrimaryGoalType.FatLoss]: 'Fat Loss',
  [PrimaryGoalType.Recomposition]: 'Body Recomposition',
  [PrimaryGoalType.GeneralFitness]: 'General Fitness'
};

export enum MusclePriorityLevel {
  Primary = 1,
  Secondary = 2,
  Maintenance = 3,
  NotTargeted = 4
}

export const MusclePriorityLevelLabels: Record<MusclePriorityLevel, string> = {
  [MusclePriorityLevel.Primary]: 'Primary Focus',
  [MusclePriorityLevel.Secondary]: 'Secondary',
  [MusclePriorityLevel.Maintenance]: 'Maintenance',
  [MusclePriorityLevel.NotTargeted]: 'Not Targeted'
};

export enum RecoveryCapacity {
  Low = 1,
  Moderate = 2,
  High = 3
}

export const RecoveryCapacityLabels: Record<RecoveryCapacity, string> = {
  [RecoveryCapacity.Low]: 'Low',
  [RecoveryCapacity.Moderate]: 'Moderate',
  [RecoveryCapacity.High]: 'High'
};

export enum ProgressionRuleType {
  LinearLoad = 1,
  RepTarget = 2,
  RirTarget = 3
}

export const ProgressionRuleTypeLabels: Record<ProgressionRuleType, string> = {
  [ProgressionRuleType.LinearLoad]: 'Linear Load',
  [ProgressionRuleType.RepTarget]: 'Rep Target',
  [ProgressionRuleType.RirTarget]: 'RIR Target'
};

export interface GoalSnapshot {
  primaryGoal: PrimaryGoalType;
  secondaryGoal?: PrimaryGoalType;
  goalEmphasis?: string;
  targetTimelineWeeks?: number;
}

export interface ProgressionRule {
  type: ProgressionRuleType;
  currentTarget: string;
  incrementValue: string;
  incrementCondition?: string;
}

export interface ExerciseSlot {
  id: string;
  trainingSessionId: string;
  exerciseId: string;
  exerciseName: string;
  movementPatternName: string;
  order: number;
  targetSets: number;
  targetRepRange: string;
  effortGuideline: string;
  restSeconds: number;
  selectionRationale: string;
  progressionRule?: ProgressionRule;
  coachingNote?: string;
}

export interface TrainingSession {
  id: string;
  trainingWeekId: string;
  dayNumber: number;
  dayOfWeek?: number;
  name: string;
  sessionIntent: string;
  estimatedDurationMinutes: number;
  slots: ExerciseSlot[];
}

export interface TrainingWeek {
  id: string;
  programVersionId: string;
  weekNumber: number;
  sessions: TrainingSession[];
}

export interface ProgramMusclePriority {
  id: string;
  muscleId: string;
  muscleName: string;
  priorityLevel: MusclePriorityLevel;
  justification?: string;
}

export interface ProgramVersion {
  id: string;
  programId: string;
  versionNumber: number;
  recoveryCapacity: RecoveryCapacity;
  changeReason?: string;
  isActive: boolean;
  musclePriorities: ProgramMusclePriority[];
  weeks: TrainingWeek[];
  createdAtUtc: string;
}

export interface MuscleVolumeOutput {
  muscleId: string;
  muscleName: string;
  priorityLevel: MusclePriorityLevel;
  directWeeklySets: number;
  indirectWeeklySets: number;
  totalWeeklySets: number;
  frequencyPerWeek: number;
}

export interface VolumeSummary {
  totalWeeklySessions: number;
  totalWeeklySets: number;
  averageSessionDurationMinutes: number;
  muscleVolumes: MuscleVolumeOutput[];
}

export interface ProgramSummary {
  id: string;
  clientId: string;
  clientName: string;
  coachId: string;
  name: string;
  status: ProgramStatus;
  primaryGoal: PrimaryGoalType;
  secondaryGoal?: PrimaryGoalType;
  activeVersionNumber: number;
  totalSessionsPerWeek: number;
  rationaleSummary: string;
  createdAtUtc: string;
}

export interface Program {
  id: string;
  clientId: string;
  clientName: string;
  coachId: string;
  name: string;
  status: ProgramStatus;
  goalSnapshot: GoalSnapshot;
  rationaleSummary: string;
  activeVersion?: ProgramVersion;
  versions: ProgramVersion[];
  volumeSummary?: VolumeSummary;
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface GenerateProgramRequest {
  clientId: string;
  programName?: string;
  coachNotes?: string;
  numberOfWeeks?: number;
}

export interface UpdateProgramStatusRequest {
  status: ProgramStatus;
}
