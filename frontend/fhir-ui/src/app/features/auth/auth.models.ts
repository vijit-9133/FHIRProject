export interface LoginRequest {
  username: string;
  role: UserRole;
}

export interface LoginResponse {
  success: boolean;
  userId: number;
  role: string;
  token: string;
}

export enum UserRole {
  Patient = 1,
  Practitioner = 2,
  Organization = 3
}