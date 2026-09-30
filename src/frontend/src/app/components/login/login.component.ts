import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { extractErrorMessage } from '../../core/api-error';
import { LoginRequest } from '../../models/auth.models';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      // No length rule here. The password policy applies at registration, and a minimum length
      // here would only produce a misleading client-side error for a wrong password.
      password: ['', [Validators.required]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const request: LoginRequest = this.loginForm.getRawValue();

    this.authService.login(request).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/clients']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = extractErrorMessage(
          err?.error,
          'Invalid email or password. Please try again.'
        );
      }
    });
  }
}
