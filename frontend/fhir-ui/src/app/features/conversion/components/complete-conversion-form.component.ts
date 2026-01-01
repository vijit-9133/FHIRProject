import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ConvertToFhirRequest, PatientData } from '../../../core/api/api.models';

@Component({
  selector: 'app-complete-conversion-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <form [formGroup]="conversionForm" (ngSubmit)="onSubmit()" class="card">
      <div class="card-body">
        <div class="mb-3">
          <label class="form-label">First Name</label>
          <input type="text" formControlName="firstName" class="form-control">
        </div>
        <div class="mb-3">
          <label class="form-label">Last Name</label>
          <input type="text" formControlName="lastName" class="form-control">
        </div>
        <div class="mb-3">
          <label class="form-label">Date of Birth</label>
          <input type="date" formControlName="dateOfBirth" class="form-control">
        </div>
        <div class="mb-3">
          <label class="form-label">Gender</label>
          <select formControlName="gender" class="form-control">
            <option value="male">Male</option>
            <option value="female">Female</option>
          </select>
        </div>
        <div class="mb-3">
          <label class="form-label">Phone Number</label>
          <input type="text" formControlName="phoneNumber" class="form-control">
        </div>
        <div class="mb-3">
          <label class="form-label">Email</label>
          <input type="email" formControlName="email" class="form-control">
        </div>
        
        <div formGroupName="address">
          <h6>Address</h6>
          <div class="mb-3">
            <label class="form-label">Street Address</label>
            <input type="text" formControlName="line1" class="form-control">
          </div>
          <div class="mb-3">
            <label class="form-label">City</label>
            <input type="text" formControlName="city" class="form-control">
          </div>
          <div class="mb-3">
            <label class="form-label">State</label>
            <input type="text" formControlName="state" class="form-control">
          </div>
          <div class="mb-3">
            <label class="form-label">Postal Code</label>
            <input type="text" formControlName="postalCode" class="form-control">
          </div>
          <div class="mb-3">
            <label class="form-label">Country</label>
            <input type="text" formControlName="country" class="form-control">
          </div>
        </div>
        
        <button type="submit" [disabled]="!conversionForm.valid" class="btn btn-primary">
          Convert to FHIR
        </button>
      </div>
    </form>
  `
})
export class CompleteConversionFormComponent {
  @Output() convert = new EventEmitter<ConvertToFhirRequest>();

  conversionForm: FormGroup;

  constructor(private fb: FormBuilder) {
    this.conversionForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      gender: ['male', Validators.required],
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
  }

  onSubmit() {
    if (this.conversionForm.valid) {
      const formData = this.conversionForm.value;
      const patientData: PatientData = {
        firstName: formData.firstName,
        lastName: formData.lastName,
        dateOfBirth: new Date(formData.dateOfBirth).toISOString(),
        gender: formData.gender,
        phoneNumber: formData.phoneNumber || '',
        email: formData.email || '',
        address: formData.address
      };
      
      const request: ConvertToFhirRequest = {
        resourceType: 1,
        data: patientData
      };
      
      this.convert.emit(request);
    }
  }
}