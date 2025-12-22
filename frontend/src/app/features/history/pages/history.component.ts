import { Component, OnInit } from '@angular/core';
import { FhirApiService } from '../../core/api/fhir-api.service';
import { ConversionRequest } from '../../core/api/api.models';

@Component({
  selector: 'app-history',
  template: `
    <div class="container">
      <h2>Conversion History</h2>
      <table class="table">
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
          <tr *ngFor="let request of history">
            <td>{{request.id}}</td>
            <td>{{request.resourceType}}</td>
            <td>{{getStatusText(request.status)}}</td>
            <td>{{request.createdAt | date}}</td>
            <td>
              <a [routerLink]="['/details', request.id]" class="btn btn-sm btn-primary">View</a>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  `
})
export class HistoryComponent implements OnInit {
  history: ConversionRequest[] = [];

  constructor(private fhirApi: FhirApiService) {}

  ngOnInit() {
    this.loadHistory();
  }

  loadHistory() {
    this.fhirApi.getConversionHistory().subscribe({
      next: (data) => this.history = data,
      error: (error) => console.error('Failed to load history:', error)
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