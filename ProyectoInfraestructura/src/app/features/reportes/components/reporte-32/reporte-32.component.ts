import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { ReportesService } from '../../services/reportes.service';
import { ExportService } from '../../../../shared/services/export.service';
import { EstatusBadgePipe } from '../../../../shared/pipes/estatus-badge.pipe';
import { Reporte32Fila } from '../../models/reporte.model';

const COLS = [
  { key: 'folioSolicitud',       header: 'Folio de solicitud' },
  { key: 'sector',               header: 'Sector' },
  { key: 'dependencia',          header: 'Dependencia' },
  { key: 'responsableServidor',  header: 'Responsable' },
  { key: 'contactoResponsable',  header: 'Contacto' },
  { key: 'estatusProcesamieto',  header: 'Estatus' },
  { key: 'ipServidor',           header: 'IP servidor' },
  { key: 'subdominiosAprobados', header: 'Subdominios' },
  { key: 'tipoDespliegue',       header: 'Tipo despliegue' },
  { key: 'puertosSolicitados',   header: 'Puertos solicitados' },
  { key: 'reglasFirewall',       header: 'Reglas Firewall' },
  { key: 'integracionesExternas', header: 'Integraciones externas' },
  { key: 'otras',                header: 'Otras' },
];

@Component({
  selector: 'app-reporte-32',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatIconModule, EstatusBadgePipe],
  templateUrl: './reporte-32.component.html',
  styleUrl: './reporte-32.component.scss',
})
export class Reporte32Component implements OnInit {
  fechaDesde = '';
  fechaHasta = '';
  ip = '';
  datos: Reporte32Fila[] = [];

  constructor(private svc: ReportesService, private exp: ExportService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.buscar(); }

  buscar(): void {
    this.svc.getReporte32(this.ip, { fechaInicio: this.fechaDesde, fechaFin: this.fechaHasta })
      .subscribe(d => { this.datos = d; this.cdr.detectChanges(); });
  }

  limpiar(): void { this.ip = ''; this.fechaDesde = ''; this.fechaHasta = ''; this.buscar(); }

  exportar(formato: 'pdf' | 'excel'): void {
    const file = 'reporte-3.2-comunicaciones-ip';
    formato === 'excel'
      ? this.exp.toExcel(this.datos, COLS, file)
      : this.exp.toPdf(this.datos, COLS, file, '3.2 Comunicaciones y aplicativos por IP');
  }
}
