import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  SafetyScreeningDto,
  CreateSafetyReportRequestDto,
  AcknowledgeSafetyScreeningRequestDto,
  RedFlagRuleDto
} from '../models/safety.models';

@Injectable({
  providedIn: 'root'
})
export class SafetyService {
  private readonly baseUrl = '/api/safety';

  constructor(private http: HttpClient) {}

  screenReport(request: CreateSafetyReportRequestDto): Observable<SafetyScreeningDto> {
    return this.http.post<SafetyScreeningDto>(`${this.baseUrl}/screen`, request);
  }

  acknowledgeScreening(screeningId: string, request: AcknowledgeSafetyScreeningRequestDto): Observable<SafetyScreeningDto> {
    return this.http.post<SafetyScreeningDto>(`${this.baseUrl}/screenings/${screeningId}/acknowledge`, request);
  }

  getClientScreenings(clientId: string): Observable<SafetyScreeningDto[]> {
    return this.http.get<SafetyScreeningDto[]>(`${this.baseUrl}/clients/${clientId}`);
  }

  getActiveRules(): Observable<RedFlagRuleDto[]> {
    return this.http.get<RedFlagRuleDto[]>(`${this.baseUrl}/rules`);
  }
}
