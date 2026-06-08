import { Component, Input, ViewChild, ElementRef } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { Solicitud } from '../../../../models/solicitud.model';
import { EvidenciaDto } from '../../../../services/evidencia.service';

const MAX_MB = 30;
const MAX_BYTES = MAX_MB * 1024 * 1024;

@Component({
  selector: 'app-etapa-evidencias',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './etapa-evidencias.component.html',
  styleUrl: './etapa-evidencias.component.scss',
})
export class EtapaEvidenciasComponent {

  @Input({ required: true }) solicitud!: Solicitud;
  @Input() esRevision = false;
  @Input() motivoRechazoAnterior = '';
  @Input() iteracion = 0;
  @Input() historicoRechazadas: EvidenciaDto[] = [];
  @Input() evidenciasPendientes: EvidenciaDto[] = [];

  @ViewChild('fileInput') fileInputRef!: ElementRef<HTMLInputElement>;

  form: FormGroup;
  archivos: File[] = [];
  errorArchivos = false;
  erroresTamano: string[] = [];
  arrastrando = false;

  constructor(fb: FormBuilder) {
    this.form = fb.group({ confirmado: [false, Validators.requiredTrue] });
  }

  get dependencia():    string { return this.solicitud.dependencia; }
  get nombreServidor(): string { return this.solicitud.nombreServidor; }

  get confirmadoInvalido(): boolean {
    const c = this.form.get('confirmado');
    return !!c?.invalid && !!c?.touched;
  }

  // ── File input ───────────────────────────────────────────────────────────

  abrirSelector(): void {
    this.fileInputRef.nativeElement.value = '';
    this.fileInputRef.nativeElement.click();
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) this.procesarArchivos(Array.from(input.files));
  }

  onDragover(event: DragEvent): void {
    event.preventDefault();
    this.arrastrando = true;
  }

  onDragleave(): void { this.arrastrando = false; }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.arrastrando = false;
    const files = event.dataTransfer?.files;
    if (files) this.procesarArchivos(Array.from(files));
  }

  private procesarArchivos(files: File[]): void {
    this.erroresTamano = [];
    for (const f of files) {
      if (f.type !== 'application/pdf') continue;
      if (f.size > MAX_BYTES) {
        this.erroresTamano.push(`${f.name} supera el límite de ${MAX_MB} MB`);
        continue;
      }
      if (!this.archivos.find(a => a.name === f.name && a.size === f.size)) {
        this.archivos.push(f);
      }
    }
    if (this.archivos.length > 0) this.errorArchivos = false;
  }

  eliminar(i: number): void {
    this.archivos.splice(i, 1);
  }

  getDisplayName(archivoNombre: string): string {
    const match = archivoNombre.match(/^[a-z_]+_ronda\d+_\d{14}_(.+)$/);
    return match ? match[1] : archivoNombre;
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
