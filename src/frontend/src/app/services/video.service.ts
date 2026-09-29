import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ClientVideoDto,
  ClientVideoSummaryDto,
  SignedMediaUrlDto,
  VideoFrameDto,
  EnqueueVideoAnalysisResponseDto,
  VideoAnalysisJobStatusDto,
  ExerciseTechniqueObservationResultDto
} from '../models/video.models';

@Injectable({
  providedIn: 'root'
})
export class VideoService {
  private readonly baseUrl = '/api/clients';

  constructor(private http: HttpClient) {}

  getVideos(clientId: string): Observable<ClientVideoSummaryDto[]> {
    return this.http.get<ClientVideoSummaryDto[]>(`${this.baseUrl}/${clientId}/videos`);
  }

  uploadVideo(clientId: string, formData: FormData): Observable<ClientVideoDto> {
    return this.http.post<ClientVideoDto>(`${this.baseUrl}/${clientId}/videos`, formData);
  }

  getSignedUrl(clientId: string, videoId: string): Observable<SignedMediaUrlDto> {
    return this.http.get<SignedMediaUrlDto>(`${this.baseUrl}/${clientId}/videos/${videoId}/url`);
  }

  getVideoFrames(clientId: string, videoId: string): Observable<VideoFrameDto[]> {
    return this.http.get<VideoFrameDto[]>(`${this.baseUrl}/${clientId}/videos/${videoId}/frames`);
  }

  deleteVideo(clientId: string, videoId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${clientId}/videos/${videoId}`);
  }

  analyzeVideo(
    clientId: string,
    videoId: string,
    focusAreas?: string[]
  ): Observable<EnqueueVideoAnalysisResponseDto> {
    return this.http.post<EnqueueVideoAnalysisResponseDto>(
      `${this.baseUrl}/${clientId}/videos/${videoId}/analyze`,
      { focusAreas: focusAreas || [] }
    );
  }

  getAnalysisStatus(clientId: string, videoId: string): Observable<VideoAnalysisJobStatusDto> {
    return this.http.get<VideoAnalysisJobStatusDto>(
      `${this.baseUrl}/${clientId}/videos/${videoId}/analysis-status`
    );
  }

  getObservation(
    clientId: string,
    videoId: string
  ): Observable<ExerciseTechniqueObservationResultDto> {
    return this.http.get<ExerciseTechniqueObservationResultDto>(
      `${this.baseUrl}/${clientId}/videos/${videoId}/observation`
    );
  }

  getObservations(
    clientId: string,
    videoId: string
  ): Observable<ExerciseTechniqueObservationResultDto[]> {
    return this.http.get<ExerciseTechniqueObservationResultDto[]>(
      `${this.baseUrl}/${clientId}/videos/${videoId}/observations`
    );
  }
}
