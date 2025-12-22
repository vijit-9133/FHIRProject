import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FhirApiService } from '../../../core/api/fhir-api.service';
import { ConversionRequest } from '../../../core/api/api.models';

@Component({
  selector: 'app-history',
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container">
      <h2>Conversion History</h2>
      <div class="table-responsive">
        <table class="table table-striped">
          <thead>
            <tr>
              <th>ID</th>
              <th>Resource Type</th>
              <th>Status</th>
              <th>Created At</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let request of history; trackBy: trackByFn">
              <td>{{request.id}}</td>
              <td>{{request.resourceType}}</td>
              <td>
                <span class="badge" [class]="getStatusClass(request.status)">
                  {{getStatusText(request.status)}}
                </span>
              </td>
              <td>{{request.createdAt | date}}</td>
              <td>
                <a [routerLink]="['/details', request.id]" class="btn btn-sm btn-primary">View</a>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class HistoryComponent implements OnInit {
  history: ConversionRequest[] = [];

  constructor(
    private fhirApi: FhirApiService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadHistory();
  }

  trackByFn(index: number, item: ConversionRequest): number {
    return item.id;
  }

  loadHistory() {
    console.log('Loading history...');
    this.fhirApi.getConversionHistory().subscribe({
      next: (data: any) => {
        console.log('History data received:', data);
        this.history = data;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Failed to load history:', error);
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
}