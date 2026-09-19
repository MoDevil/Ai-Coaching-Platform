export enum ClientStatus {
  Active = 1,
  Archived = 2
}

export const ClientStatusLabels: Record<ClientStatus, string> = {
  [ClientStatus.Active]: 'Active',
  [ClientStatus.Archived]: 'Archived'
};

export enum Gender {
  Male = 1,
  Female = 2,
  Other = 3
}

export const GenderLabels: Record<Gender, string> = {
  [Gender.Male]: 'Male',
  [Gender.Female]: 'Female',
  [Gender.Other]: 'Other'
};

export interface ClientGoal {
  primaryGoal: string;
  targetTimelineWeeks?: number | null;
  description?: string | null;
}

export interface ClientSummary {
  id: string;
  firstName: string;
  lastName: string;
  email?: string | null;
  phone?: string | null;
  status: ClientStatus;
  primaryGoal?: string | null;
  createdAtUtc: string;
}

export interface ConsentRecord {
  id: string;
  consentType: string;
  isGranted: boolean;
  grantedAtUtc: string;
  notes?: string | null;
}

export interface Client {
  id: string;
  firstName: string;
  lastName: string;
  email?: string | null;
  phone?: string | null;
  dateOfBirth?: string | null;
  gender?: Gender | null;
  goal?: ClientGoal | null;
  intakeNotes?: string | null;
  status: ClientStatus;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
  consentRecords: ConsentRecord[];
}

export interface CreateClientRequest {
  firstName: string;
  lastName: string;
  email?: string | null;
  phone?: string | null;
  dateOfBirth?: string | null;
  gender?: Gender | null;
  goal?: ClientGoal | null;
  intakeNotes?: string | null;
}

export interface UpdateClientRequest {
  firstName: string;
  lastName: string;
  email?: string | null;
  phone?: string | null;
  dateOfBirth?: string | null;
  gender?: Gender | null;
  goal?: ClientGoal | null;
  intakeNotes?: string | null;
}

export interface AddConsentRequest {
  consentType: string;
  isGranted: boolean;
  notes?: string | null;
}
