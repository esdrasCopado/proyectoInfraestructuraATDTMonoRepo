import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modal.component.html',
  styleUrl: './modal.component.css'
})
export class ModalComponent {
  mostrarPopup: boolean = false;
  mensaje: string | null = null;
  titulo: string | null = null;
  iconSrc: string = '';
  private cerrarPopupSubject = new Subject<void>();

  constructor() { }


  abrirPopup({ error, mensaje, titulo }: { error?: any; mensaje: string; titulo: string; }) {
    this.mostrarPopup = true;

    if (error || error === undefined) {
      this.mensaje = mensaje;
      this.titulo = "Error";
      this.iconSrc = "error.png";
    } else {
      this.mensaje = mensaje;
      this.titulo = titulo;
      this.iconSrc = "correcto.png";
    }
  }

  cerrarPopup() {
    this.mostrarPopup = false;

    this.cerrarPopupSubject.next();
    this.cerrarPopupSubject.complete();

    this.cerrarPopupSubject = new Subject<void>();
  }

  onCerrarPopup() {
    return this.cerrarPopupSubject.asObservable();
  }
}
