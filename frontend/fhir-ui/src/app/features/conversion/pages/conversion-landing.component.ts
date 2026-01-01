import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-conversion-landing',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container mt-4">
      <div class="row justify-content-center">
        <div class="col-md-8">
          <h2 class="text-center mb-4">Create FHIR Resources</h2>
          <p class="text-center text-muted mb-5">Choose how you'd like to create your FHIR resource</p>
          
          <div class="row">
            <div class="col-md-6 mb-4">
              <div class="card h-100 border-primary">
                <div class="card-body text-center">
                  <div class="mb-3">
                    <span class="display-4">✏️</span>
                  </div>
                  <h5 class="card-title">Manual Entry</h5>
                  <p class="card-text text-muted">Enter details manually and convert to FHIR</p>
                  <a routerLink="/manual-entry" class="btn btn-primary">Start Manual Entry</a>
                </div>
              </div>
            </div>
            
            <div class="col-md-6 mb-4">
              <div class="card h-100 border-success">
                <div class="card-body text-center">
                  <div class="mb-3">
                    <span class="display-4">📄</span>
                  </div>
                  <h5 class="card-title">Upload Document (AI-assisted)</h5>
                  <p class="card-text text-muted">Upload PDF/image and review AI-extracted data</p>
                  <a routerLink="/upload" class="btn btn-success">Start Upload Document (AI-assisted)</a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ConversionLandingComponent {}