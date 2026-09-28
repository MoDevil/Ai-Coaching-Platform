import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ClientMemoryRecordDto,
  CreateClientMemoryRequestDto,
  CorrectClientMemoryRequestDto,
  ClientMemoryConflictDto,
  ResolveClientMemoryConflictRequestDto,
  ClientMemorySnapshotDto,
  UnresolvedQuestionDto,
  AnonymizeClientMemoryResultDto,
  MemoryCategory
} from '../models/memory.model';

@Injectable({
  providedIn: 'root'
})
export class MemoryService {
  private readonly baseUrl = '/api/clients';

  constructor(private http: HttpClient) {}

  getMemories(clientId: string, category?: MemoryCategory, includeAnonymized: boolean = false): Observable<ClientMemoryRecordDto[]> {
    let params = new HttpParams().set('includeAnonymized', includeAnonymized.toString());
    if (category !== undefined) {
      params = params.set('category', category.toString());
    }
    return this.http.get<ClientMemoryRecordDto[]>(`${this.baseUrl}/${clientId}/memory`, { params });
  }

  getMemoryById(clientId: string, recordId: string): Observable<ClientMemoryRecordDto> {
    return this.http.get<ClientMemoryRecordDto>(`${this.baseUrl}/${clientId}/memory/${recordId}`);
  }

  getMemoryHistory(clientId: string, recordId: string): Observable<ClientMemoryRecordDto[]> {
    return this.http.get<ClientMemoryRecordDto[]>(`${this.baseUrl}/${clientId}/memory/${recordId}/history`);
  }

  createMemory(clientId: string, request: CreateClientMemoryRequestDto): Observable<ClientMemoryRecordDto> {
    return this.http.post<ClientMemoryRecordDto>(`${this.baseUrl}/${clientId}/memory`, request);
  }

  correctMemory(clientId: string, recordId: string, request: CorrectClientMemoryRequestDto): Observable<ClientMemoryRecordDto> {
    return this.http.post<ClientMemoryRecordDto>(`${this.baseUrl}/${clientId}/memory/${recordId}/correct`, request);
  }

  archiveMemory(clientId: string, recordId: string): Observable<ClientMemoryRecordDto> {
    return this.http.patch<ClientMemoryRecordDto>(`${this.baseUrl}/${clientId}/memory/${recordId}/archive`, {});
  }

  flagUncertain(clientId: string, recordId: string): Observable<ClientMemoryRecordDto> {
    return this.http.patch<ClientMemoryRecordDto>(`${this.baseUrl}/${clientId}/memory/${recordId}/flag-uncertain`, {});
  }

  getConflicts(clientId: string, unresolvedOnly: boolean = false): Observable<ClientMemoryConflictDto[]> {
    const params = new HttpParams().set('unresolvedOnly', unresolvedOnly.toString());
    return this.http.get<ClientMemoryConflictDto[]>(`${this.baseUrl}/${clientId}/memory/conflicts`, { params });
  }

  resolveConflict(clientId: string, conflictId: string, request: ResolveClientMemoryConflictRequestDto): Observable<ClientMemoryConflictDto> {
    return this.http.post<ClientMemoryConflictDto>(`${this.baseUrl}/${clientId}/memory/conflicts/${conflictId}/resolve`, request);
  }

  getSnapshot(clientId: string): Observable<ClientMemorySnapshotDto> {
    return this.http.get<ClientMemorySnapshotDto>(`${this.baseUrl}/${clientId}/memory/snapshot`);
  }

  generateSnapshot(clientId: string, trigger: number = 1): Observable<ClientMemorySnapshotDto> {
    return this.http.post<ClientMemorySnapshotDto>(`${this.baseUrl}/${clientId}/memory/snapshot/generate`, { trigger });
  }

  getUnresolvedQuestions(clientId: string): Observable<UnresolvedQuestionDto[]> {
    return this.http.get<UnresolvedQuestionDto[]>(`${this.baseUrl}/${clientId}/memory/unresolved-questions`);
  }

  anonymizeMemories(clientId: string, reason?: string): Observable<AnonymizeClientMemoryResultDto> {
    return this.http.post<AnonymizeClientMemoryResultDto>(`${this.baseUrl}/${clientId}/memory/anonymize`, { anonymizationReason: reason });
  }
}
