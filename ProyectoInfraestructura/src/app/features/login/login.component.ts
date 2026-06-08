import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LoginService } from './data-access/login.service';
import { Usuario } from '../../shared/components/interfaces/Usuario';
import { ModalComponent } from '../../shared/ui/modal/modal.component';
import { RecuperarModalComponent } from './ui/recuperar-modal/recuperar-modal.component';

@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: `./login.component.html`,
  imports: [CommonModule, FormsModule, ModalComponent, RecuperarModalComponent],
  styleUrl: './login.component.css',
})
export class LoginComponent {
  currentRoute: string = '';
  @ViewChild(ModalComponent) popupComponent!: ModalComponent;
  @ViewChild(RecuperarModalComponent) popupRecuperarComponent!: RecuperarModalComponent;

  correo: string = '';
  password: string = '';

  constructor(private router: Router, private loginService: LoginService) {}

  ngOnInit(): void {
    this.currentRoute = this.router.url;
  }

  isRoute(route: string): boolean {
    return this.currentRoute.includes(route);
  }

  iniciarSesion() {
    if (this.correo === '' || this.password === '') {
      this.popupComponent.abrirPopup({
        error: undefined,
        mensaje: 'Llena todos los campos',
        titulo: 'Error',
      });
    // }else if(this.correo !== this.correo.toLowerCase()){
    //   this.popupComponent.abrirPopup({
    //     error: undefined,
    //     mensaje: 'Escriba el correo correctamente',
    //     titulo: 'Formato inválido',
    //   });

    } else {
      const credentials: Usuario = {
        correo: this.correo.trim().toLocaleLowerCase(),
        password: this.password,
      };

      this.loginService.iniciarSesion(credentials).subscribe({
        next: (response) => {
          this.loginService.saveToken(response.token);
          const mustChange = response.user?.mustChangePassword ?? false;
          this.loginService.saveMustChangePassword(mustChange);
          this.router.navigate([mustChange ? 'cambio-password' : 'dashboard']);
        },
        error: (error) => {
          this.popupComponent.abrirPopup({
            error,
            mensaje: 'Correo o contraseña incorrectos',
            titulo: 'Error',
          });
        },
      });
    }
  }

  recuperarContrasena() {
    this.popupRecuperarComponent.abrirPopup();
  }
}
