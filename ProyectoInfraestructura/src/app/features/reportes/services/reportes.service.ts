import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  FiltrosReporte,
  Reporte11Fila, Reporte12Fila, Reporte12Response, Reporte13Fila,
  Reporte21Fila, Reporte22Fila,
  Reporte31Fila, Reporte32Fila,
  Reporte41Fila, Reporte41Response,
  Reporte42Fila, Reporte42Response,
} from '../models/reporte.model';

const MOCK_11: Reporte11Fila[] = [
  { folioSolicitud: 'SOL-2025-001', sector: 'Trabajo',         dependencia: 'STPS / Secretaría del Trabajo', responsable: 'Juan Pérez',  contacto: 'juan.perez@sonora.gob.mx',    estatusProcesamieto: 'En proceso', fechaCreacion: '2025-01-10' },
  { folioSolicitud: 'SOL-2025-002', sector: 'Salud',           dependencia: 'ISSSTESON',                     responsable: 'María López', contacto: 'maria.lopez@isssteson.gob.mx', estatusProcesamieto: 'Completado', fechaCreacion: '2025-01-20' },
  { folioSolicitud: 'SOL-2025-003', sector: 'Desarrollo Social', dependencia: 'SEDESSON',                    responsable: 'Carlos Ruiz', contacto: 'carlos.ruiz@sedesson.gob.mx',  estatusProcesamieto: 'Pendiente',  fechaCreacion: '2025-02-05' },
];

const MOCK_SERVIDOR: Reporte12Fila[] = MOCK_11.map((r, i) => ({
  folioSolicitud: r.folioSolicitud,
  dependencia:    r.sector + ' / ' + r.dependencia,
  responsable:    r.responsable,
  contacto:       r.contacto,
  estatusProcesamieto: r.estatusProcesamieto,
  fechaCreacion:  r.fechaCreacion,
  ipServidor:             `10.0.${i}.${10 + i}`,
  administradorServidor:  `Admin Infraestructura ${i + 1}`,
  descripcionProyecto:    `Sistema de gestión institucional ${i + 1}`,
  sistemaOperativo:       i % 2 === 0 ? 'Linux Ubuntu 22.04' : 'Windows Server 2022',
  vcpu:                   4 * (i + 1),
  ram:                    8 * (i + 1),
  almacenamiento:         200 * (i + 1),
}));

const MOCK_21: Reporte21Fila[] = [
  { folioSolicitud: 'SOL-20260424-034203-748', sector: 'Trabajo', dependencia: 'STPS', responsableServidor: 'Juan Pérez',  contactoResponsable: 'juan@sonora.gob.mx',    estatusProcesamieto: 'Activo',   ipServidor: '10.0.0.10', identificadorVpn: 'VPN-001', usuarioAsignado: 'jadmin',  fechaCreacionVpn: '2026-04-24T03:42:03Z', fechaVencimientoVpn: '2027-04-24T03:42:03Z', vigencia: 'Vigente', tipoVpn: 'dependencia' },
  { folioSolicitud: 'SOL-20260424-035701-228', sector: 'Salud',   dependencia: 'ISSSTESON', responsableServidor: 'María López', contactoResponsable: 'maria@isssteson.gob.mx', estatusProcesamieto: 'Activo', ipServidor: '10.0.1.11', identificadorVpn: 'VPN-002', usuarioAsignado: 'madmin',  fechaCreacionVpn: '2026-04-24T03:57:01Z', fechaVencimientoVpn: '2027-04-24T03:57:01Z', vigencia: 'Vigente', tipoVpn: 'proveedor' },
];

