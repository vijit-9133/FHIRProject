import { Component } from '@angular/core';
import { FhirApiService } from '../../core/api/fhir-api.service';
import { ConvertToFhirRequest, ConvertToFhirResponse } from '../../core/api/api.models';

@Component({
  selector: 'app-convert',
  template: `
    <div class="container">
      <h2>Convert to FHIR</h2>
      <app-conversion-form (convert)="onConvert($event)"></app-conversion-form>
      
      <div *ngIf="result" class="mt-4">
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

  constructor(private fhirApi: FhirApiService) {}

  onConvert(request: ConvertToFhirRequest) {
    this.fhirApi.convertToFhir(request).subscribe({
      next: (response) => this.result = response,
      error: (error) => console.error('Conversion failed:', error)
    });
  }
}