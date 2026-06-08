import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { ReportesService } from '../../services/reportes.service';
import { ExportService } from '../../../../shared/services/export.service';
import { EstatusBadgePipe } from '../../../../shared/pipes/estatus-badge.pipe';
import { Reporte22Fila } from '../../models/reporte.model';

const COLS = [
  { key: 'folioSolicitud',      header: 'Folio de solicitud' },
  { key: 'sector',              header: 'Sector' },
  { key: 'dependencia',         header: 'Dependencia' },
  { key: 'responsableServidor', header: 'Responsable' },
  { key: 'contacto',            header: 'Contacto' },
  { key: 'estatusProcesamieto', header: 'Estatus' },
  { key: 'ipServidor',          header: 'IP servidor' },
  { key: 'subdominioAprobado',  header: 'Subdominio' },
  { key: 'proxyAsignado',       header: 'Proxy asignado' },
  { key: 'tipoDespliegue',      header: 'Tipo despliegue' },
  { key: 'puerto',              header: 'Puerto' },
  { key: 'fechaAsignacion',     header: 'Fecha asignación' },
];

@Component({
  selector: 'app-reporte-22',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatIconModule, EstatusBadgePipe],
  templateUrl: './reporte-22.component.html',
  styleUrl: './reporte-22.component.scss',
})
export class Reporte22Component implements OnInit {
  fechaDesde = '';
  fechaHasta = '';
  datos: Reporte22Fila[] = [];

  constructor(private svc: ReportesService, private exp: ExportService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.svc.getReporte22({ fechaInicio: this.fechaDesde, fechaFin: this.fechaHasta })
      .subscribe(d => { this.datos = d; this.cdr.detectChanges(); });
  }

  limpiar(): void { this.fechaDesde = ''; this.fechaHasta = ''; this.cargar(); }

  exportar(formato: 'pdf' | 'excel'): void {
    const file = 'reporte-2.2-subdominios';
    formato === 'excel'
      ? this.exp.toExcel(this.datos, COLS, file)
      : this.exp.toPdf(this.datos, COLS, file, '2.2 Reporte de subdominios');
  }
}