const MOCK_22: Reporte22Fila[] = [
  { folioSolicitud: 'SOL-2025-001', sector: 'Trabajo', dependencia: 'STPS / Secretaría del Trabajo', responsableServidor: 'Juan Pérez',  contacto: 'juan.perez@sonora.gob.mx',    estatusProcesamieto: 'Activo', ipServidor: '10.0.0.10', subdominioAprobado: 'stps.sonora.gob.mx',          proxyAsignado: 'nginx-01', tipoDespliegue: 'publicado', puerto: '443',  fechaAsignacion: '2025-01-10' },
  { folioSolicitud: 'SOL-2025-002', sector: 'Salud',   dependencia: 'ISSSTESON',                     responsableServidor: 'María López', contacto: 'maria.lopez@isssteson.gob.mx', estatusProcesamieto: 'Activo', ipServidor: '10.0.1.11', subdominioAprobado: 'expedientes.isssteson.gob.mx', proxyAsignado: 'nginx-02', tipoDespliegue: 'interno',   puerto: '8080', fechaAsignacion: '2025-01-20' },
];

@Injectable({ providedIn: 'root' })
export class ReportesService {
  private readonly base = `${environment.apiUrl}/reporte`;

  constructor(private http: HttpClient) {}

  // ── 1.x Admin Centro de Datos ──────────────────────────────────────────────

  getReporte11(filtros: FiltrosReporte): Observable<Reporte11Fila[]> {
    if (environment.useMock) return of(MOCK_11);
    return this.http.get<Reporte11Fila[]>(`${this.base}/solicitudes/por-dependencia`, { params: this.params(filtros) });
  }

  exportarReporte11(filtros: FiltrosReporte, formato: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.get(`${this.base}/solicitudes/por-dependencia`, {
      params: this.params({ ...filtros, formato }),
      responseType: 'blob',
    });
  }

  getReporte12(filtros: FiltrosReporte): Observable<Reporte12Response> {
    if (environment.useMock) return of({
      items: MOCK_SERVIDOR,
      totalVcpu:           MOCK_SERVIDOR.reduce((s, r) => s + r.vcpu, 0),
      totalRam:            MOCK_SERVIDOR.reduce((s, r) => s + r.ram, 0),
      totalAlmacenamiento: MOCK_SERVIDOR.reduce((s, r) => s + r.almacenamiento, 0),
    });
    return this.http.get<Reporte12Response>(`${this.base}/solicitudes/recursos-solicitados`, { params: this.params(filtros) });
  }

  exportarReporte12(filtros: FiltrosReporte, formato: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.get(`${this.base}/solicitudes/recursos-solicitados`, {
      params: this.params({ ...filtros, formato }),
      responseType: 'blob',
    });
  }

  getReporte13(ip: string, filtros: FiltrosReporte): Observable<Reporte13Fila[]> {
    if (environment.useMock) {
      return of(MOCK_SERVIDOR.map(r => ({
        ...r,
        contactoResponsable:  r.contacto,
        subdominiosAprobados: [`app-${r.folioSolicitud.toLowerCase()}.sonora.gob.mx`],
        vpns:                 ['VPN-2025-001'],
      })));
    }
    return this.http.get<Reporte13Fila[]>(`${this.base}/solicitudes/por-ip`, { params: this.params({ ip, ...filtros }) });
  }

  exportarReporte13(ip: string, filtros: FiltrosReporte, formato: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.get(`${this.base}/solicitudes/por-ip`, {
      params: this.params({ ip, ...filtros, formato }),
      responseType: 'blob',
    });
  }

  // ── 2.x Admin Infraestructura ──────────────────────────────────────────────

  getReporte21(filtros: FiltrosReporte): Observable<Reporte21Fila[]> {
    if (environment.useMock) return of(MOCK_21);
    return this.http.get<Reporte21Fila[]>(`${this.base}/infraestructura/vpn`, { params: this.params(filtros) });
  }

  exportarReporte21(filtros: FiltrosReporte, formato: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.get(`${this.base}/infraestructura/vpn`, {
      params: this.params({ ...filtros, formato }),
      responseType: 'blob',
    });
  }

