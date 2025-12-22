import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainLayoutComponent } from './layouts/main-layout.component';

const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', redirectTo: '/convert', pathMatch: 'full' },
      { path: 'convert', loadChildren: () => import('./features/conversion/conversion.module').then(m => m.ConversionModule) },
      { path: 'history', loadChildren: () => import('./features/history/history.module').then(m => m.HistoryModule) },
      { path: 'details/:id', loadChildren: () => import('./features/details/details.module').then(m => m.DetailsModule) }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }