import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { ConversionRoutingModule } from './conversion-routing.module';
import { SharedModule } from '../../shared/shared.module';
import { ConvertComponent } from './pages/convert.component';
import { ConversionFormComponent } from './components/conversion-form.component';

@NgModule({
  declarations: [
    ConvertComponent,
    ConversionFormComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ConversionRoutingModule,
    SharedModule
  ]
})
export class ConversionModule { }