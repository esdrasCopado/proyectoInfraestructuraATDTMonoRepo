import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { ReportesService } from '../../services/reportes.service';
import { ExportService } from '../../../../shared/services/export.service';
import { EstatusBadgePipe } from '../../../../shared/pipes/estatus-badge.pipe';
import { Reporte11Fila } from '../../models/reporte.model';

const COLS = [
  { key: 'folioSolicitud',    header: 'Folio de solicitud' },
  { key: 'sector',            header: 'Sector' },
  { key: 'dependencia',       header: 'Dependencia' },
  { key: 'responsable',       header: 'Responsable' },
  { key: 'contacto',          header: 'Contacto' },
  { key: 'estatusProcesamieto', header: 'Estatus' },
  { key: 'fechaCreacion',     header: 'Fecha creación' },
];

@Component({
  selector: 'app-reporte-11',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatIconModule, EstatusBadgePipe],
  templateUrl: './reporte-11.component.html',
  styleUrl: './reporte-11.component.scss',
})
export class Reporte11Component implements OnInit {
  fechaDesde = '';
  fechaHasta = '';
  datos: Reporte11Fila[] = [];

  constructor(private svc: ReportesService, private exp: ExportService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.svc.getReporte11({ fechaInicio: this.fechaDesde, fechaFin: this.fechaHasta })
      .subscribe(d => { this.datos = d; this.cdr.detectChanges(); });
  }

  limpiar(): void { this.fechaDesde = ''; this.fechaHasta = ''; this.cargar(); }

  exportar(formato: 'pdf' | 'excel'): void {
    const file = 'reporte-1.1-solicitudes-por-dependencia';
    formato === 'excel'
      ? this.exp.toExcel(this.datos, COLS, file)
      : this.exp.toPdf(this.datos, COLS, file, '1.1 Solicitudes por dependencia');
  }
}
