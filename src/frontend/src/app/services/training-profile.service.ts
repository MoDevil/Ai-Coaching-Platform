import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ClientTrainingPriority,
  TrainingAvailability,
  TrainingProfile,
  UpdateTrainingProfileRequest
} from '../models/training-profile.models';

@Injectable({
  providedIn: 'root'
})
export class TrainingProfileService {
  private readonly baseUrl = `${environment.apiUrl}/clients`;

  constructor(private http: HttpClient) {}

  public getProfile(clientId: string): Observable<TrainingProfile> {
    return this.http.get<TrainingProfile>(`${this.baseUrl}/${clientId}/training-profile`);
  }

  public updateProfile(clientId: string, request: UpdateTrainingProfileRequest): Observable<TrainingProfile> {
    return this.http.put<TrainingProfile>(`${this.baseUrl}/${clientId}/training-profile`, request);
  }

  public updateAvailability(clientId: string, availability: TrainingAvailability): Observable<TrainingProfile> {
    return this.http.put<TrainingProfile>(`${this.baseUrl}/${clientId}/training-profile/availability`, availability);
  }

  public updatePriorities(clientId: string, priorities: ClientTrainingPriority[]): Observable<TrainingProfile> {
    return this.http.put<TrainingProfile>(`${this.baseUrl}/${clientId}/training-profile/priorities`, priorities);
  }
}
