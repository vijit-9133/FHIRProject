import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FhirResourceType, DocumentIngestionResponse } from '../../../core/api/api.models';
import { FhirApiService } from '../../../core/api/fhir-api.service';
import { AssistedReviewFormComponent } from './assisted-review-form.component';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [CommonModule, FormsModule, AssistedReviewFormComponent],
  template: `
    <div class="container mt-4">
      <h2>Document Upload (AI-assisted)</h2>
      <p class="text-muted mb-4">Upload a document and review AI-extracted data before conversion</p>
      <div class="card">
      <div class="card-header">
        <h5 class="card-title mb-0">Upload Document (AI-assisted)</h5>
      </div>
      <div class="card-body">
        <form>
          <div class="mb-3">
            <label for="resourceType" class="form-label">Resource Type</label>
            <select 
              id="resourceType" 
              class="form-select" 
              [(ngModel)]="selectedResourceType" 
              name="resourceType">
              <option value="">Select resource type</option>
              <option [value]="FhirResourceType.Patient">Patient</option>
              <option [value]="FhirResourceType.Practitioner">Practitioner</option>
              <option [value]="FhirResourceType.Organization">Organization</option>
            </select>
          </div>

          <div class="mb-3">
            <label for="file" class="form-label">Document File</label>
            <input 
              type="file" 
              id="file" 
              class="form-control" 
              (change)="onFileSelected($event)"
              accept=".pdf,.png,.jpg,.jpeg">
            <div class="form-text">Supported formats: PDF, PNG, JPG, JPEG (max 10MB)</div>
          </div>

          <button 
            type="button" 
            class="btn btn-primary" 
            [disabled]="!selectedFile || !selectedResourceType || isLoading"
            (click)="onUpload()">
            <span *ngIf="isLoading" class="spinner-border spinner-border-sm me-2"></span>
            {{ isLoading ? 'Processing...' : 'Upload Document' }}
          </button>
        </form>

        <div *ngIf="errorMessage" class="alert alert-danger mt-3">
          {{ errorMessage }}
        </div>

        <div *ngIf="!isLoading && geminiResponse" class="mt-4">
          <h6>Processing Complete</h6>
          <div class="alert alert-success">
            {{ geminiResponse.message }}
          </div>
          
          <app-assisted-review-form 
            [extractedData]="geminiResponse.geminiExtraction?.extractedData || null"
            [resourceType]="geminiResponse.resourceType"
            [fieldConfidences]="geminiResponse.geminiExtraction?.fieldConfidences || {}"
            [overallConfidence]="geminiResponse.geminiExtraction?.overallConfidence || 0"
            [extractionWarnings]="geminiResponse.geminiExtraction?.extractionWarnings || []">
          </app-assisted-review-form>
        </div>
        
        <!-- Debug info -->
        <div *ngIf="geminiResponse" class="mt-2 small text-muted">
          Debug: isLoading={{isLoading}}, hasResponse={{!!geminiResponse}}
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      max-width: 500px;
      margin: 0 auto;
    }
    .spinner-border-sm {
      width: 1rem;
      height: 1rem;
    }
  `]
})
export class DocumentUploadComponent {
  selectedFile: File | null = null;
  selectedResourceType: FhirResourceType | '' = '';
  isLoading = false;
  geminiResponse: DocumentIngestionResponse | null = null;
  errorMessage: string | null = null;
  
  readonly FhirResourceType = FhirResourceType;

  constructor(private fhirApiService: FhirApiService, private cdr: ChangeDetectorRef) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] || null;
  }

  onUpload(): void {
    if (!this.selectedFile || !this.selectedResourceType) return;
    
    this.isLoading = true;
    this.errorMessage = null;
    this.geminiResponse = null;
    
    this.fhirApiService.ingestDocument(this.selectedFile, this.selectedResourceType as FhirResourceType)
      .subscribe({
        next: (response) => {
          console.log('Ingestion API response:', response);
          console.log('Setting geminiResponse and isLoading=false');
          this.geminiResponse = response;
          this.isLoading = false;
          this.cdr.detectChanges();
          console.log('isLoading is now:', this.isLoading);
          console.log('geminiResponse is now:', this.geminiResponse);
        },
        error: (error) => {
          console.error('Ingestion failed:', error);
          this.errorMessage = error?.error?.message || error?.message || 'Document ingestion failed';
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }
}