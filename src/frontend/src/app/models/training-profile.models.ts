export enum TrainingExperienceLevel {
  Beginner = 1,
  Novice = 2,
  Intermediate = 3,
  Advanced = 4
}

export const TrainingExperienceLevelLabels: Record<TrainingExperienceLevel, string> = {
  [TrainingExperienceLevel.Beginner]: 'Beginner (< 1 year structured lifting)',
  [TrainingExperienceLevel.Novice]: 'Novice (1–2 years)',
  [TrainingExperienceLevel.Intermediate]: 'Intermediate (2–5 years)',
  [TrainingExperienceLevel.Advanced]: 'Advanced (5+ years dedicated training)'
};

export const DayOfWeekLabels: Record<number, string> = {
  0: 'Sunday',
  1: 'Monday',
  2: 'Tuesday',
  3: 'Wednesday',
  4: 'Thursday',
  5: 'Friday',
  6: 'Saturday'
};

export interface TrainingAvailability {
  sessionsPerWeek: number;
  availableDays: number[];
  preferredDays: number[];
}

export interface ClientTrainingPriority {
  id?: string;
  order: number;
  focusArea: string;
  notes?: string;
}

export interface TrainingProfile {
  id: string;
  clientId: string;
  experienceLevel: TrainingExperienceLevel;
  sessionDurationMinMinutes?: number;
  sessionDurationTargetMinutes?: number;
  sessionDurationMaxMinutes?: number;
  weeklyAvailability: TrainingAvailability;
  availableEquipmentIds: string[];
  exercisePreferences?: string;
  exerciseConstraints?: string;
  priorities: ClientTrainingPriority[];
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface UpdateTrainingProfileRequest {
  experienceLevel: TrainingExperienceLevel;
  sessionDurationMinMinutes?: number | null;
  sessionDurationTargetMinutes?: number | null;
  sessionDurationMaxMinutes?: number | null;
  weeklyAvailability: TrainingAvailability;
  availableEquipmentIds: string[];
  exercisePreferences?: string | null;
  exerciseConstraints?: string | null;
  priorities: ClientTrainingPriority[];
}
