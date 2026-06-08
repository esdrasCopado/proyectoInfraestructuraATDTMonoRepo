import { TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of, throwError } from 'rxjs';

import { CartaStepperComponent } from './carta-stepper.component';
import { CartaService } from '../../services/carta.service';

// ---------------------------------------------------------------------------
// Helper: llena el formulario con valores válidos
// ---------------------------------------------------------------------------
function fillValidForm(component: CartaStepperComponent): void {
  component.form.patchValue({
    areaRequirente: {
      sector:      'Tecnología',
      dependencia: 'DGIT',
      responsable: 'Ana López',
      cargo:       'Directora',
      telefono:    '(333) 123-4567',
      correo:      'ana@ejemplo.gob.mx',
    },
    adminServidor: {
      proveedor:   'Proveedor SA',
      dependencia: 'DGIT',
      responsable: 'Carlos Ruiz',
      cargo:       'Admin',
      telefono:    '(333) 765-4321',
      correo:      'carlos@ejemplo.gob.mx',
    },
    descripcion: {
      descripcionServidor: 'Servidor de prueba',
      nombreServidor:      'SRV-TEST-01',
      fechaArranque:       '2025-06-01',
      tipoUso:             'interno',
      vigencia:            '12 meses',
      nombreAplicacion:    'App Test',
    },
    specs: {
      tipoRequerimiento: 'estandar',
      arquitectura:      'virtual',
      modalidad:         'nuevo',
      sistemaOperativo:  'linux',
      vCores:            2,
      memoriaRam:        4,
      almacenamiento:    50,
    },
    responsiva: {
      firmante:       'Juan Pérez',
      numEmpleado:    'EMP-001',
      puestoFirmante: 'Jefe de Área',
      aceptaTerminos: true,
    },
  });
}

