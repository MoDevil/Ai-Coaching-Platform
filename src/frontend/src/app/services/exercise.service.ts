import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Equipment,
  ExerciseDetail,
  ExerciseFilter,
  ExerciseSubstitution,
  ExerciseSummary,
  MovementPattern,
  Muscle
} from '../models/exercise.models';

@Injectable({
  providedIn: 'root'
})
export class ExerciseService {
  private readonly apiUrl = `${environment.apiUrl}/exercises`;

  constructor(private http: HttpClient) {}

  public getExercises(filter?: ExerciseFilter): Observable<ExerciseSummary[]> {
    let params = new HttpParams();
    if (filter?.search) {
      params = params.set('search', filter.search.trim());
    }
    if (filter?.movementPatternId) {
      params = params.set('movementPatternId', filter.movementPatternId);
    }
    if (filter?.muscleId) {
      params = params.set('muscleId', filter.muscleId);
    }
    if (filter?.equipmentId) {
      params = params.set('equipmentId', filter.equipmentId);
    }
    if (filter?.category !== undefined) {
      params = params.set('category', filter.category.toString());
    }

    return this.http.get<ExerciseSummary[]>(this.apiUrl, { params });
  }

  public getExerciseById(id: string): Observable<ExerciseDetail> {
    return this.http.get<ExerciseDetail>(`${this.apiUrl}/${id}`);
  }

  public getSubstitutions(id: string): Observable<ExerciseSubstitution[]> {
    return this.http.get<ExerciseSubstitution[]>(`${this.apiUrl}/${id}/substitutions`);
  }

  public getMovementPatterns(): Observable<MovementPattern[]> {
    return this.http.get<MovementPattern[]>(`${this.apiUrl}/meta/movement-patterns`);
  }

  public getMuscles(): Observable<Muscle[]> {
    return this.http.get<Muscle[]>(`${this.apiUrl}/meta/muscles`);
  }

  public getEquipment(): Observable<Equipment[]> {
    return this.http.get<Equipment[]>(`${this.apiUrl}/meta/equipment`);
  }
}
