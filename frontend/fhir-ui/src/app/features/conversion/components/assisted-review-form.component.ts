import { Component, Input, OnChanges, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PatientData, PractitionerData, OrganizationData, FhirResourceType, ConvertToFhirRequest, ConvertToFhirResponse } from '../../../core/api/api.models';
import { FhirApiService } from '../../../core/api/fhir-api.service';

@Component({
  selector: 'app-assisted-review-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h5 class="card-title mb-0">Review Extracted Data</h5>
      </div>
      <div class="card-body">
        <!-- Overall Confidence and Warnings -->
        <div class="mb-4">
          <div class="d-flex justify-content-between align-items-center mb-2">
            <span class="fw-bold">Overall Confidence:</span>
            <span [class]="getOverallConfidenceClass()" class="fw-bold">
              {{ (overallConfidence * 100) | number:'1.1-1' }}%
            </span>
          </div>
          
          <div *ngIf="extractionWarnings.length > 0" class="alert alert-warning">
            <strong><i class="bi bi-exclamation-triangle"></i> Extraction Warnings:</strong>
            <ul class="mb-0 mt-2">
              <li *ngFor="let warning of extractionWarnings">{{ warning }}</li>
            </ul>
          </div>
        </div>

        <form [formGroup]="reviewForm">
          <!-- Patient Form -->
          <div *ngIf="resourceType === FhirResourceType.Patient">
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                First Name
                <span [class]="getConfidenceClass('firstName')" class="ms-2">●</span>
              </label>
              <input type="text" formControlName="firstName" class="form-control">
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Last Name
                <span [class]="getConfidenceClass('lastName')" class="ms-2">●</span>
              </label>
              <input type="text" formControlName="lastName" class="form-control">
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Date of Birth
                <span [class]="getConfidenceClass('dateOfBirth')" class="ms-2">●</span>
              </label>
              <input type="date" formControlName="dateOfBirth" class="form-control">
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Gender
                <span [class]="getConfidenceClass('gender')" class="ms-2">●</span>
              </label>
              <select formControlName="gender" class="form-control">
                <option value="">Select gender</option>
                <option value="male">Male</option>
                <option value="female">Female</option>
                <option value="other">Other</option>
              </select>
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Phone Number
                <span [class]="getConfidenceClass('phoneNumber')" class="ms-2">●</span>
              </label>
              <input type="text" formControlName="phoneNumber" class="form-control">
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Email
                <span [class]="getConfidenceClass('email')" class="ms-2">●</span>
              </label>
              <input type="email" formControlName="email" class="form-control">
            </div>
            <div formGroupName="address">
              <h6>Address</h6>
              <div class="mb-3">
                <label class="form-label d-flex align-items-center">
                  Street Address
                  <span [class]="getConfidenceClass('address.line1')" class="ms-2">●</span>
                </label>
                <input type="text" formControlName="line1" class="form-control">
              </div>
              <div class="mb-3">
                <label class="form-label d-flex align-items-center">
                  City
                  <span [class]="getConfidenceClass('address.city')" class="ms-2">●</span>
                </label>
                <input type="text" formControlName="city" class="form-control">
              </div>
              <div class="mb-3">
                <label class="form-label d-flex align-items-center">
                  State
                  <span [class]="getConfidenceClass('address.state')" class="ms-2">●</span>
                </label>
                <input type="text" formControlName="state" class="form-control">
              </div>
              <div class="mb-3">
                <label class="form-label d-flex align-items-center">
                  Postal Code
                  <span [class]="getConfidenceClass('address.postalCode')" class="ms-2">●</span>
                </label>
                <input type="text" formControlName="postalCode" class="form-control">
              </div>
              <div class="mb-3">
                <label class="form-label d-flex align-items-center">
                  Country
                  <span [class]="getConfidenceClass('address.country')" class="ms-2">●</span>
                </label>
                <input type="text" formControlName="country" class="form-control">
              </div>
            </div>
          </div>

          <!-- Practitioner Form -->
          <div *ngIf="resourceType === FhirResourceType.Practitioner">
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                First Name
                <span [class]="getConfidenceClass('firstName')" class="ms-2">●</span>
              </label>
              <input type="text" formControlName="firstName" class="form-control">
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Last Name
                <span [class]="getConfidenceClass('lastName')" class="ms-2">●</span>
              </label>
              <input type="text" formControlName="lastName" class="form-control">
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Gender
                <span [class]="getConfidenceClass('gender')" class="ms-2">●</span>
              </label>
              <select formControlName="gender" class="form-control">
                <option value="">Select gender</option>
                <option value="male">Male</option>
                <option value="female">Female</option>
                <option value="other">Other</option>
              </select>
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Qualification
                <span [class]="getConfidenceClass('qualification')" class="ms-2">●</span>
              </label>
              <input type="text" formControlName="qualification" class="form-control">
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Speciality
                <span [class]="getConfidenceClass('speciality')" class="ms-2">●</span>
              </label>
              <input type="text" formControlName="speciality" class="form-control">
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                License Number
                <span [class]="getConfidenceClass('licenseNumber')" class="ms-2">●</span>
              </label>
              <input type="text" formControlName="licenseNumber" class="form-control">
            </div>
          </div>

          <!-- Organization Form -->
          <div *ngIf="resourceType === FhirResourceType.Organization">
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Organization Name
                <span [class]="getConfidenceClass('name')" class="ms-2">●</span>
              </label>
              <input type="text" formControlName="name" class="form-control">
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Type
                <span [class]="getConfidenceClass('type')" class="ms-2">●</span>
              </label>
              <input type="text" formControlName="type" class="form-control">
            </div>
            <div class="mb-3">
              <label class="form-label d-flex align-items-center">
                Registration Number
                <span [class]="getConfidenceClass('registrationNumber')" class="ms-2">●</span>
              </label>
              <input type="text" formControlName="registrationNumber" class="form-control">
            </div>
          </div>
        </form>

        <div class="mt-4">
          <div *ngIf="conversionError" class="alert alert-danger mb-3">
            {{ conversionError }}
          </div>
          
          <button 
            type="button" 
            class="btn btn-primary" 
            [disabled]="!reviewForm.valid || isConverting"
            (click)="onConfirmAndConvert()">
            <span *ngIf="isConverting" class="spinner-border spinner-border-sm me-2"></span>
            {{ isConverting ? 'Converting...' : 'Confirm & Convert to FHIR' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      max-width: 600px;
      margin: 0 auto;
    }
    .confidence-high { color: #28a745; }
    .confidence-medium { color: #ffc107; }
    .confidence-low { color: #dc3545; }
  `]
})
export class AssistedReviewFormComponent implements OnChanges {
  @Input() extractedData: PatientData | PractitionerData | OrganizationData | null = null;
  @Input() resourceType: FhirResourceType = FhirResourceType.Patient;
  @Input() fieldConfidences: Record<string, number> = {};
  @Input() overallConfidence: number = 0;
  @Input() extractionWarnings: string[] = [];


  reviewForm: FormGroup;
  isConverting = false;
  conversionError: string | null = null;
  readonly FhirResourceType = FhirResourceType;

  constructor(private fb: FormBuilder, private fhirApiService: FhirApiService, private router: Router) {
    this.reviewForm = this.createForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['extractedData'] || changes['resourceType'] || changes['fieldConfidences'] || changes['overallConfidence'] || changes['extractionWarnings']) {
      this.reviewForm = this.createForm();
      this.populateForm();
    }
  }

  private createForm(): FormGroup {
    switch (this.resourceType) {
      case FhirResourceType.Patient:
        return this.fb.group({
          firstName: ['', Validators.required],
          lastName: ['', Validators.required],
          dateOfBirth: ['', Validators.required],
          gender: ['', Validators.required],
          phoneNumber: [''],
          email: [''],
          address: this.fb.group({
            line1: [''],
            city: [''],
            state: [''],
            postalCode: [''],
            country: ['']
          })
        });
      
      case FhirResourceType.Practitioner:
        return this.fb.group({
          firstName: ['', Validators.required],
          lastName: ['', Validators.required],
          gender: ['', Validators.required],
          qualification: ['', Validators.required],
          speciality: ['', Validators.required],
          licenseNumber: ['', Validators.required]
        });
      
      case FhirResourceType.Organization:
        return this.fb.group({
          name: ['', Validators.required],
          type: ['', Validators.required],
          registrationNumber: ['', Validators.required]
        });
      
      default:
        return this.fb.group({});
    }
  }

  private populateForm(): void {
    if (!this.extractedData) return;

    if (this.resourceType === FhirResourceType.Patient) {
      const patientData = this.extractedData as PatientData;
      this.reviewForm.patchValue({
        firstName: patientData.firstName || '',
        lastName: patientData.lastName || '',
        dateOfBirth: this.formatDateForInput(patientData.dateOfBirth),
        gender: patientData.gender || '',
        phoneNumber: patientData.phoneNumber || '',
        email: patientData.email || '',
        address: {
          line1: patientData.address?.line1 || '',
          city: patientData.address?.city || '',
          state: patientData.address?.state || '',
          postalCode: patientData.address?.postalCode || '',
          country: patientData.address?.country || ''
        }
      });
    } else if (this.resourceType === FhirResourceType.Practitioner) {
      const practitionerData = this.extractedData as PractitionerData;
      this.reviewForm.patchValue({
        firstName: practitionerData.firstName || '',
        lastName: practitionerData.lastName || '',
        gender: practitionerData.gender || '',
        qualification: practitionerData.qualification || '',
        speciality: practitionerData.speciality || '',
        licenseNumber: practitionerData.licenseNumber || ''
      });
    } else if (this.resourceType === FhirResourceType.Organization) {
      const organizationData = this.extractedData as OrganizationData;
      this.reviewForm.patchValue({
        name: organizationData.name || '',
        type: organizationData.type || '',
        registrationNumber: organizationData.registrationNumber || ''
      });
    }
  }

  private formatDateForInput(dateString: string | undefined): string {
    if (!dateString) return '';
    try {
      const date = new Date(dateString);
      return date.toISOString().split('T')[0];
    } catch {
      return '';
    }
  }

  getConfidenceClass(fieldName: string): string {
    const confidence = this.fieldConfidences[fieldName] || 0;
    if (confidence >= 0.8) return 'confidence-high';
    if (confidence >= 0.5) return 'confidence-medium';
    return 'confidence-low';
  }

  getOverallConfidenceClass(): string {
    if (this.overallConfidence >= 0.8) return 'text-success';
    if (this.overallConfidence >= 0.5) return 'text-warning';
    return 'text-danger';
  }

  onConfirmAndConvert(): void {
    if (!this.reviewForm.valid) return;

    this.isConverting = true;
    this.conversionError = null;

    const formData = this.reviewForm.value;
    const request: ConvertToFhirRequest = {
      resourceType: this.resourceType,
      data: this.buildRequestData(formData)
    };

    console.log('Sending conversion request:', JSON.stringify(request, null, 2));

    this.fhirApiService.convertToFhir(request)
      .subscribe({
        next: (response: ConvertToFhirResponse) => {
          console.log('Conversion response:', response);
          this.isConverting = false;
          if (response.success && response.id) {
            this.router.navigate(['/details', response.id]);
          } else {
            this.conversionError = response.message || 'Conversion failed';
          }
        },
        error: (error) => {
          console.error('Conversion error:', error);
          console.error('Error details:', error.error);
          console.error('Request that failed:', JSON.stringify(request, null, 2));
          this.isConverting = false;
          this.conversionError = error.error?.message || error.message || 'Conversion failed';
        }
      });
  }

  private buildRequestData(formData: any): PatientData | PractitionerData | OrganizationData {
    switch (this.resourceType) {
      case FhirResourceType.Patient:
        let address = undefined;
        if (formData.address) {
          const addressData: any = {};
          if (formData.address.line1) addressData.line1 = formData.address.line1;
          if (formData.address.city) addressData.city = formData.address.city;
          if (formData.address.state) addressData.state = formData.address.state;
          if (formData.address.postalCode) addressData.postalCode = formData.address.postalCode;
          if (formData.address.country) addressData.country = formData.address.country;
          if (Object.keys(addressData).length > 0) address = addressData;
        }
        
        return {
          firstName: formData.firstName,
          lastName: formData.lastName,
          dateOfBirth: formData.dateOfBirth + 'T00:00:00Z',
          gender: formData.gender,
          phoneNumber: formData.phoneNumber || undefined,
          email: formData.email || undefined,
          address: address
        } as PatientData;
      
      case FhirResourceType.Practitioner:
        return {
          firstName: formData.firstName,
          lastName: formData.lastName,
          gender: formData.gender,
          qualification: formData.qualification,
          speciality: formData.speciality,
          licenseNumber: formData.licenseNumber
        } as PractitionerData;
      
      case FhirResourceType.Organization:
        return {
          name: formData.name,
          type: formData.type,
          registrationNumber: formData.registrationNumber
        } as OrganizationData;
      
      default:
        throw new Error('Unsupported resource type');
    }
  }
}