import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthApiService } from './features/auth/auth-api.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, CommonModule],
  template: `
    <div *ngIf="authService.isLoggedIn()">
      <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
        <div class="container">
          <a class="navbar-brand" href="#">FHIR Converter</a>
          <div class="navbar-nav">
            <a class="nav-link" routerLink="/convert">Convert</a>
            <a class="nav-link" routerLink="/history">History</a>
          </div>
        </div>
      </nav>
    </div>
    <router-outlet></router-outlet>
  `,
  styles: [`
    .navbar { margin-bottom: 0; }
    .container { max-width: 1200px; }
    .nav-link { color: rgba(255,255,255,.75) !important; }
    .nav-link:hover { color: white !important; }
  `]
})
export class App {
  constructor(public authService: AuthApiService) {}
}
