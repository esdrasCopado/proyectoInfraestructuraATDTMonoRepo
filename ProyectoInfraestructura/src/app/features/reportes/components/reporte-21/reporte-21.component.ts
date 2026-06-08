import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { ReportesService } from '../../services/reportes.service';
import { ExportService } from '../../../../shared/services/export.service';
import { Reporte21Fila } from '../../models/reporte.model';

const COLS = [
  { key: 'folioSolicitud',      header: 'Folio de solicitud' },
  { key: 'sector',              header: 'Sector' },
  { key: 'dependencia',         header: 'Dependencia' },
  { key: 'responsableServidor', header: 'Responsable' },
  { key: 'contactoResponsable', header: 'Contacto' },
  { key: 'estatusProcesamieto', header: 'Estatus' },
  { key: 'ipServidor',          header: 'IP servidor' },
  { key: 'identificadorVpn',    header: 'Identificador VPN' },
  { key: 'usuarioAsignado',     header: 'Usuario asignado' },
  { key: 'fechaCreacionVpn',    header: 'Fecha creación VPN' },
  { key: 'fechaVencimientoVpn', header: 'Fecha vencimiento' },
  { key: 'vigencia',            header: 'Vigencia' },
  { key: 'tipoVpn',             header: 'Tipo VPN' },
];

@Component({
  selector: 'app-reporte-21',
  standalone: true,
  imports: [CommonModule, DatePipe, FormsModule, MatFormFieldModule, MatInputModule, MatIconModule],
  templateUrl: './reporte-21.component.html',
  styleUrl: './reporte-21.component.scss',
})
export class Reporte21Component implements OnInit {
  fechaDesde = '';
  fechaHasta = '';
  datos: Reporte21Fila[] = [];

  constructor(private svc: ReportesService, private exp: ExportService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.svc.getReporte21({ fechaInicio: this.fechaDesde, fechaFin: this.fechaHasta })
      .subscribe(d => { this.datos = d; this.cdr.detectChanges(); });
  }

  limpiar(): void { this.fechaDesde = ''; this.fechaHasta = ''; this.cargar(); }

  exportar(formato: 'pdf' | 'excel'): void {
    const file = 'reporte-2.1-vpn';
    formato === 'excel'
      ? this.exp.toExcel(this.datos, COLS, file)
      : this.exp.toPdf(this.datos, COLS, file, '2.1 Reporte de VPN');
  }
}
