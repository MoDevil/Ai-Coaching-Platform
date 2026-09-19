import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  WorkoutSessionDto,
  WorkoutSummaryDto,
  StartWorkoutRequestDto,
  AddWorkoutExerciseRequestDto,
  RecordWorkoutSetRequestDto,
  UpdateWorkoutSetRequestDto,
  CompleteWorkoutRequestDto
} from '../models/workout.models';

@Injectable({
  providedIn: 'root'
})
export class WorkoutService {
  private readonly baseUrl = '/api/workouts';

  constructor(private http: HttpClient) {}

  startWorkout(request: StartWorkoutRequestDto): Observable<WorkoutSessionDto> {
    return this.http.post<WorkoutSessionDto>(`${this.baseUrl}/start`, request);
  }

  getWorkoutById(id: string): Observable<WorkoutSessionDto> {
    return this.http.get<WorkoutSessionDto>(`${this.baseUrl}/${id}`);
  }

  getWorkoutsByClientId(clientId: string): Observable<WorkoutSummaryDto[]> {
    return this.http.get<WorkoutSummaryDto[]>(`${this.baseUrl}/client/${clientId}`);
  }

  addExercise(workoutId: string, request: AddWorkoutExerciseRequestDto): Observable<WorkoutSessionDto> {
    return this.http.post<WorkoutSessionDto>(`${this.baseUrl}/${workoutId}/exercises`, request);
  }

  recordSet(workoutId: string, workoutExerciseId: string, request: RecordWorkoutSetRequestDto): Observable<WorkoutSessionDto> {
    return this.http.post<WorkoutSessionDto>(`${this.baseUrl}/${workoutId}/exercises/${workoutExerciseId}/sets`, request);
  }

  updateSet(workoutId: string, setId: string, request: UpdateWorkoutSetRequestDto): Observable<WorkoutSessionDto> {
    return this.http.put<WorkoutSessionDto>(`${this.baseUrl}/${workoutId}/sets/${setId}`, request);
  }

  completeWorkout(workoutId: string, request: CompleteWorkoutRequestDto): Observable<WorkoutSessionDto> {
    return this.http.post<WorkoutSessionDto>(`${this.baseUrl}/${workoutId}/complete`, request);
  }

  abandonWorkout(workoutId: string): Observable<WorkoutSessionDto> {
    return this.http.post<WorkoutSessionDto>(`${this.baseUrl}/${workoutId}/abandon`, {});
  }
}
