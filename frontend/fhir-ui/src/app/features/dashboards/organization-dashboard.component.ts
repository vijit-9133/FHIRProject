import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthApiService } from '../auth/auth-api.service';
import { FhirApiService } from '../../core/api/fhir-api.service';
import { PrettyJsonPipe } from '../../shared/pipes/pretty-json.pipe';
import { FhirResourceType, OrganizationData } from '../../core/api/api.models';

@Component({
  selector: 'app-organization-dashboard',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, PrettyJsonPipe],
  template: `
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>🏥 Organization Dashboard</h2>
        <button class="btn btn-outline-secondary" (click)="logout()">Logout</button>
      </div>
      
      <div class="row">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header bg-primary text-white">
              <h5 class="mb-0">Create FHIR Organization Resource</h5>
            </div>
            <div class="card-body">
              <form [formGroup]="organizationForm" (ngSubmit)="onSubmit()">
                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Organization Name *</label>
                    <input type="text" class="form-control" formControlName="name">
                    <div class="text-danger" *ngIf="organizationForm.get('name')?.invalid && organizationForm.get('name')?.touched">
                      Organization name is required
                    </div>
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Organization Type *</label>
                    <select class="form-select" formControlName="type">
                      <option value="">Select Type</option>
                      <option value="Hospital">Hospital</option>
                      <option value="Clinic">Clinic</option>
                      <option value="DiagnosticCenter">Diagnostic Center</option>
                      <option value="Laboratory">Laboratory</option>
                    </select>
                    <div class="text-danger" *ngIf="organizationForm.get('type')?.invalid && organizationForm.get('type')?.touched">
                      Organization type is required
                    </div>
                  </div>
                </div>

                <div class="mb-3">
                  <label class="form-label">Registration Number *</label>
                  <input type="text" class="form-control" formControlName="registrationNumber">
                  <div class="text-danger" *ngIf="organizationForm.get('registrationNumber')?.invalid && organizationForm.get('registrationNumber')?.touched">
                    Registration number is required
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Phone Number</label>
                    <input type="tel" class="form-control" formControlName="phoneNumber">
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Email</label>
                    <input type="email" class="form-control" formControlName="email">
                  </div>
                </div>

                <div class="mb-3">
                  <label class="form-label">Address Line</label>
                  <input type="text" class="form-control" formControlName="addressLine">
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">City</label>
                    <input type="text" class="form-control" formControlName="city">
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">State</label>
                    <input type="text" class="form-control" formControlName="state">
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Postal Code</label>
                    <input type="text" class="form-control" formControlName="postalCode">
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Country</label>
                    <input type="text" class="form-control" formControlName="country">
                  </div>
                </div>

                <button type="submit" class="btn btn-primary" [disabled]="organizationForm.invalid || isLoading">
                  <span *ngIf="isLoading" class="spinner-border spinner-border-sm me-2"></span>
                  Convert to FHIR Organization
                </button>
              </form>
            </div>
          </div>

          <div class="mt-3">
            <div class="card">
              <div class="card-header bg-info text-white">
                <h6 class="mb-0">Quick Actions</h6>
              </div>
              <div class="card-body">
                <a routerLink="/conversion" class="btn btn-success me-2">🔄 Create FHIR Resource</a>
                <a routerLink="/history" class="btn btn-outline-secondary">📋 View History</a>
              </div>
            </div>
          </div>
        </div>

        <div class="col-md-6" *ngIf="conversionResult">
          <div class="card">
            <div class="card-header" [ngClass]="conversionResult.success ? 'bg-success text-white' : 'bg-danger text-white'">
              <h5 class="mb-0">
                {{ conversionResult.success ? '✅ FHIR Organization Generated' : '❌ Conversion Failed' }}
              </h5>
            </div>
            <div class="card-body">
              <div *ngIf="conversionResult.success" class="mb-3">
                <strong>Conversion ID:</strong> {{ conversionResult.id }}
              </div>
              <div class="mb-3">
                <strong>Message:</strong> {{ conversionResult.message }}
              </div>
              <div *ngIf="conversionResult.fhirResource">
                <strong>Generated FHIR JSON:</strong>
                <pre class="bg-light p-3 mt-2 border rounded" style="max-height: 400px; overflow-y: auto;">{{ conversionResult.fhirResource | prettyJson }}</pre>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class OrganizationDashboardComponent {
  organizationForm: FormGroup;
  conversionResult: any = null;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthApiService,
    private fhirApiService: FhirApiService,
    private cdr: ChangeDetectorRef
  ) {
    this.organizationForm = this.fb.group({
      name: ['', Validators.required],
      type: ['', Validators.required],
      registrationNumber: ['', Validators.required],
      phoneNumber: [''],
      email: [''],
      addressLine: [''],
      city: [''],
      state: [''],
      postalCode: [''],
      country: ['']
    });
  }

  onSubmit(): void {
    if (this.organizationForm.valid) {
      this.isLoading = true;
      this.conversionResult = null;

      const organizationData: OrganizationData = this.organizationForm.value;
      const request = {
        resourceType: FhirResourceType.Organization,
        data: organizationData
      };

      this.fhirApiService.convertToFhir(request).subscribe({
        next: (response) => {
          this.conversionResult = response;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.conversionResult = {
            success: false,
            message: 'Conversion failed: ' + error.message
          };
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    }
  }

  logout(): void {
    this.authService.logout();
    window.location.href = '/login';
  }
}