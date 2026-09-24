import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  TrainingLimitationDto,
  RehabAwarenessConsiderationDto,
  CreateTrainingLimitationRequestDto,
  ActivateLimitationRequestDto,
  UpdateLimitationStatusRequestDto,
  RecordConsiderationDecisionRequestDto,
  GenerateConsiderationsRequestDto
} from '../models/rehab.models';

@Injectable({
  providedIn: 'root'
})
export class RehabService {
  private readonly baseUrl = '/api/rehab';

  constructor(private http: HttpClient) {}

  createLimitation(request: CreateTrainingLimitationRequestDto): Observable<TrainingLimitationDto> {
    return this.http.post<TrainingLimitationDto>(`${this.baseUrl}/limitations`, request);
  }

  activateLimitation(limitationId: string, request: ActivateLimitationRequestDto): Observable<TrainingLimitationDto> {
    return this.http.post<TrainingLimitationDto>(`${this.baseUrl}/limitations/${limitationId}/activate`, request);
  }

  updateLimitationStatus(limitationId: string, request: UpdateLimitationStatusRequestDto): Observable<TrainingLimitationDto> {
    return this.http.post<TrainingLimitationDto>(`${this.baseUrl}/limitations/${limitationId}/status`, request);
  }

  generateConsiderations(limitationId: string, request: GenerateConsiderationsRequestDto): Observable<RehabAwarenessConsiderationDto[]> {
    return this.http.post<RehabAwarenessConsiderationDto[]>(`${this.baseUrl}/limitations/${limitationId}/generate`, request);
  }

  recordDecision(considerationId: string, request: RecordConsiderationDecisionRequestDto): Observable<RehabAwarenessConsiderationDto> {
    return this.http.post<RehabAwarenessConsiderationDto>(`${this.baseUrl}/considerations/${considerationId}/decision`, request);
  }

  getClientLimitations(clientId: string): Observable<TrainingLimitationDto[]> {
    return this.http.get<TrainingLimitationDto[]>(`${this.baseUrl}/clients/${clientId}/limitations`);
  }
}
