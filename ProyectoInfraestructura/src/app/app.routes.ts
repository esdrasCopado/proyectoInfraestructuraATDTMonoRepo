import { Routes } from '@angular/router';
import { authGuard } from './auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/login/login.component')
        .then(m => m.LoginComponent)
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/components/dashboard/dashboard.component')
        .then(m => m.DashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'carta-aprovisionamiento',
    loadComponent: () =>
      import('./features/carta-aprovisionamiento/components/carta-stepper/carta-stepper.component')
        .then(m => m.CartaStepperComponent),
    canActivate: [authGuard]
  },
  {
    path: 'expediente/:id',
    loadComponent: () =>
      import('./features/dashboard/components/expediente-detalle/expediente-detalle.component')
        .then(m => m.ExpedienteDetalleComponent),
    canActivate: [authGuard]
  },
  {
    path: 'cambio-password',
    loadComponent: () =>
      import('./features/cambioPassword/components/cambio-password.component')
        .then(m => m.CambioPasswordComponent),
    canActivate: [authGuard]
  },
  {
    path: 'crear-usuario',
    loadComponent: () =>
      import('./features/crearUsuario/components/nuevoUsuario.component')
        .then(m => m.NuevoUsuarioComponent),
    canActivate: [authGuard]
  },
  {
    path: 'perfil-usuario',
    loadComponent: () =>
      import('./features/perfilUsuario/components/perfilUsuario.component')
        .then(m => m.PerfilUsuarioComponent),
    canActivate: [authGuard]
  },
  {
    path: 'reportes',
    loadComponent: () =>
      import('./features/reportes/reportes-outlet.component')
        .then(m => m.ReportesOutletComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'solicitudes-dependencia', pathMatch: 'full' },
      { path: 'solicitudes-dependencia', loadComponent: () => import('./features/reportes/components/reporte-11/reporte-11.component').then(m => m.Reporte11Component) },
      { path: 'recursos-totalizados',    loadComponent: () => import('./features/reportes/components/reporte-12/reporte-12.component').then(m => m.Reporte12Component) },
      { path: 'por-ip',                  loadComponent: () => import('./features/reportes/components/reporte-13/reporte-13.component').then(m => m.Reporte13Component) },
      { path: 'vpn',                     loadComponent: () => import('./features/reportes/components/reporte-21/reporte-21.component').then(m => m.Reporte21Component) },
      { path: 'subdominios',             loadComponent: () => import('./features/reportes/components/reporte-22/reporte-22.component').then(m => m.Reporte22Component) },
      { path: 'vulnerabilidades',        loadComponent: () => import('./features/reportes/components/reporte-31/reporte-31.component').then(m => m.Reporte31Component) },
      { path: 'comunicaciones-ip',       loadComponent: () => import('./features/reportes/components/reporte-32/reporte-32.component').then(m => m.Reporte32Component) },
      { path: 'estatus-solicitudes',     loadComponent: () => import('./features/reportes/components/reporte-41/reporte-41.component').then(m => m.Reporte41Component) },
      { path: 'recursos-general',        loadComponent: () => import('./features/reportes/components/reporte-42/reporte-42.component').then(m => m.Reporte42Component) },
    ]
  },
  {
    path: 'notificaciones',
    loadComponent: () =>
      import('./features/notificaciones/components/notificaciones.component')
        .then(m => m.NotificacionesComponent),
    canActivate: [authGuard]
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  }
];
