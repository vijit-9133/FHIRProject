import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ConvertToFhirRequest } from '../../../core/api/api.models';

@Component({
  selector: 'app-conversion-form',
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
        <button type="submit" [disabled]="!conversionForm.valid" class="btn btn-primary">
          Convert to FHIR
        </button>
      </div>
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
      gender: ['male', Validators.required]
    });
  }

  onSubmit() {
    console.log('Form submitted');
    console.log('Form valid:', this.conversionForm.valid);
    console.log('Form value:', this.conversionForm.value);
    console.log('Form errors:', this.conversionForm.errors);
    
    if (this.conversionForm.valid) {
      const formData = this.conversionForm.value;
      const request: ConvertToFhirRequest = {
        resourceType: 1,
        data: {
          firstName: formData.firstName,
          lastName: formData.lastName,
          dateOfBirth: formData.dateOfBirth + 'T00:00:00Z',
          gender: formData.gender,
          phoneNumber: formData.phoneNumber || '',
          email: formData.email || ''
        }
      };
      console.log('Emitting convert event:', request);
      this.convert.emit(request);
    } else {
      console.log('Form is invalid');
      Object.keys(this.conversionForm.controls).forEach(key => {
        const control = this.conversionForm.get(key);
        if (control && control.invalid) {
          console.log(`${key} is invalid:`, control.errors);
        }
      });
    }
  }
}