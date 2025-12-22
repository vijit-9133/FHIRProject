import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FhirApiService } from '../../../core/api/fhir-api.service';
import { PrettyJsonPipe } from '../../../shared/pipes/pretty-json.pipe';

@Component({
  selector: 'app-details',
  imports: [CommonModule, PrettyJsonPipe],
  template: `
    <div class="container">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="mb-0">Conversion Details</h2>
        <button class="btn btn-outline-secondary" (click)="goBack()">← Back to History</button>
      </div>
      
      <div *ngIf="conversionRequest">
        <!-- Status Overview Card -->
        <div class="card mb-4 border-0 shadow-sm">
          <div class="card-body">
            <div class="row align-items-center">
              <div class="col-md-8">
                <h5 class="card-title mb-1">Conversion Request #{{conversionRequest.id}}</h5>
                <p class="text-muted mb-0">{{conversionRequest.resourceType}} Resource • {{conversionRequest.createdAt | date:'medium'}}</p>
              </div>
              <div class="col-md-4 text-end">
                <span class="badge fs-6 px-3 py-2" [class]="getStatusClass(conversionRequest.status)">
                  {{getStatusText(conversionRequest.status)}}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Request Details -->
        <div class="row">
          <div class="col-lg-6 mb-4">
            <div class="card h-100 border-0 shadow-sm">
              <div class="card-header bg-primary text-white">
                <h5 class="mb-0"><i class="fas fa-info-circle me-2"></i>Request Information</h5>
              </div>
              <div class="card-body">
                <div class="row g-3">
                  <div class="col-12">
                    <label class="form-label fw-bold text-muted">Conversion ID</label>
                    <p class="mb-0">{{conversionRequest.id}}</p>
                  </div>
                  <div class="col-12">
                    <label class="form-label fw-bold text-muted">Resource Type</label>
                    <p class="mb-0">{{conversionRequest.resourceType}}</p>
                  </div>
                  <div class="col-12">
                    <label class="form-label fw-bold text-muted">Created At</label>
                    <p class="mb-0">{{conversionRequest.createdAt | date:'full'}}</p>
                  </div>
                  <div class="col-12">
                    <label class="form-label fw-bold text-muted">Mapping Version</label>
                    <p class="mb-0">{{conversionRequest.mappingVersion}}</p>
                  </div>
                  <div class="col-12" *ngIf="conversionRequest.errorMessage">
                    <label class="form-label fw-bold text-danger">Error Message</label>
                    <div class="alert alert-danger mb-0">
                      {{conversionRequest.errorMessage}}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="col-lg-6 mb-4">
            <div class="card h-100 border-0 shadow-sm">
              <div class="card-header bg-info text-white">
                <h5 class="mb-0"><i class="fas fa-upload me-2"></i>Input Data</h5>
              </div>
              <div class="card-body">
                <div class="bg-light rounded p-3" style="max-height: 300px; overflow-y: auto;">
                  <pre class="mb-0 small">{{parseJson(conversionRequest.inputDataJson) | prettyJson}}</pre>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <!-- FHIR Resource -->
        <div *ngIf="fhirResource" class="card border-0 shadow-sm">
          <div class="card-header bg-success text-white">
            <h5 class="mb-0"><i class="fas fa-code me-2"></i>Generated FHIR Resource</h5>
          </div>
          <div class="card-body">
            <div class="bg-light rounded p-3" style="max-height: 400px; overflow-y: auto;">
              <pre class="mb-0 small">{{parseJson(fhirResource.fhirJson) | prettyJson}}</pre>
            </div>
            <div class="mt-3">
              <small class="text-muted">
                <i class="fas fa-clock me-1"></i>
                Generated on {{fhirResource.createdAt | date:'medium'}}
              </small>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div *ngIf="!conversionRequest" class="text-center py-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p class="mt-3 text-muted">Loading conversion details...</p>
      </div>
    </div>
  `,
  styles: [`
    .card {
      transition: transform 0.2s ease-in-out;
    }
    .card:hover {
      transform: translateY(-2px);
    }
    pre {
      font-family: 'Courier New', monospace;
      font-size: 0.85rem;
      line-height: 1.4;
      color: #333;
    }
    .badge {
      font-weight: 500;
    }
    .form-label {
      font-size: 0.875rem;
      margin-bottom: 0.25rem;
    }
  `]
})
export class DetailsComponent implements OnInit {
  conversionRequest: any = null;
  fhirResource: any = null;

  constructor(
    private route: ActivatedRoute,
    private fhirApi: FhirApiService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    console.log('Details component loaded with ID:', id);
    this.loadDetails(id);
  }

  loadDetails(id: number) {
    console.log('Loading details for ID:', id);
    this.fhirApi.getConversionRequest(id).subscribe({
      next: (data: any) => {
        console.log('Conversion request data received:', data);
        this.conversionRequest = data;
        this.cdr.detectChanges();
        if (data.status === 1) {
          console.log('Status is success, loading FHIR resource...');
          this.loadFhirResource(id);
        } else {
          console.log('Status is not success:', data.status);
        }
      },
      error: (error: any) => {
        console.error('Failed to load details:', error);
        console.error('Error details:', error.error);
      }
    });
  }

  loadFhirResource(conversionRequestId: number) {
    console.log('Loading FHIR resource for conversion request ID:', conversionRequestId);
    this.fhirApi.getFhirResource(conversionRequestId).subscribe({
      next: (data: any) => {
        console.log('FHIR resource data received:', data);
        this.fhirResource = data;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Failed to load FHIR resource:', error);
        console.error('Error details:', error.error);
      }
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

  getStatusClass(status: number): string {
    switch (status) {
      case 0: return 'badge-warning';
      case 1: return 'badge-success';
      case 2: return 'badge-danger';
      default: return 'badge-secondary';
    }
  }

  parseJson(jsonString: string): any {
    try {
      return JSON.parse(jsonString);
    } catch {
      return jsonString;
    }
  }

  goBack(): void {
    window.history.back();
  }
}