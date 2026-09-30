import { TestBed } from '@angular/core/testing';import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { AuthResponse, CurrentCoach, RegisterCoachRequest } from '../models/auth.models';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  const authResponse: AuthResponse = {
    token: 'jwt-token-value',
    expiresAt: '2030-01-01T00:00:00Z',
    coachId: 'c2f0a4d2-0000-0000-0000-000000000002',
    coachName: 'Captain Ahmed Hassan',
    email: 'ahmed@egyptgym.com'
  };

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, RouterTestingModule],
      providers: [AuthService]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('posts a register request to the auth register endpoint', () => {
    const request: RegisterCoachRequest = {
      fullName: 'Captain Ahmed Hassan',
      email: 'ahmed@egyptgym.com',
      password: 'Str0ng!Pass'
    };

    service.register(request).subscribe();

    const httpRequest = httpMock.expectOne(req => req.url.endsWith('/auth/register'));
    expect(httpRequest.request.method).toBe('POST');
    expect(httpRequest.request.body).toEqual(request);

    httpRequest.flush(authResponse);
  });

  it('persists the token and coach after a successful register', () => {
    service.register({ fullName: 'Captain Ahmed Hassan', email: 'ahmed@egyptgym.com', password: 'Str0ng!Pass' })
      .subscribe();

    httpMock.expectOne(req => req.url.endsWith('/auth/register')).flush(authResponse);

    expect(service.getToken()).toBe('jwt-token-value');
    expect(service.isAuthenticated()).toBeTrue();
    expect(service.currentUserValue).toEqual({
      coachId: authResponse.coachId,
      coachName: authResponse.coachName,
      email: authResponse.email
    } satisfies CurrentCoach);
  });

  it('emits the signed-in coach on currentUser$', () => {
    // Collected in an array because TypeScript narrows a plain `let` to null after the
    // subscription is registered, which would make the assertion below a type error.
    const emitted: CurrentCoach[] = [];
    service.currentUser$.subscribe(value => {
      if (value) {
        emitted.push(value);
      }
    });

    service.login({ email: 'ahmed@egyptgym.com', password: 'Str0ng!Pass' }).subscribe();
    httpMock.expectOne(req => req.url.endsWith('/auth/login')).flush(authResponse);

    expect(emitted).toEqual([
      {
        coachId: authResponse.coachId,
        coachName: authResponse.coachName,
        email: authResponse.email
      }
    ]);
  });

  it('clears the stored session on logout', () => {
    // logout() redirects to /login, which RouterTestingModule has no route for, so navigation is
    // stubbed to keep the assertion focused on the cleared session.
    const router = TestBed.inject(Router);
    const navigate = spyOn(router, 'navigate').and.resolveTo(true);

    service.login({ email: 'ahmed@egyptgym.com', password: 'Str0ng!Pass' }).subscribe();
    httpMock.expectOne(req => req.url.endsWith('/auth/login')).flush(authResponse);

    service.logout();

    expect(service.getToken()).toBeNull();
    expect(service.isAuthenticated()).toBeFalse();
    expect(service.currentUserValue).toBeNull();
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });

  it('is not authenticated when there is no stored token', () => {
    expect(service.isAuthenticated()).toBeFalse();
    expect(service.currentUserValue).toBeNull();
  });
});
