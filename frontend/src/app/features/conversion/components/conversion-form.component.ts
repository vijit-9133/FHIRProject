import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConvertToFhirRequest } from '../../core/api/api.models';

@Component({
  selector: 'app-conversion-form',
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
      gender: ['male', Validators.required]
    });
  }

  onSubmit() {
    if (this.conversionForm.valid) {
      const request: ConvertToFhirRequest = {
        resourceType: 1,
        data: this.conversionForm.value
      };
      this.convert.emit(request);
    }
  }
}