import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrettyJsonPipe } from './pipes/pretty-json.pipe';

@NgModule({
  declarations: [
    PrettyJsonPipe
  ],
  imports: [CommonModule],
  exports: [
    PrettyJsonPipe
  ]
})
export class SharedModule { }