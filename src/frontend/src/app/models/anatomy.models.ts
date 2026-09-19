export enum CertaintyLevel {
  Established = 1,
  Inferred = 2,
  Hypothesis = 3
}

export const CertaintyLevelLabels: Record<CertaintyLevel, string> = {
  [CertaintyLevel.Established]: 'Established Principle',
  [CertaintyLevel.Inferred]: 'Mechanical Inference',
  [CertaintyLevel.Hypothesis]: 'Contextual Hypothesis'
};

export enum JointActionType {
  Flexion = 1,
  Extension = 2,
  Abduction = 3,
  Adduction = 4,
  HorizontalAbduction = 5,
  HorizontalAdduction = 6,
  InternalRotation = 7,
  ExternalRotation = 8,
  Elevation = 9,
  Depression = 10,
  Retraction = 11,
  Protraction = 12,
  Plantarflexion = 13,
  Dorsiflexion = 14,
  Pronation = 15,
  Supination = 16,
  LateralFlexion = 17,
  Rotation = 18
}

export const JointActionTypeLabels: Record<JointActionType, string> = {
  [JointActionType.Flexion]: 'Flexion',
  [JointActionType.Extension]: 'Extension',
  [JointActionType.Abduction]: 'Abduction',
  [JointActionType.Adduction]: 'Adduction',
  [JointActionType.HorizontalAbduction]: 'Horizontal Abduction',
  [JointActionType.HorizontalAdduction]: 'Horizontal Adduction',
  [JointActionType.InternalRotation]: 'Internal Rotation',
  [JointActionType.ExternalRotation]: 'External Rotation',
  [JointActionType.Elevation]: 'Elevation',
  [JointActionType.Depression]: 'Depression',
  [JointActionType.Retraction]: 'Retraction',
  [JointActionType.Protraction]: 'Protraction',
  [JointActionType.Plantarflexion]: 'Plantarflexion',
  [JointActionType.Dorsiflexion]: 'Dorsiflexion',
  [JointActionType.Pronation]: 'Pronation',
  [JointActionType.Supination]: 'Supination',
  [JointActionType.LateralFlexion]: 'Lateral Flexion',
  [JointActionType.Rotation]: 'Rotation'
};

export enum PlaneOfMotion {
  Sagittal = 1,
  Frontal = 2,
  Transverse = 3,
  Multiplanar = 4
}

export const PlaneOfMotionLabels: Record<PlaneOfMotion, string> = {
  [PlaneOfMotion.Sagittal]: 'Sagittal Plane',
  [PlaneOfMotion.Frontal]: 'Frontal Plane',
  [PlaneOfMotion.Transverse]: 'Transverse Plane',
  [PlaneOfMotion.Multiplanar]: 'Multiplanar'
};

export enum JointActionRole {
  PrimaryMover = 1,
  SecondaryMover = 2,
  Stabilizer = 3
}

export const JointActionRoleLabels: Record<JointActionRole, string> = {
  [JointActionRole.PrimaryMover]: 'Primary Mover',
  [JointActionRole.SecondaryMover]: 'Secondary Mover',
  [JointActionRole.Stabilizer]: 'Dynamic Stabilizer'
};

export enum BiomechanicalAspect {
  MomentArm = 1,
  ResistanceDirection = 2,
  MuscleLength = 3,
  RangeOfMotion = 4,
  Stability = 5,
  SetupVariable = 6,
  MachineGeometry = 7
}

export const BiomechanicalAspectLabels: Record<BiomechanicalAspect, string> = {
  [BiomechanicalAspect.MomentArm]: 'Moment Arm / Leverage',
  [BiomechanicalAspect.ResistanceDirection]: 'Resistance Direction',
  [BiomechanicalAspect.MuscleLength]: 'Muscle Length / Tension',
  [BiomechanicalAspect.RangeOfMotion]: 'Active ROM',
  [BiomechanicalAspect.Stability]: 'External & Internal Stability',
  [BiomechanicalAspect.SetupVariable]: 'Setup & Execution Variable',
  [BiomechanicalAspect.MachineGeometry]: 'Machine Geometry & Pivot Axis'
};

export interface AnatomicalRegionSummary {
  id: string;
  name: string;
  description?: string;
  jointCount: number;
}

export interface JointSummary {
  id: string;
  regionId: string;
  regionName: string;
  name: string;
  commonName?: string;
  description?: string;
}

export interface JointActionSummary {
  id: string;
  jointId: string;
  jointName: string;
  actionType: JointActionType;
  actionTypeName: string;
  planeOfMotion: PlaneOfMotion;
  description?: string;
}

export interface ExerciseJointAction {
  jointActionId: string;
  jointId: string;
  jointName: string;
  actionType: JointActionType;
  actionTypeName: string;
  planeOfMotion: PlaneOfMotion;
  role: JointActionRole;
}

export interface BiomechanicalConsideration {
  id: string;
  exerciseId: string;
  aspect: BiomechanicalAspect;
  certainty: CertaintyLevel;
  summary: string;
  explanation: string;
  practicalCues?: string;
  knowledgeClaimId?: string;
  knowledgeClaimTopic?: string;
  knowledgeClaimText?: string;
  createdAtUtc: string;
}

export interface ExerciseBiomechanics {
  exerciseId: string;
  exerciseName: string;
  movementPatternName: string;
  resistanceProfileName: string;
  jointActions: ExerciseJointAction[];
  considerations: BiomechanicalConsideration[];
}
