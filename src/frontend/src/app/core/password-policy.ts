import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Mirrors the Identity password policy configured in
 * AiCoachOs.Infrastructure/DependencyInjection.cs and the rules in RegisterCoachRequestValidator.
 *
 * The API is the authority; this exists purely so a coach sees which requirement failed before a
 * round trip, instead of a bare 400. Keep the three definitions in step.
 */
export const PASSWORD_MIN_LENGTH = 8;

export function passwordPolicyValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (typeof value !== 'string' || value.length === 0) {
      // An empty value is the concern of Validators.required, not of the policy.
      return null;
    }

    const failures: Record<string, boolean> = {};

    if (value.length < PASSWORD_MIN_LENGTH) {
      failures['passwordMinLength'] = true;
    }
    if (!/[A-Z]/.test(value)) {
      failures['passwordUppercase'] = true;
    }
    if (!/[a-z]/.test(value)) {
      failures['passwordLowercase'] = true;
    }
    if (!/[0-9]/.test(value)) {
      failures['passwordDigit'] = true;
    }
    if (!/[^a-zA-Z0-9]/.test(value)) {
      failures['passwordSymbol'] = true;
    }

    return Object.keys(failures).length > 0 ? failures : null;
  };
}

/** Describes every unmet password requirement so the UI can list them all at once. */
export function describePasswordFailures(error: ValidationErrors | null): string[] {
  if (!error) {
    return [];
  }

  const messages: string[] = [];

  if (error['passwordMinLength']) {
    messages.push(`at least ${PASSWORD_MIN_LENGTH} characters`);
  }
  if (error['passwordUppercase']) {
    messages.push('an uppercase letter');
  }
  if (error['passwordLowercase']) {
    messages.push('a lowercase letter');
  }
  if (error['passwordDigit']) {
    messages.push('a digit');
  }
  if (error['passwordSymbol']) {
    messages.push('a symbol');
  }

  return messages;
}
