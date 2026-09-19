import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Program,
  ProgramSummary,
  GenerateProgramRequest,
  UpdateProgramStatusRequest
} from '../models/program.models';

@Injectable({
  providedIn: 'root'
})
export class ProgramService {
  private readonly baseUrl = '/api/programs';

  constructor(private http: HttpClient) {}

  generateProgram(request: GenerateProgramRequest): Observable<Program> {
    return this.http.post<Program>(`${this.baseUrl}/generate`, request);
  }

  getProgramById(id: string): Observable<Program> {
    return this.http.get<Program>(`${this.baseUrl}/${id}`);
  }

  getProgramsByClientId(clientId: string): Observable<ProgramSummary[]> {
    return this.http.get<ProgramSummary[]>(`${this.baseUrl}/client/${clientId}`);
  }

  getActiveProgramForClient(clientId: string): Observable<Program> {
    return this.http.get<Program>(`${this.baseUrl}/client/${clientId}/active`);
  }

  updateProgramStatus(id: string, request: UpdateProgramStatusRequest): Observable<Program> {
    return this.http.put<Program>(`${this.baseUrl}/${id}/status`, request);
  }
}
