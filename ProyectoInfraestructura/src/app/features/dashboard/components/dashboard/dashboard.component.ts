import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, combineLatest } from 'rxjs';
import { debounceTime, distinctUntilChanged, startWith, takeUntil } from 'rxjs/operators';
import { DatePipe } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DashboardService, ETAPAS_PROCESO } from '../../services/dashboard.service';
import { Solicitud, DashboardMetricas, EstadoEtapa } from '../../models/solicitud.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DatePipe,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatTooltipModule,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {

  solicitudes: Solicitud[] = [];
  metricas: DashboardMetricas = { total: 0, enProgreso: 0, pendientes: 0, completadas: 0 };
  cargando = true;
  error = false;

  busquedaCtrl = new FormControl('');
  estadoCtrl   = new FormControl('');
  etapaCtrl    = new FormControl('');

  readonly etapasNombres = ETAPAS_PROCESO.map(e => e.nombre);

  private destroy$ = new Subject<void>();

  constructor(
    private dashboardService: DashboardService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.escucharFiltros();
  }

  cargarDashboard(filtros = {}): void {
    this.cargando = true;
    this.error = false;
    this.dashboardService.obtenerDashboard(filtros).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (data) => {
        this.solicitudes = data.solicitudes;
        this.metricas    = data.metricas;
        this.cargando    = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error    = true;
        this.cargando = false;
        this.cdr.detectChanges();
      }
    });
  }

  private escucharFiltros(): void {
    combineLatest([
      this.busquedaCtrl.valueChanges.pipe(debounceTime(400), distinctUntilChanged(), startWith('')),
      this.estadoCtrl.valueChanges.pipe(startWith('')),
      this.etapaCtrl.valueChanges.pipe(startWith(''))
    ]).pipe(takeUntil(this.destroy$))
      .subscribe(([busqueda, estado, etapa]) => {
        this.cargarDashboard({
          busqueda: busqueda ?? '',
          estado:   estado ?? '',
          etapa:    etapa ? Number(etapa) : undefined
        });
      });
  }

  verDetalle(solicitud: Solicitud): void {
    this.router.navigate(['/expediente', solicitud.id]);
  }

  // Helpers para el template
  claseEstadoEtapa(estado: EstadoEtapa): string {
    const clases: Record<EstadoEtapa, string> = {
      'completada':          'dot-done',
      'en-curso':            'dot-active',
      'pendiente-respuesta': 'dot-waiting',
      'sin-iniciar':         'dot-pending'
    };
    return clases[estado];
  }

  tooltipEtapa(etapa: { nombre: string; estado: EstadoEtapa }): string {
    const etiquetas: Record<EstadoEtapa, string> = {
      'completada':          'Completada',
      'en-curso':            'En curso',
      'pendiente-respuesta': 'Pendiente de respuesta',
      'sin-iniciar':         'Sin iniciar'
    };
    return `${etapa.nombre}: ${etiquetas[etapa.estado]}`;
  }

  trackById(_: number, s: Solicitud): string {
    return s.id;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
