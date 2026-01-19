import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthApiService } from './auth-api.service';
import { UserRole } from './auth.models';
import { ExternalSystemApiService } from '../../core/api/external-system-api.service';
import { ExternalSystemDto } from '../../shared/models/external-system.models';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
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
              <div class="form-check">
                <input class="form-check-input" type="radio" formControlName="role" [value]="UserRole.Admin" id="admin">
                <label class="form-check-label" for="admin">
                  🔐 Admin
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
            <button class="btn btn-link btn-sm" (click)="quickLogin('hospital1', UserRole.Organization)">hospital1</button> |
            <button class="btn btn-link btn-sm" (click)="quickLogin('admin', UserRole.Admin)">admin</button>
          </div>

          <hr>

          <div class="text-center">
            <p class="text-muted mb-2">External System?</p>
            <button class="btn btn-outline-secondary btn-sm me-2" (click)="goToExternalSystem()">
              🔗 Register External System
            </button>
            <button class="btn btn-outline-info btn-sm" (click)="showStatusCheck = !showStatusCheck">
              📊 Check Status
            </button>
          </div>

          <div *ngIf="showStatusCheck" class="mt-3">
            <div class="input-group">
              <input 
                type="text" 
                class="form-control" 
                [(ngModel)]="clientIdToCheck" 
                placeholder="Enter Client ID">
              <button 
                class="btn btn-info" 
                (click)="checkStatus()" 
                [disabled]="!clientIdToCheck || checkingStatus">
                {{ checkingStatus ? 'Checking...' : 'Check' }}
              </button>
            </div>
            
            <div *ngIf="statusResult" class="alert mt-2" [ngClass]="{
              'alert-success': statusResult.status === 'Active',
              'alert-warning': statusResult.status === 'PendingApproval',
              'alert-danger': statusResult.status === 'Suspended'
            }">
              <strong>{{ statusResult.systemName }}</strong><br>
              <span class="badge" [ngClass]="{
                'bg-success': statusResult.status === 'Active',
                'bg-warning': statusResult.status === 'PendingApproval',
                'bg-danger': statusResult.status === 'Suspended'
              }">{{ statusResult.status }}</span>
              <p class="mt-2 mb-0">
                <span *ngIf="statusResult.status === 'Active'">✅ Your system is approved and ready to use!</span>
                <span *ngIf="statusResult.status === 'PendingApproval'">⏳ Your system is pending admin approval. Please wait.</span>
                <span *ngIf="statusResult.status === 'Suspended'">🚫 Your system has been suspended. Contact admin.</span>
              </p>
            </div>
            
            <div *ngIf="statusError" class="alert alert-danger mt-2">
              {{ statusError }}
            </div>
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
  showStatusCheck = false;
  clientIdToCheck = '';
  checkingStatus = false;
  statusResult: ExternalSystemDto | null = null;
  statusError = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthApiService,
    private router: Router,
    private externalSystemApi: ExternalSystemApiService,
    private cdr: ChangeDetectorRef
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
      case 'Admin':
        this.router.navigate(['/admin/dashboard']);
        break;
    }
  }

  goToExternalSystem(): void {
    this.router.navigate(['/external/register']);
  }

  checkStatus(): void {
    if (!this.clientIdToCheck.trim()) return;

    this.checkingStatus = true;
    this.statusError = '';
    this.statusResult = null;

    console.log('Checking status for:', this.clientIdToCheck);

    this.externalSystemApi.getSystemStatus(this.clientIdToCheck).subscribe({
      next: (result: ExternalSystemDto) => {
        console.log('Status result:', result);
        this.statusResult = result;
        this.checkingStatus = false;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Status error:', error);
        this.statusError = error.error?.message || 'System not found';
        this.checkingStatus = false;
        this.cdr.detectChanges();
      }
    });
  }
}