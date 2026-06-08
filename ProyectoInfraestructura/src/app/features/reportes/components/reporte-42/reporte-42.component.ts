import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { ReportesService } from '../../services/reportes.service';
import { ExportService } from '../../../../shared/services/export.service';
import { EstatusBadgePipe } from '../../../../shared/pipes/estatus-badge.pipe';
import { Reporte42Fila } from '../../models/reporte.model';

const COLS = [
  { key: 'folioSolicitud',       header: 'Folio de solicitud' },
  { key: 'sector',               header: 'Sector' },
  { key: 'dependencia',          header: 'Dependencia' },
  { key: 'responsable',          header: 'Responsable' },
  { key: 'contacto',             header: 'Contacto' },
  { key: 'estatusProcesamieto',  header: 'Estatus' },
  { key: 'ipServidor',           header: 'IP servidor' },
  { key: 'administradorServidor', header: 'Admin servidor' },
  { key: 'descripcionProyecto',  header: 'Descripción' },
  { key: 'sistemaOperativo',     header: 'S.O.' },
  { key: 'vcpu',                 header: 'vCPU' },
  { key: 'ram',                  header: 'RAM (GB)' },
  { key: 'almacenamiento',       header: 'Almacenamiento (GB)' },
  { key: 'subdominiosAprobados', header: 'Subdominios' },
  { key: 'vpns',                 header: 'VPNs' },
];

@Component({
  selector: 'app-reporte-42',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatIconModule, MatSelectModule, MatOptionModule, EstatusBadgePipe],
  templateUrl: './reporte-42.component.html',
  styleUrl: './reporte-42.component.scss',
})
export class Reporte42Component implements OnInit {
  fechaDesde = '';
  fechaHasta = '';
  agrupacion: 'general' | 'dependencia' | 'sistemaOperativo' | 'ip' = 'general';
  dependencia = '';
  sistemaOperativo = '';
  ip = '';
  datos: Reporte42Fila[] = [];
  totalVcpu = 0;
  totalRam = 0;
  totalAlm = 0;
  totalServidores = 0;

  constructor(private svc: ReportesService, private exp: ExportService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.cargar(); }

  private get filtros() {
    return {
      fechaInicio:    this.fechaDesde,
      fechaFin:       this.fechaHasta,
      agrupacion:     this.agrupacion,
      dependencia:    this.dependencia,
      sistemaOperativo: this.sistemaOperativo,
      ip:             this.ip,
    };
  }

  cargar(): void {
    this.svc.getReporte42(this.filtros)
      .subscribe(res => {
        this.datos           = res.items;
        this.totalVcpu       = res.totalVcpu;
        this.totalRam        = res.totalRam;
        this.totalAlm        = res.totalAlmacenamiento;
        this.totalServidores = res.totalServidores;
        this.cdr.detectChanges();
      });
  }

  limpiar(): void {
    this.fechaDesde      = '';
    this.fechaHasta      = '';
    this.agrupacion      = 'general';
    this.dependencia     = '';
    this.sistemaOperativo = '';
    this.ip              = '';
    this.cargar();
  }

  exportar(formato: 'pdf' | 'excel'): void {
    const file = 'reporte-4.2-recursos-totalizados';
    formato === 'excel'
      ? this.exp.toExcel(this.datos, COLS, file)
      : this.exp.toPdf(this.datos, COLS, file, '4.2 Recursos totalizados');
  }
}
