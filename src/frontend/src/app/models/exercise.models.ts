export enum QualitativeRating {
  Low = 1,
  Moderate = 2,
  High = 3
}

export const QualitativeRatingLabels: Record<QualitativeRating, string> = {
  [QualitativeRating.Low]: 'Low',
  [QualitativeRating.Moderate]: 'Moderate',
  [QualitativeRating.High]: 'High'
};

export enum ResistanceProfile {
  Lengthened = 1,
  MidRange = 2,
  Shortened = 3,
  Even = 4
}

export const ResistanceProfileLabels: Record<ResistanceProfile, string> = {
  [ResistanceProfile.Lengthened]: 'Lengthened',
  [ResistanceProfile.MidRange]: 'Mid-Range',
  [ResistanceProfile.Shortened]: 'Shortened',
  [ResistanceProfile.Even]: 'Even'
};

export enum ExerciseCategory {
  Compound = 1,
  Isolation = 2,
  Machine = 3,
  Bodyweight = 4
}

export const ExerciseCategoryLabels: Record<ExerciseCategory, string> = {
  [ExerciseCategory.Compound]: 'Compound',
  [ExerciseCategory.Isolation]: 'Isolation',
  [ExerciseCategory.Machine]: 'Machine',
  [ExerciseCategory.Bodyweight]: 'Bodyweight'
};

export enum MetadataStatus {
  Provisional = 1,
  Verified = 2
}

export const MetadataStatusLabels: Record<MetadataStatus, string> = {
  [MetadataStatus.Provisional]: 'Provisional',
  [MetadataStatus.Verified]: 'Verified'
};

export interface MovementPattern {
  id: string;
  name: string;
  description?: string;
}

export interface Muscle {
  id: string;
  name: string;
  commonName?: string;
  bodyPart: string;
}

export interface Equipment {
  id: string;
  name: string;
  category?: string;
}

export interface ExerciseSummary {
  id: string;
  name: string;
  aliases?: string;
  category: ExerciseCategory;
  movementPatternId: string;
  movementPatternName: string;
  stabilityRequirement: QualitativeRating;
  technicalDemand: QualitativeRating;
  localFatigueCost: QualitativeRating;
  systemicFatigueCost: QualitativeRating;
  stimulusPotential: QualitativeRating;
  progressionPotential: QualitativeRating;
  resistanceProfile: ResistanceProfile;
  metadataStatus: MetadataStatus;
  primaryMuscles: string[];
  secondaryMuscles: string[];
  equipmentNames: string[];
  substitutionGroupId?: string;
}

export interface ExerciseMuscle {
  muscleId: string;
  name: string;
  commonName?: string;
  bodyPart: string;
  isPrimary: boolean;
}

export interface ExerciseEquipment {
  equipmentId: string;
  name: string;
  category?: string;
  isRequired: boolean;
}

export interface ExerciseSubstitution {
  substituteExerciseId: string;
  substituteExerciseName: string;
  category: ExerciseCategory;
  movementPatternName: string;
  resistanceProfile: ResistanceProfile;
  stabilityRequirement: QualitativeRating;
  intentPreservationNotes?: string;
}

export interface ExerciseDetail {
  id: string;
  name: string;
  aliases?: string;
  category: ExerciseCategory;
  movementPatternId: string;
  movementPatternName: string;
  jointActions?: string;
  stabilityRequirement: QualitativeRating;
  technicalDemand: QualitativeRating;
  localFatigueCost: QualitativeRating;
  systemicFatigueCost: QualitativeRating;
  stimulusPotential: QualitativeRating;
  progressionPotential: QualitativeRating;
  resistanceProfile: ResistanceProfile;
  metadataStatus: MetadataStatus;
  substitutionGroupId?: string;
  muscles: ExerciseMuscle[];
  equipment: ExerciseEquipment[];
  substitutions: ExerciseSubstitution[];
  createdAtUtc: string;
}

export interface ExerciseFilter {
  search?: string;
  movementPatternId?: string;
  muscleId?: string;
  equipmentId?: string;
  category?: ExerciseCategory;
}
