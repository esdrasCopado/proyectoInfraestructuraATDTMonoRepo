import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { ReportesService } from '../../services/reportes.service';
import { ExportService } from '../../../../shared/services/export.service';
import { EstatusBadgePipe } from '../../../../shared/pipes/estatus-badge.pipe';
import { Reporte31Fila } from '../../models/reporte.model';

const COLS = [
  { key: 'folioSolicitud',        header: 'Folio de solicitud' },
  { key: 'sector',                header: 'Sector' },
  { key: 'dependencia',           header: 'Dependencia' },
  { key: 'responsableServidor',   header: 'Responsable' },
  { key: 'telefonoContacto',      header: 'Teléfono' },
  { key: 'emailContacto',         header: 'Email' },
  { key: 'estatusProcesamieto',   header: 'Estatus' },
  { key: 'ipServidor',            header: 'IP servidor' },
  { key: 'subdominiosAprobados',  header: 'Subdominios' },
  { key: 'fechaSolicitudAnalisis', header: 'Fecha solicitud análisis' },
  { key: 'fechaAplicacionPrueba', header: 'Fecha aplicación prueba' },
  { key: 'resultadoPrueba',       header: 'Resultado' },
  { key: 'hallazgos',             header: 'Hallazgos' },
  { key: 'ronda',                 header: 'Ronda' },
];

@Component({
  selector: 'app-reporte-31',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatIconModule, EstatusBadgePipe],
  templateUrl: './reporte-31.component.html',
  styleUrl: './reporte-31.component.scss',
})
export class Reporte31Component implements OnInit {
  fechaDesde = '';
  fechaHasta = '';
  datos: Reporte31Fila[] = [];

  constructor(private svc: ReportesService, private exp: ExportService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.svc.getReporte31({ fechaInicio: this.fechaDesde, fechaFin: this.fechaHasta })
      .subscribe(d => { this.datos = d; this.cdr.detectChanges(); });
  }

  limpiar(): void { this.fechaDesde = ''; this.fechaHasta = ''; this.cargar(); }

  exportar(formato: 'pdf' | 'excel'): void {
    const file = 'reporte-3.1-vulnerabilidades';
    formato === 'excel'
      ? this.exp.toExcel(this.datos, COLS, file)
      : this.exp.toPdf(this.datos, COLS, file, '3.1 Análisis de vulnerabilidades');
  }
}