  getReporte22(filtros: FiltrosReporte): Observable<Reporte22Fila[]> {
    if (environment.useMock) return of(MOCK_22);
    return this.http.get<Reporte22Fila[]>(`${this.base}/infraestructura/subdominios`, { params: this.params(filtros) });
  }

  exportarReporte22(filtros: FiltrosReporte, formato: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.get(`${this.base}/infraestructura/subdominios`, {
      params: this.params({ ...filtros, formato }),
      responseType: 'blob',
    });
  }

  // ── 3.x Admin Vulnerabilidades ─────────────────────────────────────────────

  getReporte31(filtros: FiltrosReporte): Observable<Reporte31Fila[]> {
    if (environment.useMock) {
      return of([
        { folioSolicitud: 'SOL-2025-001', sector: 'Trabajo', dependencia: 'STPS / Secretaría del Trabajo', responsableServidor: 'Juan Pérez',  telefonoContacto: '6621234567', emailContacto: 'juan.perez@sonora.gob.mx',    estatusProcesamieto: 'Completado',  ipServidor: '10.0.0.10', subdominiosAprobados: ['stps.sonora.gob.mx'],          fechaSolicitudAnalisis: '2025-02-01', fechaAplicacionPrueba: '2025-02-05', resultadoPrueba: 'Sin vulnerabilidades críticas', hallazgos: 'Ninguno', ronda: 1 },
        { folioSolicitud: 'SOL-2025-002', sector: 'Salud',   dependencia: 'ISSSTESON',                     responsableServidor: 'María López', telefonoContacto: '6629876543', emailContacto: 'maria.lopez@isssteson.gob.mx', estatusProcesamieto: 'En proceso', ipServidor: '10.0.1.11', subdominiosAprobados: ['expedientes.isssteson.gob.mx'], fechaSolicitudAnalisis: '2025-03-10', fechaAplicacionPrueba: '',           resultadoPrueba: 'Pendiente',                      hallazgos: '',        ronda: 1 },
      ]);
    }
    return this.http.get<Reporte31Fila[]>(`${this.base}/seguridad/vulnerabilidades`, { params: this.params(filtros) });
  }

  exportarReporte31(filtros: FiltrosReporte, formato: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.get(`${this.base}/seguridad/vulnerabilidades`, {
      params: this.params({ ...filtros, formato }),
      responseType: 'blob',
    });
  }

  getReporte32(ip: string, filtros: FiltrosReporte): Observable<Reporte32Fila[]> {
    if (environment.useMock) {
      return of([
        { folioSolicitud: 'SOL-2025-001', sector: 'Trabajo', dependencia: 'STPS / Secretaría del Trabajo', responsableServidor: 'Juan Pérez', contactoResponsable: 'juan.perez@sonora.gob.mx', estatusProcesamieto: 'Activo', ipServidor: '10.0.0.10', subdominiosAprobados: ['stps.sonora.gob.mx'], tipoDespliegue: 'publicado', puertosSolicitados: [80, 443], reglasFirewall: 'Permitir 80/443 desde internet', integracionesExternas: 'SAP, SIIA', otras: 'Certificado SSL requerido' },
      ]);
    }
    return this.http.get<Reporte32Fila[]>(`${this.base}/seguridad/comunicaciones-por-ip`, { params: this.params({ ip, ...filtros }) });
  }

  exportarReporte32(ip: string, filtros: FiltrosReporte, formato: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.get(`${this.base}/seguridad/comunicaciones-por-ip`, {
      params: this.params({ ip, ...filtros, formato }),
      responseType: 'blob',
    });
  }

  // ── 4.x Admin General ──────────────────────────────────────────────────────

