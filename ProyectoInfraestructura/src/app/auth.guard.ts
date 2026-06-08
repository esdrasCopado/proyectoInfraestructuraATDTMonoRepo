import { inject } from '@angular/core';
import { CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { LoginService } from './features/login/data-access/login.service';

export const authGuard: CanActivateFn = (next: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const loginService = inject(LoginService);
  const router = inject(Router);

  const token = loginService.getToken();

  if (!token || loginService.isTokenExpired(token)) {
    router.navigate(['login']);
    return false;
  }

  if (loginService.getMustChangePassword() && state.url !== '/cambio-password') {
    router.navigate(['cambio-password']);
    return false;
  }

  const requiredPermission = next.data['requiredPermission'];
  const requiredAccessLevel = next.data['accessLevel'];

  if (!requiredPermission || !requiredAccessLevel) {
    return true;
  }

  const permisos = getPermisosFromToken(token, loginService);
  const userLevel = permisos[requiredPermission];

  if (!userLevel || !hasPermission(userLevel, requiredAccessLevel)) {
    const nuevaRuta = getRutaPermitida(permisos, state.url);

    if (nuevaRuta) {
      if (nuevaRuta !== state.url) {
        router.navigate([nuevaRuta]);
      } else {
        router.navigate(['sin-permiso']);
      }
    } else {
      router.navigate(['sin-permiso']);
    }

    return false;
  }

  return true;
};

function getPermisosFromToken(token: string, loginService: LoginService): any {
  try {
    const decoded: any = loginService.decodeToken(token);
    const rawPermisos = decoded['Permisos'];

    if (typeof rawPermisos === 'string') {
      const arreglar = rawPermisos.trim().toUpperCase();

      if (arreglar === 'ADMINISTRADOR' || arreglar === 'ADMIN') {
        return {
          agenda: 'administrador',
          eventos: 'administrador',
          reportes: 'administrador',
          directorio: 'administrador',
          obsequios: 'administrador'
        };
      }

      return JSON.parse(rawPermisos);
    }

    return rawPermisos || {};
  } catch (error) {
    return {};
  }
}

function hasPermission(userLevel: string, requiredLevel: string): boolean {
  const hierarchy = ['sin permisos', 'lector', 'editor', 'administrador'];
  const userIndex = hierarchy.indexOf(userLevel.toLowerCase());
  const requiredIndex = hierarchy.indexOf(requiredLevel.toLowerCase());
  return userIndex >= requiredIndex;
}

function getRutaPermitida(permisos: any, rutaActual: string): string | null {
  const rutasPermitidas = [
    { path: 'agenda', permiso: 'agenda', nivelRequerido: 'lector' },
    { path: 'directorio', permiso: 'directorio', nivelRequerido: 'lector' },
    { path: 'obsequios', permiso: 'obsequios', nivelRequerido: 'lector' },
    { path: 'eventos', permiso: 'eventos', nivelRequerido: 'lector' },
    { path: 'reportes', permiso: 'reportes', nivelRequerido: 'lector' }
  ];

  for (const ruta of rutasPermitidas) {
    const nivelUsuario = permisos[ruta.permiso]?.toLowerCase();

    if (
      nivelUsuario &&
      hasPermission(nivelUsuario, ruta.nivelRequerido) &&
      ruta.path !== rutaActual.replace('/', '')
    ) {
      return ruta.path;
    }
  }

  return null;
}
