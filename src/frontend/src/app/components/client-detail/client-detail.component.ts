import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ClientService } from '../../services/client.service';
import {
  AddConsentRequest,
  Client,
  ClientStatus,
  ClientStatusLabels,
  GenderLabels
} from '../../models/client.models';

import { SafetyService } from '../../services/safety.service';
import {
  SafetyScreeningDto,
  SafetyCategory,
  SafetyActionType,
  SignalType,
  SignalSeverity
} from '../../models/safety.models';
import { RehabService } from '../../services/rehab.service';
import {
  TrainingLimitationDto,
  RehabAwarenessConsiderationDto,
  LimitationSource,
  LimitationStatus,
  ConsiderationType,
  ConsiderationStatus
} from '../../models/rehab.models';
import { GymService } from '../../services/gym.service';
import {
  GymProfileDto,
  EquipmentTier
} from '../../models/gym.models';
import { NutritionService } from '../../services/nutrition.service';
import {
  ClientNutritionProfileDto,
  NutritionTargetCalculationResultDto,
  NutritionCalibrationRecordDto,
  EgyptianFoodDto,
  FoodSuggestionResult,
  FoodSuggestionStatus,
  BudgetTier,
  ActivityLevel,
  NutritionGoalType,
  AdjustmentRecommendation,
  CalibrationDecision,
  FoodCategory,
  DataConfidence
} from '../../models/nutrition.models';

@Component({
  selector: 'app-client-detail',
  templateUrl: './client-detail.component.html',
  styleUrls: ['./client-detail.component.css']
})
export class ClientDetailComponent implements OnInit {
  clientId = '';
  client: Client | null = null;
  isLoading = true;
  errorMessage = '';
  successMessage = '';

  // Gym & Equipment Tier state
  gyms: GymProfileDto[] = [];
  selectedGymId: string | null = null;
  isLoadingGyms = false;
  isAssigningGym = false;
  isCreatingGym = false;
  newGymName = '';
  newGymLocation = '';
  newGymTier = EquipmentTier.Commercial;
  EquipmentTier = EquipmentTier;

  // Safety screenings state
  screenings: SafetyScreeningDto[] = [];
  isLoadingSafety = false;
  acknowledgingScreeningId: string | null = null;
  coachAckNote = '';
  SafetyCategory = SafetyCategory;
  SafetyActionType = SafetyActionType;
  SignalType = SignalType;
  SignalSeverity = SignalSeverity;

  // Rehab awareness state
  limitations: TrainingLimitationDto[] = [];
  isLoadingRehab = false;
  LimitationSource = LimitationSource;
  LimitationStatus = LimitationStatus;
  ConsiderationType = ConsiderationType;
  ConsiderationStatus = ConsiderationStatus;

  // Nutrition state
  nutritionProfile: ClientNutritionProfileDto | null = null;
  isLoadingNutrition = false;
  isSavingNutrition = false;
  calculationResult: NutritionTargetCalculationResultDto | null = null;
  isCalculating = false;
  egyptianFoods: EgyptianFoodDto[] = [];
  isLoadingFoods = false;
  foodSuggestions: FoodSuggestionResult | null = null;
  isLoadingSuggestions = false;
  FoodSuggestionStatus = FoodSuggestionStatus;

  // Nutrition enums
  BudgetTier = BudgetTier;
  ActivityLevel = ActivityLevel;
  NutritionGoalType = NutritionGoalType;
  AdjustmentRecommendation = AdjustmentRecommendation;
  CalibrationDecision = CalibrationDecision;
  FoodCategory = FoodCategory;
  DataConfidence = DataConfidence;

  // Target calculation form
  calcWeightKg = 75;
  calcHeightCm = 175;
  calcAgeYears = 30;
  calcIsMale = true;
  calcActivityLevel = ActivityLevel.ModeratelyActive;
  calcGoalType = NutritionGoalType.Maintenance;
  calcCustomSurplusDeficit?: number;

  // Profile edit form
  profileBudgetTier = BudgetTier.Moderate;
  profileMealsPerDay = 3;
  profileCalorieTarget?: number;
  profileProteinTarget?: number;
  profileDietaryPreferences = '';
  profileFoodExclusions = '';

  // Calibration form
  calibWeightKg = 75;
  calibGoalType = NutritionGoalType.Maintenance;
  calibCalorieIntake = 2500;
  calibWeeklyWeights = '75.2, 75.0';
  isAddingCalibration = false;
  calibDecisionNotes: { [recordId: string]: string } = {};

