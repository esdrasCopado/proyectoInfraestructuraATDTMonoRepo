import { Component, Input } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { Solicitud } from '../../../../models/solicitud.model';
import { EvidenciaDto, EvidenciaService } from '../../../../services/evidencia.service';

@Component({
  selector: 'app-etapa-vulnerabilidades',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './etapa-vulnerabilidades.component.html',
  styleUrl: './etapa-vulnerabilidades.component.scss',
})
export class EtapaVulnerabilidadesComponent {

  @Input({ required: true }) solicitud!: Solicitud;
  @Input() evidencias: EvidenciaDto[] = [];
  @Input() iteracion = 0;

  modoRechazo = false;
  formRechazo: FormGroup;
  descargando = new Set<number>();
  viendo      = new Set<number>();

  constructor(fb: FormBuilder, private evidenciaService: EvidenciaService) {
    this.formRechazo = fb.group({
      hallazgos: ['', [Validators.required, Validators.minLength(10)]],
    });
  }

  get dependencia():    string {
    return (this.solicitud.carta as any)?.areaRequirente?.dependencia
      || this.solicitud.dependencia || '—';
  }
  get nombreServidor(): string { return this.solicitud.nombreServidor; }
  get ip():             string {
    return this.solicitud.etapas.find(e => e.numero === 2)?.ip || '—';
  }
  get subdominios():    string {
    return this.solicitud.subdominios?.map(s => s.requestedName).join(', ') || '—';
  }

  getDisplayName(archivoNombre: string): string {
    const match = archivoNombre.match(/^[a-z_]+_ronda\d+_\d{14}_(.+)$/);
    return match ? match[1] : archivoNombre;
  }

  verPdf(f: EvidenciaDto): void {
    if (this.viendo.has(f.id)) return;
    this.viendo.add(f.id);
    this.evidenciaService.descargarEvidencia(f.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => URL.revokeObjectURL(url), 30_000);
        this.viendo.delete(f.id);
      },
      error: () => this.viendo.delete(f.id),
    });
  }

  descargar(f: EvidenciaDto): void {
    if (this.descargando.has(f.id)) return;
    this.descargando.add(f.id);
    this.evidenciaService.descargarEvidencia(f.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = this.getDisplayName(f.archivoNombre);
        a.click();
        URL.revokeObjectURL(url);
        this.descargando.delete(f.id);
      },
      error: () => this.descargando.delete(f.id),
    });
  }

  get hallazgos(): string {
    return this.formRechazo.get('hallazgos')?.value ?? '';
  }

  get formRechazoInvalido(): boolean {
    return this.formRechazo.invalid;
  }

  mostrarFormRechazo(): void { this.modoRechazo = true; }

  cancelarRechazo(): void {
    this.modoRechazo = false;
    this.formRechazo.reset();
  }

  marcarHallazgosPendiente(): void {
    this.modoRechazo = true;
    this.formRechazo.get('hallazgos')?.markAsTouched();
  }

  formatSize(kb: number): string {
    if (kb < 1) return `${Math.round(kb * 1024)} B`;
    if (kb < 1024) return `${kb.toFixed(1)} KB`;
    return `${(kb / 1024).toFixed(1)} MB`;
  }
}
