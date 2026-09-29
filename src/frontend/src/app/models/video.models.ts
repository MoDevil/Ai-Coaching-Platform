export interface ClientVideoSummaryDto {
  id: string;
  clientId: string;
  coachId: string;
  exerciseId?: string | null;
  exerciseName: string;
  durationSeconds: number;
  fileSizeBytes: number;
  mimeType: string;
  frameCount: number;
  isAnonymized: boolean;
  coachNotes?: string | null;
  observationRecordId?: string | null;
  uploadedAtUtc: string;
}

export interface ClientVideoDto {
  id: string;
  clientId: string;
  coachId: string;
  exerciseId?: string | null;
  exerciseName: string;
  durationSeconds: number;
  fileSizeBytes: number;
  mimeType: string;
  frameCount: number;
  isAnonymized: boolean;
  coachNotes?: string | null;
  observationRecordId?: string | null;
  uploadedAtUtc: string;
}

export interface SignedMediaUrlDto {
  mediaType: string;
  signedUrl: string;
  expiresAtUtc: string;
}

export interface VideoFrameDto {
  frameIndex: number;
  timestampSeconds: number;
  signedUrl: string;
  expiresAtUtc: string;
}

export interface EnqueueVideoAnalysisResponseDto {
  jobId: string;
  status: string;
  message: string;
}

export interface VideoAnalysisJobStatusDto {
  jobId: string;
  status: string;
  progressPercentage: number;
  errorMessage?: string | null;
  observationRecordId?: string | null;
  completedAtUtc?: string | null;
}

export interface ExerciseTechniqueObservationResultDto {
  summary: string;
  keyObservations: string[];
  safetyFlags: string[];
  recommendations: string[];
  limitations: string;
  confidenceScore: number;
  analysisDateUtc: string;
  coachActionRequired: boolean;
}
