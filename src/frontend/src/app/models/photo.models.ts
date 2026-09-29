export enum PhotoSetType {
  Front = 1,
  Side = 2,
  Back = 3,
  Custom = 4
}

export interface ClientPhotoDto {
  id: string;
  clientId: string;
  coachId: string;
  photoSetType: PhotoSetType;
  mimeType: string;
  fileSizeBytes: number;
  takenAt: string;
  uploadedAt: string;
  notes?: string;
  observationRecordId?: string;
  isAnonymized: boolean;
  anonymizedAt?: string;
}

export interface ClientPhotoSummaryDto {
  id: string;
  clientId: string;
  photoSetType: PhotoSetType;
  mimeType: string;
  fileSizeBytes: number;
  takenAt: string;
  uploadedAt: string;
  hasObservation: boolean;
  observationRecordId?: string;
  isAnonymized: boolean;
}

export interface SignedPhotoUrlDto {
  photoId: string;
  url: string;
  expiresAtUtc: string;
}

export interface UploadPhotoRequestDto {
  fileBytes: number[] | string;
  mimeType: string;
  photoSetType: PhotoSetType;
  takenAt?: string;
  notes?: string;
}

export interface AnalyzePhotoRequestDto {
  baselinePhotoId?: string;
  coachPrompt?: string;
}

export interface PhysiqueObservationResultDto {
  photoId: string;
  memoryRecordId: string;
  baselinePhotoId?: string;
  generalObservations: string;
  apparentSymmetryNotes: string;
  postureObservations: string;
  muscularDevelopmentNotes: string;
  comparisonNotes: string;
  limitationsStatement: string;
  coachActionRequired: boolean;
  confidenceStatement: string;
  analyzedAtUtc: string;
}
