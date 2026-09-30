import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ExpertContentIngestionDto,
  ExpertContentIngestionSummaryDto,
  ExpertSourceDto,
  CreateExpertSourceDto,
  SubmitIngestionRequestDto,
  ReviewClaimRequestDto,
  ExpertClaimDto,
  IngestionStatus
} from '../models/expert-ingestion.models';

@Injectable({
  providedIn: 'root'
})
export class ExpertIngestionService {
  private readonly ingestionsUrl = '/api/expert-ingestions';
  private readonly sourcesUrl = '/api/expert-sources';

  constructor(private http: HttpClient) {}

  getIngestions(status?: IngestionStatus): Observable<ExpertContentIngestionSummaryDto[]> {
    const url = status ? `${this.ingestionsUrl}?status=${status}` : this.ingestionsUrl;
    return this.http.get<ExpertContentIngestionSummaryDto[]>(url);
  }

  getIngestionById(ingestionId: string): Observable<ExpertContentIngestionDto> {
    return this.http.get<ExpertContentIngestionDto>(`${this.ingestionsUrl}/${ingestionId}`);
  }

  submitIngestion(request: SubmitIngestionRequestDto): Observable<ExpertContentIngestionSummaryDto> {
    return this.http.post<ExpertContentIngestionSummaryDto>(this.ingestionsUrl, request);
  }

  reviewClaim(
    ingestionId: string,
    claimId: string,
    request: ReviewClaimRequestDto
  ): Observable<ExpertClaimDto> {
    return this.http.patch<ExpertClaimDto>(
      `${this.ingestionsUrl}/${ingestionId}/claims/${claimId}/review`,
      request
    );
  }

  getSources(): Observable<ExpertSourceDto[]> {
    return this.http.get<ExpertSourceDto[]>(this.sourcesUrl);
  }

  createSource(request: CreateExpertSourceDto): Observable<ExpertSourceDto> {
    return this.http.post<ExpertSourceDto>(this.sourcesUrl, request);
  }
}
