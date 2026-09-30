/**
 * Mirrors the ProblemDetails and ValidationProblemDetails payloads produced by the API's
 * ExceptionHandlingMiddleware. There is no `message` field on either type, so components that
 * read `err.error.message` always fell through to a generic message and silently discarded the
 * per-field validation messages.
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

export const GENERIC_ERROR_MESSAGE = 'Something went wrong. Please try again.';

/**
 * Turns an HttpErrorResponse body into a message that is useful to a coach.
 *
 * Priority order:
 *  1. Per-field validation messages, joined, because they name the actual problem.
 *  2. `detail` / `title` for other handled errors such as 404 and 409.
 *  3. The caller's fallback when the body is not a ProblemDetails payload at all, which is the
 *     case for a 401 from the auth endpoints.
 */
export function extractErrorMessage(body: unknown, fallback: string = GENERIC_ERROR_MESSAGE): string {
  if (!body || typeof body !== 'object') {
    return fallback;
  }

  const problem = body as ProblemDetails;

  if (problem.errors) {
    const fieldMessages = Object.values(problem.errors)
      .reduce<string[]>((all, messages) => all.concat(messages ?? []), [])
      .filter(message => !!message);

    if (fieldMessages.length > 0) {
      return fieldMessages.join(' ');
    }
  }

  if (problem.detail) {
    return problem.detail;
  }

  if (problem.title) {
    return problem.title;
  }

  return fallback;
}
