import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FhirApiService } from '../core/api/fhir-api.service';
import { ConvertToFhirRequest, ConvertToFhirResponse } from '../core/api/api.models';
import { ConversionFormComponent } from './conversion-form.component';
import { PrettyJsonPipe } from '../shared/pipes/pretty-json.pipe';

@Component({
  selector: 'app-convert',
  imports: [CommonModule, ConversionFormComponent, PrettyJsonPipe],
  template: `
    <div class="container">
      <h2>Convert to FHIR</h2>
      <app-conversion-form (convert)="onConvert($event)"></app-conversion-form>
      
      <div *ngIf="loading" class="mt-4">
        <div class="alert alert-info">
          <p>Converting to FHIR...</p>
        </div>
      </div>
      
      <div *ngIf="result && !loading" class="mt-4">
        <h3>Result</h3>
        <div class="alert" [class.alert-success]="result.success" [class.alert-danger]="!result.success">
          <p><strong>ID:</strong> {{result.id}}</p>
          <p><strong>Status:</strong> {{result.success ? 'Success' : 'Failed'}}</p>
          <p><strong>Message:</strong> {{result.message}}</p>
        </div>
        <div *ngIf="result.fhirResource">
          <h4>FHIR Resource</h4>
          <pre>{{result.fhirResource | prettyJson}}</pre>
        </div>
      </div>
    </div>
  `
})
export class ConvertComponent {
  result: ConvertToFhirResponse | null = null;
  loading = false;

  constructor(private fhirApi: FhirApiService, private cdr: ChangeDetectorRef) {}

  onConvert(request: ConvertToFhirRequest) {
    console.log('Sending request:', request);
    this.loading = true;
    this.result = null;
    this.cdr.detectChanges();
    
    this.fhirApi.convertToFhir(request).subscribe({
      next: (response) => {
        console.log('Received response:', response);
        this.result = response;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Conversion failed:', error);
        this.result = null;
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }
}