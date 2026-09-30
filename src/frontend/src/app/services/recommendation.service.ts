import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AIRecommendationSummary,
  AIRecommendationDetail,
  ReviewAIRecommendationRequest,
  GenerateReasoningRequest
} from '../models/recommendation.models';

@Injectable({
  providedIn: 'root'
})
export class RecommendationService {
  constructor(private http: HttpClient) {}

  getClientRecommendations(
    clientId: string,
    status?: string,
    category?: string
  ): Observable<AIRecommendationSummary[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    if (category) {
      params = params.set('category', category);
    }
    return this.http.get<AIRecommendationSummary[]>(`/api/clients/${clientId}/recommendations`, { params });
  }

  getRecommendationDetail(
    clientId: string,
    recommendationId: string
  ): Observable<AIRecommendationDetail> {
    return this.http.get<AIRecommendationDetail>(`/api/clients/${clientId}/recommendations/${recommendationId}`);
  }

  reviewRecommendation(
    recommendationId: string,
    request: ReviewAIRecommendationRequest
  ): Observable<any> {
    return this.http.patch(`/api/reasoning/${recommendationId}/review`, request);
  }

  linkProgramVersion(
    recommendationId: string,
    request: { programVersionId: string }
  ): Observable<any> {
    return this.http.patch(`/api/reasoning/${recommendationId}/link-program-version`, request);
  }

  generateReasoning(request: GenerateReasoningRequest): Observable<any> {
    return this.http.post(`/api/reasoning/generate`, request);
  }
}
