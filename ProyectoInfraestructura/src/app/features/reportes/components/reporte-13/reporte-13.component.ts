import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { ReportesService } from '../../services/reportes.service';
import { ExportService } from '../../../../shared/services/export.service';
import { EstatusBadgePipe } from '../../../../shared/pipes/estatus-badge.pipe';
import { Reporte13Fila } from '../../models/reporte.model';

const COLS = [
  { key: 'folioSolicitud',      header: 'Folio de solicitud' },
  { key: 'dependencia',         header: 'Dependencia' },
  { key: 'responsable',         header: 'Responsable' },
  { key: 'contactoResponsable', header: 'Contacto' },
  { key: 'estatusProcesamieto', header: 'Estatus' },
  { key: 'ipServidor',          header: 'IP servidor' },
  { key: 'administradorServidor', header: 'Admin servidor' },
  { key: 'sistemaOperativo',    header: 'S.O.' },
  { key: 'vcpu',                header: 'vCPU' },
  { key: 'ram',                 header: 'RAM (GB)' },
  { key: 'almacenamiento',      header: 'Almacenamiento (GB)' },
  { key: 'subdominiosAprobados', header: 'Subdominios' },
  { key: 'vpns',                header: 'VPNs' },
];

@Component({
  selector: 'app-reporte-13',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatIconModule, EstatusBadgePipe],
  templateUrl: './reporte-13.component.html',
  styleUrl: './reporte-13.component.scss',
})
export class Reporte13Component implements OnInit {
  fechaDesde = '';
  fechaHasta = '';
  ip = '';
  datos: Reporte13Fila[] = [];

  constructor(private svc: ReportesService, private exp: ExportService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.buscar(); }

  buscar(): void {
    if (!this.ip.trim()) { this.datos = []; this.cdr.detectChanges(); return; }
    this.svc.getReporte13(this.ip, { fechaInicio: this.fechaDesde, fechaFin: this.fechaHasta })
      .subscribe(d => { this.datos = d; this.cdr.detectChanges(); });
  }

  limpiar(): void { this.ip = ''; this.fechaDesde = ''; this.fechaHasta = ''; this.buscar(); }

  exportar(formato: 'pdf' | 'excel'): void {
    const file = 'reporte-1.3-por-ip';
    formato === 'excel'
      ? this.exp.toExcel(this.datos, COLS, file)
      : this.exp.toPdf(this.datos, COLS, file, '1.3 Solicitudes por IP');
  }
}
