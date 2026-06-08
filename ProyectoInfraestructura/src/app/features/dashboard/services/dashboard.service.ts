import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, forkJoin, map, catchError } from 'rxjs';
import { DashboardResponse, DashboardMetricas, Solicitud, EtapaSolicitud, EstadoSolicitud, VpnServidor, SubdominioServidor } from '../models/solicitud.model';
import { CartaAprovisionamiento } from '../../carta-aprovisionamiento/models/carta-aprovisionamiento.model';
import { environment } from '../../../../environments/environment';
import { DASHBOARD_MOCK } from './dashboard.mock';
import { AuthService } from './auth.service';

export interface DashboardFiltros {
  busqueda?: string;
  estado?: string;
  etapa?: number;
  pagina?: number;
  porPagina?: number;
}

// ── Interfaces del backend ────────────────────────────────────────────────────

interface DashboardResumenBackend {
  total:       number;
  solicitudes: SolicitudResumenBackend[];
}

interface SolicitudResumenBackend {
  id:           number;
  folio:        string;
  dependencia:  string;
  estado:       string;
  etapaActual:  number;
  nombreEtapa:  string;
  actualizacion: string;
}

interface SolicitudBackend {
  id:               number;
  folio:            string;
  nombreAplicacion: string;
  estatus:          string;
  etapaActual?:     number;
  createdAt:        string;
  updatedAt:        string;
  servidor:         { hostname: string; etapaOperativa: string; [k: string]: any } | null;
  [k: string]:      any;
}

interface CartaBackend {
  id?:                        number;
  solicitudId?:               number;
  createdAt?:                 string;
  updatedAt?:                 string;
  firmaDependenciaAt?:        string;
  firmanteAtdtNombre?:        string;
  firmanteAtdtPuesto?:        string;
  firmanteDependenciaNombre?: string;
  firmanteDependenciaPuesto?: string;
  firmanteDependenciaEmpleado?: string;
  solicitud?: {
    id?:                      number;
    folio?:                   string;
    nombreServidor?:          string;
    nombreAplicacion?:        string;
    descripcionUso?:          string;
    tipoUso?:                 string;
    tipoRequerimiento?:       string;
    sistemaOperativo?:        string;
    vcpuSolicitado?:          number;
    ramSolicitadaGb?:         number;
    almacenamientoSolicitadoGb?: number;
    motorBaseDatos?:          string;
    reglasFirewall?:          string;
    integracionesExternas?:   string;
    conectividadOtras?:       string;
    fechaArranqueDeseada?:    string;
    vigenciaMeses?:           number;
    esClonacion?:             boolean;
    // Área requirente (contacto específico de la solicitud)
    telefonoResponsable?:     string;
    correoResponsable?:       string;
    // Administrador del servidor
    responsableInfraestructura?: string;
    proveedor?:               string;
    dependenciaAdmin?:        string;
    cargoAdmin?:              string;
    telefonoAdmin?:           string;
    correoAdmin?:             string;
    dependency?: {
      dependencyId?: number;
      sector?:       string;
      name?:         string;
      responsable?:  string;
      cargo?:        string;
      [k: string]:   any;
    };
    [k: string]: any;
  };
}

// ── Definición canónica de etapas ─────────────────────────────────────────────
// Los nombres deben coincidir exactamente con los que devuelve el backend en etapaActual

export const ETAPAS_PROCESO: { numero: number; nombre: string }[] = [
  { numero: 1,  nombre: 'Carta responsiva'    },
  { numero: 2,  nombre: 'Validación recursos' },
  { numero: 3,  nombre: 'Creación servidor'   },
  { numero: 4,  nombre: 'Comunicaciones'      },
  { numero: 5,  nombre: 'Parches'             },
  { numero: 6,  nombre: 'XDR y agente'        },
  { numero: 7,  nombre: 'VPN'                 },
  { numero: 8,  nombre: 'Subdominio'          },
  { numero: 9,  nombre: 'Credenciales'        },
  { numero: 10, nombre: 'WAF'                 },
  { numero: 11, nombre: 'Evidencias'          },
  { numero: 12, nombre: 'Val. evidencias'     },
  { numero: 13, nombre: 'Sol. publicación'    },
  { numero: 14, nombre: 'Vulnerabilidades'    },
];

