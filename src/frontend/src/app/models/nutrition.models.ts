export enum BudgetTier {
  Constrained = 1,
  Moderate = 2,
  Flexible = 3
}

export enum ActivityLevel {
  Sedentary = 1,
  LightlyActive = 2,
  ModeratelyActive = 3,
  VeryActive = 4,
  ExtremelyActive = 5
}

export enum NutritionGoalType {
  FatLoss = 1,
  Maintenance = 2,
  MuscleGain = 3,
  Strength = 4,
  Recomposition = 5
}

export enum AdjustmentRecommendation {
  Maintain = 1,
  IncreaseCalories = 2,
  DecreaseCalories = 3,
  InsufficientData = 4
}

export enum CalibrationDecision {
  Pending = 1,
  Applied = 2,
  Rejected = 3
}

export enum FoodCategory {
  ProteinSource = 1,
  CarbohydrateSource = 2,
  FatSource = 3,
  MixedMeal = 4,
  VegetableOrFruit = 5
}

export enum DataConfidence {
  Preliminary = 1,
  Moderate = 2,
  High = 3
}

export interface NutritionTargetCalculationResultDto {
  bmr: number;
  tdee: number;
  recommendedCalories: number;
  proteinGrams: number;
  proteinGramsPerKg: number;
  formulaUsed: string;
  isHypothesis: boolean;
  uncertaintyAcknowledgment: string;
  explanation: string;
  hasDeficitWarning: boolean;
  warningMessage?: string;
}

export interface NutritionCalibrationRecordDto {
  id: string;
  clientNutritionProfileId: string;
  recordedAtUtc: string;
  weightKg: number;
  estimatedTDEE?: number;
  adjustmentRecommendation: AdjustmentRecommendation;
  adjustmentKcal?: number;
  weeksObserved: number;
  coachDecision: CalibrationDecision;
  coachNote?: string;
  weeklyWeightAverages: number[];
}

export interface ClientNutritionProfileDto {
  id: string;
  clientId: string;
  budgetTier: BudgetTier;
  mealsPerDay?: number;
  currentCalorieTarget?: number;
  currentProteinTargetGrams?: number;
  targetSetAtUtc?: string;
  targetSetMethod?: string;
  dietaryPreferences: string[];
  foodExclusions: string[];
  calibrationRecords: NutritionCalibrationRecordDto[];
}

export interface EgyptianFoodDto {
  id: string;
  nameAr: string;
  nameEn: string;
  servingDescription: string;
  servingGrams: number;
  caloriesPer100g: number;
  proteinPer100g: number;
  carbsPer100g: number;
  fatPer100g: number;
  fiberPer100g?: number;
  foodCategory: FoodCategory;
  isAffordableLow: boolean;
  isAffordableMid: boolean;
  dataSource: string;
  dataConfidence: DataConfidence;
  variabilityNote?: string;
}

export interface CalculateNutritionTargetRequestDto {
  weightKg: number;
  heightCm: number;
  ageYears: number;
  isMale: boolean;
  activityLevel: ActivityLevel;
  goalType: NutritionGoalType;
  customDeficitOrSurplusKcal?: number;
}

export interface CreateOrUpdateNutritionProfileRequestDto {
  clientId: string;
  budgetTier: BudgetTier;
  mealsPerDay?: number;
  dietaryPreferences?: string[];
  foodExclusions?: string[];
  currentCalorieTarget?: number;
  currentProteinTargetGrams?: number;
  targetSetMethod?: string;
}

export interface AddCalibrationRecordRequestDto {
  weightKg: number;
  goalType: NutritionGoalType;
  currentCalorieIntake: number;
  weeklyWeightAverages: number[];
}

export interface ApplyCalibrationDecisionRequestDto {
  decision: CalibrationDecision;
  coachNote?: string;
  applyAdjustmentToProfile?: boolean;
}

export enum FoodSuggestionStatus {
  Success = 1,
  InsufficientData = 2,
  NoDataAvailable = 3
}

export interface FoodOptionDto {
  foodId: string;
  nameAr: string;
  nameEn: string;
  servingDescription: string;
  servingGrams: number;
  caloriesPer100g: number;
  proteinPer100g: number;
  carbsPer100g: number;
  fatPer100g: number;
  foodCategory: FoodCategory;
  dataSource: string;
  dataConfidence: DataConfidence;
  variabilityNote?: string;
}

export interface ComplementaryProteinPairDto {
  legumeSource: FoodOptionDto;
  grainSource: FoodOptionDto;
  combinationTitle: string;
  guidanceNote: string;
}

export interface FoodSuggestionResult {
  status: FoodSuggestionStatus;
  directProteinOptions: FoodOptionDto[];
  generalFoodOptions: FoodOptionDto[];
  complementaryCombinations: ComplementaryProteinPairDto[];
  contextNote: string;
  scientificFramingNote: string;
}
