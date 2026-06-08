import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../dashboard/services/auth.service';

export type TipoNotificacion =
  | 'solicitud_nueva'
  | 'subdominio_asignado'
  | 'subdominio'
  | 'evidencias_cargadas'
  | 'evidencias_aprobadas'
  | 'evidencias_rechazadas'
  | 'solicitud_publicacion'
  | 'vulnerabilidades_aprobadas'
  | 'vulnerabilidades_rechazadas';

export interface Notificacion {
  notificationId: number;
  recipientUserId: number;
  senderUserId: number;
  tipo: TipoNotificacion;
  entityType: string;
  entityId: number;
  solicitudId?: number;
  titulo: string;
  mensaje: string;
  leida: boolean;
  leidaAt?: string;
  createdAt: string;
}

export interface CrearNotificacionDto {
  recipientUserId: number;
  senderUserId: number;
  tipo: TipoNotificacion;
  solicitudId?: number;
  entityType: string;
  entityId: number;
  titulo: string;
  mensaje: string;
}

const ROL_NOMBRE: Record<string, string> = {
  'dependencia':            'Dependencia / Cliente',
  'admin-cd':               'Administrador de Centro de Datos',
  'admin-infraestructura':  'Administrador de Infraestructura',
  'admin-vulnerabilidades': 'Administrador de Vulnerabilidades',
  'admin-general':          'Administrador General',
};

@Injectable({ providedIn: 'root' })
export class NotificacionService {

  private readonly base = `${environment.apiUrl}/notifications`;

  constructor(private http: HttpClient, private auth: AuthService) {}

  private rolNombreActual(): string | null {
    const rol = this.auth.obtenerRol();
    // dependencia no puede usar ?rol= (requiere admin); el backend filtra por JWT
    if (!rol || rol === 'dependencia') return null;
    return ROL_NOMBRE[rol] ?? null;
  }

  private userIdActual(): number {
    return parseInt(this.auth.obtenerUsuario()?.id ?? '0', 10) || 0;
  }

  getNotificaciones(opts?: { leida?: boolean }): Observable<Notificacion[]> {
    if (environment.useMock) return of([]);
    let params = new HttpParams();
    if (opts?.leida !== undefined) params = params.set('leida', String(opts.leida));
    const rolNombre = this.rolNombreActual();
    if (rolNombre) {
      // admin: consulta todas las notifs del rol y filtra por recipientUserId propio
      params = params.set('rol', rolNombre);
      const uid = this.userIdActual();
      return this.http.get<Notificacion[]>(this.base, { params }).pipe(
        map(notifs => uid ? notifs.filter(n => n.recipientUserId === uid) : notifs),
      );
    }
    // dependencia: el backend devuelve solo las del usuario autenticado (sin ?rol=)
    return this.http.get<Notificacion[]>(this.base, { params });
  }

  getConteoNoLeidas(): Observable<number> {
    if (environment.useMock) return of(0);
    return this.getNotificaciones().pipe(
      map(notifs => notifs.filter(n => !n.leida).length),
    );
  }

  marcarLeida(id: number, leida = true): Observable<any> {
    if (environment.useMock) return of(null);
    return this.http.patch(`${this.base}/${id}/leer`, { leida });
  }

  crearNotificacion(dto: CrearNotificacionDto): Observable<Notificacion> {
    return this.http.post<Notificacion>(this.base, dto);
  }

  /** POST /api/notifications/rol — el backend crea una notif por cada usuario activo del rol */
  notificarRol(
    rol: string,
    tipo: TipoNotificacion,
    titulo: string,
    mensaje: string,
    solicitudId?: number,
    senderUserId?: number,
  ): Observable<any> {
    if (environment.useMock) return of(null);
    return this.http.post(`${this.base}/rol`, {
      rolNombre: ROL_NOMBRE[rol] ?? rol,
      tipo,
      solicitudId,
      titulo,
      mensaje,
      ...(senderUserId ? { senderUserId } : {}),
    }).pipe(
      catchError(() => of(null)),
    );
  }

  /** POST /api/notifications/solicitud/cliente — notifica al creador de la solicitud */
  notificarCliente(
    solicitudId: number,
    tipo: TipoNotificacion,
    titulo: string,
    mensaje: string,
  ): Observable<any> {
    if (environment.useMock) return of(null);
    return this.http.post(`${this.base}/solicitud/cliente`, {
      solicitudId,
      tipo,
      titulo,
      mensaje,
    }).pipe(
      catchError(() => of(null)),
    );
  }

  /** POST /api/notifications — notifica a un usuario concreto por su ID */
  notificarUsuario(
    recipientUserId: number,
    senderUserId: number,
    tipo: TipoNotificacion,
    titulo: string,
    mensaje: string,
    solicitudId?: number,
    entityId?: number,
  ): Observable<any> {
    if (environment.useMock) return of(null);
    return this.crearNotificacion({
      recipientUserId,
      senderUserId,
      tipo,
      titulo,
      mensaje,
      entityType: 'solicitud',
      entityId: entityId ?? solicitudId ?? 0,
      solicitudId,
    }).pipe(catchError(() => of(null)));
  }
}
