import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthApiService } from '../auth/auth-api.service';
import { FhirApiService } from '../../core/api/fhir-api.service';
import { PrettyJsonPipe } from '../../shared/pipes/pretty-json.pipe';
import { FhirResourceType, PractitionerData } from '../../core/api/api.models';

@Component({
  selector: 'app-doctor-dashboard',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, PrettyJsonPipe],
  template: `
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>🩺 Practitioner Dashboard</h2>
        <button class="btn btn-outline-secondary" (click)="logout()">Logout</button>
      </div>
      
      <div class="row">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header bg-success text-white">
              <h5 class="mb-0">Create FHIR Practitioner Resource</h5>
            </div>
            <div class="card-body">
              <form [formGroup]="practitionerForm" (ngSubmit)="onSubmit()">
                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">First Name *</label>
                    <input type="text" class="form-control" formControlName="firstName">
                    <div class="text-danger" *ngIf="practitionerForm.get('firstName')?.invalid && practitionerForm.get('firstName')?.touched">
                      First name is required
                    </div>
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Last Name *</label>
                    <input type="text" class="form-control" formControlName="lastName">
                    <div class="text-danger" *ngIf="practitionerForm.get('lastName')?.invalid && practitionerForm.get('lastName')?.touched">
                      Last name is required
                    </div>
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Gender *</label>
                    <select class="form-select" formControlName="gender">
                      <option value="">Select Gender</option>
                      <option value="male">Male</option>
                      <option value="female">Female</option>
                      <option value="other">Other</option>
                    </select>
                    <div class="text-danger" *ngIf="practitionerForm.get('gender')?.invalid && practitionerForm.get('gender')?.touched">
                      Gender is required
                    </div>
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">License Number *</label>
                    <input type="text" class="form-control" formControlName="licenseNumber">
                    <div class="text-danger" *ngIf="practitionerForm.get('licenseNumber')?.invalid && practitionerForm.get('licenseNumber')?.touched">
                      License number is required
                    </div>
                  </div>
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Qualification *</label>
                    <input type="text" class="form-control" formControlName="qualification" placeholder="e.g., MBBS, MD">
                    <div class="text-danger" *ngIf="practitionerForm.get('qualification')?.invalid && practitionerForm.get('qualification')?.touched">
                      Qualification is required
                    </div>
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Speciality *</label>
                    <input type="text" class="form-control" formControlName="speciality" placeholder="e.g., Cardiology">
                    <div class="text-danger" *ngIf="practitionerForm.get('speciality')?.invalid && practitionerForm.get('speciality')?.touched">
                      Speciality is required
                    </div>
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
                  <label class="form-label">Organization Name</label>
                  <input type="text" class="form-control" formControlName="organizationName" placeholder="e.g., City General Hospital">
                </div>

                <button type="submit" class="btn btn-success" [disabled]="practitionerForm.invalid || isLoading">
                  <span *ngIf="isLoading" class="spinner-border spinner-border-sm me-2"></span>
                  Convert to FHIR Practitioner
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
                <a routerLink="/convert" class="btn btn-outline-primary me-2">Convert Patient Data</a>
                <a routerLink="/history" class="btn btn-outline-secondary">View History</a>
              </div>
            </div>
          </div>
        </div>

        <div class="col-md-6" *ngIf="conversionResult">
          <div class="card">
            <div class="card-header" [ngClass]="conversionResult.success ? 'bg-success text-white' : 'bg-danger text-white'">
              <h5 class="mb-0">
                {{ conversionResult.success ? '✅ FHIR Practitioner Generated' : '❌ Conversion Failed' }}
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
export class DoctorDashboardComponent {
  practitionerForm: FormGroup;
  conversionResult: any = null;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthApiService,
    private fhirApiService: FhirApiService,
    private cdr: ChangeDetectorRef
  ) {
    this.practitionerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      gender: ['', Validators.required],
      qualification: ['', Validators.required],
      speciality: ['', Validators.required],
      licenseNumber: ['', Validators.required],
      phoneNumber: [''],
      email: [''],
      organizationName: ['']
    });
  }

  onSubmit(): void {
    if (this.practitionerForm.valid) {
      this.isLoading = true;
      this.conversionResult = null;

      const practitionerData: PractitionerData = this.practitionerForm.value;
      const request = {
        resourceType: FhirResourceType.Practitioner,
        data: practitionerData
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