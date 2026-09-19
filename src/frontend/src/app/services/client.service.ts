import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AddConsentRequest,
  Client,
  ClientStatus,
  ClientSummary,
  ConsentRecord,
  CreateClientRequest,
  UpdateClientRequest
} from '../models/client.models';

@Injectable({
  providedIn: 'root'
})
export class ClientService {
  private readonly apiUrl = `${environment.apiUrl}/clients`;

  constructor(private http: HttpClient) {}

  public getClients(status?: ClientStatus): Observable<ClientSummary[]> {
    let params = new HttpParams();
    if (status !== undefined) {
      params = params.set('status', status.toString());
    }
    return this.http.get<ClientSummary[]>(this.apiUrl, { params });
  }

  public getClientById(id: string): Observable<Client> {
    return this.http.get<Client>(`${this.apiUrl}/${id}`);
  }

  public createClient(request: CreateClientRequest): Observable<Client> {
    return this.http.post<Client>(this.apiUrl, request);
  }

  public updateClient(id: string, request: UpdateClientRequest): Observable<Client> {
    return this.http.put<Client>(`${this.apiUrl}/${id}`, request);
  }

  public archiveClient(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  public getConsentRecords(clientId: string): Observable<ConsentRecord[]> {
    return this.http.get<ConsentRecord[]>(`${this.apiUrl}/${clientId}/consent`);
  }

  public addConsentRecord(clientId: string, request: AddConsentRequest): Observable<ConsentRecord> {
    return this.http.post<ConsentRecord>(`${this.apiUrl}/${clientId}/consent`, request);
  }
}
