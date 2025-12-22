import { Component } from '@angular/core';

@Component({
  selector: 'app-main-layout',
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
      <div class="container">
        <a class="navbar-brand" href="#">FHIR Converter</a>
        <div class="navbar-nav">
          <a class="nav-link" routerLink="/convert">Convert</a>
          <a class="nav-link" routerLink="/history">History</a>
        </div>
      </div>
    </nav>
    <main class="py-4">
      <router-outlet></router-outlet>
    </main>
  `
})
export class MainLayoutComponent { }