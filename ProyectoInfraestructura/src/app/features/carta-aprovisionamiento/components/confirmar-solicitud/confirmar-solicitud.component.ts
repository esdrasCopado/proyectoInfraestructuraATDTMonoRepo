import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { CartaPdfService } from '../../services/carta-pdf.service';

@Component({
  selector: 'app-confirmar-solicitud',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatDividerModule],
  template: `
    <div class="confirmar-container">
      <h3 class="titulo">Resumen de la solicitud</h3>
      <p class="subtitulo">Revisa los datos antes de enviar. Puedes descargar la carta en PDF.</p>

      <div class="seccion">
        <h4>I. Área requirente</h4>
        <mat-divider></mat-divider>
        <div class="grid">
          @for (item of rowsContacto; track item[0]) {
            <span class="label">{{ item[0] }}</span>
            <span class="valor">{{ item[1] || '—' }}</span>
          }
        </div>
      </div>

      <div class="seccion">
        <h4>II. Administrador del servidor</h4>
        <mat-divider></mat-divider>
        <div class="grid">
          @for (item of rowsAdmin; track item[0]) {
            <span class="label">{{ item[0] }}</span>
            <span class="valor">{{ item[1] || '—' }}</span>
          }
        </div>
      </div>

      <div class="seccion">
        <h4>III. Descripción del servidor</h4>
        <mat-divider></mat-divider>
        <div class="grid">
          @for (item of rowsDescripcion; track item[0]) {
            <span class="label">{{ item[0] }}</span>
            <span class="valor">{{ item[1] || '—' }}</span>
          }
        </div>
      </div>

      <div class="seccion">
        <h4>IV. Especificaciones técnicas</h4>
        <mat-divider></mat-divider>
        <div class="grid">
          @for (item of rowsSpecs; track item[0]) {
            <span class="label">{{ item[0] }}</span>
            <span class="valor">{{ item[1] || '—' }}</span>
          }
        </div>
      </div>

      <div class="seccion">
        <h4>V. Infraestructura</h4>
        <mat-divider></mat-divider>
        <div class="grid">
          @for (item of rowsInfra; track item[0]) {
            <span class="label">{{ item[0] }}</span>
            <span class="valor">{{ item[1] || '—' }}</span>
          }
        </div>
      </div>

      <div class="seccion">
        <h4>VI. Responsiva</h4>
        <mat-divider></mat-divider>
        <div class="grid">
          @for (item of rowsResponsiva; track item[0]) {
            <span class="label">{{ item[0] }}</span>
            <span class="valor">{{ item[1] || '—' }}</span>
          }
        </div>
      </div>

      <div class="acciones-pdf">
        <button mat-stroked-button color="primary" (click)="descargarPDF()">
          <mat-icon>picture_as_pdf</mat-icon>
          Descargar carta en PDF
        </button>
      </div>
    </div>
  `,
  styles: [`
    .confirmar-container { padding: 8px 0; }
    .titulo { font-size: 1.1rem; font-weight: 600; margin-bottom: 4px; }
    .subtitulo { font-size: 0.875rem; color: #6b7280; margin-bottom: 20px; }

    .seccion {
      margin-bottom: 20px;
    }
    .seccion h4 {
      font-size: 0.9rem;
      font-weight: 600;
      color: #1e40af;
      margin-bottom: 6px;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    mat-divider { margin-bottom: 10px; }

    .grid {
      display: grid;
      grid-template-columns: 200px 1fr;
      row-gap: 6px;
      column-gap: 12px;
      font-size: 0.875rem;
    }
    .label { font-weight: 500; color: #374151; }
    .valor { color: #111827; }

    .acciones-pdf {
      margin-top: 24px;
      display: flex;
      justify-content: flex-end;
    }
  `]
})
export class ConfirmarSolicitudComponent {
  @Input() form!: FormGroup;

  constructor(private pdfService: CartaPdfService) {}

  get v() { return this.form.value; }

  get rowsContacto(): [string, string][] {
    const g = this.v.areaRequirente ?? {};
    return [
      ['Sector:', g.sector],
      ['Dependencia:', g.dependencia],
      ['Responsable:', g.responsable],
      ['Cargo:', g.cargo],
      ['Teléfono:', g.telefono],
      ['Correo:', g.correo],
    ];
  }

  get rowsAdmin(): [string, string][] {
    const g = this.v.adminServidor ?? {};
    return [
      ['Proveedor:', g.proveedor],
      ['Dependencia:', g.dependencia],
      ['Responsable:', g.responsable],
      ['Cargo:', g.cargo],
      ['Teléfono:', g.telefono],
      ['Correo:', g.correo],
    ];
  }

  get rowsDescripcion(): [string, string][] {
    const g = this.v.descripcion ?? {};
    return [
      ['Descripción:', g.descripcionServidor],
      ['Nombre del servidor:', g.nombreServidor],
      ['Fecha de arranque:', g.fechaArranque],
      ['Tipo de uso:', g.tipoUso],
      ['Vigencia:', g.vigencia],
      ['Nombre de la aplicación:', g.nombreAplicacion],
      ['Características especiales:', g.caracteristicasEspeciales],
    ];
  }

  get rowsSpecs(): [string, string][] {
    const g = this.v.specs ?? {};
    const so = g.sistemaOperativo === 'otro' ? g.sistemaOperativoOtro : g.sistemaOperativo;
    return [
      ['Tipo de requerimiento:', g.tipoRequerimiento],
      ['Modalidad:', g.modalidad],
      ['Sistema operativo:', so],
      ['vCores:', String(g.vCores)],
      ['Memoria RAM (GB):', String(g.memoriaRam)],
      ...((g.discosDuros ?? []) as any[]).map((d: any, i: number) => {
        const etiqueta = d.etiqueta ? ` (${d.etiqueta})` : '';
        return [`Disco ${i + 1}:`, `${d.capacidad} GB — ${d.tipo}${etiqueta}`] as [string, string];
      }),
      ['Motor de base de datos:', g.motorBD],
      ['Puertos:', g.puertos],
      ['Integraciones:', g.integraciones],
      ['Otras especificaciones:', g.otrasSpecs],
    ];
  }

  get rowsInfra(): [string, string][] {
    const g = this.v.infraestructura ?? {};
    const subdominios: [string, string][] = ((g.subdominios ?? []) as { subdominio: string; puerto: string }[])
      .filter(e => e.subdominio?.trim())
      .flatMap((e, i) => [
        [`Subdominio ${i + 1}:`, e.subdominio] as [string, string],
        [`Puerto ${i + 1}:`,    e.puerto || '—'] as [string, string],
      ]);
    return subdominios;
  }

  get rowsResponsiva(): [string, string][] {
    const g = this.v.responsiva ?? {};
    return [
      ['Firmante:', g.firmante],
      ['Número de empleado:', g.numEmpleado],
      ['Puesto:', g.puestoFirmante],
      ['Acepta términos:', g.aceptaTerminos ? 'Sí' : 'No'],
    ];
  }

  descargarPDF(): void {
    this.pdfService.generarPDF(this.v);
  }
}
