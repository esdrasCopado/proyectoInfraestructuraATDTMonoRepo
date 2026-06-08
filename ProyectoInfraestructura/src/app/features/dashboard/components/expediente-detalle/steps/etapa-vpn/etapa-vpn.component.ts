import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Solicitud, VpnServidor } from '../../../../models/solicitud.model';
import { VpnEntry } from '../../../../../carta-aprovisionamiento/models/carta-aprovisionamiento.model';

@Component({
  selector: 'app-etapa-vpn',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './etapa-vpn.component.html',
  styleUrl: './etapa-vpn.component.scss',
})
export class EtapaVpnComponent implements OnInit {

  @Input({ required: true }) solicitud!: Solicitud;

  form!: FormGroup;

  // ── Getters de referencia ────────────────────────────────────────────────

  private get vpnsServidor(): VpnServidor[] {
    return this.solicitud.vpns ?? [];
  }

  private get vpnsCarta(): VpnEntry[] {
    return this.solicitud.carta?.infraestructura?.vpns ?? [];
  }

  get vpnsArray(): FormArray {
    return this.form.get('vpns') as FormArray;
  }

  get vpnsControls(): FormGroup[] {
    return this.vpnsArray.controls as FormGroup[];
  }

  get ip(): string {
    return this.solicitud.etapas.find(e => e.numero === 2)?.ip || '—';
  }

  get dependencia(): string { return this.solicitud.dependencia || '—'; }

  get subdominios(): string[] {
    const deCarta = this.solicitud.carta?.infraestructura?.subdominios ?? [];
    if (deCarta.length > 0) {
      return deCarta.map(s => s.subdominio).filter(Boolean);
    }
    return (this.solicitud.subdominios ?? []).map(s => s.requestedName).filter(Boolean);
  }

  // ── Progreso ─────────────────────────────────────────────────────────────

  get confirmadas(): number {
    return this.vpnsControls.filter(g => g.get('confirmada')?.value === true).length;
  }

  get total(): number { return this.vpnsControls.length; }

  get todasConfirmadas(): boolean { return this.total > 0 && this.confirmadas === this.total; }

  // ── Helpers de display ───────────────────────────────────────────────────

  // Devuelve datos de referencia: primero de la carta, luego del servidor
  vpnRef(i: number): VpnEntry | null {
    const deCarta = this.vpnsCarta[i];
    if (deCarta) return deCarta;

    const srv = this.vpnsServidor[i];
    if (!srv) return null;

    return {
      tipoVpn:           srv.vpnType as VpnEntry['tipoVpn'],
      vpnResponsable:    srv.responsable,
      vpnCargo:          srv.cargo           ?? '',
      vpnTelefono:       srv.phone           ?? '',
      vpnCorreo:         srv.email           ?? '',
      vpnPerfilAnterior: srv.perfilAnterior  ?? '',
      vpnServidores:     srv.servidores      ?? '',
      vpnFolio:          srv.folio           ?? '',
      vpnIp:             srv.vpnIp           ?? '',
      vpnEmpresa:        srv.empresa         ?? '',
      vpnVigencia:       srv.vigenciaDias != null ? String(srv.vigenciaDias) as VpnEntry['vpnVigencia'] : '',
    };
  }

  tipoLabel(tipo: string): string {
    const map: Record<string, string> = {
      dependencia:   'Dependencia',
      actualizacion: 'Actualización',
      proveedor:     'Proveedor',
    };
    return map[tipo] ?? tipo;
  }

  vigenciaLabel(v: string): string {
    return v ? `${v} días` : '—';
  }

  // ── Construcción ─────────────────────────────────────────────────────────

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    const servidorVpns = this.vpnsServidor;
    const cartaVpns    = this.vpnsCarta;
    const count        = Math.max(servidorVpns.length, cartaVpns.length, 1);

    const grupos = Array.from({ length: count }, (_, i) =>
      this.nuevoGrupo(servidorVpns[i])
    );

    this.form = this.fb.group({ vpns: this.fb.array(grupos) });
  }

  private nuevoGrupo(vpn?: VpnServidor): FormGroup {
    return this.fb.group({
      idVpn:      [vpn?.folio  ?? '', Validators.required],
      ipVpn:      [vpn?.vpnIp  ?? ''],
      confirmada: [false, Validators.requiredTrue],
    });
  }

  // ── Acciones ─────────────────────────────────────────────────────────────

  confirmar(i: number): void {
    const g = this.vpnsControls[i];
    g.get('idVpn')?.markAsTouched();
    if (g.get('idVpn')?.invalid) return;
    g.get('confirmada')?.setValue(true);
  }

  editar(i: number): void {
    this.vpnsControls[i].get('confirmada')?.setValue(false);
  }

  agregarVpn(): void {
    this.vpnsArray.push(this.nuevoGrupo());
  }
}