const ESTADO_MAP: Record<string, EstadoSolicitud> = {
  // estados canónicos
  'pendiente':                     'pendiente',
  'Pendiente':                     'pendiente',
  'recursos_asignados':            'recursos_asignados',
  'aprovisionado':                 'aprovisionado',
  'Aprovisionado':                 'aprovisionado',
  'en_configuracion':              'en_configuracion',
  'credenciales_entregadas':       'credenciales_entregadas',
  'en_pruebas':                    'en_pruebas',
  'en_validacion_evidencias':      'en_validacion_evidencias',
  'en_analisis_vulnerabilidades':  'en_analisis_vulnerabilidades',
  'publicado':                     'publicado',
  'Publicado':                     'publicado',
  'finalizado':                    'finalizado',
  'Finalizado':                    'finalizado',
  'rechazado':                     'rechazado',
  'Rechazado':                     'rechazado',
  // legacy
  'en_validacion':                 'en_validacion_evidencias',
  'en-validacion':                 'en_validacion_evidencias',
  'en-pruebas':                    'en_pruebas',
  'en-progreso':                   'en_configuracion',
  'en proceso':                    'en_configuracion',
  'En proceso':                    'en_configuracion',
  'En Proceso':                    'en_configuracion',
  'terminado':                     'finalizado',
  'Terminado':                     'finalizado',
  'completado':                    'finalizado',
  'Completado':                    'finalizado',
  'completada':                    'finalizado',
};

const EN_PROCESO: EstadoSolicitud[] = [
  'recursos_asignados', 'aprovisionado', 'en_configuracion',
  'credenciales_entregadas', 'en_pruebas', 'en_validacion_evidencias',
  'en_analisis_vulnerabilidades',
];

const COMPLETADAS: EstadoSolicitud[] = ['publicado', 'finalizado'];

@Injectable({ providedIn: 'root' })
export class DashboardService {

  private solicitudUrl = `${environment.apiUrl}/solicitud`;
  private readonly useMock = environment.useMock;

  constructor(private http: HttpClient, private auth: AuthService) {}

  obtenerDashboard(filtros: DashboardFiltros = {}): Observable<DashboardResponse> {
    if (this.useMock) {
      return of(this.filtrarLocal(DASHBOARD_MOCK, filtros));
    }

    if (this.auth.esDependencia()) {
      return this.obtenerDashboardDependencia(filtros);
    }

    return this.http.get<DashboardResumenBackend>(
      `${this.solicitudUrl}/dashboard/resumen`
    ).pipe(
      map(res => {
        const mapped    = res.solicitudes.map(s => this.mapearSolicitudResumen(s));
        const metricas  = this.calcularMetricas(res.total, mapped);
        const filtradas = this.filtrarLocal({ solicitudes: mapped, metricas }, filtros).solicitudes;
        return { solicitudes: filtradas, metricas };
      })
    );
  }

  private obtenerDashboardDependencia(filtros: DashboardFiltros): Observable<DashboardResponse> {
    const idUsuario = this.auth.obtenerUsuario()?.id;
    if (!idUsuario) return of({ solicitudes: [], metricas: { total: 0, enProgreso: 0, pendientes: 0, completadas: 0 } });

    const params = new HttpParams().set('idUsuario', idUsuario);
    return this.http.get<SolicitudBackend[]>(this.solicitudUrl, { params }).pipe(
      map(res => {
        const lista = Array.isArray(res) ? res : [];
        const mapped = lista.map(s => this.mapearSolicitud(s));
        const metricas = this.calcularMetricas(mapped.length, mapped);
        const filtradas = this.filtrarLocal({ solicitudes: mapped, metricas }, filtros).solicitudes;
        return { solicitudes: filtradas, metricas };
      }),
      catchError(() => of({ solicitudes: [], metricas: { total: 0, enProgreso: 0, pendientes: 0, completadas: 0 } })),
    );
  }

  actualizarServidor(servidorId: string, payload: Record<string, any>): Observable<any> {
    return this.http.put(`${environment.apiUrl}/servidor/${servidorId}`, payload);
  }

  descargarPdfFinal(solicitudId: string): Observable<Blob> {
    return this.http.get(`${this.solicitudUrl}/${solicitudId}/pdf`, { responseType: 'blob' });
  }

