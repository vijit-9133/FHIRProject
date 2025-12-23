import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthApiService } from '../auth/auth-api.service';

@Component({
  selector: 'app-patient-dashboard',
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Patient Dashboard</h2>
        <button class="btn btn-outline-secondary" (click)="logout()">Logout</button>
      </div>
      
      <div class="row">
        <div class="col-md-8">
          <div class="card">
            <div class="card-header bg-info text-white">
              <h5 class="mb-0">👤 Patient Role</h5>
            </div>
            <div class="card-body">
              <h6>Allowed Actions:</h6>
              <ul class="list-unstyled">
                <li>✓ Convert personal health data to FHIR Patient resources</li>
                <li>✓ View own conversion history</li>
                <li>✓ Access personal FHIR records</li>
                <li>✗ Cannot access other patients' data</li>
                <li>✗ Cannot create Practitioner or Organization resources</li>
              </ul>
              
              <div class="mt-3">
                <a routerLink="/convert" class="btn btn-primary me-2">Convert Data</a>
                <a routerLink="/history" class="btn btn-outline-primary">View History</a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PatientDashboardComponent {
  constructor(private authService: AuthApiService) {}

  logout(): void {
    this.authService.logout();
    window.location.href = '/login';
  }
}