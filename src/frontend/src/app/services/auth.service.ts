import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthResponse, CurrentCoach, LoginRequest, RegisterCoachRequest } from '../models/auth.models';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly tokenKey = 'aicoach_jwt';
  private readonly coachKey = 'aicoach_current_coach';

  private currentUserSubject = new BehaviorSubject<CurrentCoach | null>(this.getStoredCoach());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  public get currentUserValue(): CurrentCoach | null {
    return this.currentUserSubject.value;
  }

  public getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  public isAuthenticated(): boolean {
    return !!this.getToken();
  }

  public register(request: RegisterCoachRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  public login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  public logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.coachKey);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  private handleAuthSuccess(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    const coach: CurrentCoach = {
      coachId: response.coachId,
      coachName: response.coachName,
      email: response.email
    };
    localStorage.setItem(this.coachKey, JSON.stringify(coach));
    this.currentUserSubject.next(coach);
  }

  private getStoredCoach(): CurrentCoach | null {
    const json = localStorage.getItem(this.coachKey);
    if (!json) return null;
    try {
      return JSON.parse(json) as CurrentCoach;
    } catch {
      return null;
    }
  }
}
