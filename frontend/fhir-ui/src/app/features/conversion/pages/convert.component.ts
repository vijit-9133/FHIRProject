import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FhirApiService } from '../../../core/api/fhir-api.service';
import { ConvertToFhirRequest, ConvertToFhirResponse } from '../../../core/api/api.models';
import { CompleteConversionFormComponent } from '../components/complete-conversion-form.component';
import { PrettyJsonPipe } from '../../../shared/pipes/pretty-json.pipe';

@Component({
  selector: 'app-convert',
  imports: [CommonModule, CompleteConversionFormComponent, PrettyJsonPipe],
  template: `
    <div class="container mt-4">
      <h2>Manual Entry</h2>
      <p class="text-muted mb-4">Enter patient details manually and convert to FHIR</p>
      <app-complete-conversion-form (convert)="onConvert($event)"></app-complete-conversion-form>
      
      <div *ngIf="result" class="mt-4">
        <h3>Result</h3>
        <div class="alert" [class.alert-success]="result.success" [class.alert-danger]="!result.success">
          <p><strong>ID:</strong> {{result.id}}</p>
          <p><strong>Status:</strong> {{result.success ? 'Success' : 'Failed'}}</p>
          <p><strong>Message:</strong> {{result.message}}</p>
        </div>
        <div *ngIf="result.success && result.id" class="mt-3">
          <button class="btn btn-success" (click)="viewDetails(result.id.toString())">View Details</button>
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

  constructor(private fhirApi: FhirApiService, private router: Router) {}

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

  viewDetails(id: string): void {
    this.router.navigate(['/details', id]);
  }
}