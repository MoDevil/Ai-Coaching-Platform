import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AddClaimSource,
  CreateKnowledgeClaim,
  CreateKnowledgeSource,
  KnowledgeClaim,
  KnowledgeClaimSummary,
  KnowledgeFilter,
  KnowledgeSource,
  KnowledgeSourceSummary,
  SupersedeClaim
} from '../models/knowledge.models';

@Injectable({
  providedIn: 'root'
})
export class KnowledgeService {
  private readonly baseUrl = '/api/knowledge';

  constructor(private http: HttpClient) {}

  // Sources
  createSource(dto: CreateKnowledgeSource): Observable<KnowledgeSource> {
    return this.http.post<KnowledgeSource>(`${this.baseUrl}/sources`, dto);
  }

  getSources(): Observable<KnowledgeSourceSummary[]> {
    return this.http.get<KnowledgeSourceSummary[]>(`${this.baseUrl}/sources`);
  }

  getSourceById(id: string): Observable<KnowledgeSource> {
    return this.http.get<KnowledgeSource>(`${this.baseUrl}/sources/${id}`);
  }

  // Claims
  createClaim(dto: CreateKnowledgeClaim): Observable<KnowledgeClaim> {
    return this.http.post<KnowledgeClaim>(`${this.baseUrl}/claims`, dto);
  }

  getClaims(filter?: KnowledgeFilter): Observable<KnowledgeClaimSummary[]> {
    let params = new HttpParams();
    if (filter) {
      if (filter.search) params = params.set('search', filter.search);
      if (filter.topic) params = params.set('topic', filter.topic);
      if (filter.exerciseId) params = params.set('exerciseId', filter.exerciseId);
      if (filter.status !== undefined && filter.status !== null) params = params.set('status', filter.status.toString());
      if (filter.minEvidenceLevel !== undefined && filter.minEvidenceLevel !== null) {
        params = params.set('minEvidenceLevel', filter.minEvidenceLevel.toString());
      }
    }
    return this.http.get<KnowledgeClaimSummary[]>(`${this.baseUrl}/claims`, { params });
  }

  getClaimById(id: string): Observable<KnowledgeClaim> {
    return this.http.get<KnowledgeClaim>(`${this.baseUrl}/claims/${id}`);
  }

  getClaimsByExerciseId(exerciseId: string): Observable<KnowledgeClaimSummary[]> {
    return this.http.get<KnowledgeClaimSummary[]>(`/api/exercises/${exerciseId}/claims`);
  }

  addSourceToClaim(claimId: string, dto: AddClaimSource): Observable<KnowledgeClaim> {
    return this.http.post<KnowledgeClaim>(`${this.baseUrl}/claims/${claimId}/sources`, dto);
  }

  supersedeClaim(claimId: string, dto: SupersedeClaim): Observable<KnowledgeClaim> {
    return this.http.post<KnowledgeClaim>(`${this.baseUrl}/claims/${claimId}/supersede`, dto);
  }
}
