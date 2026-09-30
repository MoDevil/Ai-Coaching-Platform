export interface RegisterCoachRequest {
  // Must stay in step with RegisterCoachRequestDto.FullName on the API. ASP.NET serialises the
  // record with camelCase naming, so the wire property is `fullName`; sending `name` left the
  // field null on the server and registration was rejected.
  fullName: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  coachId: string;
  coachName: string;
  email: string;
}

export interface CurrentCoach {
  coachId: string;
  coachName: string;
  email: string;
}
