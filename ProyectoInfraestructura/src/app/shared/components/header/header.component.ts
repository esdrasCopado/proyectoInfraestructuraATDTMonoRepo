import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { catchError, filter, startWith, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService, RolUsuario } from '../../../features/dashboard/services/auth.service';
import { NotificacionService, Notificacion, TipoNotificacion } from '../../../features/notificaciones/services/notificacion.service';

const TIPO_ICONO: Record<TipoNotificacion, { icono: string; color: string }> = {
  solicitud_nueva:             { icono: 'assignment',    color: '#7c3aed' },
  subdominio_asignado:         { icono: 'language',      color: '#1d4ed8' },
  subdominio:                  { icono: 'language',      color: '#1d4ed8' },
  evidencias_cargadas:         { icono: 'upload_file',   color: '#0891b2' },
  evidencias_aprobadas:        { icono: 'task_alt',      color: '#16a34a' },
  evidencias_rechazadas:       { icono: 'cancel',        color: '#dc2626' },
  solicitud_publicacion:       { icono: 'public',        color: '#7c3aed' },
  vulnerabilidades_aprobadas:  { icono: 'verified_user', color: '#16a34a' },
  vulnerabilidades_rechazadas: { icono: 'gpp_bad',       color: '#dc2626' },
};

const ETIQUETA_ROL: Record<RolUsuario, string> = {
  'dependencia':            'Dependencia / Cliente',
  'admin-cd':               'Administrador de Centro de Datos',
  'admin-infraestructura':  'Administrador de Infraestructura',
  'admin-vulnerabilidades': 'Administrador de Vulnerabilidades',
  'admin-general':          'Administrador General',
};

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    RouterLink,
    MatMenuModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    MatBadgeModule,
    MatTooltipModule,
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent implements OnInit, OnDestroy {
  nombreUsuario    = '';
  etiquetaRol      = '';
  iniciales        = '';
  esAdminGeneral = false;
  esDependencia  = false;
  esAdmin        = false;
  paginaActual   = '';

  notificaciones: Notificacion[] = [];
  totalMensajes = 0;

  private notifSub?: Subscription;

  get totalNotificaciones(): number {
    return this.notificaciones.filter(n => !n.leida).length;
  }

  get notificacionesRecientes(): Notificacion[] {
    return this.notificaciones.slice(0, 5);
  }

  private readonly PAGINAS: Record<string, string> = {
    '/dashboard':               'Dashboard',
    '/carta-aprovisionamiento': 'Nueva solicitud',
    '/crear-usuario':           'Crear usuario',
    '/perfil-usuario':          'Mi perfil',
    '/notificaciones':          'Notificaciones',
    '/mensajes':                'Mensajes',
    '/historial':               'Historial de actividad',
    '/ayuda':                   'Ayuda',
    '/acerca':                  'Acerca del sistema',
    '/reportes':                'Reportes',
  };

  constructor(
    private authService: AuthService,
    private router: Router,
    private notifService: NotificacionService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    const usuario = this.authService.obtenerUsuario();
    if (usuario) {
      this.nombreUsuario  = usuario.nombre || usuario.correo;
      this.etiquetaRol    = ETIQUETA_ROL[usuario.rol] ?? usuario.rol;
      this.iniciales      = this.calcularIniciales(this.nombreUsuario);
      this.esAdminGeneral = usuario.rol === 'admin-general';
      this.esDependencia  = usuario.rol === 'dependencia';
      this.esAdmin        = usuario.rol !== 'dependencia';
    }

    this.paginaActual = this.resolverPagina(this.router.url);

    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(e => {
        this.paginaActual = this.resolverPagina((e as NavigationEnd).urlAfterRedirects);
      });

    this.notifSub = interval(60_000).pipe(
      startWith(0),
      switchMap(() => this.notifService.getNotificaciones().pipe(catchError(() => of([])))),
    ).subscribe((list: Notificacion[]) => {
      this.notificaciones = list.sort(
        (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
      );
      this.cdr.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.notifSub?.unsubscribe();
  }

  metaNotif(tipo: TipoNotificacion) {
    return TIPO_ICONO[tipo] ?? { icono: 'notifications', color: '#6b7280' };
  }

  onNotifClick(n: Notificacion): void {
    if (!n.leida) {
      n.leida = true;
      this.notifService.marcarLeida(n.notificationId).subscribe();
    }
    if (n.solicitudId) {
      this.router.navigate(['/expediente', n.solicitudId]);
    } else {
      this.router.navigate(['/notificaciones']);
    }
  }

  cerrarSesion(): void {
    this.authService.cerrarSesion();
    this.router.navigate(['/login']);
  }

  private resolverPagina(url: string): string {
    const ruta = '/' + url.split('/')[1].split('?')[0];
    return this.PAGINAS[ruta] ?? '';
  }

  private calcularIniciales(nombre: string): string {
    const partes = nombre.trim().split(/\s+/);
    if (partes.length >= 2) {
      return (partes[0][0] + partes[1][0]).toUpperCase();
    }
    return nombre.slice(0, 2).toUpperCase();
  }
}
