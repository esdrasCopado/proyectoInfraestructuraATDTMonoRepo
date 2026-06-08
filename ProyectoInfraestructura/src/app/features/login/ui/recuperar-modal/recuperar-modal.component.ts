import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize, Subject } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { LoginService } from '../../data-access/login.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-recuperar-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './recuperar-modal.component.html',
  styleUrl: './recuperar-modal.component.css',
  standalone: true,
})
export class RecuperarModalComponent {
  mostrarPopup: boolean = false;
  mensaje: string | null = null;
  titulo: string | null = null;
  correo: string = '';
  private cerrarPopupSubject = new Subject<void>();
  correoEnviado: boolean = false;

  constructor(private loginService: LoginService, private httpClient: HttpClient) {}

  abrirPopup() {
    this.mostrarPopup = true;

    this.mensaje =
      'Por favor, ingrese la dirección de correo electrónico asociada con su cuenta. Le enviaremos un correo electrónico con una contraseña temporal, para que pueda cambiar al configurar su perfil.';
    this.titulo = 'Reestablecer contraseña';
    this.correoEnviado = false;
  }

  cerrarPopup() {
    this.mostrarPopup = false;

    this.cerrarPopupSubject.next();
    this.cerrarPopupSubject.complete();
    this.correo = '';
    this.correoEnviado = false;

    this.cerrarPopupSubject = new Subject<void>();
  }

  onCerrarPopup() {
    return this.cerrarPopupSubject.asObservable();
  }

  enviarCorreo() {
    this.loginService.enviarCorreo(this.correo).pipe(
      finalize(() => {
        this.correoEnviado = true;
        this.mensaje = 'Correo enviado, revise su bandeja';
        this.titulo = '¡Éxito!';
        const popup = document.querySelector('.popup');
        if (popup) {
          popup.classList.add('small');
        }
      })
    ).subscribe({
      error: () => {}
    });
  }
}
