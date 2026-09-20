import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AdaptationAssessment,
  AdaptationRecommendation,
  CoachRecommendationDecision
} from '../models/adaptation.models';

@Injectable({
  providedIn: 'root'
})
export class AdaptationService {
  private readonly baseUrl = '/api/adaptations';

  constructor(private http: HttpClient) {}

  triggerAssessment(programVersionId: string): Observable<AdaptationAssessment> {
    return this.http.post<AdaptationAssessment>(`${this.baseUrl}/assess/${programVersionId}`, {});
  }

  getAssessmentById(id: string): Observable<AdaptationAssessment> {
    return this.http.get<AdaptationAssessment>(`${this.baseUrl}/${id}`);
  }

  getAssessmentsForProgram(programId: string): Observable<AdaptationAssessment[]> {
    return this.http.get<AdaptationAssessment[]>(`${this.baseUrl}/program/${programId}`);
  }

  decideRecommendation(
    recommendationId: string,
    decision: CoachRecommendationDecision
  ): Observable<AdaptationRecommendation> {
    return this.http.post<AdaptationRecommendation>(
      `${this.baseUrl}/recommendations/${recommendationId}/decision`,
      decision
    );
  }
}
