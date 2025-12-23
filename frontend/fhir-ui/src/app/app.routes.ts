import { Routes } from '@angular/router';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },
  { path: 'patient/dashboard', loadComponent: () => import('./features/dashboards/patient-dashboard.component').then(m => m.PatientDashboardComponent) },
  { path: 'doctor/dashboard', loadComponent: () => import('./features/dashboards/doctor-dashboard.component').then(m => m.DoctorDashboardComponent) },
  { path: 'organization/dashboard', loadComponent: () => import('./features/dashboards/organization-dashboard.component').then(m => m.OrganizationDashboardComponent) },
  { path: 'convert', loadComponent: () => import('./features/conversion/pages/convert.component').then(m => m.ConvertComponent) },
  { path: 'history', loadComponent: () => import('./features/history/pages/history.component').then(m => m.HistoryComponent) },
  { path: 'details/:id', loadComponent: () => import('./features/details/pages/details.component').then(m => m.DetailsComponent) }
];

export { routes };
