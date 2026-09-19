export interface RegisterCoachRequest {
  name: string;
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
