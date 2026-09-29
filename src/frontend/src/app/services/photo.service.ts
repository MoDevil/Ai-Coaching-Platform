import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ClientPhotoDto,
  ClientPhotoSummaryDto,
  SignedPhotoUrlDto,
  UploadPhotoRequestDto,
  AnalyzePhotoRequestDto,
  PhysiqueObservationResultDto
} from '../models/photo.models';

@Injectable({
  providedIn: 'root'
})
export class PhotoService {
  private readonly baseUrl = '/api/clients';

  constructor(private http: HttpClient) {}

  getPhotos(clientId: string): Observable<ClientPhotoSummaryDto[]> {
    return this.http.get<ClientPhotoSummaryDto[]>(`${this.baseUrl}/${clientId}/photos`);
  }

  uploadPhoto(clientId: string, request: UploadPhotoRequestDto): Observable<ClientPhotoDto> {
    return this.http.post<ClientPhotoDto>(`${this.baseUrl}/${clientId}/photos`, request);
  }

  getSignedUrl(clientId: string, photoId: string): Observable<SignedPhotoUrlDto> {
    return this.http.get<SignedPhotoUrlDto>(`${this.baseUrl}/${clientId}/photos/${photoId}/url`);
  }

  deletePhoto(clientId: string, photoId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${clientId}/photos/${photoId}`);
  }

  analyzePhoto(
    clientId: string,
    photoId: string,
    request?: AnalyzePhotoRequestDto
  ): Observable<PhysiqueObservationResultDto> {
    return this.http.post<PhysiqueObservationResultDto>(
      `${this.baseUrl}/${clientId}/photos/${photoId}/analyze`,
      request || {}
    );
  }

  getObservation(clientId: string, photoId: string): Observable<PhysiqueObservationResultDto> {
    return this.http.get<PhysiqueObservationResultDto>(
      `${this.baseUrl}/${clientId}/photos/${photoId}/observation`
    );
  }
}
