import { Component, Input } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { PhoneMaskDirective } from '../../../../../shared/directives/phone-mask.directive';

@Component({
  selector: 'app-step-admin-servidor',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, PhoneMaskDirective],
  template: `
    <div [formGroup]="form">
      <mat-form-field appearance="outline">
        <mat-label>Proveedor</mat-label>
        <input matInput formControlName="proveedor">
      </mat-form-field>
      @if (form.get('proveedor')?.invalid && form.get('proveedor')?.touched) {
        <span class="field-error">Este campo es obligatorio</span>
      }

      <mat-form-field appearance="outline">
        <mat-label>Dependencia</mat-label>
        <input matInput formControlName="dependencia">
      </mat-form-field>
      @if (form.get('dependencia')?.invalid && form.get('dependencia')?.touched) {
        <span class="field-error">Este campo es obligatorio</span>
      }

      <mat-form-field appearance="outline">
        <mat-label>Responsable</mat-label>
        <input matInput formControlName="responsable">
      </mat-form-field>
      @if (form.get('responsable')?.invalid && form.get('responsable')?.touched) {
        <span class="field-error">Este campo es obligatorio</span>
      }

      <mat-form-field appearance="outline">
        <mat-label>Cargo</mat-label>
        <input matInput formControlName="cargo">
      </mat-form-field>
      @if (form.get('cargo')?.invalid && form.get('cargo')?.touched) {
        <span class="field-error">Este campo es obligatorio</span>
      }

      <mat-form-field appearance="outline">
        <mat-label>Teléfono</mat-label>
        <input matInput formControlName="telefono" appPhoneMask placeholder="(333) 333-3333">
      </mat-form-field>
      @if (form.get('telefono')?.touched) {
        @if (form.get('telefono')?.hasError('required')) {
          <span class="field-error">Este campo es obligatorio</span>
        } @else if (form.get('telefono')?.hasError('pattern')) {
          <span class="field-error">Ingresa un teléfono con formato (XXX) XXX-XXXX</span>
        }
      }

      <mat-form-field appearance="outline">
        <mat-label>Correo</mat-label>
        <input matInput formControlName="correo" type="email">
      </mat-form-field>
      @if (form.get('correo')?.touched) {
        @if (form.get('correo')?.hasError('required')) {
          <span class="field-error">Este campo es obligatorio</span>
        } @else if (form.get('correo')?.hasError('email')) {
          <span class="field-error">Ingresa un correo electrónico válido</span>
        }
      }
    </div>
  `
})
export class StepAdminServidorComponent {
  @Input() form!: FormGroup;
}
