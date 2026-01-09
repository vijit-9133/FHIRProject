import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { LoginRequest, LoginResponse, UserRole } from './auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private baseUrl = 'http://localhost:5078/api/auth';
  private currentRoleSubject = new BehaviorSubject<string | null>(null);
  public currentRole$ = this.currentRoleSubject.asObservable();

  constructor(private http: HttpClient) {}

  login(username: string, role: UserRole): Observable<LoginResponse> {
    const request: LoginRequest = { username, role };
    
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, request)
      .pipe(
        tap(response => {
          if (response.success) {
            this.currentRoleSubject.next(response.role);
            localStorage.setItem('auth_token', response.token);
          }
        })
      );
  }

  getCurrentRole(): string | null {
    return this.currentRoleSubject.value;
  }

  isLoggedIn(): boolean {
    return this.getCurrentRole() !== null;
  }

  logout(): void {
    this.currentRoleSubject.next(null);
    localStorage.removeItem('auth_token');
  }
}