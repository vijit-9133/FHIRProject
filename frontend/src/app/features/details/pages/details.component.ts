import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FhirApiService } from '../../core/api/fhir-api.service';

@Component({
  selector: 'app-details',
  template: `
    <div class="container">
      <h2>Conversion Details</h2>
      <div *ngIf="conversionRequest">
        <div class="card">
          <div class="card-header">
            <h4>Request Information</h4>
          </div>
          <div class="card-body">
            <p><strong>ID:</strong> {{conversionRequest.id}}</p>
            <p><strong>Resource Type:</strong> {{conversionRequest.resourceType}}</p>
            <p><strong>Status:</strong> {{getStatusText(conversionRequest.status)}}</p>
            <p><strong>Created At:</strong> {{conversionRequest.createdAt | date}}</p>
            <p *ngIf="conversionRequest.errorMessage"><strong>Error:</strong> {{conversionRequest.errorMessage}}</p>
          </div>
        </div>
        
        <div class="card mt-3">
          <div class="card-header">
            <h4>Input Data</h4>
          </div>
          <div class="card-body">
            <pre>{{conversionRequest.inputDataJson | prettyJson}}</pre>
          </div>
        </div>
        
        <div *ngIf="fhirResource" class="card mt-3">
          <div class="card-header">
            <h4>Generated FHIR Resource</h4>
          </div>
          <div class="card-body">
            <pre>{{fhirResource.fhirJson | prettyJson}}</pre>
          </div>
        </div>
      </div>
    </div>
  `
})
export class DetailsComponent implements OnInit {
  conversionRequest: any = null;
  fhirResource: any = null;

  constructor(
    private route: ActivatedRoute,
    private fhirApi: FhirApiService
  ) {}

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadDetails(id);
  }

  loadDetails(id: number) {
    this.fhirApi.getConversionRequest(id).subscribe({
      next: (data) => {
        this.conversionRequest = data;
        if (data.status === 1) {
          this.loadFhirResource(id);
        }
      },
      error: (error) => console.error('Failed to load details:', error)
    });
  }

  loadFhirResource(conversionRequestId: number) {
    this.fhirApi.getFhirResource(conversionRequestId).subscribe({
      next: (data) => this.fhirResource = data,
      error: (error) => console.error('Failed to load FHIR resource:', error)
    });
  }

  getStatusText(status: number): string {
    switch (status) {
      case 0: return 'Pending';
      case 1: return 'Success';
      case 2: return 'Failed';
      default: return 'Unknown';
    }
  }
}