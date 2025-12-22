import { Routes } from '@angular/router';

const routes: Routes = [
  { path: '', redirectTo: '/convert', pathMatch: 'full' },
  { path: 'convert', loadComponent: () => import('./features/conversion/pages/convert.component').then(m => m.ConvertComponent) },
  { path: 'history', loadComponent: () => import('./features/history/pages/history.component').then(m => m.HistoryComponent) },
  { path: 'details/:id', loadComponent: () => import('./features/details/pages/details.component').then(m => m.DetailsComponent) }
];

export { routes };
