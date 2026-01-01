import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ConvertToFhirRequest } from '../core/api/api.models';

@Component({
  selector: 'app-conversion-form',
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <form [formGroup]="conversionForm" (ngSubmit)="onSubmit()">
      <div class="form-group">
        <label>First Name</label>
        <input type="text" formControlName="firstName" class="form-control">
      </div>
      <div class="form-group">
        <label>Last Name</label>
        <input type="text" formControlName="lastName" class="form-control">
      </div>
      <div class="form-group">
        <label>Date of Birth</label>
        <input type="date" formControlName="dateOfBirth" class="form-control">
      </div>
      <div class="form-group">
        <label>Gender</label>
        <select formControlName="gender" class="form-control">
          <option value="male">Male</option>
          <option value="female">Female</option>
        </select>
      </div>
      <div class="form-group">
        <label>Phone Number</label>
        <input type="text" formControlName="phoneNumber" class="form-control">
      </div>
      <div class="form-group">
        <label>Email</label>
        <input type="email" formControlName="email" class="form-control">
      </div>
      <div class="form-group">
        <label>Address Line 1</label>
        <input type="text" formControlName="addressLine1" class="form-control">
      </div>
      <div class="form-group">
        <label>City</label>
        <input type="text" formControlName="city" class="form-control">
      </div>
      <div class="form-group">
        <label>State</label>
        <input type="text" formControlName="state" class="form-control">
      </div>
      <div class="form-group">
        <label>Postal Code</label>
        <input type="text" formControlName="postalCode" class="form-control">
      </div>
      <div class="form-group">
        <label>Country</label>
        <input type="text" formControlName="country" class="form-control">
      </div>
      <button type="submit" [disabled]="!conversionForm.valid" class="btn btn-primary">
        Convert to FHIR
      </button>
    </form>
  `
})
export class ConversionFormComponent {
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
      addressLine1: [''],
      city: [''],
      state: [''],
      postalCode: [''],
      country: ['']
    });
  }

  onSubmit() {
    console.log('Form submitted');
    console.log('Form valid:', this.conversionForm.valid);
    console.log('Form value:', this.conversionForm.value);
    
    if (this.conversionForm.valid) {
      const formData = this.conversionForm.value;
      const request: ConvertToFhirRequest = {
        resourceType: 1,
        data: {
          firstName: formData.firstName,
          lastName: formData.lastName,
          dateOfBirth: formData.dateOfBirth + 'T00:00:00Z',
          gender: formData.gender,
          phoneNumber: formData.phoneNumber,
          email: formData.email,
          address: {
            line1: formData.addressLine1,
            city: formData.city,
            state: formData.state,
            postalCode: formData.postalCode,
            country: formData.country
          }
        }
      };
      console.log('Emitting request:', request);
      this.convert.emit(request);
    }
  }
}