import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  BudgetTier,
  FoodCategory,
  ClientNutritionProfileDto,
  NutritionTargetCalculationResultDto,
  NutritionCalibrationRecordDto,
  EgyptianFoodDto,
  CalculateNutritionTargetRequestDto,
  CreateOrUpdateNutritionProfileRequestDto,
  AddCalibrationRecordRequestDto,
  ApplyCalibrationDecisionRequestDto
} from '../models/nutrition.models';

@Injectable({
  providedIn: 'root'
})
export class NutritionService {
  private readonly baseUrl = '/api/nutrition';

  constructor(private http: HttpClient) {}

  getProfile(clientId: string): Observable<ClientNutritionProfileDto> {
    return this.http.get<ClientNutritionProfileDto>(`${this.baseUrl}/clients/${clientId}`);
  }

  createOrUpdateProfile(request: CreateOrUpdateNutritionProfileRequestDto): Observable<ClientNutritionProfileDto> {
    return this.http.post<ClientNutritionProfileDto>(`${this.baseUrl}/profiles`, request);
  }

  calculateTargets(request: CalculateNutritionTargetRequestDto): Observable<NutritionTargetCalculationResultDto> {
    return this.http.post<NutritionTargetCalculationResultDto>(`${this.baseUrl}/calculate-targets`, request);
  }

  addCalibrationRecord(clientId: string, request: AddCalibrationRecordRequestDto): Observable<NutritionCalibrationRecordDto> {
    return this.http.post<NutritionCalibrationRecordDto>(`${this.baseUrl}/clients/${clientId}/calibrations`, request);
  }

  applyCoachDecision(calibrationId: string, request: ApplyCalibrationDecisionRequestDto): Observable<NutritionCalibrationRecordDto> {
    return this.http.post<NutritionCalibrationRecordDto>(`${this.baseUrl}/calibrations/${calibrationId}/decision`, request);
  }

  getFoods(budgetTier?: BudgetTier, category?: FoodCategory): Observable<EgyptianFoodDto[]> {
    let params = new HttpParams();
    if (budgetTier !== undefined && budgetTier !== null) {
      params = params.set('budgetTier', budgetTier.toString());
    }
    if (category !== undefined && category !== null) {
      params = params.set('category', category.toString());
    }
    return this.http.get<EgyptianFoodDto[]>(`${this.baseUrl}/foods`, { params });
  }

  getFoodSuggestions(clientId: string, budgetTier?: BudgetTier): Observable<import('../models/nutrition.models').FoodSuggestionResult> {
    let params = new HttpParams();
    if (budgetTier !== undefined && budgetTier !== null) {
      params = params.set('budgetTier', budgetTier.toString());
    }
    return this.http.get<import('../models/nutrition.models').FoodSuggestionResult>(`${this.baseUrl}/clients/${clientId}/suggestions`, { params });
  }
}
