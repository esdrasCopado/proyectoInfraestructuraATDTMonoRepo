import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Usuario } from '../../../shared/components/interfaces/Usuario';
import {jwtDecode} from 'jwt-decode';

import { environment } from '../../../../environments/environment';

import { of, Subscription, Subject } from 'rxjs';
import { delay } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class LoginService {
  currentRoute: any = '';
  private apiUrl = environment.apiUrl;
  private imgUrl = environment.imgUrl;

  private tokenSubscription: Subscription = new Subscription();

  private alertaExpiracionSubject = new Subject<void>();

  constructor(private router: Router, private httpClient: HttpClient) {}

  ngOnInit(): void {
    this.currentRoute = this.router.url;
  }

  iniciarSesion(usuario: Usuario) {
    return this.httpClient.post<{ token: string; user?: { mustChangePassword?: boolean; id?: string | number; [k: string]: any } }>(
      `${this.apiUrl}/auth/login`,
      { email: usuario.correo, password: usuario.password }
    );
  }

  saveMustChangePassword(value: boolean): void {
    sessionStorage.setItem('mustChangePassword', String(value));
  }

  getMustChangePassword(): boolean {
    return sessionStorage.getItem('mustChangePassword') === 'true';
  }

  clearMustChangePassword(): void {
    sessionStorage.removeItem('mustChangePassword');
  }

  enviarCorreo(correo: string) {
    return this.httpClient.post<{}>(
      `${this.apiUrl}/acceso/recuperar/${correo}`,
      null
    );
  }

  saveToken(token: string) {
    sessionStorage.setItem('jwtToken', token);
    this.startTokenExpirationMonitor();
  }

  getToken(): string | null {
    return sessionStorage.getItem('jwtToken');
  }

  removeToken() {
    sessionStorage.removeItem('jwtToken');
  }

  verificarSesion() {

    if (this.getToken() == null) {
      this.router.navigate(['login']);
    } else if (
      this.router.url === '/admin' &&
      this.getUserRoleFromToken() != 'ADMINISTRADOR'
    ) {
      this.router.navigate(['directorio']);
    }
  }

  decodeToken(token: string): any | null {
    try {
      return jwtDecode<any>(token);
    } catch {
      return null;
    }
  }

  getUserNameFromToken(): string | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    try {
      const decoded: any = this.decodeToken(token);
      const userName =
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
      return userName || null;
    } catch (error) {
      return null;
    }
  }

  getUserRoleFromToken(): string | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    try {
      const decoded: any = this.decodeToken(token);

      const role =
        decoded[
          'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
        ] ||
        decoded['role'] ||
        decoded['roles'] ||
        decoded['Permisos'];

      const arreglar = role.trim().toUpperCase();

      if (arreglar !== 'ADMINISTRADOR' && arreglar !== 'admin') {
        return 'USUARIO';
      }

      return arreglar || null;
    } catch (error) {
      return null;
    }
  }

  getIDFromToken(): string | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    try {
      const decoded: any = this.decodeToken(token);
      const id =
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
        decoded['id'];

      return id || null;
    } catch (error) {
      return null;
    }
  }

  getUserImageFromToken(): string | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    try {
      const decoded: any = this.decodeToken(token);
      const imagen = decoded['ImagenUrl'];
      return imagen
        ? `${this.imgUrl}/imagenes/sesiones/${imagen}`
        : 'https://ejemplo.com/default.jpg';
    } catch {
      return 'https://ejemplo.com/default.jpg';
    }
  }

  isTokenExpired(token: string): boolean {
    const expirationTime = this.getTokenExpiration(token);
    const currentTime = Math.floor(Date.now() / 1000);
    return expirationTime < currentTime;
  }

  getTokenExpiration(token: string): number {
    const decodedToken: any = jwtDecode(token);
    return decodedToken.exp;
  }

  startTokenExpirationMonitor() {
    const token = this.getToken();
    if (!token) return;

    const expiration = this.getTokenExpiration(token) * 1000;
    const now = Date.now();
    const timeout = expiration - now;

    if (timeout <= 0) {
      this.removeToken();
      this.mostrarAlertaExpiracion();
      return;
    }

    this.expirationCounter(timeout);
  }

  expirationCounter(timeout: number) {
    this.tokenSubscription?.unsubscribe();

    this.tokenSubscription = of(null).pipe(delay(timeout)).subscribe(() => {
      this.removeToken();
      this.mostrarAlertaExpiracion();
    });
  }

  get alertaExpiracion$() {
    return this.alertaExpiracionSubject.asObservable();
  }

  mostrarAlertaExpiracion() {
    this.alertaExpiracionSubject.next();
  }
}