  // Limitation creation form
  newLimitationRegion = '';
  newLimitationSource = LimitationSource.ReportedByClient;
  newLimitationDescription = '';
  isCreatingLimitation = false;

  // Activation & Decision notes
  activatingLimitationId: string | null = null;
  coachActivationNote = '';
  decisionNotes: { [considerationId: string]: string } = {};

  // Consent form state
  isSubmittingConsent = false;
  consentType = 'DataProcessing';
  consentGranted = true;
  consentNotes = '';
  consentError = '';

  ClientStatus = ClientStatus;
  ClientStatusLabels = ClientStatusLabels;
  GenderLabels = GenderLabels;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private clientService: ClientService,
    private safetyService: SafetyService,
    private rehabService: RehabService,
    private nutritionService: NutritionService,
    private gymService: GymService
  ) {}

  ngOnInit(): void {
    this.clientId = this.route.snapshot.paramMap.get('id') || '';
    if (!this.clientId) {
      this.errorMessage = 'Client ID not provided.';
      this.isLoading = false;
      return;
    }
    this.loadClient();
    this.loadSafetyScreenings();
    this.loadRehabLimitations();
    this.loadNutritionProfile();
    this.loadEgyptianFoods();
    this.loadFoodSuggestions();
    this.loadGyms();
  }

  loadGyms(): void {
    this.isLoadingGyms = true;
    this.gymService.getGyms().subscribe({
      next: (gyms) => {
        this.gyms = gyms;
        this.isLoadingGyms = false;
      },
      error: () => {
        this.isLoadingGyms = false;
      }
    });
  }

  onAssignGym(): void {
    this.isAssigningGym = true;
    this.gymService.assignClientGym(this.clientId, this.selectedGymId || null).subscribe({
      next: () => {
        this.isAssigningGym = false;
        this.successMessage = 'Gym assignment updated.';
      },
      error: (err) => {
        this.isAssigningGym = false;
        this.errorMessage = err.error?.message || 'Failed to assign gym.';
      }
    });
  }

  onCreateGym(): void {
    if (!this.newGymName.trim()) {
      this.errorMessage = 'Gym name is required.';
      return;
    }
    this.isCreatingGym = true;
    this.gymService.createGym({
      name: this.newGymName.trim(),
      location: this.newGymLocation.trim() || undefined,
      tier: this.newGymTier
    }).subscribe({
      next: (created) => {
        this.isCreatingGym = false;
        this.newGymName = '';
        this.newGymLocation = '';
        this.gyms = [created, ...this.gyms];
        this.selectedGymId = created.id;
        this.successMessage = `Gym profile '${created.name}' created.`;
      },
      error: (err) => {
        this.isCreatingGym = false;
        this.errorMessage = err.error?.message || 'Failed to create gym.';
      }
    });
  }

  loadFoodSuggestions(): void {
    this.isLoadingSuggestions = true;
    this.nutritionService.getFoodSuggestions(this.clientId, this.profileBudgetTier).subscribe({
      next: (res) => {
        this.foodSuggestions = res;
        this.isLoadingSuggestions = false;
      },
      error: () => {
        this.isLoadingSuggestions = false;
      }
    });
  }

  loadNutritionProfile(): void {
    this.isLoadingNutrition = true;
    this.nutritionService.getProfile(this.clientId).subscribe({
      next: (profile) => {
        this.nutritionProfile = profile;
        this.profileBudgetTier = profile.budgetTier;
        this.profileMealsPerDay = profile.mealsPerDay || 3;
        this.profileCalorieTarget = profile.currentCalorieTarget;
        this.profileProteinTarget = profile.currentProteinTargetGrams;
        this.profileDietaryPreferences = (profile.dietaryPreferences || []).join(', ');
        this.profileFoodExclusions = (profile.foodExclusions || []).join(', ');
        this.isLoadingNutrition = false;
        this.loadFoodSuggestions();
      },
      error: () => {
        this.nutritionProfile = null;
        this.isLoadingNutrition = false;
      }
    });
  }

  loadEgyptianFoods(): void {
    this.isLoadingFoods = true;
    this.nutritionService.getFoods(this.profileBudgetTier).subscribe({
      next: (foods) => {
        this.egyptianFoods = foods;
        this.isLoadingFoods = false;
      },
      error: () => {
        this.isLoadingFoods = false;
      }
    });
  }

  onCalculateTargets(): void {
    this.isCalculating = true;
    this.nutritionService.calculateTargets({
      weightKg: this.calcWeightKg,
      heightCm: this.calcHeightCm,
      ageYears: this.calcAgeYears,
      isMale: this.calcIsMale,
      activityLevel: this.calcActivityLevel,
      goalType: this.calcGoalType,
      customDeficitOrSurplusKcal: this.calcCustomSurplusDeficit
    }).subscribe({
      next: (res) => {
        this.calculationResult = res;
        this.isCalculating = false;
      },
      error: (err) => {
        this.isCalculating = false;
        this.errorMessage = err.error?.message || 'Failed to calculate nutrition targets.';
      }
    });
  }

  onApplyCalculatedTargets(): void {
    if (!this.calculationResult) return;
    this.profileCalorieTarget = this.calculationResult.recommendedCalories;
    this.profileProteinTarget = this.calculationResult.proteinGrams;
    this.successMessage = 'Applied calculated targets to profile draft.';
  }

  onSaveNutritionProfile(): void {
    this.isSavingNutrition = true;
    const prefs = this.profileDietaryPreferences.split(',').map(s => s.trim()).filter(s => !!s);
    const exclusions = this.profileFoodExclusions.split(',').map(s => s.trim()).filter(s => !!s);

    this.nutritionService.createOrUpdateProfile({
      clientId: this.clientId,
      budgetTier: this.profileBudgetTier,
      mealsPerDay: this.profileMealsPerDay,
      dietaryPreferences: prefs,
      foodExclusions: exclusions,
      currentCalorieTarget: this.profileCalorieTarget,
      currentProteinTargetGrams: this.profileProteinTarget,
      targetSetMethod: 'Coach Defined via M10 Mifflin-St Jeor'
    }).subscribe({
      next: (profile) => {
        this.nutritionProfile = profile;
        this.isSavingNutrition = false;
        this.successMessage = 'Client nutrition profile saved.';
        this.loadEgyptianFoods();
        this.loadFoodSuggestions();
      },
      error: (err) => {
        this.isSavingNutrition = false;
        this.errorMessage = err.error?.message || 'Failed to save nutrition profile.';
      }
    });
  }

  onAddCalibration(): void {
    this.isAddingCalibration = true;
    const weights = this.calibWeeklyWeights.split(',').map(s => parseFloat(s.trim())).filter(n => !isNaN(n));

    this.nutritionService.addCalibrationRecord(this.clientId, {
      weightKg: this.calibWeightKg,
      goalType: this.calibGoalType,
      currentCalorieIntake: this.calibCalorieIntake,
      weeklyWeightAverages: weights
    }).subscribe({
      next: () => {
        this.isAddingCalibration = false;
        this.successMessage = 'Calibration evaluation recorded.';
        this.loadNutritionProfile();
      },
      error: (err) => {
        this.isAddingCalibration = false;
        this.errorMessage = err.error?.message || 'Failed to record calibration.';
      }
    });
  }

  onApplyCalibrationDecision(recordId: string, decision: CalibrationDecision, applyAdjustment: boolean): void {
    const note = this.calibDecisionNotes[recordId]?.trim() || undefined;
    this.nutritionService.applyCoachDecision(recordId, {
      decision,
      coachNote: note,
      applyAdjustmentToProfile: applyAdjustment
    }).subscribe({
      next: () => {
        this.successMessage = `Calibration decision recorded (${decision === CalibrationDecision.Applied ? 'Applied' : 'Rejected'}).`;
        this.loadNutritionProfile();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to record calibration decision.';
      }
    });
  }

  loadSafetyScreenings(): void {
    this.isLoadingSafety = true;
    this.safetyService.getClientScreenings(this.clientId).subscribe({
      next: (data) => {
        this.screenings = data;
        this.isLoadingSafety = false;
      },
      error: () => {
        this.isLoadingSafety = false;
      }
    });
  }

  loadRehabLimitations(): void {
    this.isLoadingRehab = true;
    this.rehabService.getClientLimitations(this.clientId).subscribe({
      next: (data) => {
        this.limitations = data;
        this.isLoadingRehab = false;
      },
      error: () => {
        this.isLoadingRehab = false;
      }
    });
  }

  onCreateLimitation(): void {
    if (!this.newLimitationRegion.trim()) {
      this.errorMessage = 'Affected body region is required.';
      return;
    }

    this.isCreatingLimitation = true;
    this.rehabService.createLimitation({
      clientId: this.clientId,
      affectedBodyRegion: this.newLimitationRegion.trim(),
      limitationSource: this.newLimitationSource,
      description: this.newLimitationDescription.trim() || undefined
    }).subscribe({
      next: () => {
        this.isCreatingLimitation = false;
        this.newLimitationRegion = '';
        this.newLimitationDescription = '';
        this.successMessage = 'Training limitation recorded.';
        this.loadRehabLimitations();
      },
      error: (err) => {
        this.isCreatingLimitation = false;
        this.errorMessage = err.error?.message || 'Failed to record training limitation.';
      }
    });
  }

  onActivateLimitation(limitationId: string): void {
    if (!this.coachActivationNote.trim()) {
      this.errorMessage = 'Coach activation note is required.';
      return;
    }

    this.rehabService.activateLimitation(limitationId, { coachNote: this.coachActivationNote.trim() }).subscribe({
      next: () => {
        this.coachActivationNote = '';
        this.activatingLimitationId = null;
        this.successMessage = 'Limitation activated for M9 considerations.';
        this.loadRehabLimitations();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to activate limitation.';
      }
    });
  }

  onGenerateConsiderations(limitationId: string): void {
    this.rehabService.generateConsiderations(limitationId, {}).subscribe({
      next: () => {
        this.successMessage = 'Non-diagnostic considerations generated for coach review.';
        this.loadRehabLimitations();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to generate considerations.';
      }
    });
  }

  onRecordDecision(considerationId: string, decision: ConsiderationStatus): void {
    const note = this.decisionNotes[considerationId]?.trim() || undefined;
    this.rehabService.recordDecision(considerationId, { decision, note }).subscribe({
      next: () => {
        this.successMessage = `Consideration marked as ${decision === ConsiderationStatus.ApprovedByCoach ? 'Approved' : decision === ConsiderationStatus.RejectedByCoach ? 'Rejected' : 'Applied'}.`;
        this.loadRehabLimitations();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to record consideration decision.';
      }
    });
  }

  onUpdateLimitationStatus(limitationId: string, status: LimitationStatus): void {
    this.rehabService.updateLimitationStatus(limitationId, { status }).subscribe({
      next: () => {
        this.successMessage = `Limitation status updated.`;
        this.loadRehabLimitations();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to update limitation status.';
      }
    });
  }

  onAcknowledgeScreening(screeningId: string): void {
    this.safetyService.acknowledgeScreening(screeningId, { coachNote: this.coachAckNote.trim() || undefined }).subscribe({
      next: (updated) => {
        this.coachAckNote = '';
        this.acknowledgingScreeningId = null;
        this.screenings = this.screenings.map(s => s.id === updated.id ? updated : s);
        this.successMessage = 'Safety screening acknowledged.';
      },
      error: () => {
        this.errorMessage = 'Failed to acknowledge safety screening.';
      }
    });
  }

  loadClient(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.clientService.getClientById(this.clientId).subscribe({
      next: (data) => {
        this.client = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.status === 404
          ? 'Client not found or you do not have permission to view this client.'
          : 'Failed to load client details.';
      }
    });
  }

  onArchiveClient(): void {
    if (!confirm('Are you sure you want to archive this client? You can still view their history later.')) {
      return;
    }

    this.clientService.archiveClient(this.clientId).subscribe({
      next: () => {
        this.successMessage = 'Client archived successfully.';
        this.loadClient();
      },
      error: () => {
        this.errorMessage = 'Failed to archive client.';
      }
    });
  }

  onAddConsent(): void {
    if (!this.consentType.trim()) {
      this.consentError = 'Consent type is required.';
      return;
    }

    this.isSubmittingConsent = true;
    this.consentError = '';

    const request: AddConsentRequest = {
      consentType: this.consentType.trim(),
      isGranted: this.consentGranted,
      notes: this.consentNotes.trim() ? this.consentNotes.trim() : null
    };

    this.clientService.addConsentRecord(this.clientId, request).subscribe({
      next: (record) => {
        this.isSubmittingConsent = false;
        this.consentNotes = '';
        if (this.client) {
          this.client.consentRecords = [record, ...this.client.consentRecords];
        }
        this.successMessage = 'Consent record added.';
      },
      error: (err) => {
        this.isSubmittingConsent = false;
        this.consentError = 'Failed to record consent.';
      }
    });
  }

  getStatusBadgeClass(status: ClientStatus): string {
    return status === ClientStatus.Active ? 'badge-active' : 'badge-archived';
  }
}
