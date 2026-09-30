import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../services/auth.service';
import { AuthResponse } from '../../models/auth.models';

describe('RegisterComponent', () => {
  let fixture: ComponentFixture<RegisterComponent>;
  let component: RegisterComponent;
  let httpMock: HttpTestingController;

  const authResponse: AuthResponse = {
    token: 'test-jwt',
    expiresAt: '2030-01-01T00:00:00Z',
    coachId: 'c2f0a4d2-0000-0000-0000-000000000001',
    coachName: 'Captain Ahmed Hassan',
    email: 'ahmed@egyptgym.com'
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [RegisterComponent],
      imports: [ReactiveFormsModule, HttpClientTestingModule, RouterTestingModule],
      providers: [AuthService]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);

    // A successful registration redirects to /clients, which RouterTestingModule has no route
    // for. Left unstubbed the rejected navigation becomes an unhandled promise rejection and
    // Karma reports a disconnect instead of a clean run.
    spyOn(TestBed.inject(Router), 'navigate').and.resolveTo(true);

    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  function fillValidForm(): void {
    component.registerForm.setValue({
      fullName: 'Captain Ahmed Hassan',
      email: 'ahmed@egyptgym.com',
      password: 'Str0ng!Pass'
    });
  }

  it('exposes a fullName control rather than name', () => {
    // The control name is the wire contract. When the form used `name` the API bound FullName to
    // null and every registration was rejected, so this asserts the rename directly.
    expect(component.registerForm.contains('fullName')).toBeTrue();
    expect(component.registerForm.contains('name')).toBeFalse();
  });

  it('sends fullName to the register endpoint on submit', () => {
    fillValidForm();
    component.onSubmit();

    const request = httpMock.expectOne(req => req.url.endsWith('/auth/register'));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      fullName: 'Captain Ahmed Hassan',
      email: 'ahmed@egyptgym.com',
      password: 'Str0ng!Pass'
    });

    request.flush(authResponse);
  });

  it('never sends a name property', () => {
    fillValidForm();
    component.onSubmit();

    const request = httpMock.expectOne(req => req.url.endsWith('/auth/register'));
    expect(Object.keys(request.request.body)).not.toContain('name');
    expect(request.request.body.fullName).toBe('Captain Ahmed Hassan');

    request.flush(authResponse);
  });

  it('does not call the API when the form is invalid', () => {
    component.registerForm.setValue({
      fullName: '',
      email: 'not-an-email',
      password: 'weak'
    });
    component.onSubmit();

    httpMock.expectNone(req => req.url.endsWith('/auth/register'));
    expect(component.registerForm.touched).toBeTrue();
  });

  it('reports a weak password without contacting the API', () => {
    component.registerForm.setValue({
      fullName: 'Captain Ahmed Hassan',
      email: 'ahmed@egyptgym.com',
      password: 'alllowercase1!'
    });
    component.onSubmit();

    httpMock.expectNone(req => req.url.endsWith('/auth/register'));
  });

  it('accepts a password meeting the backend policy', () => {
    component.registerForm.setValue({
      fullName: 'Captain Ahmed Hassan',
      email: 'ahmed@egyptgym.com',
      password: 'Str0ng!Pass'
    });

    expect(component.registerForm.valid).toBeTrue();
  });

  it('surfaces the per-field messages from a 400 response', () => {
    // Guards the error path: the component used to read err.error.message, which ProblemDetails
    // does not have, so a rejected registration only ever showed the generic message.
    fillValidForm();
    component.onSubmit();

    const request = httpMock.expectOne(req => req.url.endsWith('/auth/register'));
    request.flush(
      {
        type: 'https://tools.ietf.org/html/rfc7231#section-6.5.1',
        title: 'Validation Error',
        status: 400,
        errors: { Email: ['A valid email address is required.'] }
      },
      { status: 400, statusText: 'Bad Request' }
    );

    expect(component.errorMessage).toBe('A valid email address is required.');
    expect(component.isLoading).toBeFalse();
  });

  it('lists the unmet password requirements for inline guidance', () => {
    component.registerForm.get('password')!.setValue('abcdefgh');

    expect(component.passwordHints).toEqual(['an uppercase letter', 'a digit', 'a symbol']);
  });
});
