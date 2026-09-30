import { FormControl } from '@angular/forms';
import {
  PASSWORD_MIN_LENGTH,
  describePasswordFailures,
  passwordPolicyValidator
} from './password-policy';

describe('passwordPolicyValidator', () => {
  const validate = (value: string) => passwordPolicyValidator()(new FormControl(value));

  it('accepts a password meeting every requirement', () => {
    expect(validate('Str0ng!Pass')).toBeNull();
  });

  it('rejects a password shorter than the minimum', () => {
    const errors = validate('Ab1!xy'); // 6 characters

    expect(errors).not.toBeNull();
    expect(errors!['passwordMinLength']).toBeTrue();
  });

  it('rejects a password with no uppercase letter', () => {
    expect(validate('str0ng!pass')!['passwordUppercase']).toBeTrue();
  });

  it('rejects a password with no lowercase letter', () => {
    expect(validate('STR0NG!PASS')!['passwordLowercase']).toBeTrue();
  });

  it('rejects a password with no digit', () => {
    expect(validate('Strong!Pass')!['passwordDigit']).toBeTrue();
  });

  it('rejects a password with no non-alphanumeric character', () => {
    expect(validate('Str0ngPass1')!['passwordSymbol']).toBeTrue();
  });

  it('leaves an empty value to Validators.required', () => {
    // Returning null here avoids reporting every policy failure on a pristine empty field.
    expect(validate('')).toBeNull();
  });

  it('reports a password of exactly the minimum length as valid', () => {
    expect(PASSWORD_MIN_LENGTH).toBe(8);
    expect(validate('Abcdef1!')).toBeNull();
  });
});

describe('describePasswordFailures', () => {
  it('returns no messages for a valid password', () => {
    expect(describePasswordFailures(null)).toEqual([]);
  });

  it('describes every unmet requirement', () => {
    // Four characters with a digit, so three separate rules fail at once.
    const errors = passwordPolicyValidator()(new FormControl('abc1'));

    expect(describePasswordFailures(errors)).toEqual([
      `at least ${PASSWORD_MIN_LENGTH} characters`,
      'an uppercase letter',
      'a symbol'
    ]);
  });
});
