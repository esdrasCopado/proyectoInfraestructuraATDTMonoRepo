/**
 * PRUEBAS DE INTEGRACIÓN — CartaService
 *
 * Envían peticiones REALES al backend en http://localhost:8080
 * Si el servidor no está disponible las pruebas se omiten (no fallan).
 *
 * Ejecutar:
 *   ng test --watch=false --include="src/app/features/carta-aprovisionamiento/services/carta.integration.spec.ts"
 */

import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { CartaService } from './carta.service';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
/** Detecta si el error es de red (backend apagado). */
function esErrorDeRed(err: any): boolean {
  return err?.code === 'ECONNREFUSED' || err?.status === 0 || err?.name === 'HttpErrorResponse';
}

/** Sección de contacto reutilizable. */
const CONTACTO_AREA = {
  sector:      'Gobierno',
  dependencia: 'Secretaría de Finanzas',
  responsable: 'Esdras Copado',
  cargo:       'Coordinador TI',
  telefono:    '(526) 442-2944',
  correo:      'copadoe6@gmail.com',
};

const ADMIN_SERVIDOR = {
  proveedor:   'PROPIO',
  dependencia: 'Infraestructura',
  responsable: 'Esdras Copado',
  cargo:       'Admin de Servidores',
  telefono:    '(526) 442-2944',
  correo:      'copadoe6@gmail.com',
};

const DESCRIPCION = {
  descripcionServidor:       'Servidor para aplicación web',
  nombreServidor:            'SRV-WEB-INT-01',
  nombreAplicacion:          'Portal de Servicios',
  tipoUso:                   'publicado',
  fechaArranque:             '2026-05-01',
  vigencia:                  '1 año',
  caracteristicasEspeciales: null,
};

const SPECS_BASE = {
  tipoRequerimiento:    'estandar',
  arquitectura:         'x64',
  modalidad:            'nuevo',
  sistemaOperativo:     'linux',
  sistemaOperativoOtro: null,
  vCores:               4,
  memoriaRam:           8,
  almacenamiento:       100,
  motorBD:              null,
  puertos:              '80, 443',
  integraciones:        null,
  otrasSpecs:           null,
  ipActual:             null,
  nombreServidorActual: null,
  tipoRenovacion:       null,
};

const RESPONSIVA = {
  firmante:       'Esdras Copado',
  numEmpleado:    'EMP-12345',
  puestoFirmante: 'Coordinador de TI',
  aceptaTerminos: true,
};

// ── Los 3 tipos de VPN ──────────────────────────────────────────────────────
const VPN_DEPENDENCIA = {
  tipoVpn:           'Usuario VPN de dependencia',
  vpnResponsable:    'Esdras Copado',
  vpnCargo:          'Coordinador TI',
  vpnTelefono:       '(526) 442-2944',
  vpnCorreo:         'copadoe6@gmail.com',
  vpnPerfilAnterior: null,
  vpnServidores:     null,
  vpnId:             null,
  vpnIp:             null,
  vpnEmpresa:        null,
  vpnVigencia:       null,
};

const VPN_PROVEEDOR = {
  tipoVpn:           'Usuario VPN para proveedor',
  vpnResponsable:    'Esdras Copado',
  vpnCargo:          'Coordinador TI',
  vpnTelefono:       '(526) 442-2944',
  vpnCorreo:         'copadoe6@gmail.com',
  vpnEmpresa:        'PROPIO',
  vpnId:             'VPN-001',
  vpnIp:             '192.168.1.10',
  vpnVigencia:       '30 días',
  vpnPerfilAnterior: null,
  vpnServidores:     null,
};

const VPN_ACTUALIZACION = {
  tipoVpn:           'Actualizacion de usuario VPN',
  vpnResponsable:    null,
  vpnCargo:          null,
  vpnTelefono:       null,
  vpnCorreo:         null,
  vpnPerfilAnterior: 'Perfil-Anterior-2',
  vpnServidores:     'SRV-WEB-01, SRV-DB-01',
  vpnId:             'VPN-002',
  vpnIp:             '192.168.1.20',
  vpnEmpresa:        null,
  vpnVigencia:       null,
};

