import { Component, Input } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { PhoneMaskDirective } from '../../../../../shared/directives/phone-mask.directive';

@Component({
  selector: 'app-step-contacto',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, PhoneMaskDirective],
  templateUrl: './step-contacto.component.html',
  styleUrl: './step-contacto.component.scss',
})
export class StepContactoComponent {
  @Input({ required: true }) form!: FormGroup;

  get area() { return this.form.get('areaRequirente') as FormGroup; }
  get admin() { return this.form.get('adminServidor') as FormGroup; }

  hasError(group: FormGroup, field: string, error: string): boolean {
    const ctrl = group.get(field);
    return !!(ctrl?.touched && ctrl?.hasError(error));
  }

  isInvalid(group: FormGroup, field: string): boolean {
    const ctrl = group.get(field);
    return !!(ctrl?.touched && ctrl?.invalid);
  }
}