  obtenerDetalle(id: string): Observable<Solicitud> {
    if (this.useMock) {
      return of(DASHBOARD_MOCK.solicitudes.find(s => s.id === id)!);
    }
    return forkJoin({
      solicitud:  this.http.get<SolicitudBackend>(`${this.solicitudUrl}/${id}`),
      servidores: this.http.get<any[]>(`${environment.apiUrl}/servidor/solicitud/${id}`).pipe(
        catchError(() => of([] as any[]))
      ),
      carta: this.http.get<CartaBackend>(`${environment.apiUrl}/cartas/solicitud/${id}`).pipe(
        catchError(() => of(null))
      ),
    }).pipe(
      map(({ solicitud, servidores, carta }) => {
        const mapped = this.mapearSolicitud(solicitud);

        mapped.vpns = (solicitud.servidor?.['serverVpns'] ?? [])
          .map((sv: any): VpnServidor => ({
            id:              sv.vpn?.id              ?? sv.vpnId ?? undefined,
            vpnType:         sv.vpn?.vpnType         ?? '',
            responsable:     sv.vpn?.responsable     ?? '',
            cargo:           sv.vpn?.cargo           || undefined,
            phone:           sv.vpn?.phone           || undefined,
            email:           sv.vpn?.email           || undefined,
            folio:           sv.vpn?.folio           || undefined,
            vpnIp:           sv.vpn?.vpnIp           || undefined,
            perfilAnterior:  sv.vpn?.perfilAnterior  || undefined,
            servidores:      sv.vpn?.servidores      || undefined,
            empresa:         sv.vpn?.empresa         || undefined,
            vigenciaDias:    sv.vpn?.vigenciaDias    ?? undefined,
            estado:          sv.vpn?.estado          || undefined,
            fechaAsignacion: sv.vpn?.fechaAsignacion || undefined,
            fechaExpiracion: sv.vpn?.fechaExpiracion || undefined,
            assignedAt:      sv.assignedAt           ?? '',
          }));

        mapped.subdominios = (solicitud.servidor?.['serverSubdominios'] ?? [])
          .map((ss: any): SubdominioServidor => ({
            id:            ss.subdominio?.id          ?? ss.subdominioId ?? undefined,
            requestedName: ss.subdominio?.requestedName ?? '',
            port:          ss.subdominio?.port          ?? 0,
            sslRequired:   ss.subdominio?.sslRequired   ?? false,
            status:        ss.subdominio?.status        ?? '',
            assignedAt:    ss.assignedAt                ?? '',
          }));

        // Inyectar datos del servidor
        if (servidores.length > 0) {
          const srv = servidores[0];
          mapped.servidorId = String(srv.id);

          const etapa2 = mapped.etapas.find(e => e.numero === 2);
          if (etapa2) {
            etapa2.vCores         = srv.nucleos       ?? etapa2.vCores;
            etapa2.memoriaRam     = srv.ram            ?? etapa2.memoriaRam;
            etapa2.almacenamiento = srv.almacenamiento ?? etapa2.almacenamiento;
            etapa2.ip             = srv.ip             || etapa2.ip;
          }

          const etapa3 = mapped.etapas.find(e => e.numero === 3);
          if (etapa3 && srv.createdAt) {
            etapa3.fechaActualizacion = srv.createdAt;
          }

        }

        // Inyectar datos de la carta responsiva (Etapa 1)
        if (carta) {
          mapped.carta = this.mapearCarta(carta);
        }

        return mapped;
      })
    );
  }

  // ── Mapeo backend → modelo frontend ─────────────────────────────────────────

  private mapearSolicitud(s: SolicitudBackend): Solicitud {
    const etapaNum  = s.etapaActual ?? 1;
    const estadoSol = ESTADO_MAP[s.estatus] ?? 'pendiente';

    // Si el seguimiento de la etapa 14 ya está completado, el flujo está terminado
    // independientemente de si el campo estatus fue actualizado en el backend.
    const segs: any[]          = (s as any).seguimientos ?? [];
    const etapa14Completada    = segs.some((seg: any) => seg.etapaNumero === 14 && seg.status === 'completado');
    const terminada            = etapa14Completada
      || estadoSol === 'finalizado'
      || estadoSol === 'publicado';
    const estadoFinal: EstadoSolicitud = etapa14Completada ? 'publicado' : estadoSol;

    const etapas: EtapaSolicitud[] = ETAPAS_PROCESO.map(e => ({
      numero: e.numero,
      nombre: e.nombre,
      estado: terminada
            ? 'completada'
            : e.numero < etapaNum   ? 'completada'
            : e.numero === etapaNum ? 'en-curso'
            : 'sin-iniciar',
    }));

    return {
      id:                 String(s.id),
      folio:              s.folio,
      dependencia:        s.nombreAplicacion,
      nombreServidor:     s.servidor?.hostname ?? '',
      estado:             estadoFinal,
      etapaActual:        etapaNum,
      etapas,
      fechaRegistro:      s.createdAt,
      fechaActualizacion: s.updatedAt ?? s.createdAt,
    };
  }

