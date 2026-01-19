import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ExternalSystemApiService } from '../../../core/api/external-system-api.service';
import { ExternalSystemDto } from '../../../shared/models/external-system.models';

@Component({
  selector: 'app-external-status',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container mt-5">
      <div class="row justify-content-center">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header bg-info text-white">
              <h4 class="mb-0">Check System Status</h4>
            </div>
            <div class="card-body">
              <form (ngSubmit)="checkStatus()">
                <div class="mb-3">
                  <label for="clientId" class="form-label">Client ID</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    id="clientId" 
                    [(ngModel)]="clientId" 
                    name="clientId"
                    required
                    placeholder="Enter your Client ID">
                </div>
                <button type="submit" class="btn btn-info" [disabled]="loading || !clientId">
                  {{ loading ? 'Checking...' : 'Check Status' }}
                </button>
              </form>

              <div *ngIf="errorMessage" class="alert alert-danger mt-3">
                {{ errorMessage }}
              </div>

              <div *ngIf="system" class="mt-4">
                <h5>System Information</h5>
                <table class="table table-bordered">
                  <tbody>
                    <tr>
                      <th>System Name</th>
                      <td>{{ system.systemName }}</td>
                    </tr>
                    <tr>
                      <th>Client ID</th>
                      <td><code>{{ system.clientId }}</code></td>
                    </tr>
                    <tr>
                      <th>Status</th>
                      <td>
                        <span [ngClass]="getStatusBadgeClass()">
                          {{ getStatusMessage() }}
                        </span>
                      </td>
                    </tr>
                    <tr>
                      <th>Created At</th>
                      <td>{{ system.createdAt | date:'medium' }}</td>
                    </tr>
                    <tr *ngIf="system.approvedAt">
                      <th>Approved At</th>
                      <td>{{ system.approvedAt | date:'medium' }}</td>
                    </tr>
                    <tr *ngIf="system.lastAccessedAt">
                      <th>Last Accessed</th>
                      <td>{{ system.lastAccessedAt | date:'medium' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ExternalStatusComponent {
  clientId = '';
  loading = false;
  errorMessage = '';
  system: ExternalSystemDto | null = null;

  constructor(private externalSystemApi: ExternalSystemApiService) {}

  checkStatus(): void {
    if (!this.clientId.trim()) return;

    this.loading = true;
    this.errorMessage = '';
    this.system = null;

    this.externalSystemApi.getAllSystems().subscribe({
      next: (systems) => {
        const found = systems.find(s => s.clientId === this.clientId.trim());
        if (found) {
          this.system = found;
        } else {
          this.errorMessage = 'System not found with this Client ID';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to fetch system status';
        this.loading = false;
      }
    });
  }

  getStatusBadgeClass(): string {
    switch (this.system?.status) {
      case 'Active': return 'badge bg-success';
      case 'PendingApproval': return 'badge bg-warning';
      case 'Suspended': return 'badge bg-danger';
      default: return 'badge bg-secondary';
    }
  }

  getStatusMessage(): string {
    switch (this.system?.status) {
      case 'Active': return '✓ Good to go';
      case 'PendingApproval': return '⏳ Waiting for admin approval';
      case 'Suspended': return '⛔ Access suspended';
      default: return 'Unknown';
    }
  }
}
