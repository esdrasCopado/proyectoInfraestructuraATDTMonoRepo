import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { DatePipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NotificacionService, Notificacion, TipoNotificacion } from '../services/notificacion.service';

type FiltroNotif = 'todas' | 'no-leidas';

const TIPO_META: Record<TipoNotificacion, { icono: string; color: string; etiqueta: string }> = {
  solicitud_nueva:             { icono: 'assignment',    color: '#7c3aed', etiqueta: 'Nueva solicitud'             },
  subdominio_asignado:         { icono: 'language',      color: '#1d4ed8', etiqueta: 'Subdominio asignado'         },
  subdominio:                  { icono: 'language',      color: '#1d4ed8', etiqueta: 'Subdominio asignado'         },
  evidencias_cargadas:         { icono: 'upload_file',   color: '#0891b2', etiqueta: 'Evidencias cargadas'         },
  evidencias_aprobadas:        { icono: 'task_alt',      color: '#16a34a', etiqueta: 'Evidencias aprobadas'        },
  evidencias_rechazadas:       { icono: 'cancel',        color: '#dc2626', etiqueta: 'Evidencias rechazadas'       },
  solicitud_publicacion:       { icono: 'public',        color: '#7c3aed', etiqueta: 'Solicitud de publicación'    },
  vulnerabilidades_aprobadas:  { icono: 'verified_user', color: '#16a34a', etiqueta: 'Vulnerabilidades aprobadas'  },
  vulnerabilidades_rechazadas: { icono: 'gpp_bad',       color: '#dc2626', etiqueta: 'Vulnerabilidades rechazadas' },
};

@Component({
  selector: 'app-notificaciones',
  standalone: true,
  imports: [DatePipe, MatIconModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './notificaciones.component.html',
  styleUrl: './notificaciones.component.scss',
})
export class NotificacionesComponent implements OnInit {

  notificaciones: Notificacion[] = [];
  filtro: FiltroNotif = 'todas';
  cargando = true;

  constructor(
    private notifService: NotificacionService,
    private cdr: ChangeDetectorRef,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  private cargar(): void {
    this.cargando = true;
    const leida = this.filtro === 'no-leidas' ? false : undefined;
    this.notifService.getNotificaciones({ leida }).subscribe({
      next: (data) => {
        this.notificaciones = data.sort(
          (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
        );
        this.cargando = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.cargando = false;
        this.cdr.detectChanges();
      },
    });
  }

  get listado(): Notificacion[] { return this.notificaciones; }

  get totalNoLeidas(): number {
    return this.notificaciones.filter(n => !n.leida).length;
  }

  setFiltro(f: FiltroNotif): void {
    if (this.filtro === f) return;
    this.filtro = f;
    this.cargar();
  }

  meta(tipo: TipoNotificacion) {
    return TIPO_META[tipo] ?? { icono: 'notifications', color: '#6b7280', etiqueta: tipo };
  }

  onNotifClick(notif: Notificacion): void {
    if (!notif.leida) {
      notif.leida = true;
      this.cdr.detectChanges();
      this.notifService.marcarLeida(notif.notificationId).subscribe();
    }
    if (notif.solicitudId) {
      this.router.navigate(['/expediente', notif.solicitudId]);
    }
  }

  marcarLeida(notif: Notificacion): void {
    if (notif.leida) return;
    notif.leida = true;
    if (this.filtro === 'no-leidas') {
      this.notificaciones = this.notificaciones.filter(n => n.notificationId !== notif.notificationId);
    }
    this.cdr.detectChanges();
    this.notifService.marcarLeida(notif.notificationId).subscribe();
  }

  marcarTodasLeidas(): void {
    const noLeidas = this.notificaciones.filter(n => !n.leida);
    if (!noLeidas.length) return;
    forkJoin(noLeidas.map(n => this.notifService.marcarLeida(n.notificationId))).subscribe({
      next: () => {
        noLeidas.forEach(n => (n.leida = true));
        if (this.filtro === 'no-leidas') this.notificaciones = [];
        this.cdr.detectChanges();
      },
    });
  }
}
