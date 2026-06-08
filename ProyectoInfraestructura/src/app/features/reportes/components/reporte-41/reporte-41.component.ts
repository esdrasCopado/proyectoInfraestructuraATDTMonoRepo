import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { ReportesService } from '../../services/reportes.service';
import { ExportService } from '../../../../shared/services/export.service';
import { EstatusBadgePipe } from '../../../../shared/pipes/estatus-badge.pipe';
import { Reporte41Fila } from '../../models/reporte.model';

const COLS = [
  { key: 'sector',                 header: 'Sector' },
  { key: 'dependencia',            header: 'Dependencia' },
  { key: 'responsable',            header: 'Responsable' },
  { key: 'emailContacto',          header: 'Email contacto' },
  { key: 'descripcionServidor',    header: 'Descripción servidor' },
  { key: 'ip',                     header: 'IP servidor' },
  { key: 'fechaSolicitud',         header: 'Fecha solicitud' },
  { key: 'estatusProcesamieto',    header: 'Estatus' },
  { key: 'etapaActual',            header: 'Etapa actual' },
  { key: 'nombreEtapaActual',      header: 'Nombre etapa' },
  { key: 'fechaProcesamientoEtapa', header: 'Fecha procesamiento' },
  { key: 'rolResponsableEtapa',    header: 'Rol responsable etapa' },
  { key: 'fechaPublicacion',       header: 'Fecha publicación' },
  { key: 'tipoDespliegue',         header: 'Tipo despliegue' },
];

@Component({
  selector: 'app-reporte-41',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatIconModule, EstatusBadgePipe],
  templateUrl: './reporte-41.component.html',
  styleUrl: './reporte-41.component.scss',
})
export class Reporte41Component implements OnInit {
  fechaDesde = '';
  fechaHasta = '';
  datos: Reporte41Fila[] = [];

  constructor(private svc: ReportesService, private exp: ExportService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.svc.getReporte41({ fechaInicio: this.fechaDesde, fechaFin: this.fechaHasta })
      .subscribe(d => { this.datos = d; this.cdr.detectChanges(); });
  }

  limpiar(): void { this.fechaDesde = ''; this.fechaHasta = ''; this.cargar(); }

  exportar(formato: 'pdf' | 'excel'): void {
    const file = 'reporte-4.1-estatus-solicitudes';
    formato === 'excel'
      ? this.exp.toExcel(this.datos, COLS, file)
      : this.exp.toPdf(this.datos, COLS, file, '4.1 Estatus de solicitudes');
  }
}
