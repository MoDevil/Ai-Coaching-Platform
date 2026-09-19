export enum WorkoutStatus {
  InProgress = 1,
  Completed = 2,
  Abandoned = 3
}

export const WorkoutStatusLabels: Record<WorkoutStatus, string> = {
  [WorkoutStatus.InProgress]: 'In Progress',
  [WorkoutStatus.Completed]: 'Completed',
  [WorkoutStatus.Abandoned]: 'Abandoned'
};

export enum ProgressionEvaluationStatus {
  Met = 1,
  NotMet = 2,
  Incomplete = 3,
  NotEvaluable = 4
}

export const ProgressionEvaluationStatusLabels: Record<ProgressionEvaluationStatus, string> = {
  [ProgressionEvaluationStatus.Met]: 'Met (Ready to Progress)',
  [ProgressionEvaluationStatus.NotMet]: 'Not Met (Repeat Target)',
  [ProgressionEvaluationStatus.Incomplete]: 'Incomplete Sets',
  [ProgressionEvaluationStatus.NotEvaluable]: 'Not Evaluable'
};

export interface ProgressionEvaluationResultDto {
  status: ProgressionEvaluationStatus;
  reason: string;
  suggestedNextTarget?: string;
}

export interface WorkoutSetDto {
  id: string;
  workoutExerciseId: string;
  setNumber: number;
  repetitions: number;
  loadKg: number;
  rir?: number;
  isCompleted: boolean;
  notes?: string;
}

export interface WorkoutExerciseDto {
  id: string;
  workoutSessionId: string;
  exerciseId: string;
  exerciseName: string;
  exerciseSlotId?: string;
  orderInSession: number;
  plannedTargetRepRange?: string;
  plannedEffortGuideline?: string;
  plannedTargetSets?: number;
  plannedProgressionRule?: string;
  notes?: string;
  sets: WorkoutSetDto[];
  progressionResult?: ProgressionEvaluationResultDto;
}

export interface WorkoutSessionDto {
  id: string;
  clientId: string;
  clientName: string;
  coachId: string;
  programVersionId?: string;
  trainingSessionId?: string;
  plannedSessionName?: string;
  startedAtUtc: string;
  completedAtUtc?: string;
  status: WorkoutStatus;
  notes?: string;
  exercises: WorkoutExerciseDto[];
}

export interface WorkoutSummaryDto {
  id: string;
  clientId: string;
  clientName: string;
  trainingSessionId?: string;
  plannedSessionName?: string;
  startedAtUtc: string;
  completedAtUtc?: string;
  status: WorkoutStatus;
  totalExercises: number;
  totalSetsCompleted: number;
  notes?: string;
}

export interface StartWorkoutRequestDto {
  clientId: string;
  trainingSessionId?: string;
  notes?: string;
}

export interface AddWorkoutExerciseRequestDto {
  exerciseId: string;
  exerciseSlotId?: string;
  notes?: string;
}

export interface RecordWorkoutSetRequestDto {
  setNumber: number;
  repetitions: number;
  loadKg: number;
  rir?: number;
  isCompleted: boolean;
  notes?: string;
}

export interface UpdateWorkoutSetRequestDto {
  repetitions: number;
  loadKg: number;
  rir?: number;
  isCompleted: boolean;
  notes?: string;
}

export interface CompleteWorkoutRequestDto {
  notes?: string;
}
