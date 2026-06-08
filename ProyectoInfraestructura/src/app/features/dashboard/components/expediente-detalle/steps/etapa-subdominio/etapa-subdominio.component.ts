import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Solicitud, SubdominioServidor } from '../../../../models/solicitud.model';
import { SubdominioEntry } from '../../../../../carta-aprovisionamiento/models/carta-aprovisionamiento.model';

@Component({
  selector: 'app-etapa-subdominio',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './etapa-subdominio.component.html',
  styleUrl: './etapa-subdominio.component.scss',
})
export class EtapaSubdominioComponent implements OnInit {

  @Input({ required: true }) solicitud!: Solicitud;

  form!: FormGroup;

  // ── Getters de referencia ────────────────────────────────────────────────

  private get subdominiosServidor(): SubdominioServidor[] {
    return this.solicitud.subdominios ?? [];
  }

  private get subdominiosCarta(): SubdominioEntry[] {
    return this.solicitud.carta?.infraestructura?.subdominios ?? [];
  }

  get subArray(): FormArray {
    return this.form.get('subdominios') as FormArray;
  }

  get subControls(): FormGroup[] {
    return this.subArray.controls as FormGroup[];
  }

  get ip(): string {
    return this.solicitud.etapas.find(e => e.numero === 2)?.ip || '—';
  }

  get dependencia(): string { return this.solicitud.dependencia || '—'; }

  get requiereSSL(): boolean {
    const deCarta = this.solicitud.carta?.infraestructura?.requiereSSL;
    if (deCarta != null) return deCarta;
    return this.subdominiosServidor.some(s => s.sslRequired);
  }

  get totalVpns(): number {
    return this.solicitud.vpns?.length
        ?? this.solicitud.carta?.infraestructura?.vpns?.length
        ?? 0;
  }

  // ── Progreso ─────────────────────────────────────────────────────────────

  get confirmadas(): number {
    return this.subControls.filter(g => g.get('confirmado')?.value === true).length;
  }

  get total(): number { return this.subControls.length; }

  get todasConfirmadas(): boolean { return this.total > 0 && this.confirmadas === this.total; }

  // ── Helpers de display ───────────────────────────────────────────────────

  subRef(i: number): SubdominioEntry | null {
    const deCarta = this.subdominiosCarta[i];
    if (deCarta) return deCarta;

    const srv = this.subdominiosServidor[i];
    if (!srv) return null;

    return {
      subdominio: srv.requestedName,
      puerto:     String(srv.port),
    };
  }

  sslLabel(i?: number): string {
    if (i != null) {
      const srv = this.subdominiosServidor[i];
      if (srv) return srv.sslRequired ? 'Sí' : 'No';
    }
    return this.requiereSSL ? 'Sí' : 'No';
  }

  // ── Construcción ─────────────────────────────────────────────────────────

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    const servidorSubs = this.subdominiosServidor;
    const cartaSubs    = this.subdominiosCarta;
    const count        = Math.max(servidorSubs.length, cartaSubs.length, 1);

    const grupos = Array.from({ length: count }, (_, i) =>
      this.nuevoGrupo(servidorSubs[i])
    );

    this.form = this.fb.group({ subdominios: this.fb.array(grupos) });
  }

  private nuevoGrupo(sub?: SubdominioServidor): FormGroup {
    return this.fb.group({
      subdominioAprobado: [sub?.requestedName ?? '', [
        Validators.required,
        Validators.pattern(/^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$/),
      ]],
      confirmado: [false, Validators.requiredTrue],
    });
  }

  // ── Acciones ─────────────────────────────────────────────────────────────

  confirmar(i: number): void {
    const g = this.subControls[i];
    g.get('subdominioAprobado')?.markAsTouched();
    if (g.get('subdominioAprobado')?.invalid) return;
    g.get('confirmado')?.setValue(true);
  }

  editar(i: number): void {
    this.subControls[i].get('confirmado')?.setValue(false);
  }

  agregarSubdominio(): void {
    this.subArray.push(this.nuevoGrupo());
  }
}