  private mapearCarta(c: CartaBackend): CartaAprovisionamiento {
    const sol = c.solicitud ?? {};
    const dep = sol.dependency ?? {};

    return {
      areaRequirente: {
        sector:      dep.sector      ?? '',
        dependencia: dep.name        ?? '',
        responsable: dep.responsable ?? '',
        cargo:       dep.cargo       ?? '',
        telefono:    sol.telefonoResponsable ?? '',
        correo:      sol.correoResponsable   ?? '',
      },
      adminServidor: {
        proveedor:   sol.proveedor    ?? '',
        sector:      '',
        dependencia: sol.dependenciaAdmin ?? '',
        responsable: sol.responsableInfraestructura ?? c.firmanteAtdtNombre ?? '',
        cargo:       sol.cargoAdmin   ?? c.firmanteAtdtPuesto ?? '',
        telefono:    sol.telefonoAdmin ?? '',
        correo:      sol.correoAdmin   ?? '',
      },
      descripcion: {
        descripcionServidor:       sol.descripcionUso       ?? '',
        nombreServidor:            sol.nombreServidor        ?? '',
        nombreAplicacion:          sol.nombreAplicacion      ?? '',
        tipoUso:                   (sol.tipoUso as 'interno' | 'publicado') ?? 'interno',
        fechaArranque:             sol.fechaArranqueDeseada ?? '',
        vigencia:                  sol.vigenciaMeses != null ? `${sol.vigenciaMeses} meses` : '',
        caracteristicasEspeciales: sol.conectividadOtras    ?? '',
      },
      specs: {
        tipoRequerimiento:    (sol.tipoRequerimiento as 'estandar' | 'especifico') ?? 'estandar',
        modalidad:            sol.esClonacion ? 'renovacion' : 'nuevo',
        sistemaOperativo:     (sol.sistemaOperativo as 'windows' | 'linux' | 'otro') ?? 'linux',
        sistemaOperativoOtro: '',
        vCores:               sol.vcpuSolicitado            ?? 0,
        memoriaRam:           sol.ramSolicitadaGb           ?? 0,
        discosDuros:          sol.almacenamientoSolicitadoGb != null
                                ? [{ capacidad: sol.almacenamientoSolicitadoGb, tipo: 'SSD' as const, etiqueta: 'Sistema' }]
                                : [],
        motorBD:              sol.motorBaseDatos       ?? '',
        puertos:              sol.reglasFirewall        ?? '',
        integraciones:        sol.integracionesExternas ?? '',
        otrasSpecs:           '',
        ipActual:             '',
        nombreServidorActual: '',
        tipoRenovacion:       '',
      },
      infraestructura: {
        subdominios: [],
        requiereSSL: false,
        vpns:        [],
      },
      responsiva: {
        firmante:       c.firmanteDependenciaNombre   ?? '',
        numEmpleado:    c.firmanteDependenciaEmpleado ?? '',
        puestoFirmante: c.firmanteDependenciaPuesto   ?? '',
        aceptaTerminos: true,
      },
    };
  }

  private mapearSolicitudResumen(s: SolicitudResumenBackend): Solicitud {
    const etapaNum  = s.etapaActual ?? 1;
    const estadoSol = ESTADO_MAP[s.estado] ?? 'pendiente';
    const terminada = estadoSol === 'finalizado' || estadoSol === 'publicado';

    const etapas: EtapaSolicitud[] = ETAPAS_PROCESO.map(e => ({
      numero: e.numero,
      nombre: e.nombre,
      estado: terminada
            ? 'completada'
            : e.numero < etapaNum   ? 'completada'
            : e.numero === etapaNum ? 'en-curso'
            : 'sin-iniciar',
    }));

    return {
      id:                 String(s.id),
      folio:              s.folio,
      dependencia:        s.dependencia,
      nombreServidor:     '',
      estado:             estadoSol,
      etapaActual:        etapaNum,
      etapas,
      fechaRegistro:      s.actualizacion,
      fechaActualizacion: s.actualizacion,
    };
  }

  private calcularMetricas(total: number, solicitudes: Solicitud[]): DashboardMetricas {
    return {
      total,
      enProgreso:  solicitudes.filter(s => (EN_PROCESO as string[]).includes(s.estado)).length,
      pendientes:  solicitudes.filter(s => s.estado === 'pendiente').length,
      completadas: solicitudes.filter(s => (COMPLETADAS as string[]).includes(s.estado)).length,
    };
  }

  // ── Filtrado local (mock y post-fetch) ───────────────────────────────────────

  private filtrarLocal(data: DashboardResponse, filtros: DashboardFiltros): DashboardResponse {
    let solicitudes = data.solicitudes;

    if (filtros.busqueda) {
      const q = filtros.busqueda.toLowerCase();
      solicitudes = solicitudes.filter(s =>
        s.folio.toLowerCase().includes(q) || s.dependencia.toLowerCase().includes(q)
      );
    }
    if (filtros.estado) {
      solicitudes = solicitudes.filter(s => s.estado === filtros.estado);
    }
    if (filtros.etapa) {
      solicitudes = solicitudes.filter(s => s.etapaActual === filtros.etapa);
    }

    const metricas: DashboardMetricas = {
      total:      solicitudes.length,
      enProgreso:  solicitudes.filter(s => (EN_PROCESO as string[]).includes(s.estado)).length,
      pendientes:  solicitudes.filter(s => s.estado === 'pendiente').length,
      completadas: solicitudes.filter(s => (COMPLETADAS as string[]).includes(s.estado)).length,
    };

    return { solicitudes, metricas };
  }
}
