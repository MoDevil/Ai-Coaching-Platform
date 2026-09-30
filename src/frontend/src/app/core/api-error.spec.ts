import { extractErrorMessage, GENERIC_ERROR_MESSAGE } from './api-error';

describe('extractErrorMessage', () => {
  it('joins the per-field messages from a ValidationProblemDetails body', () => {
    // This is the case the components previously got wrong: ProblemDetails has no `message`
    // field, so reading err.error.message always fell through to the generic string and the
    // coach never saw which field was rejected.
    const body = {
      type: 'https://tools.ietf.org/html/rfc7231#section-6.5.1',
      title: 'Validation Error',
      status: 400,
      errors: {
        FullName: ['Full name is required.'],
        Email: ['A valid email address is required.']
      }
    };

    expect(extractErrorMessage(body)).toBe(
      'Full name is required. A valid email address is required.'
    );
  });

  it('prefers the field messages over the generic title', () => {
    const body = { title: 'Validation Error', status: 400, errors: { Rir: ['RIR must be between 0 and 10.'] } };

    expect(extractErrorMessage(body)).toBe('RIR must be between 0 and 10.');
  });

  it('falls back to detail when there are no field errors', () => {
    const body = { title: 'Not Found', status: 404, detail: 'Client not found.' };

    expect(extractErrorMessage(body)).toBe('Client not found.');
  });

  it('falls back to title when detail is absent', () => {
    expect(extractErrorMessage({ title: 'Conflict', status: 409 })).toBe('Conflict');
  });

  it('returns the supplied fallback for an unrecognised body', () => {
    expect(extractErrorMessage(undefined, 'Invalid email or password. Please try again.'))
      .toBe('Invalid email or password. Please try again.');
  });

  it('returns the supplied fallback for a non-object body', () => {
    expect(extractErrorMessage('boom', 'Please try again.')).toBe('Please try again.');
  });

  it('ignores an empty errors object and uses the title', () => {
    expect(extractErrorMessage({ title: 'Validation Error', status: 400, errors: {} }))
      .toBe('Validation Error');
  });

  it('uses the default message when no fallback is supplied', () => {
    expect(extractErrorMessage(null)).toBe(GENERIC_ERROR_MESSAGE);
  });
});
