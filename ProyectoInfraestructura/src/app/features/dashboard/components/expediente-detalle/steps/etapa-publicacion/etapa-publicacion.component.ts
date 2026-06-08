import { Component, Input } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Solicitud } from '../../../../models/solicitud.model';

@Component({
  selector: 'app-etapa-publicacion',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './etapa-publicacion.component.html',
  styleUrl: './etapa-publicacion.component.scss',
})
export class EtapaPublicacionComponent {

  @Input({ required: true }) solicitud!: Solicitud;

  form: FormGroup;

  constructor(fb: FormBuilder) {
    this.form = fb.group({ confirmado: [false, Validators.requiredTrue] });
  }

  get dependencia():    string {
    return (this.solicitud.carta as any)?.areaRequirente?.dependencia
      || this.solicitud.dependencia
      || '—';
  }
  get nombreServidor(): string { return this.solicitud.nombreServidor; }
  get ip():             string {
    return this.solicitud.etapas.find(e => e.numero === 2)?.ip || '—';
  }
  get subdominios():    string {
    return this.solicitud.subdominios?.map(s => s.requestedName).join(', ') || '—';
  }

  get vpnFolio(): string {
    return this.solicitud.vpns?.[0]?.folio || '—';
  }

  get etapa12Completada(): boolean {
    return this.solicitud.etapas.find(e => e.numero === 12)?.estado === 'completada';
  }

  get confirmadoInvalido(): boolean {
    const c = this.form.get('confirmado');
    return !!c?.invalid && !!c?.touched;
  }
}
