import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AnatomicalRegionSummary,
  ExerciseBiomechanics,
  JointSummary
} from '../models/anatomy.models';

@Injectable({
  providedIn: 'root'
})
export class AnatomyService {
  private readonly baseUrl = '/api';

  constructor(private http: HttpClient) {}

  getRegions(): Observable<AnatomicalRegionSummary[]> {
    return this.http.get<AnatomicalRegionSummary[]>(`${this.baseUrl}/anatomy/regions`);
  }

  getJoints(regionId?: string): Observable<JointSummary[]> {
    const url = regionId 
      ? `${this.baseUrl}/anatomy/joints?regionId=${regionId}`
      : `${this.baseUrl}/anatomy/joints`;
    return this.http.get<JointSummary[]>(url);
  }

  getExerciseBiomechanics(exerciseId: string): Observable<ExerciseBiomechanics> {
    return this.http.get<ExerciseBiomechanics>(`${this.baseUrl}/biomechanics/exercises/${exerciseId}`);
  }
}
