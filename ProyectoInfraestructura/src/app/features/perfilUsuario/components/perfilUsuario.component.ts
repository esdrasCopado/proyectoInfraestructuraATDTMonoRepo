import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService, PerfilUsuario, RolUsuario } from '../../dashboard/services/auth.service';

const ETIQUETA_ROL: Record<RolUsuario, string> = {
  'dependencia':            'Dependencia / Cliente',
  'admin-cd':               'Administrador de Centro de Datos',
  'admin-infraestructura':  'Administrador de Infraestructura',
  'admin-vulnerabilidades': 'Administrador de Vulnerabilidades',
  'admin-general':          'Administrador General',
};

interface PerfilVista {
  nombreCompleto: string;
  rol:            string;
  correo:         string;
  puesto:         string;
  numeroPuesto:   string;
  celular:        string;
  permisos:       string;
}

@Component({
  selector: 'app-perfil-usuario',
  templateUrl: './perfilUsuario.component.html',
  styleUrls: ['./perfilUsuario.component.scss'],
  standalone: true,
  imports: [CommonModule],
})
export class PerfilUsuarioComponent implements OnInit {

  perfil: PerfilVista | null = null;
  cargando = true;
  error: string | null = null;

  constructor(
    private authService: AuthService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    const usuario = this.authService.obtenerUsuario();
    if (!usuario) {
      this.error    = 'No se encontró sesión activa.';
      this.cargando = false;
      return;
    }

    this.authService.getPerfil().subscribe({
      next: (data: PerfilUsuario) => {
        this.perfil = {
          nombreCompleto: usuario.nombre || data.email,
          rol:            ETIQUETA_ROL[usuario.rol] ?? usuario.rol,
          correo:         data.email,
          puesto:         data.cargo,
          numeroPuesto:   data.numeroEmpleado,
          celular:        data.phone,
          permisos:       data.rolNombre,
        };
        this.cargando = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error    = 'No se pudo cargar el perfil. Intenta de nuevo más tarde.';
        this.cargando = false;
        this.cdr.detectChanges();
      },
    });
  }
}
