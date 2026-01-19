import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { LoginRequest, LoginResponse, UserRole } from './auth.models';
import { AuthService } from '../../core/api/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private baseUrl = 'http://localhost:5078/api/auth';
  private currentRoleSubject = new BehaviorSubject<string | null>(null);
  public currentRole$ = this.currentRoleSubject.asObservable();

  constructor(private http: HttpClient, private authService: AuthService) {}

  login(username: string, role: UserRole): Observable<LoginResponse> {
    const request: LoginRequest = { username, role };
    
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, request)
      .pipe(
        tap(response => {
          if (response.success) {
            this.currentRoleSubject.next(response.role);
            localStorage.setItem('auth_token', response.token);
            
            // Update core AuthService for guard compatibility
            const user = {
              userId: response.userId,
              role: response.role as 'Patient' | 'Practitioner' | 'Organization' | 'Admin'
            };
            localStorage.setItem('currentUser', JSON.stringify(user));
            // Trigger AuthService to reload user from localStorage
            this.authService['currentUserSubject'].next(user);
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
    this.authService.logout();
  }
}