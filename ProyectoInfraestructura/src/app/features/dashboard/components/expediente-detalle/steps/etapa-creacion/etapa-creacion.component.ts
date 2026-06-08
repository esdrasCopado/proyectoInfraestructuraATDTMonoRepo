import { Component, Input, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Solicitud } from '../../../../models/solicitud.model';

@Component({
  selector: 'app-etapa-creacion',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './etapa-creacion.component.html',
  styleUrl: './etapa-creacion.component.scss'
})
export class EtapaCreacionComponent implements OnInit {

  @Input({ required: true }) solicitud!: Solicitud;

  form!: FormGroup;

  private get etapa2() {
    return this.solicitud.etapas.find(e => e.numero === 2);
  }

  private get specs() { return this.solicitud.carta?.specs; }

  get vCores(): number {
    return this.etapa2?.vCores ?? this.specs?.vCores ?? 0;
  }

  get memoriaRam(): number {
    return this.etapa2?.memoriaRam ?? this.specs?.memoriaRam ?? 0;
  }

  get almacenamiento(): number {
    return this.etapa2?.almacenamiento
      ?? (this.specs?.discosDuros ?? []).reduce((a, d) => a + d.capacidad, 0);
  }

  get ip(): string {
    return this.etapa2?.ip || '—';
  }

  get soLabel(): string {
    return ({ windows: 'Windows Server', linux: 'Linux', otro: 'Otro' } as const)
      [this.specs?.sistemaOperativo ?? 'otro'] ?? '—';
  }

  get confirmadoInvalido(): boolean {
    const ctrl = this.form.get('confirmado');
    return !!ctrl?.invalid && !!ctrl?.touched;
  }

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      confirmado: [false, Validators.requiredTrue],
    });
  }
}
