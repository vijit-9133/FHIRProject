export interface LoginRequest {
  username: string;
}

export interface LoginResponse {
  success: boolean;
  userId: number;
  role: string;
}

export interface User {
  userId: number;
  role: 'Patient' | 'Practitioner' | 'Organization';
}