  getReporte41(filtros: FiltrosReporte): Observable<Reporte41Fila[]> {
    if (environment.useMock) {
      return of([
        { sector: 'Trabajo',          dependencia: 'STPS',      responsable: 'Juan Pérez',  emailContacto: 'juan.perez@sonora.gob.mx',    descripcionServidor: 'Portal web STPS',              ip: '10.0.0.10', fechaSolicitud: '2025-01-10', estatusProcesamieto: 'En proceso',  etapaActual: 6,  nombreEtapaActual: 'Asignación VPN',          fechaProcesamientoEtapa: '2025-01-15', rolResponsableEtapa: 'Admin Infraestructura',    fechaPublicacion: '',           tipoDespliegue: 'publicado' },
        { sector: 'Salud',            dependencia: 'ISSSTESON', responsable: 'María López', emailContacto: 'maria.lopez@isssteson.gob.mx', descripcionServidor: 'Sistema expedientes médicos',  ip: '10.0.1.11', fechaSolicitud: '2025-01-20', estatusProcesamieto: 'Completado',  etapaActual: 14, nombreEtapaActual: 'Publicado',              fechaProcesamientoEtapa: '2025-02-01', rolResponsableEtapa: 'Admin Vulnerabilidades', fechaPublicacion: '2025-02-10', tipoDespliegue: 'interno'   },
        { sector: 'Desarrollo Social', dependencia: 'SEDESSON', responsable: 'Carlos Ruiz', emailContacto: 'carlos.ruiz@sedesson.gob.mx', descripcionServidor: 'Portal de servicios sociales', ip: '10.0.2.12', fechaSolicitud: '2025-02-05', estatusProcesamieto: 'Pendiente',   etapaActual: 2,  nombreEtapaActual: 'Validación de recursos',  fechaProcesamientoEtapa: '2025-02-06', rolResponsableEtapa: 'Admin Centro de Datos',   fechaPublicacion: '',           tipoDespliegue: 'publicado' },
      ]);
    }
    return this.http.get<Reporte41Response>(`${this.base}/resumen/estatus-solicitudes`, { params: this.params(filtros) })
      .pipe(map(res => res.items));
  }

  exportarReporte41(filtros: FiltrosReporte, formato: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.get(`${this.base}/resumen/estatus-solicitudes`, {
      params: this.params({ ...filtros, formato }),
      responseType: 'blob',
    });
  }

  getReporte42(filtros: FiltrosReporte): Observable<Reporte42Response> {
    if (environment.useMock) {
      const items: Reporte42Fila[] = MOCK_SERVIDOR.map(r => ({
        ...r,
        sector:              r.dependencia.split(' / ')[0] ?? '',
        subdominiosAprobados: [`app-${r.folioSolicitud.toLowerCase()}.sonora.gob.mx`],
        vpns:                ['VPN-2025-001'],
      }));
      return of({
        items,
        totalVcpu:           items.reduce((s, r) => s + r.vcpu, 0),
        totalRam:            items.reduce((s, r) => s + r.ram, 0),
        totalAlmacenamiento: items.reduce((s, r) => s + r.almacenamiento, 0),
        totalServidores:     items.length,
      });
    }
    return this.http.get<Reporte42Response>(`${this.base}/resumen/recursos-totalizados`, { params: this.params(filtros) });
  }

  exportarReporte42(filtros: FiltrosReporte, formato: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.get(`${this.base}/resumen/recursos-totalizados`, {
      params: this.params({ ...filtros, formato }),
      responseType: 'blob',
    });
  }

  // ── Utilidades ────────────────────────────────────────────────────────────

  private params(filtros: Record<string, any>): HttpParams {
    let p = new HttpParams();
    for (const [k, v] of Object.entries(filtros)) {
      if (v !== undefined && v !== null && v !== '') p = p.set(k, String(v));
    }
    return p;
  }

  descargarBlob(blob: Blob, nombreBase: string, formato: 'pdf' | 'excel'): void {
    const ext = formato === 'pdf' ? 'pdf' : 'xlsx';
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${nombreBase}.${ext}`;
    a.click();
    URL.revokeObjectURL(url);
  }
}
