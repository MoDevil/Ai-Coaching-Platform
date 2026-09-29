export enum PhotoSetType {
  FrontRelaxed = 1,
  SideRelaxed = 2,
  BackRelaxed = 3,
  FrontFlexed = 4,
  SideFlexed = 5,
  BackFlexed = 6
}

export interface ClientPhotoDto {
  id: string;
  clientId: string;
  coachId: string;
  photoSetType: PhotoSetType;
  mimeType: string;
  fileSizeBytes: number;
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
