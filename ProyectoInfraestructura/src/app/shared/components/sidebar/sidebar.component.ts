import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { filter } from 'rxjs/operators';
import { AuthService, RolUsuario } from '../../../features/dashboard/services/auth.service';

interface Submodulo {
  label: string;
  ruta: string;
  roles?: RolUsuario[];
}

interface Modulo {
  label: string;
  icono: string;
  ruta?: string;
  roles?: RolUsuario[];
  submodulos?: Submodulo[];
}

interface Seccion {
  label: string;
  modulos: Modulo[];
}

const MENU: Seccion[] = [
  {
    label: 'Navegación',
    modulos: [
      {
        label: 'Dashboard',
        icono: 'dashboard',
        ruta: '/dashboard',
      },
      {
        label: 'Nueva solicitud',
        icono: 'add_circle_outline',
        ruta: '/carta-aprovisionamiento',
        roles: ['dependencia', 'admin-general'],
      },
      {
        label: 'Reportes',
        icono: 'assessment',
        ruta: '/reportes',
        roles: ['admin-cd', 'admin-infraestructura', 'admin-vulnerabilidades', 'admin-general'],
        submodulos: [
          { label: '1.1 Solicitudes por dependencia', ruta: '/reportes/solicitudes-dependencia', roles: ['admin-cd'] },
          { label: '1.2 Recursos solicitados',        ruta: '/reportes/recursos-totalizados',    roles: ['admin-cd'] },
          { label: '1.3 Reporte por IP',              ruta: '/reportes/por-ip',                  roles: ['admin-cd'] },
          { label: '2.1 Reporte de VPN',              ruta: '/reportes/vpn',                     roles: ['admin-infraestructura'] },
          { label: '2.2 Reporte de Subdominios',      ruta: '/reportes/subdominios',             roles: ['admin-infraestructura'] },
          { label: '3.1 Vulnerabilidades',            ruta: '/reportes/vulnerabilidades',        roles: ['admin-vulnerabilidades'] },
          { label: '3.2 Comunicaciones por IP',       ruta: '/reportes/comunicaciones-ip',       roles: ['admin-vulnerabilidades'] },
          { label: '4.1 Estatus de solicitudes',      ruta: '/reportes/estatus-solicitudes',     roles: ['admin-general'] },
          { label: '4.2 Recursos totalizados',        ruta: '/reportes/recursos-general',        roles: ['admin-general'] },
        ],
      },
    ],
  },
  {
    label: 'Administración',
    modulos: [
      {
        label: 'Crear usuario',
        icono: 'person_add',
        ruta: '/crear-usuario',
        roles: ['admin-general'],
      },
      {
        label: 'Mi perfil',
        icono: 'manage_accounts',
        ruta: '/perfil-usuario',
      },
    ],
  },
];

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, MatTooltipModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent implements OnInit {
  @Input() collapsed = false;
  @Output() toggleCollapse = new EventEmitter<void>();

  rol: RolUsuario | null = null;
  secciones: Seccion[] = [];
  expandedModules = new Set<string>();
  rutaActual = '';

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.rol = this.authService.obtenerUsuario()?.rol ?? null;
    this.secciones = this.filtrarMenu(MENU);
    this.actualizarRuta(this.router.url);

    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(e => this.actualizarRuta((e as NavigationEnd).urlAfterRedirects));
  }

  private actualizarRuta(url: string): void {
    this.rutaActual = url.split('?')[0].split('#')[0];
    this.secciones.forEach(s =>
      s.modulos.forEach(m => {
        if (this.isModuloActivo(m)) this.expandedModules.add(m.label);
      })
    );
  }

  private filtrarMenu(menu: Seccion[]): Seccion[] {
    return menu
      .map(s => ({
        ...s,
        modulos: s.modulos
          .filter(m => !m.roles || (this.rol && m.roles.includes(this.rol)))
          .map(m => ({
            ...m,
            submodulos: m.submodulos?.filter(sm => !sm.roles || (this.rol && sm.roles.includes(this.rol))),
          })),
      }))
      .filter(s => s.modulos.length > 0);
  }

  toggleModulo(label: string): void {
    if (this.expandedModules.has(label)) {
      this.expandedModules.delete(label);
    } else {
      this.expandedModules.add(label);
      const modulo = this.secciones.flatMap(s => s.modulos).find(m => m.label === label);
      const primeraRuta = modulo?.submodulos?.[0]?.ruta;
      if (primeraRuta) this.router.navigate([primeraRuta]);
    }
  }

  isModuloActivo(modulo: Modulo): boolean {
    if (!modulo.ruta) return false;
    const base = '/' + modulo.ruta.split('/')[1];
    return this.rutaActual.startsWith(base);
  }

  isSubmoduloActivo(ruta: string): boolean {
    return this.rutaActual === ruta;
  }
}
