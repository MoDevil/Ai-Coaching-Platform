import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  GymProfileDto,
  GymEquipmentItemDto,
  CreateGymProfileRequestDto,
  UpdateGymProfileRequestDto,
  AssignClientGymRequestDto
} from '../models/gym.models';

@Injectable({
  providedIn: 'root'
})
export class GymService {
  private readonly baseUrl = '/api/gyms';

  constructor(private http: HttpClient) {}

  getGyms(): Observable<GymProfileDto[]> {
    return this.http.get<GymProfileDto[]>(this.baseUrl);
  }

  getGym(id: string): Observable<GymProfileDto> {
    return this.http.get<GymProfileDto>(`${this.baseUrl}/${id}`);
  }

  createGym(request: CreateGymProfileRequestDto): Observable<GymProfileDto> {
    return this.http.post<GymProfileDto>(this.baseUrl, request);
  }

  updateGym(id: string, request: UpdateGymProfileRequestDto): Observable<GymProfileDto> {
    return this.http.put<GymProfileDto>(`${this.baseUrl}/${id}`, request);
  }

  deleteGym(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getAvailableEquipment(): Observable<GymEquipmentItemDto[]> {
    return this.http.get<GymEquipmentItemDto[]>(`${this.baseUrl}/equipment-options`);
  }

  assignClientGym(clientId: string, gymProfileId?: string | null): Observable<void> {
    const request: AssignClientGymRequestDto = { gymProfileId };
    return this.http.post<void>(`${this.baseUrl}/assign-client/${clientId}`, request);
  }
}
