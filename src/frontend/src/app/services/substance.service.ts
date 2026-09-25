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
  PEDRedFlagRule,
  SupplementEvidenceStatus,
  HormoneCategory,
  PEDCategory
} from '../models/substance.model';

@Injectable({
  providedIn: 'root'
})
export class SubstanceService {
  private readonly baseUrl = '/api';

  constructor(private http: HttpClient) {}

  getSupplements(name?: string, evidenceStatus?: SupplementEvidenceStatus, includeProvisional = false): Observable<SupplementKnowledgeSummary[]> {
    let params = new HttpParams().set('includeProvisional', includeProvisional.toString());
    if (name) {
      params = params.set('name', name);
    }
    if (evidenceStatus !== undefined && evidenceStatus !== null) {
      params = params.set('evidenceStatus', evidenceStatus.toString());
    }
    return this.http.get<SupplementKnowledgeSummary[]>(`${this.baseUrl}/supplements`, { params });
  }

  getSupplementById(id: string): Observable<SupplementKnowledge> {
    return this.http.get<SupplementKnowledge>(`${this.baseUrl}/supplements/${id}`);
  }

  getHormones(category?: HormoneCategory, name?: string): Observable<HormoneKnowledgeSummary[]> {
    let params = new HttpParams();
    if (category !== undefined && category !== null) {
      params = params.set('category', category.toString());
    }
    if (name) {
      params = params.set('name', name);
    }
    return this.http.get<HormoneKnowledgeSummary[]>(`${this.baseUrl}/hormones`, { params });
  }

  getHormoneById(id: string): Observable<HormoneKnowledge> {
    return this.http.get<HormoneKnowledge>(`${this.baseUrl}/hormones/${id}`);
  }

  getPEDSafetyRecords(category?: PEDCategory): Observable<PEDSafetyRecordSummary[]> {
    let params = new HttpParams();
    if (category !== undefined && category !== null) {
      params = params.set('category', category.toString());
    }
    return this.http.get<PEDSafetyRecordSummary[]>(`${this.baseUrl}/ped-safety`, { params });
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
    return this.http.get<PEDRedFlagRule[]>(`${this.baseUrl}/substance-safety/red-flags`);
  }
}
