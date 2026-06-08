import { Component, Input, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { Solicitud } from '../../../../models/solicitud.model';
import { EvidenciaDto, EvidenciaService } from '../../../../services/evidencia.service';

@Component({
  selector: 'app-etapa-validacion-evidencias',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './etapa-validacion-evidencias.component.html',
  styleUrl: './etapa-validacion-evidencias.component.scss',
})
export class EtapaValidacionEvidenciasComponent {

  @Input({ required: true }) solicitud!: Solicitud;
  @Input() evidencias: EvidenciaDto[] = [];
  @Input() iteracion = 0;
  @Output() estadoCambiado = new EventEmitter<void>();

  // ── Panel de rechazo global (usado desde el panel derecho) ───────────────
  modoRechazo = false;
  formRechazo: FormGroup;

  // ── Validación por archivo ───────────────────────────────────────────────
  descargando = new Set<number>();
  viendo      = new Set<number>();
  validando   = new Set<number>();
  rechazandoId: number | null = null;
  private estadoLocal = new Map<number, 'pendiente' | 'aprobada' | 'rechazada'>();
  private motivoPorId = new Map<number, string>();

  constructor(fb: FormBuilder, private evidenciaService: EvidenciaService, private cdr: ChangeDetectorRef) {
    this.formRechazo = fb.group({
      comentario: ['', [Validators.required, Validators.minLength(10)]],
    });
  }

  get dependencia():    string { return this.solicitud.dependencia; }
  get nombreServidor(): string { return this.solicitud.nombreServidor; }

  get motivoRechazo(): string {
    return this.formRechazo.get('comentario')?.value ?? '';
  }

  get formRechazoInvalido(): boolean {
    return this.formRechazo.invalid;
  }

  get todasAprobadas(): boolean {
    if (this.evidencias.length === 0) return false;
    return this.evidencias.every(f => this.getEstado(f) === 'aprobada');
  }

  get hayAlgunaRechazada(): boolean {
    return this.evidencias.some(f => this.getEstado(f) === 'rechazada');
  }

  mostrarFormRechazo(): void { this.modoRechazo = true; }

  cancelarRechazo(): void {
    this.modoRechazo = false;
    this.formRechazo.reset();
  }

  marcarComentarioPendiente(): void {
    this.modoRechazo = true;
    this.formRechazo.get('comentario')?.markAsTouched();
  }

  // ── Estado por archivo ───────────────────────────────────────────────────

  getEstado(f: EvidenciaDto): 'pendiente' | 'aprobada' | 'rechazada' {
    return this.estadoLocal.get(f.id) ?? f.estadoValidacion;
  }

  getMotivo(id: number): string {
    return this.motivoPorId.get(id) ?? '';
  }

  setMotivo(id: number, value: string): void {
    this.motivoPorId.set(id, value);
  }

  iniciarRechazoArchivo(id: number): void {
    this.rechazandoId = id;
    if (!this.motivoPorId.has(id)) this.motivoPorId.set(id, '');
    this.cdr.detectChanges();
  }

  cancelarRechazoArchivo(): void {
    this.rechazandoId = null;
    this.cdr.detectChanges();
  }

  aprobarArchivo(f: EvidenciaDto): void {
    if (this.validando.has(f.id)) return;
    this.validando.add(f.id);
    this.cdr.detectChanges();
    this.evidenciaService.validarEvidencia(f.id, { estadoValidacion: 'aprobada' }).subscribe({
      next: () => {
        this.estadoLocal.set(f.id, 'aprobada');
        this.validando.delete(f.id);
        this.estadoCambiado.emit();
        this.cdr.detectChanges();
      },
      error: () => {
        this.validando.delete(f.id);
        this.cdr.detectChanges();
      },
    });
  }

  rechazarArchivo(f: EvidenciaDto): void {
    const motivo = this.getMotivo(f.id).trim();
    if (motivo.length < 10) return;
    this.estadoLocal.set(f.id, 'rechazada');
    this.rechazandoId = null;
    this.estadoCambiado.emit();
    this.cdr.detectChanges();
  }

  get rechazadosLocalmente(): Array<{ id: number; motivo: string }> {
    const result: Array<{ id: number; motivo: string }> = [];
    this.estadoLocal.forEach((estado, id) => {
      if (estado === 'rechazada') {
        result.push({ id, motivo: this.motivoPorId.get(id) ?? '' });
      }
    });
    return result;
  }

  // ── Descarga ─────────────────────────────────────────────────────────────

  verPdf(f: EvidenciaDto): void {
    if (this.viendo.has(f.id)) return;
    this.viendo.add(f.id);
    this.cdr.detectChanges();
    this.evidenciaService.descargarEvidencia(f.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => URL.revokeObjectURL(url), 30_000);
        this.viendo.delete(f.id);
        this.cdr.detectChanges();
      },
      error: () => {
        this.viendo.delete(f.id);
        this.cdr.detectChanges();
      },
    });
  }

  descargar(f: EvidenciaDto): void {
    if (this.descargando.has(f.id)) return;
    this.descargando.add(f.id);
    this.cdr.detectChanges();
    this.evidenciaService.descargarEvidencia(f.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = this.getDisplayName(f.archivoNombre);
        a.click();
        URL.revokeObjectURL(url);
        this.descargando.delete(f.id);
        this.cdr.detectChanges();
      },
      error: () => {
        this.descargando.delete(f.id);
        this.cdr.detectChanges();
      },
    });
  }

  // ── Utilidades ───────────────────────────────────────────────────────────

  formatSize(kb: number): string {
    if (kb < 1) return `${Math.round(kb * 1024)} B`;
    if (kb < 1024) return `${kb.toFixed(1)} KB`;
    return `${(kb / 1024).toFixed(1)} MB`;
  }

  getDisplayName(archivoNombre: string): string {
    const match = archivoNombre.match(/^[a-z_]+_ronda\d+_\d{14}_(.+)$/);
    return match ? match[1] : archivoNombre;
  }
}
