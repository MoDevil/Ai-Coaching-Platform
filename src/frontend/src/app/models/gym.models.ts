export enum EquipmentTier {
  Minimal = 1,
  Basic = 2,
  Commercial = 3,
  Premium = 4
}

export interface GymEquipmentItemDto {
  equipmentId: string;
  name: string;
  description?: string;
}

export interface GymProfileDto {
  id: string;
  coachId: string;
  name: string;
  location?: string;
  tier: EquipmentTier;
  explicitEquipmentIds: string[];
  explicitEquipmentDetails: GymEquipmentItemDto[];
  hasExplicitInventory: boolean;
  isInventoryAuthoritative: boolean;
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface CreateGymProfileRequestDto {
  name: string;
  location?: string;
  tier: EquipmentTier;
  explicitEquipmentIds?: string[];
  isInventoryAuthoritative?: boolean;
}

export interface UpdateGymProfileRequestDto {
  name: string;
  location?: string;
  tier: EquipmentTier;
  explicitEquipmentIds?: string[];
  isInventoryAuthoritative?: boolean;
}

export interface AssignClientGymRequestDto {
  gymProfileId?: string | null;
}
