import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AdminApiService } from '../../core/api/admin-api.service';
import { AuthService } from '../../core/api/auth.service';
import { ExternalSystemDto, ConversionRequestDto } from '../../shared/models/external-system.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="mb-0">Admin Dashboard</h2>
        <button class="btn btn-outline-danger" (click)="logout()">
          Logout
        </button>
      </div>

      <!-- Tab Navigation -->
      <ul class="nav nav-tabs mb-4">
        <li class="nav-item">
          <a class="nav-link" [class.active]="activeTab === 'systems'" (click)="activeTab = 'systems'" style="cursor: pointer">
            External Systems
          </a>
        </li>
        <li class="nav-item">
          <a class="nav-link" [class.active]="activeTab === 'requests'" (click)="activeTab = 'requests'" style="cursor: pointer">
            Conversion Requests
          </a>
        </li>
        <li class="nav-item">
          <a class="nav-link" [class.active]="activeTab === 'analytics'" (click)="activeTab = 'analytics'" style="cursor: pointer">
            Analytics
          </a>
        </li>
      </ul>

      <!-- External Systems Tab -->
      <div *ngIf="activeTab === 'systems'">
        <div class="card">
          <div class="card-header bg-primary text-white">
            <h4 class="mb-0">External Systems Management</h4>
          </div>
          <div class="card-body">
            <div *ngIf="loadingError" class="alert alert-danger">{{ loadingError }}</div>
            
            <div *ngIf="loadingSystems" class="text-center py-5">
              <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>

            <table *ngIf="!loadingSystems && systems.length > 0" class="table table-striped table-hover">
              <thead>
                <tr>
                  <th>System Name</th>
                  <th>Client ID</th>
                  <th>Status</th>
                  <th>Created At</th>
                  <th>Approved At</th>
                  <th>Last Accessed</th>
                  <th>Approved By</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let system of systems">
                  <td>{{ system.systemName }}</td>
                  <td><code>{{ system.clientId }}</code></td>
                  <td>
                    <span [ngClass]="getStatusBadgeClass(system.status)">
                      {{ system.status }}
                    </span>
                  </td>
                  <td>{{ system.createdAt | date:'short' }}</td>
                  <td>{{ system.approvedAt ? (system.approvedAt | date:'short') : '-' }}</td>
                  <td>{{ system.lastAccessedAt ? (system.lastAccessedAt | date:'short') : '-' }}</td>
                  <td>{{ system.approvedByUser?.username || '-' }}</td>
                  <td>
                    <button 
                      *ngIf="system.status === 'PendingApproval'" 
                      class="btn btn-sm btn-success me-2"
                      (click)="approveSystem(system.id)"
                      [disabled]="actionLoading">
                      Approve
                    </button>
                    <button 
                      *ngIf="system.status === 'PendingApproval'" 
                      class="btn btn-sm btn-danger me-2"
                      (click)="rejectSystem(system.id)"
                      [disabled]="actionLoading">
                      Reject
                    </button>
                    <button 
                      *ngIf="system.status === 'Active'" 
                      class="btn btn-sm btn-warning"
                      (click)="suspendSystem(system.id)"
                      [disabled]="actionLoading">
                      Suspend
                    </button>
                    <button 
                      *ngIf="system.status === 'Suspended'" 
                      class="btn btn-sm btn-success"
                      (click)="activateSystem(system.id)"
                      [disabled]="actionLoading">
                      Activate
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>

            <div *ngIf="!loadingSystems && systems.length === 0" class="alert alert-info">
              No external systems registered yet.
            </div>
          </div>
        </div>
      </div>

      <!-- Conversion Requests Tab -->
      <div *ngIf="activeTab === 'requests'">
        <div class="card">
          <div class="card-header bg-secondary text-white">
            <h4 class="mb-0">Conversion Requests Overview</h4>
          </div>
          <div class="card-body">
            <div *ngIf="loadingRequests" class="text-center py-5">
              <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>

            <div *ngIf="!loadingRequests && requests.length > 0">
              <p class="text-muted">Total: {{ requests.length }} requests</p>
              <div class="table-responsive">
                <table class="table table-striped table-hover">
                  <thead>
                    <tr>
                      <th>ID</th>
                      <th>Resource Type</th>
                      <th>Status</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let request of requests">
                      <td>{{ request.id }}</td>
                      <td>{{ request.resourceType }}</td>
                      <td>
                        <span [ngClass]="getRequestStatusBadgeClass(request.status)">
                          {{ request.status }}
                        </span>
                      </td>
                      <td>
                        <button class="btn btn-sm btn-primary" (click)="viewRequest(request)">
                          View
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <div *ngIf="!loadingRequests && requests.length === 0" class="alert alert-info">
              No conversion requests found.
            </div>
          </div>
        </div>
      </div>

      <!-- Modal -->
      <div class="modal fade" [class.show]="showModal" [style.display]="showModal ? 'block' : 'none'" tabindex="-1">
        <div class="modal-dialog modal-lg">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Conversion Request Details</h5>
              <button type="button" class="btn-close" (click)="closeModal()"></button>
            </div>
            <div class="modal-body" *ngIf="selectedRequest">
              <table class="table table-bordered">
                <tbody>
                  <tr>
                    <th style="width: 30%">ID</th>
                    <td>{{ selectedRequest.id }}</td>
                  </tr>
                  <tr>
                    <th>Resource Type</th>
                    <td>{{ selectedRequest.resourceType }}</td>
                  </tr>
                  <tr>
                    <th>Status</th>
                    <td>
                      <span [ngClass]="getRequestStatusBadgeClass(selectedRequest.status)">
                        {{ selectedRequest.status }}
                      </span>
                    </td>
                  </tr>
                  <tr>
                    <th>User ID</th>
                    <td>{{ selectedRequest.userId || 'N/A' }}</td>
                  </tr>
                  <tr>
                    <th>Created At</th>
                    <td>{{ selectedRequest.createdAt | date:'medium' }}</td>
                  </tr>
                  <tr *ngIf="selectedRequest.errorMessage">
                    <th>Error Message</th>
                    <td class="text-danger">{{ selectedRequest.errorMessage }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" (click)="closeModal()">Close</button>
            </div>
          </div>
        </div>
      </div>
      <div class="modal-backdrop fade" [class.show]="showModal" *ngIf="showModal"></div>

      <!-- Analytics Tab -->
      <div *ngIf="activeTab === 'analytics'">
        <div class="row">
          <!-- Summary Cards -->
          <div class="col-md-3 mb-4">
            <div class="card text-white bg-primary">
              <div class="card-body text-center">
                <h6 class="card-title">Total Systems</h6>
                <h2 class="display-4 mb-0">{{ systems.length }}</h2>
              </div>
            </div>
          </div>
          <div class="col-md-3 mb-4">
            <div class="card text-white bg-success">
              <div class="card-body text-center">
                <h6 class="card-title">Active Systems</h6>
                <h2 class="display-4 mb-0">{{ getActiveSystemsCount() }}</h2>
              </div>
            </div>
          </div>
          <div class="col-md-3 mb-4">
            <div class="card text-white bg-warning">
              <div class="card-body text-center">
                <h6 class="card-title">Pending Approval</h6>
                <h2 class="display-4 mb-0">{{ getPendingSystemsCount() }}</h2>
              </div>
            </div>
          </div>
          <div class="col-md-3 mb-4">
            <div class="card text-white bg-info">
              <div class="card-body text-center">
                <h6 class="card-title">Total Requests</h6>
                <h2 class="display-4 mb-0">{{ requests.length }}</h2>
              </div>
            </div>
          </div>
        </div>

        <!-- Charts Row -->
        <div class="row">
          <!-- System Status Distribution -->
          <div class="col-md-6 mb-4">
            <div class="card">
              <div class="card-header bg-primary text-white">
                <h5 class="mb-0">System Status Distribution</h5>
              </div>
              <div class="card-body">
                <div class="mb-3">
                  <div class="d-flex justify-content-between mb-2">
                    <span>Active</span>
                    <span class="badge bg-success">{{ getActiveSystemsCount() }}</span>
                  </div>
                  <div class="progress" style="height: 25px;">
                    <div class="progress-bar bg-success" [style.width.%]="getSystemStatusPercentage('Active')">{{ getSystemStatusPercentage('Active') }}%</div>
                  </div>
                </div>
                <div class="mb-3">
                  <div class="d-flex justify-content-between mb-2">
                    <span>Pending Approval</span>
                    <span class="badge bg-warning">{{ getPendingSystemsCount() }}</span>
                  </div>
                  <div class="progress" style="height: 25px;">
                    <div class="progress-bar bg-warning" [style.width.%]="getSystemStatusPercentage('PendingApproval')">{{ getSystemStatusPercentage('PendingApproval') }}%</div>
                  </div>
                </div>
                <div class="mb-3">
                  <div class="d-flex justify-content-between mb-2">
                    <span>Suspended</span>
                    <span class="badge bg-danger">{{ getSuspendedSystemsCount() }}</span>
                  </div>
                  <div class="progress" style="height: 25px;">
                    <div class="progress-bar bg-danger" [style.width.%]="getSystemStatusPercentage('Suspended')">{{ getSystemStatusPercentage('Suspended') }}%</div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Request Status Distribution -->
          <div class="col-md-6 mb-4">
            <div class="card">
              <div class="card-header bg-secondary text-white">
                <h5 class="mb-0">Request Status Distribution</h5>
              </div>
              <div class="card-body">
                <div class="mb-3">
                  <div class="d-flex justify-content-between mb-2">
                    <span>Success</span>
                    <span class="badge bg-success">{{ getRequestsByStatus('Success') }}</span>
                  </div>
                  <div class="progress" style="height: 25px;">
                    <div class="progress-bar bg-success" [style.width.%]="getRequestStatusPercentage('Success')">{{ getRequestStatusPercentage('Success') }}%</div>
                  </div>
                </div>
                <div class="mb-3">
                  <div class="d-flex justify-content-between mb-2">
                    <span>Failed</span>
                    <span class="badge bg-danger">{{ getRequestsByStatus('Failed') }}</span>
                  </div>
                  <div class="progress" style="height: 25px;">
                    <div class="progress-bar bg-danger" [style.width.%]="getRequestStatusPercentage('Failed')">{{ getRequestStatusPercentage('Failed') }}%</div>
                  </div>
                </div>
                <div class="mb-3">
                  <div class="d-flex justify-content-between mb-2">
                    <span>Pending</span>
                    <span class="badge bg-warning">{{ getRequestsByStatus('Pending') }}</span>
                  </div>
                  <div class="progress" style="height: 25px;">
                    <div class="progress-bar bg-warning" [style.width.%]="getRequestStatusPercentage('Pending')">{{ getRequestStatusPercentage('Pending') }}%</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Resource Type Distribution -->
        <div class="row">
          <div class="col-md-12 mb-4">
            <div class="card">
              <div class="card-header bg-info text-white">
                <h5 class="mb-0">Resource Type Distribution</h5>
              </div>
              <div class="card-body">
                <div class="row">
                  <div class="col-md-4 mb-3" *ngFor="let type of getResourceTypes()">
                    <div class="card">
                      <div class="card-body text-center">
                        <h6>{{ type.name }}</h6>
                        <h3 class="text-primary">{{ type.count }}</h3>
                        <div class="progress mt-2">
                          <div class="progress-bar bg-info" [style.width.%]="type.percentage"></div>
                        </div>
                        <small class="text-muted">{{ type.percentage }}% of total</small>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Recent Activity -->
        <div class="row">
          <div class="col-md-12">
            <div class="card">
              <div class="card-header bg-dark text-white">
                <h5 class="mb-0">Recent Activity</h5>
              </div>
              <div class="card-body">
                <div class="table-responsive">
                  <table class="table table-sm">
                    <thead>
                      <tr>
                        <th>Time</th>
                        <th>System</th>
                        <th>Status</th>
                        <th>Resource Type</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr *ngFor="let request of getRecentRequests()">
                        <td>{{ request.createdAt | date:'short' }}</td>
                        <td>User {{ request.userId }}</td>
                        <td>
                          <span [ngClass]="getRequestStatusBadgeClass(request.status)">
                            {{ request.status }}
                          </span>
                        </td>
                        <td>{{ request.resourceType }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class AdminDashboardComponent implements OnInit {
  activeTab: 'systems' | 'requests' | 'analytics' = 'systems';
  systems: ExternalSystemDto[] = [];
  requests: any[] = [];
  loadingSystems = false;
  loadingRequests = false;
  actionLoading = false;
  loadingError = '';
  showModal = false;
  selectedRequest: any = null;

  constructor(private adminApi: AdminApiService, private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadSystems();
    this.loadRequests();
    // Force initial render
    setTimeout(() => this.cdr.detectChanges(), 0);
  }

  loadSystems(): void {
    this.loadingSystems = true;
    this.loadingError = '';
    console.log('Loading systems...');
    this.adminApi.getAllSystems().subscribe({
      next: (data) => {
        console.log('Systems loaded:', data);
        this.systems = data || [];
        this.loadingSystems = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading systems:', error);
        this.loadingError = 'Failed to load external systems';
        this.loadingSystems = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadRequests(): void {
    this.loadingRequests = true;
    console.log('Loading requests...');
    this.adminApi.getAllConversionRequests().subscribe({
      next: (data) => {
        console.log('Requests loaded:', data);
        this.requests = data || [];
        this.loadingRequests = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading requests:', error);
        this.loadingRequests = false;
        this.cdr.detectChanges();
      }
    });
  }

  approveSystem(id: number): void {
    this.actionLoading = true;
    this.adminApi.approveSystem(id).subscribe({
      next: () => {
        this.loadSystems();
        this.actionLoading = false;
      },
      error: () => {
        alert('Failed to approve system');
        this.actionLoading = false;
      }
    });
  }

  suspendSystem(id: number): void {
    if (!confirm('Are you sure you want to suspend this system?')) return;
    
    this.actionLoading = true;
    this.adminApi.suspendSystem(id).subscribe({
      next: () => {
        this.loadSystems();
        this.actionLoading = false;
      },
      error: () => {
        alert('Failed to suspend system');
        this.actionLoading = false;
      }
    });
  }

  rejectSystem(id: number): void {
    if (!confirm('Are you sure you want to reject and delete this system? This cannot be undone.')) return;
    
    this.actionLoading = true;
    this.adminApi.rejectSystem(id).subscribe({
      next: () => {
        this.loadSystems();
        this.actionLoading = false;
      },
      error: () => {
        alert('Failed to reject system');
        this.actionLoading = false;
      }
    });
  }

  activateSystem(id: number): void {
    if (!confirm('Are you sure you want to activate this system?')) return;
    
    this.actionLoading = true;
    this.adminApi.activateSystem(id).subscribe({
      next: () => {
        this.loadSystems();
        this.actionLoading = false;
      },
      error: () => {
        alert('Failed to activate system');
        this.actionLoading = false;
      }
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Active': return 'badge bg-success';
      case 'PendingApproval': return 'badge bg-warning';
      case 'Suspended': return 'badge bg-danger';
      default: return 'badge bg-secondary';
    }
  }

  getRequestStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Completed': return 'badge bg-success';
      case 'Failed': return 'badge bg-danger';
      case 'InProgress': return 'badge bg-warning';
      default: return 'badge bg-secondary';
    }
  }

  getActiveSystemsCount(): number {
    return this.systems.filter(s => s.status === 'Active').length;
  }

  getPendingSystemsCount(): number {
    return this.systems.filter(s => s.status === 'PendingApproval').length;
  }

  getSuspendedSystemsCount(): number {
    return this.systems.filter(s => s.status === 'Suspended').length;
  }

  getSystemStatusPercentage(status: string): number {
    if (this.systems.length === 0) return 0;
    const count = this.systems.filter(s => s.status === status).length;
    return Math.round((count / this.systems.length) * 100);
  }

  getRequestsByStatus(status: string): number {
    return this.requests.filter(r => r.status === status).length;
  }

  getRequestStatusPercentage(status: string): number {
    if (this.requests.length === 0) return 0;
    const count = this.requests.filter(r => r.status === status).length;
    return Math.round((count / this.requests.length) * 100);
  }

  getResourceTypes(): any[] {
    const types: any = {};
    this.requests.forEach(r => {
      types[r.resourceType] = (types[r.resourceType] || 0) + 1;
    });
    
    return Object.keys(types).map(key => ({
      name: key,
      count: types[key],
      percentage: Math.round((types[key] / this.requests.length) * 100)
    }));
  }

  getRecentRequests(): any[] {
    return this.requests.slice(0, 10);
  }

  viewRequest(request: any): void {
    this.selectedRequest = request;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.selectedRequest = null;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
