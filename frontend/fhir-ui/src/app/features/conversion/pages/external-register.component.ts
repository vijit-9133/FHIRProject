import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ExternalSystemApiService } from '../../../core/api/external-system-api.service';
import { ExternalSystemRegistrationResponse } from '../../../shared/models/external-system.models';

@Component({
  selector: 'app-external-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container mt-5">
      <div class="row justify-content-center">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header bg-primary text-white">
              <h4 class="mb-0">External System Registration</h4>
            </div>
            <div class="card-body">
              <div *ngIf="!registrationResponse">
                <form (ngSubmit)="register()">
                  <div class="mb-3">
                    <label for="systemName" class="form-label">System Name</label>
                    <input 
                      type="text" 
                      class="form-control" 
                      id="systemName" 
                      [(ngModel)]="systemName" 
                      name="systemName"
                      required
                      placeholder="Enter your system name">
                  </div>
                  <button type="submit" class="btn btn-primary" [disabled]="loading || !systemName">
                    {{ loading ? 'Registering...' : 'Register' }}
                  </button>
                </form>
                <div *ngIf="errorMessage" class="alert alert-danger mt-3">
                  {{ errorMessage }}
                </div>
              </div>

              <div *ngIf="registrationResponse" class="alert alert-success">
                <h5>Registration Successful!</h5>
                <div class="mt-3">
                  <p><strong>System Name:</strong> {{ registrationResponse.systemName }}</p>
                  <p><strong>Client ID:</strong> <code>{{ registrationResponse.clientId }}</code></p>
                  <div class="alert alert-warning">
                    <strong>⚠️ IMPORTANT - Save this Client Secret NOW:</strong>
                    <div class="mt-2">
                      <code class="d-block p-2 bg-dark text-white">{{ registrationResponse.clientSecret }}</code>
                    </div>
                    <small class="d-block mt-2">This secret will NEVER be shown again. Store it securely.</small>
                  </div>
                  <p><strong>Status:</strong> <span class="badge bg-warning">{{ registrationResponse.status }}</span></p>
                  <p class="text-muted">Your system is pending admin approval. Check status using your Client ID.</p>
                </div>
                <a routerLink="/external/status" class="btn btn-primary mt-3">Check Status</a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ExternalRegisterComponent {
  systemName = '';
  loading = false;
  errorMessage = '';
  registrationResponse: ExternalSystemRegistrationResponse | null = null;

  constructor(private externalSystemApi: ExternalSystemApiService, private cdr: ChangeDetectorRef) {}

  register(): void {
    if (!this.systemName.trim()) return;

    this.loading = true;
    this.errorMessage = '';
    console.log('Registering system:', this.systemName);

    this.externalSystemApi.register({ systemName: this.systemName }).subscribe({
      next: (response) => {
        console.log('Registration successful:', response);
        this.registrationResponse = response;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Registration error:', error);
        this.errorMessage = error.error?.message || error.message || 'Registration failed';
        this.loading = false;
        this.cdr.detectChanges();
      },
      complete: () => {
        console.log('Registration request completed');
      }
    });
  }
}
