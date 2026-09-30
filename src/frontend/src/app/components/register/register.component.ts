import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { extractErrorMessage } from '../../core/api-error';
import { describePasswordFailures, passwordPolicyValidator } from '../../core/password-policy';
import { RegisterCoachRequest } from '../../models/auth.models';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      // Control names are the wire contract. `name` was silently accepted here while the API
      // expects `fullName`, so the request bound FullName to null and always failed.
      fullName: ['', [Validators.required, Validators.maxLength(150)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      password: ['', [Validators.required, passwordPolicyValidator()]]
    });
  }

  /** Password requirements the coach has not met yet, for inline guidance. */
  get passwordHints(): string[] {
    return describePasswordFailures(this.registerForm.get('password')?.errors ?? null);
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // getRawValue() is typed, unlike `value`, so renaming a control breaks the build instead of
    // producing a request the API rejects.
    const request: RegisterCoachRequest = this.registerForm.getRawValue();

    this.authService.register(request).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/clients']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = extractErrorMessage(
          err?.error,
          'Registration failed. Please check your information.'
        );
      }
    });
  }
}
