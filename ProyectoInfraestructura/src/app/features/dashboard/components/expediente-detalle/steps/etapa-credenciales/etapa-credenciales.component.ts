import { Component, Input } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { jsPDF } from 'jspdf';
import { Solicitud, EtapaSolicitud } from '../../../../models/solicitud.model';

@Component({
  selector: 'app-etapa-credenciales',
  standalone: true,
  imports: [CommonModule, DatePipe, ReactiveFormsModule],
  templateUrl: './etapa-credenciales.component.html',
  styleUrl: './etapa-credenciales.component.scss',
})
export class EtapaCredencialesComponent {

  @Input({ required: true }) solicitud!: Solicitud;

  form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      entregaConfirmada: [false, Validators.requiredTrue],
    });
  }

  // ── Acceso a carta y etapas ──────────────────────────────────────────────

  private get carta() { return this.solicitud.carta; }
  private get specs() { return this.carta?.specs; }

  private etapa(n: number): EtapaSolicitud | undefined {
    return this.solicitud.etapas.find(e => e.numero === n);
  }

  // ── Identificación ───────────────────────────────────────────────────────

  get folio():         string { return this.solicitud.folio; }
  get dependencia():   string { return this.solicitud.dependencia; }
  get nombreServidor():string { return this.solicitud.nombreServidor; }
  get fechaRegistro(): string { return this.solicitud.fechaRegistro; }

  // ── Recursos asignados (etapa 2 → carta como fallback) ──────────────────

  get vCores(): number {
    return this.etapa(2)?.vCores ?? this.specs?.vCores ?? 0;
  }

  get memoriaRam(): number {
    return this.etapa(2)?.memoriaRam ?? this.specs?.memoriaRam ?? 0;
  }

  get almacenamiento(): number {
    return this.etapa(2)?.almacenamiento
      ?? (this.specs?.discosDuros ?? []).reduce((a, d) => a + d.capacidad, 0);
  }

  get ip(): string {
    return this.solicitud.etapas.find(e => e.numero === 2)?.ip || '—';
  }

  get so(): string {
    const raw = this.specs?.sistemaOperativo ?? '';
    if (raw === 'windows') return 'Windows Server';
    if (raw === 'linux')   return 'Linux';
    if (raw === 'otro')    return this.specs?.sistemaOperativoOtro || 'Otro';
    return '—';
  }

  get motorBD():     string { return this.specs?.motorBD     || '—'; }
  get puertos():     string { return this.specs?.puertos     || '—'; }
  get integraciones():string { return this.specs?.integraciones || '—'; }

  // ── Verificaciones técnicas (etapas 4-6) ─────────────────────────────────

  get verificaciones(): { nombre: string; rf: string; etapa: EtapaSolicitud | undefined }[] {
    return [
      { nombre: 'Comunicaciones',      rf: 'RF05', etapa: this.etapa(4) },
      { nombre: 'Parches y actualizaciones', rf: 'RF06', etapa: this.etapa(5) },
      { nombre: 'XDR y agente',        rf: 'RF07', etapa: this.etapa(6) },
    ];
  }

  estadoLabel(e: EtapaSolicitud | undefined): string {
    if (!e) return 'Sin datos';
    return e.estado === 'completada' ? 'Completada' : e.estado === 'en-curso' ? 'En curso' : 'Sin iniciar';
  }

  estadoOk(e: EtapaSolicitud | undefined): boolean {
    return e?.estado === 'completada';
  }

  // ── VPNs (etapa 7) ───────────────────────────────────────────────────────

  get totalVpns(): number {
    return this.solicitud.vpns?.length ?? this.carta?.infraestructura?.vpns?.length ?? 0;
  }

  get vpnsSummary(): string {
    const vpns = this.solicitud.vpns ?? this.carta?.infraestructura?.vpns ?? [];
    if (!vpns.length) return '—';
    const counts: Record<string, number> = {};
    for (const v of vpns) {
      const tipo = 'vpnType' in v ? v.vpnType : (v as any).tipoVpn;
      counts[tipo] = (counts[tipo] ?? 0) + 1;
    }
    const etiquetas: Record<string, string> = {
      dependencia: 'Dependencia', actualizacion: 'Actualización', proveedor: 'Proveedor',
    };
    return Object.entries(counts)
      .map(([t, n]) => `${n} ${etiquetas[t] ?? t}`)
      .join(', ');
  }

  // ── Subdominios (etapa 8) ────────────────────────────────────────────────

  get totalSubdominios(): number {
    return this.solicitud.subdominios?.length ?? this.carta?.infraestructura?.subdominios?.length ?? 0;
  }

  get subdominiosSummary(): string {
    if (this.solicitud.subdominios?.length) {
      return this.solicitud.subdominios.map(s => s.requestedName).filter(Boolean).join(', ');
    }
    const subs = this.carta?.infraestructura?.subdominios ?? [];
    return subs.map(s => s.subdominio).filter(Boolean).join(', ') || '—';
  }

  get requiereSSL(): boolean {
    if (this.solicitud.subdominios?.length) {
      return this.solicitud.subdominios.some(s => s.sslRequired);
    }
    return this.carta?.infraestructura?.requiereSSL ?? false;
  }

  // ── Validación del checkbox ──────────────────────────────────────────────

  get confirmadoInvalido(): boolean {
    const ctrl = this.form.get('entregaConfirmada');
    return !!ctrl?.invalid && !!ctrl?.touched;
  }

  // ── Generación del PDF ───────────────────────────────────────────────────

  descargarPDF(): void {
    const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });
    this.buildPdf(doc);
    doc.save(`carta-responsiva-${this.solicitud.folio}.pdf`);
  }

  private buildPdf(doc: jsPDF): void {
    const lm = 20;
    const rm = 190;
    const cw = rm - lm;
    let y = 20;

    const nl = (mm = 5) => { y += mm; };
    const checkPage = (need = 20) => {
      if (y + need > 275) { doc.addPage(); y = 20; }
    };

    const sectionHeader = (title: string) => {
      checkPage(14);
      doc.setFillColor(239, 246, 255);
      doc.setDrawColor(219, 234, 254);
      doc.roundedRect(lm, y - 5, cw, 9, 1, 1, 'FD');
      doc.setFontSize(9);
      doc.setFont('helvetica', 'bold');
      doc.text(title, lm + 3, y + 1);
      doc.setFont('helvetica', 'normal');
      nl(11);
    };

    const kv = (key: string, value: string, indent = 0) => {
      checkPage(6);
      doc.setFontSize(8.5);
      doc.setFont('helvetica', 'bold');
      doc.text(`${key}:`, lm + indent, y);
      doc.setFont('helvetica', 'normal');
      const lines = doc.splitTextToSize(value, cw - 60 - indent);
      doc.text(lines[0] ?? '', lm + 58 + indent, y);
      nl(5.5);
      for (let i = 1; i < lines.length; i++) {
        checkPage(5);
        doc.text(lines[i], lm + 58 + indent, y);
        nl(5.5);
      }
    };

    const subLabel = (text: string) => {
      checkPage(8);
      doc.setFontSize(8.5);
      doc.setFont('helvetica', 'bold');
      doc.text(text, lm, y);
      doc.setFont('helvetica', 'normal');
      nl(5.5);
    };

    const hr = () => {
      nl(3);
      doc.setDrawColor(220, 220, 220);
      doc.line(lm, y, rm, y);
      nl(6);
    };

    // ── Encabezado ────────────────────────────────────────────────────────
    doc.setFontSize(10);
    doc.setFont('helvetica', 'bold');
    doc.text('AGENCIA DE TECNOLOGÍAS DE LA INFORMACIÓN Y DIGITALIZACIÓN', 105, y, { align: 'center' });
    nl(5);
    doc.text('DEL ESTADO DE SONORA  ·  ATDS', 105, y, { align: 'center' });
    nl(7);
    doc.setFontSize(11.5);
    doc.text('CARTA RESPONSIVA DE APROVISIONAMIENTO DE SERVIDOR VIRTUAL', 105, y, { align: 'center' });
    nl(5);
    doc.setFontSize(8.5);
    doc.setFont('helvetica', 'normal');
    const fechaStr = new Date(this.solicitud.fechaRegistro)
      .toLocaleDateString('es-MX', { day: '2-digit', month: 'long', year: 'numeric' });
    doc.text(`Folio: ${this.solicitud.folio}   ·   Fecha de registro: ${fechaStr}`, 105, y, { align: 'center' });
    nl(4);
    doc.setDrawColor(100, 100, 100);
    doc.setLineWidth(0.4);
    doc.line(lm, y, rm, y);
    doc.setLineWidth(0.2);
    nl(8);

    // ── Apartado 1 — Datos de contacto ───────────────────────────────────
    sectionHeader('Apartado 1 — Datos de contacto y ente solicitante');
    const ar = this.carta?.areaRequirente;
    const adm = this.carta?.adminServidor;
    if (ar) {
      subLabel('Área requirente');
      kv('Sector',       ar.sector       || '—');
      kv('Dependencia',  ar.dependencia  || '—');
      kv('Responsable',  ar.responsable  || '—');
      kv('Cargo',        ar.cargo        || '—');
      kv('Teléfono',     ar.telefono     || '—');
      kv('Correo',       ar.correo       || '—');
      nl(3);
    }
    if (adm) {
      subLabel('Administrador del servidor');
      kv('Responsable',  adm.responsable || '—');
      kv('Cargo',        adm.cargo       || '—');
      kv('Teléfono',     adm.telefono    || '—');
      kv('Correo',       adm.correo      || '—');
      kv('Proveedor',    adm.proveedor   || '—');
    }
    hr();

    // ── Apartado 2 — Descripción general ─────────────────────────────────
    checkPage(30);
    sectionHeader('Apartado 2 — Descripción general del requerimiento');
    const desc = this.carta?.descripcion;
    if (desc) {
      kv('Nombre del servidor',     desc.nombreServidor   || '—');
      kv('Nombre de la aplicación', desc.nombreAplicacion || '—');
      kv('Tipo de uso',             desc.tipoUso === 'publicado' ? 'Publicado en internet' : 'Uso interno');
      kv('Fecha de arranque',       desc.fechaArranque    || '—');
      kv('Vigencia',                desc.vigencia         || '—');
      if (desc.descripcionServidor) {
        checkPage(14);
        subLabel('Descripción del servidor');
        const lines = doc.splitTextToSize(desc.descripcionServidor, cw);
        for (const line of lines) { checkPage(5); doc.text(line, lm, y); nl(5); }
      }
    }
    hr();

    // ── Apartado 3 — Especificaciones técnicas ────────────────────────────
    checkPage(40);
    sectionHeader('Apartado 3 — Especificaciones técnicas asignadas');
    kv('vCPU asignadas',          `${this.vCores} núcleos`);
    kv('Memoria RAM asignada',    `${this.memoriaRam} GB`);
    kv('Almacenamiento asignado', `${this.almacenamiento} GB`);
    kv('Dirección IP',            this.ip);
    kv('Sistema operativo',       this.so);
    kv('Motor de base de datos',  this.motorBD);
    kv('Puertos requeridos',      this.puertos);
    if (this.specs?.integraciones) kv('Integraciones', this.specs.integraciones);
    if (this.specs?.otrasSpecs)    kv('Otras especificaciones', this.specs.otrasSpecs);
    hr();

    // ── Apartado 4 — Infraestructura de red ──────────────────────────────
    checkPage(30);
    sectionHeader('Apartado 4 — Infraestructura de red');
    subLabel('VPNs asignadas');
    const vpns = this.solicitud.vpns ?? this.carta?.infraestructura?.vpns ?? [];
    const etiquetasTipo: Record<string, string> = {
      dependencia: 'Dependencia', actualizacion: 'Actualización', proveedor: 'Proveedor',
    };
    if (vpns.length) {
      vpns.forEach((v: any, i) => {
        checkPage(22);
        doc.setFontSize(8.5);
        doc.setFont('helvetica', 'bold');
        doc.text(`VPN #${i + 1}`, lm + 4, y); nl(5);
        doc.setFont('helvetica', 'normal');
        const tipo = v.vpnType ?? v.tipoVpn ?? '';
        kv('Tipo',        (etiquetasTipo[tipo] ?? tipo) || '—', 8);
        kv('Responsable', v.responsable ?? v.vpnResponsable ?? '—', 8);
        kv('Empresa',     v.empresa     ?? v.vpnEmpresa     ?? '—', 8);
        const vigencia = v.vigenciaDias ?? v.vpnVigencia;
        kv('Vigencia',    vigencia ? `${vigencia} días` : '—', 8);
        nl(2);
      });
    } else {
      doc.setFontSize(8.5);
      doc.text('Sin VPNs registradas.', lm, y); nl(6);
    }

    nl(3);
    subLabel('Subdominios asignados');
    const subs = this.solicitud.subdominios ?? this.carta?.infraestructura?.subdominios ?? [];
    if (subs.length) {
      subs.forEach((s: any, i) => {
        checkPage(16);
        doc.setFontSize(8.5);
        doc.setFont('helvetica', 'bold');
        doc.text(`Subdominio #${i + 1}`, lm + 4, y); nl(5);
        doc.setFont('helvetica', 'normal');
        kv('Subdominio', s.requestedName ?? s.subdominio ?? '—', 8);
        kv('Puerto',     s.port != null ? String(s.port) : s.puerto ?? '—', 8);
        kv('SSL',        (s.sslRequired ?? this.requiereSSL) ? 'Sí' : 'No', 8);
        nl(2);
      });
    } else {
      doc.setFontSize(8.5);
      doc.text('Sin subdominios registrados.', lm, y); nl(6);
    }
    hr();

    // ── Apartado 5 — Responsabilidades ───────────────────────────────────
    checkPage(50);
    sectionHeader('Apartado 5 — Responsabilidades de las partes');
    const clausulas = [
      '1. La Agencia de Tecnologías de la Información y Digitalización del Estado de Sonora (ATDS) proveerá los recursos de cómputo en las condiciones aprobadas en el Apartado 3 de esta carta y en los plazos acordados.',
      '2. La dependencia requirente utilizará el servidor exclusivamente para los fines descritos en el Apartado 2. Cualquier cambio de uso requiere nueva solicitud.',
      '3. La ATDS proporcionará las credenciales de acceso iniciales por canal externo seguro. Se solicita expresamente que sean modificadas en el primer acceso.',
      '4. El administrador del servidor designado por la dependencia será responsable de la administración del mismo en su totalidad, incluyendo la gestión de usuarios, respaldos y actualizaciones de la aplicación.',
      '5. La dependencia se compromete a aplicar en tiempo y forma las actualizaciones de seguridad del sistema operativo que la ATDS recomiende o indique.',
      '6. El sistema SRIS no almacena ni transmite credenciales de acceso al servidor. La entrega se realiza por canal externo y se registra el acuse en este documento.',
    ];
    doc.setFontSize(8.5);
    doc.setFont('helvetica', 'normal');
    for (const c of clausulas) {
      const lines = doc.splitTextToSize(c, cw);
      for (const line of lines) { checkPage(5); doc.text(line, lm, y); nl(5); }
      nl(2);
    }
    hr();

    // ── Apartado 6 — Responsiva ───────────────────────────────────────────
    checkPage(60);
    sectionHeader('Apartado 6 — Responsiva');
    const resp = this.carta?.responsiva;
    if (resp) {
      kv('Firmante',        resp.firmante       || '—');
      kv('N.º de empleado', resp.numEmpleado    || '—');
      kv('Puesto',          resp.puestoFirmante || '—');
    }
    nl(12);

    const sigW = cw * 0.38;
    const c1x  = lm + cw * 0.03;
    const c2x  = lm + cw * 0.59;

    doc.setDrawColor(100, 100, 100);
    doc.line(c1x, y, c1x + sigW, y);
    doc.line(c2x, y, c2x + sigW, y);
    nl(5);
    doc.setFontSize(8);
    doc.setFont('helvetica', 'bold');
    doc.text('Firmante del Área Requirente', c1x + sigW / 2, y, { align: 'center' });
    doc.text('Director del Centro de Datos', c2x + sigW / 2, y, { align: 'center' });
    nl(4);
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(7.5);
    doc.text(resp?.firmante || '', c1x + sigW / 2, y, { align: 'center' });
    doc.text('Agencia de Tecnologías de la Información', c2x + sigW / 2, y, { align: 'center' });
    nl(4);
    doc.text(resp?.puestoFirmante || '', c1x + sigW / 2, y, { align: 'center' });
    doc.text(
      `Fecha: ${new Date().toLocaleDateString('es-MX', { day: '2-digit', month: 'long', year: 'numeric' })}`,
      c2x + sigW / 2, y, { align: 'center' },
    );
  }
}
