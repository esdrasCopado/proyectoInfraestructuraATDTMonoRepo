import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface UsuarioPerfil {
  id:             number;
  nombreCompleto: string;
  rol:            string;
  permisos:       string;
  correo:         string;
  puesto:         string;
  celular:        string;
  numeroPuesto:   string;
}

@Injectable({ providedIn: 'root' })
export class PerfilService {

  private url = `${environment.apiUrl}/usuario`;

  constructor(private http: HttpClient) {}

  obtenerPerfil(id: string): Observable<UsuarioPerfil> {
    return this.http.get<UsuarioPerfil>(`${this.url}/${id}`);
  }
}
