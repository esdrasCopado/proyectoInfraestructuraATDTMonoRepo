import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, shareReplay } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../../../environments/environment';

export type RolUsuario =
  | 'dependencia'
  | 'admin-cd'
  | 'admin-infraestructura'
  | 'admin-vulnerabilidades'
  | 'admin-general';

export interface UsuarioActual {
  id: string;
  nombre: string;
  correo: string;
  rol: RolUsuario;
}

export interface PerfilUsuario {
  email: string;
  cargo: string;
  numeroEmpleado: string;
  phone: string;
  rolNombre: string;
}

// Mapeo de los valores que envía el backend en el JWT → RolUsuario del frontend
// Después de normalizar: rolRaw.trim().toLowerCase().replace(/\s+/g, '_')
export const ROL_MAP: Record<string, RolUsuario> = {
  // Admin General
  'administrador_general':              'admin-general',
  'admin_general':                      'admin-general',
  'admin-general':                      'admin-general',
  'administradorgeneral':               'admin-general',
  'admingeneral':                       'admin-general',
  'administrador':                      'admin-general',
  'administrator':                      'admin-general',

  // Dependencia
  'dependencia_/_cliente':              'dependencia',
  'dependencia_cliente':                'dependencia',
  'dependencia':                        'dependencia',
  'cliente':                            'dependencia',
  'usuario':                            'dependencia',

  // Admin Centro de Datos (incluyendo nombre completo del backend)
  'administrador_de_centro_de_datos':   'admin-cd',
  'admin_centro_datos':                 'admin-cd',
  'admincentrodatos':                   'admin-cd',
  'admin_cd':                           'admin-cd',
  'admin-cd':                           'admin-cd',
  'admincd':                            'admin-cd',

  // Admin Infraestructura (incluyendo nombre completo del backend)
  'administrador_de_infraestructura':   'admin-infraestructura',
  'admin_infraestructura':              'admin-infraestructura',
  'admin-infraestructura':              'admin-infraestructura',
  'admininfraestructura':               'admin-infraestructura',
  'admininfra':                         'admin-infraestructura',
  'admin_infra':                        'admin-infraestructura',

  // Admin Vulnerabilidades (incluyendo nombre completo del backend)
  'administrador_de_vulnerabilidades':  'admin-vulnerabilidades',
  'admin_vulnerabilidades':             'admin-vulnerabilidades',
  'admin-vulnerabilidades':             'admin-vulnerabilidades',
  'adminvulnerabilidades':              'admin-vulnerabilidades',
  'admin_vuln':                         'admin-vulnerabilidades',
};

@Injectable({ providedIn: 'root' })
export class AuthService {

  private perfilCache$?: Observable<PerfilUsuario>;

  constructor(private http: HttpClient) {}

  getPerfil(): Observable<PerfilUsuario> {
    if (environment.useMock) {
      const usuario = this.obtenerUsuario();
      return of({
        email:          usuario?.correo ?? '',
        cargo:          '',
        numeroEmpleado: '',
        phone:          '',
        rolNombre:      usuario?.rol ?? '',
      });
    }
    if (!this.perfilCache$) {
      this.perfilCache$ = this.http
        .get<PerfilUsuario>(`${environment.apiUrl}/usuario/me`)
        .pipe(shareReplay(1));
    }
    return this.perfilCache$;
  }

  invalidarPerfil(): void {
    this.perfilCache$ = undefined;
  }

  obtenerUsuario(): UsuarioActual | null {
    const token = sessionStorage.getItem('jwtToken');
    if (!token) return null;

    try {
      const decoded: any = jwtDecode(token);

      // El backend puede poner el rol en distintos claims
      const rolRaw: string =
        decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        decoded['Permisos'] ||
        decoded['rol'] ||
        decoded['role'] ||
        decoded['roles'] ||
        '';

      const rolKey = rolRaw.trim().toLowerCase().replace(/\s+/g, '_');
      const rol = ROL_MAP[rolKey] ?? 'dependencia';

      const id: string =
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
        decoded['id'] ||
        '';

      const nombre: string =
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
        decoded['nombre'] ||
        decoded['sub'] ||
        '';

      const correo: string =
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ||
        decoded['correo'] ||
        decoded['email'] ||
        '';

      return { id: String(id), nombre, correo, rol };
    } catch {
      return null;
    }
  }

  obtenerRol(): RolUsuario | null {
    return this.obtenerUsuario()?.rol ?? null;
  }

  esDependencia(): boolean {
    return this.obtenerRol() === 'dependencia';
  }

  esAdminCD(): boolean {
    return this.obtenerRol() === 'admin-cd';
  }

  esAdminInfraestructura(): boolean {
    return this.obtenerRol() === 'admin-infraestructura';
  }

  esAdminVulnerabilidades(): boolean {
    return this.obtenerRol() === 'admin-vulnerabilidades';
  }

  esAdminGeneral(): boolean {
    return this.obtenerRol() === 'admin-general';
  }

  estaAutenticado(): boolean {
    return this.obtenerUsuario() !== null;
  }

  cerrarSesion(): void {
    sessionStorage.removeItem('jwtToken');
    sessionStorage.removeItem('mustChangePassword');
    this.invalidarPerfil();
  }
}
