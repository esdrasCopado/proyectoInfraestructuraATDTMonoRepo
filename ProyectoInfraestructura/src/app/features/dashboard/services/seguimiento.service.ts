import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, concat, forkJoin, of, throwError } from 'rxjs';
import { last } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { AuthService, RolUsuario } from './auth.service';

const ROLES_POR_ETAPA: Record<number, RolUsuario[]> = {
  1:  ['admin-cd', 'admin-general'],
  2:  ['admin-cd',    'admin-general'],
  3:  ['admin-cd',    'admin-general'],
  4:  ['admin-cd',    'admin-general'],
  5:  ['admin-cd',    'admin-general'],
  6:  ['admin-cd',    'admin-general'],
  7:  ['admin-infraestructura', 'admin-general'],
  8:  ['admin-infraestructura', 'admin-general'],
  9:  ['admin-cd',    'admin-general'],
  10: ['dependencia', 'admin-general'],
  11: ['dependencia', 'admin-general'],
  12: ['admin-cd', 'admin-infraestructura', 'admin-general'],
  13: ['dependencia', 'admin-general'],
  14: ['admin-vulnerabilidades', 'admin-general'],
};

const ROLES_RECHAZO: RolUsuario[] = ['admin-cd', 'admin-infraestructura', 'admin-general'];

const ROLES_REGRESAR: RolUsuario[] = [
  'admin-cd', 'admin-infraestructura', 'admin-vulnerabilidades', 'admin-general',
];

export interface CompletarEtapaExtra {
  [key: string]: any;
}

export interface RechazarEvidenciaItem {
  id: number;
  motivo: string;
}

export interface SeguimientoEtapaDto {
  seguimientoId: number;
  solicitudId: number;
  etapaNumero: number;
  etapaNombre: string;
  status: string;
  observaciones?: string;
  fechaCompletado?: string;
  createdAt: string;
  updatedAt: string;
}

@Injectable({ providedIn: 'root' })
export class SeguimientoService {

  private readonly baseUrl   = `${environment.apiUrl}/seguimiento/solicitud`;
  private readonly solicitudUrl = `${environment.apiUrl}/solicitud`;

  // ── Handlers adicionales por etapa ───────────────────────────────────────────
  // Agrega aquí funciones para etapas que requieran peticiones extra además
  // del PATCH estándar. Se ejecutan en secuencia después del PATCH principal.
  //
  // Ejemplo:
  //   3: (solicitudId, extra) =>
  //        this.http.post(`${environment.apiUrl}/servidor`, { solicitudId, ...extra }),
  private readonly etapaHandlers: Partial<
    Record<number, (solicitudId: string, extra?: CompletarEtapaExtra) => Observable<any>>
  > = {
    // Etapa 3 – Creación de servidor: descomentar y ajustar cuando el endpoint esté listo
    // 3: (solicitudId, extra) =>
    //      this.http.post(`${environment.apiUrl}/servidor`, { solicitudId, ...extra }),

    7: (_solicitudId, extra) => {
      const ids: number[] = extra?.['vpnIds'] ?? [];
      if (!ids.length) return of(null);
      return forkJoin(ids.map(id =>
        this.http.patch(`${environment.apiUrl}/vpn/${id}/estado`, { estado: 'activo' }),
      ));
    },

    8: (_solicitudId, extra) => {
      const ids: number[] = extra?.['subdominioIds'] ?? [];
      if (!ids.length) return of(null);
      return this.http.patch(
        `${environment.apiUrl}/subdominios/batch/status`,
        { ids, status: 'activo' },
      );
    },

    14: (solicitudId) =>
          this.http.patch(`${this.solicitudUrl}/${solicitudId}/estatus`, { estatus: 'finalizado' }),
  };

  constructor(private http: HttpClient, private auth: AuthService) {}

  private rolActual(): RolUsuario | null {
    return this.auth.obtenerRol();
  }

  private puedeEnEtapa(etapa: number): boolean {
    const rol = this.rolActual();
    if (!rol) return false;
    return (ROLES_POR_ETAPA[etapa] ?? []).includes(rol);
  }

  private puedeRechazar(): boolean {
    const rol = this.rolActual();
    return !!rol && ROLES_RECHAZO.includes(rol);
  }

  private puedeRegresar(): boolean {
    const rol = this.rolActual();
    return !!rol && ROLES_REGRESAR.includes(rol);
  }

  private sinPermiso(): Observable<never> {
    return throwError(() => new Error('Sin permiso para esta operación'));
  }

  rechazarEvidencias(
    solicitudId: string,
    evidencias: RechazarEvidenciaItem[],
    observacionEtapa: string,
    rechazadoBy: number,
  ): Observable<any> {
    if (environment.useMock) return of(null);
    if (!this.puedeRechazar()) return this.sinPermiso();
    return this.http.patch(
      `${this.baseUrl}/${solicitudId}/rechazar-evidencias`,
      { evidencias, observacionEtapa, rechazadoBy },
    );
  }

  getSeguimiento(solicitudId: string): Observable<SeguimientoEtapaDto[]> {
    if (environment.useMock) return of([]);
    return this.http.get<SeguimientoEtapaDto[]>(`${this.baseUrl}/${solicitudId}`);
  }

  iniciarEtapa(solicitudId: string, etapaNumero: number): Observable<any> {
    if (environment.useMock) return of(null);
    if (!this.puedeEnEtapa(etapaNumero)) return this.sinPermiso();
    return this.http.patch(
      `${this.baseUrl}/${solicitudId}/etapa/${etapaNumero}`,
      { status: 'en_proceso' },
    );
  }

  regresarEtapa(solicitudId: string, etapaDestino: number, motivo: string): Observable<any> {
    if (environment.useMock) return of(null);
    if (!this.puedeRegresar()) return this.sinPermiso();
    return this.http.patch(
      `${this.baseUrl}/${solicitudId}/regresar/${etapaDestino}`,
      { motivo },
    );
  }

  completarEtapa(
    solicitudId: string,
    etapaNumero: number,
    completadoBy: number,
    extra?: CompletarEtapaExtra,
  ): Observable<any> {
    if (environment.useMock) return of(null);
    if (!this.puedeEnEtapa(etapaNumero)) return this.sinPermiso();

    const body: Record<string, any> = { status: 'completado', completadoBy };
    if (extra?.['observacion']) body['observaciones'] = extra['observacion'];

    const patch$ = this.http.patch(
      `${this.baseUrl}/${solicitudId}/etapa/${etapaNumero}`,
      body,
    );

    const handler = this.etapaHandlers[etapaNumero];
    if (!handler) return patch$;

    return concat(patch$, handler(solicitudId, extra)).pipe(last());
  }
}