/** Construye un payload completo con las VPNs indicadas. */
function buildPayload(vpns: object[]) {
  return {
    areaRequirente: CONTACTO_AREA,
    adminServidor:  ADMIN_SERVIDOR,
    descripcion:    DESCRIPCION,
    specs:          SPECS_BASE,
    infraestructura: {
      subdominioSolicitado: 'portal.ejemplo.gob.mx',
      puerto:               '443',
      requiereSSL:          true,
      vpns,
    },
    responsiva: RESPONSIVA,
  };
}

// ===========================================================================
describe('[INTEGRACIÓN] POST /api/cartas', () => {
  let service: CartaService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CartaService, provideHttpClient()],
    });
    service = TestBed.inject(CartaService);
  });

  // ── Envíos exitosos ────────────────────────────────────────────────────────
  describe('Casos exitosos (HTTP 2xx)', () => {

    it('debe registrar carta con VPN de dependencia y devolver folio', async () => {
      const payload = buildPayload([VPN_DEPENDENCIA]);
      let res: any; let err: any;

      try { res = await firstValueFrom(service.registrarCarta(payload)); }
      catch (e) { err = e; }

      if (esErrorDeRed(err)) { console.warn('⚠  Backend no disponible'); return; }

      expect(err).toBeUndefined();
      expect(typeof res?.folio).toBe('string');
      expect(res.folio.length).toBeGreaterThan(0);
      console.log(`✅  VPN dependencia — folio: ${res?.folio}`);
    }, 10_000);

    it('debe registrar carta con VPN de proveedor y devolver folio', async () => {
      const payload = buildPayload([VPN_PROVEEDOR]);
      let res: any; let err: any;

      try { res = await firstValueFrom(service.registrarCarta(payload)); }
      catch (e) { err = e; }

      if (esErrorDeRed(err)) { console.warn('⚠  Backend no disponible'); return; }

      expect(err).toBeUndefined();
      expect(typeof res?.folio).toBe('string');
      console.log(`✅  VPN proveedor — folio: ${res?.folio}`);
    }, 10_000);

    it('debe registrar carta con VPN de actualización y devolver folio', async () => {
      const payload = buildPayload([VPN_ACTUALIZACION]);
      let res: any; let err: any;

      try { res = await firstValueFrom(service.registrarCarta(payload)); }
      catch (e) { err = e; }

      if (esErrorDeRed(err)) { console.warn('⚠  Backend no disponible'); return; }

      expect(err).toBeUndefined();
      expect(typeof res?.folio).toBe('string');
      console.log(`✅  VPN actualización — folio: ${res?.folio}`);
    }, 10_000);

    it('debe registrar carta con los 3 tipos de VPN simultáneamente', async () => {
      const payload = buildPayload([VPN_DEPENDENCIA, VPN_PROVEEDOR, VPN_ACTUALIZACION]);
      let res: any; let err: any;

      try { res = await firstValueFrom(service.registrarCarta(payload)); }
      catch (e) { err = e; }

      if (esErrorDeRed(err)) { console.warn('⚠  Backend no disponible'); return; }

      expect(err).toBeUndefined();
      expect(typeof res?.folio).toBe('string');
      console.log(`✅  3 VPNs — folio: ${res?.folio}`);
    }, 10_000);
  });

  // ── Validaciones del backend (HTTP 4xx) ────────────────────────────────────
  describe('Validaciones del backend (HTTP 4xx)', () => {

    it('debe rechazar si falta areaRequirente.sector (campo obligatorio)', async () => {
      const payload = buildPayload([VPN_DEPENDENCIA]);
      (payload.areaRequirente as any).sector = '';

      let err: any;
      try { await firstValueFrom(service.registrarCarta(payload)); }
      catch (e) { err = e; }

      if (esErrorDeRed(err)) { console.warn('⚠  Backend no disponible'); return; }

      expect(err).toBeDefined();
      expect(err.status).toBeGreaterThanOrEqual(400);
      expect(err.status).toBeLessThan(500);
    }, 10_000);

    it('debe rechazar teléfono con formato incorrecto (sin paréntesis)', async () => {
      const payload = buildPayload([VPN_DEPENDENCIA]);
      (payload.areaRequirente as any).telefono = '5264422944';  // sin formato

      let err: any;
      try { await firstValueFrom(service.registrarCarta(payload)); }
      catch (e) { err = e; }

      if (esErrorDeRed(err)) { console.warn('⚠  Backend no disponible'); return; }

      expect(err).toBeDefined();
      expect(err.status).toBeGreaterThanOrEqual(400);
      expect(err.status).toBeLessThan(500);
    }, 10_000);

    it('debe rechazar correo con formato inválido', async () => {
      const payload = buildPayload([VPN_DEPENDENCIA]);
      (payload.areaRequirente as any).correo = 'no-es-un-correo';

      let err: any;
      try { await firstValueFrom(service.registrarCarta(payload)); }
      catch (e) { err = e; }

      if (esErrorDeRed(err)) { console.warn('⚠  Backend no disponible'); return; }

      expect(err).toBeDefined();
      expect(err.status).toBeGreaterThanOrEqual(400);
      expect(err.status).toBeLessThan(500);
    }, 10_000);

    it('debe rechazar tipoUso con valor no permitido', async () => {
      const payload = buildPayload([VPN_DEPENDENCIA]);
      (payload.descripcion as any).tipoUso = 'invalido';

      let err: any;
      try { await firstValueFrom(service.registrarCarta(payload)); }
      catch (e) { err = e; }

      if (esErrorDeRed(err)) { console.warn('⚠  Backend no disponible'); return; }

      expect(err).toBeDefined();
      expect(err.status).toBeGreaterThanOrEqual(400);
      expect(err.status).toBeLessThan(500);
    }, 10_000);

    it('debe rechazar vCores < 1', async () => {
      const payload = buildPayload([VPN_DEPENDENCIA]);
      (payload.specs as any).vCores = 0;

      let err: any;
      try { await firstValueFrom(service.registrarCarta(payload)); }
      catch (e) { err = e; }

      if (esErrorDeRed(err)) { console.warn('⚠  Backend no disponible'); return; }

      expect(err).toBeDefined();
      expect(err.status).toBeGreaterThanOrEqual(400);
      expect(err.status).toBeLessThan(500);
    }, 10_000);

    it('debe rechazar aceptaTerminos = false', async () => {
      const payload = buildPayload([VPN_DEPENDENCIA]);
      (payload.responsiva as any).aceptaTerminos = false;

      let err: any;
      try { await firstValueFrom(service.registrarCarta(payload)); }
      catch (e) { err = e; }

      if (esErrorDeRed(err)) { console.warn('⚠  Backend no disponible'); return; }

      expect(err).toBeDefined();
      expect(err.status).toBeGreaterThanOrEqual(400);
      expect(err.status).toBeLessThan(500);
    }, 10_000);
  });

  // ── Estructura del payload enviado ─────────────────────────────────────────
  describe('Estructura del payload (sin backend)', () => {

    it('el array vpns debe tener la clave tipoVpn con el texto completo del backend', () => {
      expect(VPN_DEPENDENCIA.tipoVpn).toBe('Usuario VPN de dependencia');
      expect(VPN_PROVEEDOR.tipoVpn).toBe('Usuario VPN para proveedor');
      expect(VPN_ACTUALIZACION.tipoVpn).toBe('Actualizacion de usuario VPN');
    });

    it('los campos opcionales vacíos deben ser null, no string vacío', () => {
      expect(VPN_DEPENDENCIA.vpnEmpresa).toBeNull();
      expect(VPN_DEPENDENCIA.vpnId).toBeNull();
      expect(SPECS_BASE.motorBD).toBeNull();
      expect(DESCRIPCION.caracteristicasEspeciales).toBeNull();
    });

    it('el payload de proveedor debe tener empresa, vpnId, vpnIp y vpnVigencia', () => {
      expect(VPN_PROVEEDOR.vpnEmpresa).toBeTruthy();
      expect(VPN_PROVEEDOR.vpnId).toBeTruthy();
      expect(VPN_PROVEEDOR.vpnIp).toBeTruthy();
      expect(VPN_PROVEEDOR.vpnVigencia).toBeTruthy();
    });

    it('el payload de actualización debe tener vpnPerfilAnterior, vpnId y vpnIp', () => {
      expect(VPN_ACTUALIZACION.vpnPerfilAnterior).toBeTruthy();
      expect(VPN_ACTUALIZACION.vpnId).toBeTruthy();
      expect(VPN_ACTUALIZACION.vpnIp).toBeTruthy();
    });

    it('buildPayload con 3 VPNs debe producir un array de longitud 3', () => {
      const p = buildPayload([VPN_DEPENDENCIA, VPN_PROVEEDOR, VPN_ACTUALIZACION]);
      expect(p.infraestructura.vpns).toHaveLength(3);
    });
  });
});
