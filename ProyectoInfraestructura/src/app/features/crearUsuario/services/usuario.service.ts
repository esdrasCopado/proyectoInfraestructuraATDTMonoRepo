import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface CrearUsuarioDto {
  nombre: string;
  apellidos: string;
  email: string;
  roleId: number;
  dependencyId?: number;
  numeroEmpleado?: string;
  cargo?: string;
  phone?: string;
}

export interface UsuarioDto {
  id: number;
  nombre: string;
  apellidos: string;
  nombreCompleto: string;
  roleId: number;
  rolNombre: string;
  dependencyId?: number;
  email: string;
  numeroEmpleado?: string;
  cargo?: string;
  phone?: string;
  activo: boolean;
  mustChangePassword: boolean;
  createdAt: string;
}

export interface CreateUsuarioResponse {
  usuario: UsuarioDto;
  passwordTemporal: string;
  correoEnviado: boolean;
  advertenciaCorreo: string | null;
}

export interface RolOpcion {
  roleId: number;
  nombre: string;
  descripcion?: string;
}

export interface DependenciaOpcion {
  dependencyId: number;
  name: string;
  sector?: string;
  responsable?: string;
  cargo?: string;
  phone?: string;
  email?: string;
}

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  private readonly base = `${environment.apiUrl}/usuario`;

  constructor(private http: HttpClient) {}

  crearUsuario(dto: CrearUsuarioDto): Observable<CreateUsuarioResponse> {
    return this.http.post<CreateUsuarioResponse>(this.base, dto);
  }

  getRoles(): Observable<RolOpcion[]> {
    return this.http.get<RolOpcion[]>(`${this.base}/roles`)
      .pipe(catchError(() => of([])));
  }

  getDependencias(): Observable<DependenciaOpcion[]> {
    return this.http.get<DependenciaOpcion[]>(`${environment.apiUrl}/dependencia`)
      .pipe(catchError(() => of([])));
  }

  crearDependencia(nombre: string): Observable<DependenciaOpcion> {
    return this.http.post<DependenciaOpcion>(`${environment.apiUrl}/dependencia`, { name: nombre });
  }

  cambiarPassword(userId: string, password: string): Observable<UsuarioDto> {
    return this.http.patch<UsuarioDto>(`${this.base}/${userId}/password`, { password });
  }
}
