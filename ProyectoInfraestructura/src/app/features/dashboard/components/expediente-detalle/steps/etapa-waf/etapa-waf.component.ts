import { Component, Input } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Solicitud } from '../../../../models/solicitud.model';

@Component({
  selector: 'app-etapa-waf',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './etapa-waf.component.html',
  styleUrl: './etapa-waf.component.scss',
})
export class EtapaWafComponent {

  @Input({ required: true }) solicitud!: Solicitud;

  form: FormGroup;

  constructor(fb: FormBuilder) {
    this.form = fb.group({ confirmado: [false, Validators.requiredTrue] });
  }

  get ip(): string {
    return this.solicitud.etapas.find(e => e.numero === 2)?.ip || '—';
  }
  get dependencia():    string { return this.solicitud.dependencia; }
  get nombreServidor(): string { return this.solicitud.nombreServidor; }
  get puertos():        string { return this.solicitud.carta?.specs?.puertos || '—'; }
  get subdominios(): string {
    const deSolicitud = this.solicitud.subdominios ?? [];
    if (deSolicitud.length) return deSolicitud.map(s => s.requestedName).join(', ');
    return this.solicitud.carta?.infraestructura?.subdominios?.map(s => s.subdominio).join(', ') || '—';
  }
  get requiereSSL(): boolean {
    const deSolicitud = this.solicitud.subdominios ?? [];
    if (deSolicitud.length) return deSolicitud.some(s => s.sslRequired);
    return this.solicitud.carta?.infraestructura?.requiereSSL ?? false;
  }

  get confirmadoInvalido(): boolean {
    const c = this.form.get('confirmado');
    return !!c?.invalid && !!c?.touched;
  }
}
