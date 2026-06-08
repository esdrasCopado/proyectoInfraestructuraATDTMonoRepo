import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-step-specs',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
  ],
  templateUrl: './step-specs.component.html',
  styleUrl: './step-specs.component.scss',
})
export class StepSpecsComponent implements OnInit, OnDestroy {
  @Input({ required: true }) form!: FormGroup;

  private sub = new Subscription();

  constructor(private fb: FormBuilder) {}

  get sistemaOperativo() { return this.form.get('sistemaOperativo')?.value; }
  get esEstandar()       { return this.form.get('tipoRequerimiento')?.value === 'estandar'; }
  get esRenovacion()     { return this.form.get('modalidad')?.value === 'renovacion'; }
  get discosDuros()      { return this.form.get('discosDuros') as FormArray; }

  ngOnInit(): void {
    this.sub.add(
      this.form.get('tipoRequerimiento')!.valueChanges.subscribe(val => {
        if (val === 'estandar') {
          this.form.patchValue({ vCores: 2, memoriaRam: 4 });
          // Resetear a un único disco estándar
          while (this.discosDuros.length > 1) {
            this.discosDuros.removeAt(this.discosDuros.length - 1);
          }
          this.discosDuros.at(0).patchValue({ capacidad: 50, tipo: 'SSD', etiqueta: '' });
        }
      })
    );
  }

  ngOnDestroy(): void { this.sub.unsubscribe(); }

  agregarDisco(): void {
    this.discosDuros.push(
      this.fb.group({
        capacidad: [null, [Validators.required, Validators.min(1)]],
        tipo:      ['SSD', Validators.required],
        etiqueta:  [''],
      })
    );
  }

  eliminarDisco(index: number): void {
    if (this.discosDuros.length > 1) {
      this.discosDuros.removeAt(index);
    }
  }

  hasError(field: string, error: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl?.touched && ctrl?.hasError(error));
  }

  hasDiscoError(index: number, field: string, error: string): boolean {
    const ctrl = this.discosDuros.at(index)?.get(field);
    return !!(ctrl?.touched && ctrl?.hasError(error));
  }

  isInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl?.touched && ctrl?.invalid);
  }
}
