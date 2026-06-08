import { Component, OnInit, AfterViewInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { EstatusBadgePipe, EstadoLabelPipe } from '../../../../shared/pipes/estatus-badge.pipe';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { forkJoin, of } from 'rxjs';
import { DashboardService } from '../../services/dashboard.service';
import { SeguimientoService, SeguimientoEtapaDto, RechazarEvidenciaItem } from '../../services/seguimiento.service';
import { AuthService, RolUsuario } from '../../services/auth.service';
import { EvidenciaService, EvidenciaDto } from '../../services/evidencia.service';
import { NotificacionService, TipoNotificacion } from '../../../../features/notificaciones/services/notificacion.service';
import { Solicitud } from '../../models/solicitud.model';
import { EtapaCartaComponent } from './steps/etapa-carta/etapa-carta.component';
import { EtapaValidacionComponent } from './steps/etapa-validacion/etapa-validacion.component';
import { EtapaCreacionComponent } from './steps/etapa-creacion/etapa-creacion.component';
import { EtapaVerificacionComponent } from './steps/etapa-verificacion/etapa-verificacion.component';
import { EtapaVpnComponent } from './steps/etapa-vpn/etapa-vpn.component';
import { EtapaSubdominioComponent } from './steps/etapa-subdominio/etapa-subdominio.component';
import { EtapaCredencialesComponent } from './steps/etapa-credenciales/etapa-credenciales.component';
import { EtapaWafComponent } from './steps/etapa-waf/etapa-waf.component';
import { EtapaEvidenciasComponent } from './steps/etapa-evidencias/etapa-evidencias.component';
import { EtapaValidacionEvidenciasComponent } from './steps/etapa-validacion-evidencias/etapa-validacion-evidencias.component';
import { EtapaPublicacionComponent } from './steps/etapa-publicacion/etapa-publicacion.component';
import { EtapaVulnerabilidadesComponent } from './steps/etapa-vulnerabilidades/etapa-vulnerabilidades.component';

interface InfoEtapa {
  numero: number;
  nombre: string;
  rf: string;
  rol: string;
}

const ETAPAS: InfoEtapa[] = [
  { numero:  1, nombre: 'Carta responsiva',           rf: 'RF02', rol: 'Dependencia → Admin CD'      },
  { numero:  2, nombre: 'Validación de recursos',     rf: 'RF03', rol: 'Admin CD'                    },
  { numero:  3, nombre: 'Creación de servidor',       rf: 'RF04', rol: 'Admin CD'                    },
  { numero:  4, nombre: 'Comunicaciones',             rf: 'RF05', rol: 'Admin CD'                    },
  { numero:  5, nombre: 'Parches',                    rf: 'RF06', rol: 'Admin CD'                    },
  { numero:  6, nombre: 'XDR y agente',               rf: 'RF07', rol: 'Admin CD'                    },
  { numero:  7, nombre: 'VPN',                        rf: 'RF08', rol: 'Admin Infraestructura'        },
  { numero:  8, nombre: 'Subdominio',                 rf: 'RF09', rol: 'Admin Infraestructura'        },
  { numero:  9, nombre: 'Credenciales',               rf: 'RF10', rol: 'Admin CD'                    },
  { numero: 10, nombre: 'WAF',                        rf: 'RF11', rol: 'Dependencia'                  },
  { numero: 11, nombre: 'Pruebas de funcionamiento',  rf: 'RF12', rol: 'Dependencia'                  },
  { numero: 12, nombre: 'Validación de evidencias',   rf: 'RF13', rol: 'Admin CD / Admin Infra'       },
  { numero: 13, nombre: 'Solicitud de publicación',   rf: 'RF14', rol: 'Dependencia'                  },
  { numero: 14, nombre: 'Vulnerabilidades',           rf: 'RF15', rol: 'Admin Vulnerabilidades'       },
];

@Component({
  selector: 'app-expediente-detalle',
  standalone: true,
  imports: [
    DatePipe,
    MatButtonModule,
    MatProgressSpinnerModule,
    EtapaCartaComponent,
    EtapaValidacionComponent,
    EtapaCreacionComponent,
    EtapaVerificacionComponent,
    EtapaVpnComponent,
    EtapaSubdominioComponent,
    EtapaCredencialesComponent,
    EtapaWafComponent,
    EtapaEvidenciasComponent,
    EtapaValidacionEvidenciasComponent,
    EtapaPublicacionComponent,
    EtapaVulnerabilidadesComponent,
    EstatusBadgePipe,
    EstadoLabelPipe,
  ],
  templateUrl: './expediente-detalle.component.html',
  styleUrl: './expediente-detalle.component.scss'
})
export class ExpedienteDetalleComponent implements OnInit, AfterViewInit {

  @ViewChild('etapaValidacion')        etapaValidacion?:        EtapaValidacionComponent;
  @ViewChild('etapaCreacion')          etapaCreacion?:          EtapaCreacionComponent;
  @ViewChild('etapaVerificacion')      etapaVerificacion?:      EtapaVerificacionComponent;
  @ViewChild('etapaVpn')               etapaVpn?:               EtapaVpnComponent;
  @ViewChild('etapaSubdominio')        etapaSubdominio?:        EtapaSubdominioComponent;
  @ViewChild('etapaCredenciales')      etapaCredenciales?:      EtapaCredencialesComponent;
  @ViewChild('etapaWaf')               etapaWaf?:               EtapaWafComponent;
  @ViewChild('etapaEvidencias')        etapaEvidencias?:        EtapaEvidenciasComponent;
  @ViewChild('etapaValidacion12')      etapaValidacion12?:      EtapaValidacionEvidenciasComponent;
  @ViewChild('etapaPublicacion')       etapaPublicacion?:       EtapaPublicacionComponent;
  @ViewChild('etapaVulnerabilidades')  etapaVulnerabilidades?:  EtapaVulnerabilidadesComponent;

  solicitud?: Solicitud;
  cargando = true;
  error = false;
  guardando = false;
  descargandoPdf = false;

  rolUsuario: RolUsuario | null = null;

  // Estado de evidencias persistido entre etapas (snapshot al avanzar de etapa 11)
  evidenciasCargadas: File[] = [];
  evidenciasDto: EvidenciaDto[] = [];
  esRevisionEvidencias = false;
  motivoRechazoAnterior = '';
  iteracionEvidencias = 1;
  evidenciaIds: number[] = [];

  // Historial de evidencias para etapa 11
  historicoRechazadas: EvidenciaDto[] = [];
  evidenciasPendientes: EvidenciaDto[] = [];

  constructor(
    private route: ActivatedRoute,
    private dashboardService: DashboardService,
    private seguimientoService: SeguimientoService,
    private authService: AuthService,
    private evidenciaService: EvidenciaService,
    private notifService: NotificacionService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngAfterViewInit(): void {
    this.cdr.detectChanges();
  }

  ngOnInit(): void {
    this.rolUsuario = this.authService.obtenerRol();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.cargarExpediente(id);
  }

  private cargarExpediente(id: string): void {
    this.cargando = true;
    this.dashboardService.obtenerDetalle(id).subscribe({
      next: (data) => {
        this.solicitud = data;
        this.cargando = false;

        if (data.etapaActual === 11) {
          this.cargarSeguimientoEtapa11(data.id);
          this.cargarHistorialEvidencias(data.id);
        }
        if (data.etapaActual === 12 || data.etapaActual === 14) {
          this.cargarEvidenciasDto(data.id);
        }

        this.cdr.detectChanges();
      },
      error: () => {
        this.error = true;
        this.cargando = false;
        this.cdr.detectChanges();
      }
    });
  }

  private cargarSeguimientoEtapa11(solicitudId: string): void {
    this.seguimientoService.getSeguimiento(solicitudId).subscribe({
      next: (etapas: SeguimientoEtapaDto[]) => {
        const etapa11 = etapas.find(e => e.etapaNumero === 11);
        if (etapa11?.observaciones) {
          this.motivoRechazoAnterior = etapa11.observaciones;
          this.esRevisionEvidencias  = true;
        }
        this.cdr.detectChanges();
      },
    });
  }

  private cargarHistorialEvidencias(solicitudId: string): void {
    this.evidenciaService.getTodasEvidencias(solicitudId).subscribe({
      next: (data) => {
        this.historicoRechazadas  = data.filter(e => e.deletedAt != null);
        this.evidenciasPendientes = data.filter(e => e.deletedAt == null);
        this.cdr.detectChanges();
      },
    });
  }

  private cargarEvidenciasDto(solicitudId: string | number): void {
    this.evidenciaService.getEvidenciasSolicitud(solicitudId).subscribe({
      next: (data) => {
        this.evidenciasDto = data;
        this.evidenciaIds = data.map(e => e.id);
        this.cdr.detectChanges();
      },
    });
  }

  readonly totalEtapas = ETAPAS.length;

  get etapaActualVisible(): number {
    return Math.min(this.solicitud?.etapaActual ?? 0, this.totalEtapas);
  }

  get etapaInfo(): InfoEtapa | undefined {
    return ETAPAS.find(e => e.numero === this.etapaActualVisible);
  }

  get siguienteEtapa(): InfoEtapa | undefined {
    return ETAPAS.find(e => e.numero === (this.solicitud?.etapaActual ?? 0) + 1);
  }

  get progresoEtapa(): number {
    return Math.round((this.etapaActualVisible / this.totalEtapas) * 100);
  }

  get solicitudCompletada(): boolean {
    const e = this.solicitud?.estado;
    return e === 'finalizado' || e === 'publicado';
  }

  get esAdminCD(): boolean {
    return this.rolUsuario === 'admin-cd' || this.rolUsuario === 'admin-general';
  }

  get esDependencia(): boolean {
    return this.rolUsuario === 'dependencia';
  }

  get puedeActuar(): boolean {
    const etapa = this.solicitud?.etapaActual;
    const rol = this.rolUsuario;
    if (!etapa || !rol) return false;

    if (rol === 'admin-general') return true;

    const permisos: Record<number, RolUsuario[]> = {
      1:  ['admin-cd'],
      2:  ['admin-cd'],
      3:  ['admin-cd'],
      4:  ['admin-cd'],
      5:  ['admin-cd'],
      6:  ['admin-cd'],
      7:  ['admin-infraestructura'],
      8:  ['admin-infraestructura'],
      9:  ['admin-cd'],
      10: ['dependencia'],
      11: ['dependencia'],
      12: ['admin-cd', 'admin-infraestructura'],
      13: ['dependencia'],
      14: ['admin-vulnerabilidades'],
    };

    return permisos[etapa]?.includes(rol) ?? false;
  }

  onEstadoEvidenciaCambiado(): void {
    this.cdr.detectChanges();
  }

  onSolicitarCorrecciones(): void {
  }

  descargarPdfFinal(): void {
    if (!this.solicitud || this.descargandoPdf) return;
    this.descargandoPdf = true;
    this.dashboardService.descargarPdfFinal(this.solicitud.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a   = document.createElement('a');
        a.href     = url;
        a.download = `solicitud_${this.solicitud!.folio}.pdf`;
        a.click();
        URL.revokeObjectURL(url);
        this.descargandoPdf = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.descargandoPdf = false;
        this.cdr.detectChanges();
      },
    });
  }

  // ── Avance normal (etapas sin bifurcación) ───────────────────────────────

  onGuardar(): void {
    if (!this.solicitud) return;

    if (this.solicitud.etapaActual === 2) {
      const childForm = this.etapaValidacion?.form;
      if (childForm?.invalid) { childForm.markAllAsTouched(); return; }
      this.guardarEtapa2();
      return;
    }

    if (this.solicitud.etapaActual === 3) {
      const childForm = this.etapaCreacion?.form;
      if (childForm?.invalid) { childForm.markAllAsTouched(); return; }
      this.guardarEtapa3();
      return;
    }

    if ([4, 5, 6].includes(this.solicitud.etapaActual)) {
      const childForm = this.etapaVerificacion?.form;
      if (childForm?.invalid) { childForm.markAllAsTouched(); return; }
      if (this.solicitud.etapaActual === 4) { this.guardarEtapa4(); return; }
      if (this.solicitud.etapaActual === 5) { this.guardarEtapa5(); return; }
      if (this.solicitud.etapaActual === 6) { this.guardarEtapa6(); return; }
    }

    if (this.solicitud.etapaActual === 7) {
      const childForm = this.etapaVpn?.form;
      if (childForm?.invalid) { childForm.markAllAsTouched(); return; }
      this.guardarEtapa7(); return;
    }

    if (this.solicitud.etapaActual === 8) {
      const childForm = this.etapaSubdominio?.form;
      if (childForm?.invalid) { childForm.markAllAsTouched(); return; }
      this.guardarEtapa8(); return;
    }

    if (this.solicitud.etapaActual === 9) {
      const childForm = this.etapaCredenciales?.form;
      if (childForm?.invalid) { childForm.markAllAsTouched(); return; }
      this.guardarEtapa9(); return;
    }

    if (this.solicitud.etapaActual === 10) {
      const childForm = this.etapaWaf?.form;
      if (childForm?.invalid) { childForm.markAllAsTouched(); return; }
      this.guardarEtapa10(); return;
    }

    if (this.solicitud.etapaActual === 11) {
      const comp = this.etapaEvidencias;
      if (!comp) return;
      if (comp.archivos.length === 0) { comp.errorArchivos = true; return; }
      const childForm = comp.form;
      if (childForm?.invalid) { childForm.markAllAsTouched(); return; }
      this.evidenciasCargadas = [...comp.archivos];
      this.esRevisionEvidencias = false;
      this.guardarEtapa11(); return;
    }

    if (this.solicitud.etapaActual === 13) {
      const childForm = this.etapaPublicacion?.form;
      if (childForm?.invalid) { childForm.markAllAsTouched(); return; }
    }

    this.avanzarEtapa();
  }

  // ── Bifurcación: Etapas 12 y 14 (Aprobar / Rechazar) ────────────────────

  onAprobacion(): void {
    if (this.solicitud?.etapaActual === 12) {
      this.aprobarEtapa12(); return;
    }
    this.avanzarEtapa();
  }

  onRechazo(): void {
    if (!this.solicitud) return;

    if (this.solicitud.etapaActual === 12) {
      const comp = this.etapaValidacion12;
      if (!comp) return;
      comp.marcarComentarioPendiente();
      if (comp.formRechazoInvalido) return;
      this.motivoRechazoAnterior = comp.motivoRechazo;
      this.rechazarEtapa12(comp.motivoRechazo); return;
    } else if (this.solicitud.etapaActual === 14) {
      const comp = this.etapaVulnerabilidades;
      if (!comp) return;
      comp.marcarHallazgosPendiente();
      if (comp.formRechazoInvalido) return;
      this.motivoRechazoAnterior = comp.hallazgos;
      this.enviarNotifEtapa(14, false);
    } else {
      return;
    }

    this.iteracionEvidencias++;
    this.esRevisionEvidencias = true;
    this.retrocederAEtapa(11);
  }

  // ── Navegación de etapas ─────────────────────────────────────────────────

  private guardarEtapa11(): void {
    const archivos = this.evidenciasCargadas;
    if (!archivos.length) { this.avanzarEtapa(); return; }
    this.guardando = true;
    forkJoin(
      archivos.map(f => this.evidenciaService.subirEvidencia(this.solicitud!.id, f))
    ).subscribe({
      next: (res) => {
        this.evidenciaIds = res.map(r => r.id);
        this.avanzarEtapa();
        this.cargarEvidenciasDto(this.solicitud!.id);
      },
      error: () => { this.guardando = false; this.cdr.detectChanges(); },
    });
  }

  private aprobarEtapa12(): void {
    const usuarioId = this.authService.obtenerUsuario()?.id ?? '';
    this.guardando = true;

    // Las evidencias ya fueron aprobadas individualmente con el botón por archivo.
    // Solo actualizamos el servidor y avanzamos la etapa.
    const servidorOp = this.solicitud?.servidorId
      ? this.dashboardService.actualizarServidor(this.solicitud.servidorId, {
          evidenciaValidada:          true,
          fechaValidacionEvidencia:   new Date().toISOString(),
          usuarioValidacionEvidencia: usuarioId,
        })
      : of(null);

    servidorOp.subscribe({
      next:  () => this.avanzarEtapa(),
      error: () => { this.guardando = false; this.cdr.detectChanges(); },
    });
  }

  private rechazarEtapa12(observacionEtapa: string): void {
    this.guardando = true;
    const comp        = this.etapaValidacion12!;
    const rechazados  = comp.rechazadosLocalmente as RechazarEvidenciaItem[];
    const userId      = parseInt(this.authService.obtenerUsuario()?.id ?? '0', 10);

    this.seguimientoService
      .rechazarEvidencias(this.solicitud!.id, rechazados, observacionEtapa, userId)
      .subscribe({
        next: () => {
          this.guardando     = false;
          this.evidenciasDto = [];
          this.evidenciaIds  = [];
          this.enviarNotifEtapa(12, false);
          this.finalizarRechazo();
          this.cargarHistorialEvidencias(this.solicitud!.id);
        },
        error: () => {
          this.guardando = false;
          this.cdr.detectChanges();
        },
      });
  }

  private finalizarRechazo(): void {
    this.iteracionEvidencias++;
    this.esRevisionEvidencias = true;
    this.retrocederAEtapa(11);
  }

  private guardarEtapa10(): void {
    if (!this.solicitud?.servidorId) { this.avanzarEtapa(); return; }
    const usuarioId = this.authService.obtenerUsuario()?.id ?? '';
    this.guardando = true;
    this.dashboardService.actualizarServidor(this.solicitud.servidorId, {
      wafConfigurado:          true,
      fechaConfiguracionWaf:   new Date().toISOString(),
      usuarioWaf:              usuarioId,
    }).subscribe({
      next:  () => this.avanzarEtapa(),
      error: () => { this.guardando = false; this.cdr.detectChanges(); },
    });
  }

  private guardarEtapa9(): void {
    if (!this.solicitud?.servidorId) { this.avanzarEtapa(); return; }
    const usuarioId = this.authService.obtenerUsuario()?.id ?? '';
    this.guardando = true;
    this.dashboardService.actualizarServidor(this.solicitud.servidorId, {
      estado:                    'Activo',
      credencialesEntregadas:    true,
      fechaEntregaCredenciales:  new Date().toISOString(),
      usuarioCredenciales:       usuarioId,
    }).subscribe({
      next:  () => this.avanzarEtapa(),
      error: () => { this.guardando = false; this.cdr.detectChanges(); },
    });
  }

  private guardarEtapa7(): void {
    const vpnIds = (this.solicitud?.vpns ?? [])
      .map(v => v.id)
      .filter((id): id is number => id != null);
    this.avanzarEtapa(vpnIds.length ? { vpnIds } : undefined);
  }

  private guardarEtapa8(): void {
    const subdominioIds = (this.solicitud?.subdominios ?? [])
      .map(s => s.id)
      .filter((id): id is number => id != null);
    this.avanzarEtapa(subdominioIds.length ? { subdominioIds } : undefined);
  }

  private guardarEtapa6(): void {
    if (!this.solicitud?.servidorId) { this.avanzarEtapa(); return; }
    const usuarioId = this.authService.obtenerUsuario()?.id ?? '';
    this.guardando = true;
    this.dashboardService.actualizarServidor(this.solicitud.servidorId, {
      xdrInstalado: true,
      fechaXdr:     new Date().toISOString(),
      usuarioXdr:   usuarioId,
    }).subscribe({
      next:  () => this.avanzarEtapa(),
      error: () => { this.guardando = false; this.cdr.detectChanges(); },
    });
  }

  private guardarEtapa4(): void {
    if (!this.solicitud?.servidorId) { this.avanzarEtapa(); return; }
    const usuarioId = this.authService.obtenerUsuario()?.id ?? '';
    this.guardando = true;
    this.dashboardService.actualizarServidor(this.solicitud.servidorId, {
      comunicacionValidada:          true,
      fechaValidacionComunicacion:   new Date().toISOString(),
      usuarioValidacionComunicacion: usuarioId,
    }).subscribe({
      next:  () => this.avanzarEtapa(),
      error: () => { this.guardando = false; this.cdr.detectChanges(); },
    });
  }

  private guardarEtapa5(): void {
    if (!this.solicitud?.servidorId) { this.avanzarEtapa(); return; }
    const usuarioId = this.authService.obtenerUsuario()?.id ?? '';
    this.guardando = true;
    this.dashboardService.actualizarServidor(this.solicitud.servidorId, {
      parchesAplicados: true,
      fechaParches:     new Date().toISOString(),
      usuarioParches:   usuarioId,
    }).subscribe({
      next:  () => this.avanzarEtapa(),
      error: () => { this.guardando = false; this.cdr.detectChanges(); },
    });
  }

  private guardarEtapa3(): void {
    if (!this.solicitud?.servidorId) { this.avanzarEtapa(); return; }

    this.guardando = true;
    this.dashboardService.actualizarServidor(this.solicitud.servidorId, { estado: 'Aprovisionado' })
      .subscribe({
        next:  () => this.avanzarEtapa(),
        error: () => { this.guardando = false; this.cdr.detectChanges(); },
      });
  }

  private guardarEtapa2(): void {
    if (!this.solicitud?.servidorId) {
      this.avanzarEtapa();
      return;
    }

    const form = this.etapaValidacion!.form;
    const { vCores, memoriaRam, almacenamiento, ip, comentarios } = form.value;

    this.guardando = true;
    this.dashboardService.actualizarServidor(this.solicitud.servidorId, {
      nucleos:            Number(vCores),
      ram:                Number(memoriaRam),
      almacenamiento:     Number(almacenamiento),
      ip,
      fechaAsignacionIp:  new Date().toISOString(),
    }).subscribe({
      next: () => {
        const extra = comentarios?.trim() ? { observacion: comentarios.trim() } : undefined;
        this.avanzarEtapa(extra);
      },
      error: () => {
        this.guardando = false;
        this.cdr.detectChanges();
      },
    });
  }

  private avanzarEtapa(extra?: Record<string, any>): void {
    if (!this.solicitud) return;

    const etapaActual = this.solicitud.etapaActual;
    const siguiente   = etapaActual + 1;
    const userId      = parseInt(this.authService.obtenerUsuario()?.id ?? '0', 10);

    this.guardando = true;

    this.seguimientoService
      .completarEtapa(this.solicitud.id, etapaActual, userId, extra)
      .subscribe({
        next: () => {
          this.guardando = false;
          this.enviarNotifEtapa(etapaActual);

          if (siguiente > 14) {
            this.solicitud = {
              ...this.solicitud!,
              estado: 'publicado',
              etapas: this.solicitud!.etapas.map(e => ({ ...e, estado: 'completada' as const })),
            };
          } else {
            this.solicitud = {
              ...this.solicitud!,
              etapaActual: siguiente,
              etapas: this.solicitud!.etapas.map(e => ({
                ...e,
                estado: e.numero < siguiente    ? 'completada'
                       : e.numero === siguiente  ? 'en-curso'
                       : 'sin-iniciar',
              })),
            };
          }

          this.cdr.detectChanges();
        },
        error: () => {
          this.guardando = false;
          this.cdr.detectChanges();
        },
      });
  }

  // ── Notificaciones por etapa ─────────────────────────────────────────────

  private enviarNotifEtapa(etapa: number, aprobado = true): void {
    if (!this.solicitud) return;
    const senderId = parseInt(this.authService.obtenerUsuario()?.id ?? '0', 10);
    const solId    = parseInt(this.solicitud.id, 10) || 0;
    const folio    = this.solicitud.folio;
    const srv      = this.solicitud.nombreServidor;
    const dep      = this.solicitud.dependencia;

    const esAdmin = this.rolUsuario !== 'dependencia';

    // POST /api/notifications/rol requiere rol de admin
    const porRol = (rol: string, tipo: TipoNotificacion, titulo: string, msg: string) => {
      if (!esAdmin) return;
      this.notifService.notificarRol(rol, tipo, titulo, msg, solId, senderId).subscribe();
    };

    // POST /api/notifications/solicitud/cliente requiere rol de admin
    const porDep = (tipo: TipoNotificacion, titulo: string, msg: string) => {
      if (!esAdmin) return;
      this.notifService.notificarCliente(solId, tipo, titulo, msg).subscribe();
    };

    switch (etapa) {
      case 1:
        porRol('admin-cd', 'solicitud_nueva',
          'Nueva solicitud recibida',
          `La dependencia ${dep} envió la carta responsiva para ${srv} (${folio}). Revisa la etapa 2.`);
        break;
      case 6:
        porRol('admin-infraestructura', 'solicitud_nueva',
          'Nueva etapa disponible',
          `El servidor ${srv} (${folio}) está listo para la asignación de VPN (Etapa 7).`);
        break;
      case 8:
        porRol('admin-cd', 'solicitud_nueva',
          'Nueva etapa disponible',
          `El subdominio del servidor ${srv} (${folio}) fue asignado. Procede con las credenciales (Etapa 9).`);
        porDep('subdominio_asignado',
          'Tu subdominio fue asignado',
          `El subdominio de tu servidor ${srv} (${folio}) está activo.`);
        break;
      case 9:
        porDep('solicitud_nueva',
          'Credenciales disponibles',
          `Las credenciales de acceso para ${srv} (${folio}) han sido entregadas. Revisa la Etapa 10 · WAF.`);
        break;
      case 11:
        porRol('admin-cd', 'evidencias_cargadas',
          'Evidencias cargadas',
          `Se cargaron evidencias de pruebas para ${srv} (${folio}).`);
        porRol('admin-infraestructura', 'evidencias_cargadas',
          'Evidencias cargadas',
          `Se cargaron evidencias de pruebas para ${srv} (${folio}).`);
        break;
      case 12:
        if (aprobado) {
          porRol('admin-vulnerabilidades', 'evidencias_aprobadas',
            'Evidencias aprobadas',
            `Las evidencias del servidor ${srv} (${folio}) fueron aprobadas.`);
          porDep('evidencias_aprobadas',
            'Evidencias aprobadas',
            `Tus evidencias para ${srv} (${folio}) fueron aprobadas. Procede a solicitar publicación.`);
        } else {
          porDep('evidencias_rechazadas',
            'Evidencias rechazadas',
            `Las evidencias para ${srv} (${folio}) requieren correcciones. Revisa el motivo y vuelve a cargarlas.`);
        }
        break;
      case 13:
        porRol('admin-vulnerabilidades', 'solicitud_publicacion',
          'Solicitud de publicación',
          `La dependencia solicitó la publicación del servidor ${srv} (${folio}).`);
        break;
      case 14:
        if (aprobado) {
          porDep('vulnerabilidades_aprobadas',
            'Servidor aprobado para publicación',
            `El análisis de vulnerabilidades del servidor ${srv} (${folio}) fue aprobado.`);
          porRol('admin-infraestructura', 'vulnerabilidades_aprobadas',
            'Servidor aprobado para publicación',
            `El servidor ${srv} (${folio}) fue aprobado. Procede con la publicación.`);
        } else {
          porDep('vulnerabilidades_rechazadas',
            'Hallazgos de vulnerabilidades',
            `El servidor ${srv} (${folio}) requiere correcciones de seguridad. Revisa los hallazgos.`);
        }
        break;
    }
  }

  private retrocederAEtapa(n: number): void {
    if (!this.solicitud) return;
    this.solicitud = {
      ...this.solicitud,
      etapaActual: n,
      etapas: this.solicitud.etapas.map(e => ({
        ...e,
        estado: e.numero < n   ? 'completada'
               : e.numero === n ? 'en-curso'
               : 'sin-iniciar',
      })),
    };
    this.cdr.detectChanges();
  }
}
