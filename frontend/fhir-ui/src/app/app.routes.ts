import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/admin.guard';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },
  { path: 'patient/dashboard', loadComponent: () => import('./features/dashboards/patient-dashboard.component').then(m => m.PatientDashboardComponent) },
  { path: 'doctor/dashboard', loadComponent: () => import('./features/dashboards/doctor-dashboard.component').then(m => m.DoctorDashboardComponent) },
  { path: 'organization/dashboard', loadComponent: () => import('./features/dashboards/organization-dashboard.component').then(m => m.OrganizationDashboardComponent) },
  { path: 'admin/dashboard', loadComponent: () => import('./features/dashboards/admin-dashboard.component').then(m => m.AdminDashboardComponent), canActivate: [adminGuard] },
  { path: 'conversion', loadComponent: () => import('./features/conversion/pages/conversion-landing.component').then(m => m.ConversionLandingComponent) },
  { path: 'convert', loadComponent: () => import('./features/conversion/pages/convert.component').then(m => m.ConvertComponent) },
  { path: 'manual-entry', loadComponent: () => import('./manual-entry-main-flow/convert.component').then(m => m.ConvertComponent) },
  { path: 'upload', loadComponent: () => import('./features/conversion/components/document-upload.component').then(m => m.DocumentUploadComponent) },
  { path: 'history', loadComponent: () => import('./features/history/pages/history.component').then(m => m.HistoryComponent) },
  { path: 'details/:id', loadComponent: () => import('./features/details/pages/details.component').then(m => m.DetailsComponent) },
  { path: 'external/register', loadComponent: () => import('./features/conversion/pages/external-register.component').then(m => m.ExternalRegisterComponent) },
  { path: 'external/status', loadComponent: () => import('./features/conversion/pages/external-status.component').then(m => m.ExternalStatusComponent) }
];

export { routes };
