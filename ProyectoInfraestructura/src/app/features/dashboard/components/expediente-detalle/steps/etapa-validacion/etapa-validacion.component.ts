import { Component, Input, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Solicitud } from '../../../../models/solicitud.model';

const IPv4 = /^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$/;

@Component({
  selector: 'app-etapa-validacion',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule],
  templateUrl: './etapa-validacion.component.html',
  styleUrl: './etapa-validacion.component.scss'
})
export class EtapaValidacionComponent implements OnInit {

  @Input({ required: true }) solicitud!: Solicitud;

  form!: FormGroup;

  get specs() { return this.solicitud.carta?.specs; }

  get vCoresOriginal():         number { return this.specs?.vCores ?? 0; }
  get ramOriginal():            number { return this.specs?.memoriaRam ?? 0; }
  get almacenamientoOriginal(): number {
    return (this.specs?.discosDuros ?? []).reduce((a, d) => a + d.capacidad, 0);
  }
  get soLabel(): string {
    return ({ windows: 'Windows', linux: 'Linux', otro: 'Otro' } as const)
      [this.specs?.sistemaOperativo ?? 'otro'] ?? '—';
  }
  get tipoLabel(): string {
    return this.specs?.tipoRequerimiento === 'estandar' ? 'Estándar' : 'Específico';
  }

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      vCores:         [this.vCoresOriginal,         [Validators.required, Validators.min(1), Validators.max(128)]],
      memoriaRam:     [this.ramOriginal,             [Validators.required, Validators.min(1), Validators.max(1024)]],
      almacenamiento: [this.almacenamientoOriginal,  [Validators.required, Validators.min(1), Validators.max(102400)]],
      ip:             ['',                           [Validators.required, Validators.pattern(IPv4)]],
      comentarios:    [''],
    });
  }

  esMod(campo: string, original: number): boolean {
    const val = this.form.get(campo)?.value;
    return val !== null && val !== undefined && Number(val) !== original;
  }
}