// ===========================================================================
describe('CartaStepperComponent — envío de solicitud', () => {
  let component: CartaStepperComponent;
  let cartaServiceSpy: { registrarCarta: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    cartaServiceSpy = { registrarCarta: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [CartaStepperComponent],
      providers: [{ provide: CartaService, useValue: cartaServiceSpy }],
      schemas:   [NO_ERRORS_SCHEMA],
    })
    .overrideComponent(CartaStepperComponent, { set: { template: '' } })
    .compileComponents();

    const fixture = TestBed.createComponent(CartaStepperComponent);
    component = fixture.componentInstance;
    component.ngOnInit();
  });

  // ── Inicialización ─────────────────────────────────────────────────────────
  describe('inicialización del formulario', () => {

    it('debe crear el componente', () => {
      expect(component).toBeTruthy();
    });

    it('debe inicializar el FormGroup principal', () => {
      expect(component.form).toBeTruthy();
    });

    it('debe tener todos los apartados del formulario', () => {
      expect(component.form.contains('areaRequirente')).toBe(true);
      expect(component.form.contains('adminServidor')).toBe(true);
      expect(component.form.contains('descripcion')).toBe(true);
      expect(component.form.contains('specs')).toBe(true);
      expect(component.form.contains('infraestructura')).toBe(true);
      expect(component.form.contains('responsiva')).toBe(true);
    });

    it('debe inicializar con al menos una entrada en el FormArray de VPNs', () => {
      expect(component.vpnsArray.length).toBeGreaterThanOrEqual(1);
    });

    it('el formulario vacío debe ser inválido', () => {
      expect(component.form.invalid).toBe(true);
    });
  });

  // ── Validación antes de enviar ─────────────────────────────────────────────
  describe('onSubmit() — formulario inválido', () => {

    it('NO debe llamar a registrarCarta si el formulario es inválido', () => {
      component.onSubmit();
      expect(cartaServiceSpy.registrarCarta).not.toHaveBeenCalled();
    });

    it('no debe modificar enviando ni errorEnvio con formulario inválido', () => {
      component.onSubmit();
      expect(component.enviando).toBe(false);
      expect(component.errorEnvio).toBeNull();
    });
  });

  // ── Envío exitoso ──────────────────────────────────────────────────────────
  describe('onSubmit() — formulario válido, respuesta exitosa', () => {

    beforeEach(() => {
      cartaServiceSpy.registrarCarta.mockReturnValue(of({ folio: 'FOLIO-2025-001' }));
      fillValidForm(component);
    });

    it('el formulario relleno debe ser válido', () => {
      expect(component.form.valid).toBe(true);
    });

    it('debe llamar a registrarCarta exactamente una vez', () => {
      component.onSubmit();
      expect(cartaServiceSpy.registrarCarta).toHaveBeenCalledTimes(1);
    });

    it('debe asignar el folio devuelto por el backend', () => {
      component.onSubmit();
      expect(component.folio).toBe('FOLIO-2025-001');
    });

    it('debe resetear enviando a false al finalizar', () => {
      component.onSubmit();
      expect(component.enviando).toBe(false);
    });

    it('no debe tener errorEnvio tras una respuesta exitosa', () => {
      component.onSubmit();
      expect(component.errorEnvio).toBeNull();
    });
  });

  // ── Manejo de errores ──────────────────────────────────────────────────────
  describe('onSubmit() — formulario válido, error del backend', () => {

    beforeEach(() => {
      fillValidForm(component);
    });

    it('debe asignar errorEnvio con el mensaje del backend (campo message)', () => {
      cartaServiceSpy.registrarCarta.mockReturnValue(
        throwError(() => ({ error: { message: 'Ya existe una solicitud activa' } })),
      );
      component.onSubmit();
      expect(component.errorEnvio).toBe('Ya existe una solicitud activa');
    });

    it('debe usar mensaje genérico cuando el error no trae message', () => {
      cartaServiceSpy.registrarCarta.mockReturnValue(
        throwError(() => ({ status: 500 })),
      );
      component.onSubmit();
      expect(component.errorEnvio).toContain('error al enviar');
    });

    it('debe resetear enviando a false tras un error', () => {
      cartaServiceSpy.registrarCarta.mockReturnValue(
        throwError(() => ({ status: 500 })),
      );
      component.onSubmit();
      expect(component.enviando).toBe(false);
    });

    it('el folio debe seguir siendo null tras un error', () => {
      cartaServiceSpy.registrarCarta.mockReturnValue(
        throwError(() => ({ status: 500 })),
      );
      component.onSubmit();
      expect(component.folio).toBeNull();
    });
  });

  // ── Estructura del payload ─────────────────────────────────────────────────
  describe('onSubmit() — estructura del payload enviado', () => {

    beforeEach(() => {
      cartaServiceSpy.registrarCarta.mockReturnValue(of({ folio: 'FOLIO-001' }));
      fillValidForm(component);
      component.onSubmit();
    });

    it('debe incluir el apartado areaRequirente', () => {
      const payload = cartaServiceSpy.registrarCarta.mock.calls[0][0];
      expect(payload).toHaveProperty('areaRequirente');
      expect(payload.areaRequirente.correo).toBe('ana@ejemplo.gob.mx');
    });

    it('debe incluir el apartado adminServidor', () => {
      const payload = cartaServiceSpy.registrarCarta.mock.calls[0][0];
      expect(payload).toHaveProperty('adminServidor');
      expect(payload.adminServidor.proveedor).toBe('Proveedor SA');
    });

    it('debe incluir el apartado descripcion', () => {
      const payload = cartaServiceSpy.registrarCarta.mock.calls[0][0];
      expect(payload).toHaveProperty('descripcion');
      expect(payload.descripcion.nombreServidor).toBe('SRV-TEST-01');
    });

    it('debe incluir el campo arquitectura dentro de specs', () => {
      const payload = cartaServiceSpy.registrarCarta.mock.calls[0][0];
      expect(payload.specs).toHaveProperty('arquitectura');
      expect(payload.specs.arquitectura).toBe('virtual');
    });

    it('infraestructura debe contener un array vpns (no campos planos)', () => {
      const payload = cartaServiceSpy.registrarCarta.mock.calls[0][0];
      expect(Array.isArray(payload.infraestructura.vpns)).toBe(true);
      expect(payload.infraestructura.vpns.length).toBeGreaterThanOrEqual(1);
      // No deben existir campos planos de VPN
      expect(payload.infraestructura).not.toHaveProperty('vpnResponsable');
      expect(payload.infraestructura).not.toHaveProperty('vpnCorreo');
    });

    it('tipoVpn en el payload debe ser el texto completo del backend', () => {
      const payload = cartaServiceSpy.registrarCarta.mock.calls[0][0];
      const tipoVpn = payload.infraestructura.vpns[0].tipoVpn;
      expect(tipoVpn).toBe('Usuario VPN de dependencia');
    });

    it('los campos opcionales vacíos deben ser null, no string vacío', () => {
      const payload = cartaServiceSpy.registrarCarta.mock.calls[0][0];
      // motorBD, integraciones, otrasSpecs están vacíos en fillValidForm
      expect(payload.specs.motorBD).toBeNull();
      expect(payload.specs.integraciones).toBeNull();
      expect(payload.specs.otrasSpecs).toBeNull();
    });

    it('debe incluir el apartado responsiva con aceptaTerminos = true', () => {
      const payload = cartaServiceSpy.registrarCarta.mock.calls[0][0];
      expect(payload).toHaveProperty('responsiva');
      expect(payload.responsiva.aceptaTerminos).toBe(true);
    });
  });

  // ── Garantía: un solo endpoint ─────────────────────────────────────────────
  describe('flujo de envío — sin duplicación de registros', () => {

    it('NO debe tener método registrarSolicitud en el flujo de onSubmit', () => {
      // El componente solo debe invocar registrarCarta; el backend crea
      // la solicitud internamente. Verificamos que el spy solo expone
      // registrarCarta y que eso es lo único que se llama.
      cartaServiceSpy.registrarCarta.mockReturnValue(of({ folio: 'F-001' }));

      // Añadir spy de registrarSolicitud para detectar si se llama
      const registrarSolicitudSpy = vi.fn();
      (cartaServiceSpy as any)['registrarSolicitud'] = registrarSolicitudSpy;

      fillValidForm(component);
      component.onSubmit();

      expect(cartaServiceSpy.registrarCarta).toHaveBeenCalledTimes(1);
      expect(registrarSolicitudSpy).not.toHaveBeenCalled();
    });
  });

  // ── Gestión del FormArray de VPNs ──────────────────────────────────────────
  describe('gestión de VPNs en el formulario', () => {

    it('crearVpnGroup() debe generar un FormGroup con tipoVpn = "dependencia"', () => {
      const group = component.crearVpnGroup();
      expect(group.get('tipoVpn')?.value).toBe('dependencia');
    });

    it('vpnsArray debe exponer el FormArray de infraestructura', () => {
      expect(component.vpnsArray).toBeTruthy();
      expect(component.vpnsArray.length).toBe(1);
    });
  });
});
