import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface EvidenciaUploadResponse {
  id: number;
  solicitudId: number;
  proposito: string;
  ronda: number;
  estado: string;
}

export interface EvidenciaDto {
  id: number;
  solicitudId: number;
  proposito: string;
  ronda: number;
  archivoNombre: string;
  archivoPath: string;
  archivoSizeKb: number;
  estadoValidacion: 'pendiente' | 'aprobada' | 'rechazada';
  motivoRechazo?: string;
  uploadedAt: string;
  deletedAt?: string;
  subidoPor: {
    id: number;
    nombreCompleto: string;
    email: string;
    cargo: string;
  };
}

export interface ValidarEvidenciaPayload {
  estadoValidacion: 'aprobada' | 'rechazada';
  motivoRechazo?: string;
}

@Injectable({ providedIn: 'root' })
export class EvidenciaService {

  private base = `${environment.apiUrl}/evidencia`;
  private readonly useMock = environment.useMock;

  constructor(private http: HttpClient) {}

  subirEvidencia(solicitudId: string, archivo: File): Observable<EvidenciaUploadResponse> {
    if (this.useMock) {
      return of({ id: Math.floor(Math.random() * 1000), solicitudId: Number(solicitudId), proposito: 'pruebas_funcionamiento', ronda: 1, estado: 'pendiente' });
    }
    const fd = new FormData();
    fd.append('proposito', 'pruebas_funcionamiento');
    fd.append('archivo', archivo);
    return this.http.post<EvidenciaUploadResponse>(`${this.base}/solicitud/${solicitudId}`, fd);
  }

  getEvidenciasSolicitud(solicitudId: string | number): Observable<EvidenciaDto[]> {
    if (this.useMock) return of([]);
    return this.http.get<EvidenciaDto[]>(`${this.base}/solicitud/${solicitudId}`);
  }

  getTodasEvidencias(solicitudId: string | number): Observable<EvidenciaDto[]> {
    if (this.useMock) return of([]);
    return this.http.get<EvidenciaDto[]>(`${this.base}/solicitud/${solicitudId}/todas`);
  }

  descargarEvidencia(evidenciaId: number): Observable<Blob> {
    if (this.useMock) return of(new Blob());
    return this.http.get(`${this.base}/${evidenciaId}/descargar`, { responseType: 'blob' });
  }

  validarEvidencia(evidenciaId: number, payload: ValidarEvidenciaPayload): Observable<any> {
    if (this.useMock) return of(null);
    return this.http.patch(`${this.base}/${evidenciaId}/validar`, payload);
  }
}
