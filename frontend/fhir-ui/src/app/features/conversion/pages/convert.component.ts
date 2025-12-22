import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FhirApiService } from '../../../core/api/fhir-api.service';
import { ConvertToFhirRequest, ConvertToFhirResponse } from '../../../core/api/api.models';
import { ConversionFormComponent } from '../components/conversion-form.component';
import { PrettyJsonPipe } from '../../../shared/pipes/pretty-json.pipe';

@Component({
  selector: 'app-convert',
  imports: [CommonModule, ConversionFormComponent, PrettyJsonPipe],
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
        <div *ngIf="result.fhirResource" class="card">
          <div class="card-header">
            <h4>FHIR Resource</h4>
          </div>
          <div class="card-body">
            <pre>{{result.fhirResource | prettyJson}}</pre>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ConvertComponent {
  result: ConvertToFhirResponse | null = null;

  constructor(private fhirApi: FhirApiService) {}

  onConvert(request: ConvertToFhirRequest) {
    console.log('Sending request:', JSON.stringify(request, null, 2));
    this.fhirApi.convertToFhir(request).subscribe({
      next: (response: any) => {
        console.log('Received response:', response);
        this.result = response;
      },
      error: (error: any) => {
        console.error('Conversion failed:', error);
        console.error('Error details:', error.error);
      }
    });
  }
}