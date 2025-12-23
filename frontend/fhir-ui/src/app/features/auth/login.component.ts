import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthApiService } from './auth-api.service';
import { UserRole } from './auth.models';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container-fluid vh-100 d-flex align-items-center justify-content-center bg-light">
      <div class="card shadow" style="width: 450px;">
        <div class="card-header bg-primary text-white text-center">
          <h4 class="mb-0">FHIR Data Converter</h4>
          <small>Role-Based Login</small>
        </div>
        <div class="card-body">
          <form [formGroup]="loginForm" (ngSubmit)="onLogin()">
            <div class="mb-3">
              <label class="form-label">Username</label>
              <input 
                type="text" 
                formControlName="username" 
                class="form-control"
                placeholder="Enter username">
            </div>
            
            <div class="mb-3">
              <label class="form-label">Role</label>
              <div class="form-check">
                <input class="form-check-input" type="radio" formControlName="role" [value]="UserRole.Patient" id="patient">
                <label class="form-check-label" for="patient">
                  👤 Patient
                </label>
              </div>
              <div class="form-check">
                <input class="form-check-input" type="radio" formControlName="role" [value]="UserRole.Practitioner" id="practitioner">
                <label class="form-check-label" for="practitioner">
                  🩺 Practitioner
                </label>
              </div>
              <div class="form-check">
                <input class="form-check-input" type="radio" formControlName="role" [value]="UserRole.Organization" id="organization">
                <label class="form-check-label" for="organization">
                  🏥 Organization
                </label>
              </div>
            </div>
            
            <button 
              type="submit" 
              class="btn btn-primary w-100"
              [disabled]="loginForm.invalid">
              Login
            </button>
          </form>

          <div *ngIf="errorMessage" class="alert alert-danger mt-3">
            {{ errorMessage }}
          </div>

          <hr>
          
          <div class="text-center">
            <small class="text-muted">Quick Demo:</small><br>
            <button class="btn btn-link btn-sm" (click)="quickLogin('patient1', UserRole.Patient)">patient1</button> |
            <button class="btn btn-link btn-sm" (click)="quickLogin('doctor1', UserRole.Practitioner)">doctor1</button> |
            <button class="btn btn-link btn-sm" (click)="quickLogin('hospital1', UserRole.Organization)">hospital1</button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  loginForm: FormGroup;
  errorMessage = '';
  UserRole = UserRole;

  constructor(
    private fb: FormBuilder,
    private authService: AuthApiService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      role: [null, Validators.required]
    });
  }

  onLogin(): void {
    if (this.loginForm.valid) {
      const { username, role } = this.loginForm.value;
      this.performLogin(username, role);
    }
  }

  quickLogin(username: string, role: UserRole): void {
    this.loginForm.patchValue({ username, role });
    this.performLogin(username, role);
  }

  private performLogin(username: string, role: UserRole): void {
    this.errorMessage = '';

    this.authService.login(username, role).subscribe({
      next: (response) => {
        this.routeByRole(response.role);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Login failed';
      }
    });
  }

  private routeByRole(role: string): void {
    switch (role) {
      case 'Patient':
        this.router.navigate(['/patient/dashboard']);
        break;
      case 'Practitioner':
        this.router.navigate(['/doctor/dashboard']);
        break;
      case 'Organization':
        this.router.navigate(['/organization/dashboard']);
        break;
    }
  }
}