import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';

import { CartaService } from './carta.service';
import { environment } from '../../../../environments/environment';

const API_CARTAS    = `${environment.apiUrl}/cartas`;
const API_SOLICITUD = `${environment.apiUrl}/solicitud`;

// ---------------------------------------------------------------------------
// Payload mínimo válido para registrarCarta
// ---------------------------------------------------------------------------
const payloadMock = {
  areaRequirente: {
    sector: 'Tecnología', dependencia: 'DGIT', responsable: 'Ana López',
    cargo: 'Directora', telefono: '(333) 123-4567', correo: 'ana@ejemplo.gob.mx',
  },
  adminServidor: {
    proveedor: 'Proveedor SA', dependencia: 'DGIT', responsable: 'Carlos Ruiz',
    cargo: 'Admin', telefono: '(333) 765-4321', correo: 'carlos@ejemplo.gob.mx',
  },
  descripcion: {
    descripcionServidor: 'Servidor de prueba', nombreServidor: 'SRV-TEST-01',
    nombreAplicacion: 'App Test', tipoUso: 'interno',
    fechaArranque: '2025-06-01', vigencia: '12 meses',
    caracteristicasEspeciales: '',
  },
  specs: {
    tipoRequerimiento: 'estandar', arquitectura: 'virtual',
    modalidad: 'nuevo', sistemaOperativo: 'linux',
    sistemaOperativoOtro: '', vCores: 2, memoriaRam: 4, almacenamiento: 50,
    motorBD: '', puertos: '', integraciones: '', otrasSpecs: '',
  },
  infraestructura: {
    subdominioSolicitado: '', puerto: '', requiereSSL: false,
    vpnResponsable: '', vpnCargo: '', vpnTelefono: '', vpnCorreo: '',
  },
  responsiva: {
    firmante: 'Juan Pérez', numEmpleado: 'EMP-001',
    puestoFirmante: 'Jefe de Área', aceptaTerminos: true,
  },
};

// ===========================================================================
describe('CartaService', () => {
  let service: CartaService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CartaService, provideHttpClient(), provideHttpClientTesting()],
    });
    service  = TestBed.inject(CartaService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  // ── Creación ───────────────────────────────────────────────────────────────
  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ===========================================================================
  describe('registrarCarta()', () => {

    it('debe hacer POST a la URL correcta (/api/cartas)', () => {
      service.registrarCarta(payloadMock).subscribe();

      const req = httpMock.expectOne(API_CARTAS);
      expect(req.request.method).toBe('POST');
      req.flush({ folio: 'FOLIO-001' });
    });

    it('debe enviar el payload completo en el body', () => {
      service.registrarCarta(payloadMock).subscribe();

      const req = httpMock.expectOne(API_CARTAS);
      expect(req.request.body).toEqual(payloadMock);
      req.flush({ folio: 'FOLIO-001' });
    });

    it('debe retornar el folio devuelto por el backend', () => {
      let result: { folio: string } | undefined;

      service.registrarCarta(payloadMock).subscribe(res => (result = res));

      httpMock.expectOne(API_CARTAS).flush({ folio: 'FOLIO-2025-042' });

      expect(result?.folio).toBe('FOLIO-2025-042');
    });

    it('debe propagar errores HTTP (422 Unprocessable Entity)', () => {
      let capturedError: any;

      service.registrarCarta(payloadMock).subscribe({
        error: err => (capturedError = err),
      });

      httpMock.expectOne(API_CARTAS).flush(
        { message: 'Datos inválidos' },
        { status: 422, statusText: 'Unprocessable Entity' },
      );

      expect(capturedError.status).toBe(422);
    });

    it('debe propagar errores HTTP (500 Internal Server Error)', () => {
      let capturedError: any;

      service.registrarCarta(payloadMock).subscribe({
        error: err => (capturedError = err),
      });

      httpMock.expectOne(API_CARTAS).flush(
        { message: 'Error interno' },
        { status: 500, statusText: 'Internal Server Error' },
      );

      expect(capturedError.status).toBe(500);
    });

    it('NO debe generar una segunda petición a /api/solicitud', () => {
      service.registrarCarta(payloadMock).subscribe();

      httpMock.expectOne(API_CARTAS).flush({ folio: 'FOLIO-001' });

      // Verificar que no hay ninguna petición adicional
      httpMock.expectNone(API_SOLICITUD);
    });
  });

  // ===========================================================================
  describe('registrarSolicitud()', () => {

    it('debe hacer POST a la URL correcta (/api/solicitud)', () => {
      const solicitud = { cartaId: 1 };
      service.registrarSolicitud(solicitud).subscribe();

      const req = httpMock.expectOne(API_SOLICITUD);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(solicitud);
      req.flush({});
    });
  });

  // ===========================================================================
  describe('obtenerCarta()', () => {

    it('debe hacer GET a /api/cartas/:id', () => {
      service.obtenerCarta('99').subscribe();

      const req = httpMock.expectOne(`${API_CARTAS}/99`);
      expect(req.request.method).toBe('GET');
      req.flush({});
    });
  });
});
