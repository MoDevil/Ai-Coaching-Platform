import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  SupplementKnowledgeSummary,
  SupplementKnowledge,
  HormoneKnowledgeSummary,
  HormoneKnowledge,
  PEDSafetyRecordSummary,
  PEDSafetyRecord,
  EvaluateSubstanceSafetyRequest,
  SubstanceSafetyEvaluationResult,
  SubstanceEscalationRecord,
  PEDRedFlagRule
} from '../models/substance.model';

@Injectable({
  providedIn: 'root'
})
export class SubstanceService {
  private readonly baseUrl = '/api';

  constructor(private http: HttpClient) {}

  getSupplements(includeProvisional = false): Observable<SupplementKnowledgeSummary[]> {
    const params = new HttpParams().set('includeProvisional', includeProvisional.toString());
    return this.http.get<SupplementKnowledgeSummary[]>(`${this.baseUrl}/supplements`, { params });
  }

  getSupplementById(id: string): Observable<SupplementKnowledge> {
    return this.http.get<SupplementKnowledge>(`${this.baseUrl}/supplements/${id}`);
  }

  getHormones(): Observable<HormoneKnowledgeSummary[]> {
    return this.http.get<HormoneKnowledgeSummary[]>(`${this.baseUrl}/hormones`);
  }

  getHormoneById(id: string): Observable<HormoneKnowledge> {
    return this.http.get<HormoneKnowledge>(`${this.baseUrl}/hormones/${id}`);
  }

  getPEDSafetyRecords(): Observable<PEDSafetyRecordSummary[]> {
    return this.http.get<PEDSafetyRecordSummary[]>(`${this.baseUrl}/ped-safety`);
  }

  getPEDSafetyRecordById(id: string): Observable<PEDSafetyRecord> {
    return this.http.get<PEDSafetyRecord>(`${this.baseUrl}/ped-safety/${id}`);
  }

  evaluateSubstanceSafety(request: EvaluateSubstanceSafetyRequest): Observable<SubstanceSafetyEvaluationResult> {
    return this.http.post<SubstanceSafetyEvaluationResult>(`${this.baseUrl}/substance-safety/evaluate`, request);
  }

  getCoachEscalations(): Observable<SubstanceEscalationRecord[]> {
    return this.http.get<SubstanceEscalationRecord[]>(`${this.baseUrl}/substance-safety/escalations`);
  }

  getRules(): Observable<PEDRedFlagRule[]> {
    return this.http.get<PEDRedFlagRule[]>(`${this.baseUrl}/substance-safety/rules`);
  }
}
