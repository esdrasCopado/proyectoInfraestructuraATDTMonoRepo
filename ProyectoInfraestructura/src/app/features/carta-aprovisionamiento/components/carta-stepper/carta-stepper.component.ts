import { Component, OnInit, AfterViewInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { faker } from '@faker-js/faker';
import { MatStepperModule } from '@angular/material/stepper';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CartaService } from '../../services/carta.service';
import { NotificacionService } from '../../../../features/notificaciones/services/notificacion.service';

import { StepContactoComponent } from '../steps/step-contacto/step-contacto.component';
import { StepDescripcionComponent } from '../steps/step-descripcion/step-descripcion.component';
import { StepSpecsComponent } from '../steps/step-specs/step-specs.component';
import { StepInfraestructuraComponent } from '../steps/step-infraestructura/step-infraestructura.component';
import { StepResponsivaComponent } from '../steps/step-responsiva/step-responsiva.component';
import { ConfirmarSolicitudComponent } from '../confirmar-solicitud/confirmar-solicitud.component';
import { HelpPanelComponent } from '../../../../shared/components/help-panel/help-panel.component';
import { AYUDA_PASOS } from '../../../../shared/constants/ayuda-pasos.const';

@Component({
  selector: 'app-carta-stepper',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatStepperModule,
    MatButtonModule,
    MatIconModule,
    StepContactoComponent,
    StepDescripcionComponent,
    StepSpecsComponent,
    StepInfraestructuraComponent,
    StepResponsivaComponent,
    ConfirmarSolicitudComponent,
    HelpPanelComponent,
  ],
  templateUrl: './carta-stepper.component.html',
  styleUrls: ['./carta-stepper.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CartaStepperComponent implements OnInit, AfterViewInit {

  form!: FormGroup;
  enviando   = false;
  folio:     string | null = null;
  errorEnvio: string | null = null;
  readonly ayudas = AYUDA_PASOS;

  constructor(
    private fb: FormBuilder,
    private cartaService: CartaService,
    private notifService: NotificacionService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngAfterViewInit(): void {
    this.cdr.detectChanges();
  }

  ngOnInit(): void {
    this.form = this.fb.group({

      // Apartado 1 — Contacto
      areaRequirente: this.fb.group({
        sector:       ['', Validators.required],
        dependencia:  ['', Validators.required],
        responsable:  ['', Validators.required],
        cargo:        ['', Validators.required],
        telefono:     ['', [Validators.required, Validators.pattern(/^\(\d{3}\) \d{3}-\d{4}$/)]],
        correo:       ['', [Validators.required, Validators.email]],
      }),
      adminServidor: this.fb.group({
        proveedor:    ['', Validators.required],
        dependencia:  ['', Validators.required],
        responsable:  ['', Validators.required],
        cargo:        ['', Validators.required],
        telefono:     ['', [Validators.required, Validators.pattern(/^\(\d{3}\) \d{3}-\d{4}$/)]],
        correo:       ['', [Validators.required, Validators.email]],
      }),

      // Apartado 2 — Descripción
      descripcion: this.fb.group({
        descripcionServidor:      ['', Validators.required],
        nombreServidor:           ['', Validators.required],
        fechaArranque:            ['', Validators.required],
        tipoUso:                  ['interno', Validators.required],
        vigencia:                 ['', Validators.required],
        nombreAplicacion:         ['', Validators.required],
        caracteristicasEspeciales: [''],
      }),

      // Apartado 3 — Specs técnicas
      specs: this.fb.group({
        tipoRequerimiento:    ['estandar', Validators.required],
        arquitectura:         ['virtual', Validators.required],
        modalidad:            ['nuevo', Validators.required],
        sistemaOperativo:     ['windows', Validators.required],
        sistemaOperativoOtro: [''],
        vCores:               [2, [Validators.required, Validators.min(1)]],
        memoriaRam:           [4, [Validators.required, Validators.min(1)]],
        discosDuros:          this.fb.array([this.crearDiscoDuroGroup()]),
        motorBD:              [''],
        puertos:              [''],
        integraciones:        [''],
        otrasSpecs:           [''],
        // Campos de renovación
        ipActual:             [''],
        nombreServidorActual: [''],
        tipoRenovacion:       [''],
      }),

      // Apartado 4 — Infraestructura
      infraestructura: this.fb.group({
        subdominios: this.fb.array([]),
        requiereSSL: [true],
        // Cada entrada VPN es un FormGroup dentro del array
        vpns: this.fb.array([this.crearVpnGroup()]),
      }),

      // Apartado 5 — Responsiva
      responsiva: this.fb.group({
        firmante:       ['', Validators.required],
        numEmpleado:    ['', Validators.required],
        puestoFirmante: ['', Validators.required],
        aceptaTerminos: [false, Validators.requiredTrue],
      }),

    });
  }

  get stepContacto()       { return this.form.get('areaRequirente') as FormGroup; }
  get stepDescripcion()    { return this.form.get('descripcion') as FormGroup; }
  get stepSpecs()          { return this.form.get('specs') as FormGroup; }
  get stepInfra()          { return this.form.get('infraestructura') as FormGroup; }
  get stepResponsiva()     { return this.form.get('responsiva') as FormGroup; }
  get vpnsArray()          { return this.stepInfra.get('vpns') as FormArray; }
  get discosDurosArray()   { return this.stepSpecs.get('discosDuros') as FormArray; }
  get subdominiosArray()   { return this.stepInfra.get('subdominios') as FormArray; }

  crearSubdominioGroup(): FormGroup {
    return this.fb.group({
      subdominio: [''],
      puerto:     [''],
    });
  }

  crearDiscoDuroGroup(): FormGroup {
    return this.fb.group({
      capacidad: [50, [Validators.required, Validators.min(1)]],
      tipo:      ['SSD', Validators.required],
      etiqueta:  [''],
    });
  }

  crearVpnGroup(): FormGroup {
    return this.fb.group({
      tipoVpn:           ['dependencia', Validators.required],
      vpnResponsable:    ['', Validators.required],
      vpnCargo:          [''],
      vpnTelefono:       ['', Validators.pattern(/^\(\d{3}\) \d{3}-\d{4}$/)],
      vpnCorreo:         ['', Validators.email],
      vpnPerfilAnterior: [''],
      vpnServidores:     [''],
      vpnFolio:          [''],
      vpnIp:             [''],
      vpnEmpresa:        [''],
      vpnVigencia:       [''],
    });
  }

  llenarDemo(): void {
    const phone = () =>
      `(${faker.string.numeric(3)}) ${faker.string.numeric(3)}-${faker.string.numeric(4)}`;

    const sectores     = ['Educación', 'Salud', 'Hacienda', 'Seguridad Pública', 'Desarrollo Social'];
    const dependencias = ['DGIT', 'SEP', 'IMSS', 'SAT', 'SEFIN', 'SSP'];
    const fechaFutura  = new Date(Date.now() + 90 * 24 * 60 * 60 * 1000)
      .toISOString().split('T')[0];

    this.form.patchValue({
      areaRequirente: {
        sector:      faker.helpers.arrayElement(sectores),
        dependencia: faker.helpers.arrayElement(dependencias),
        responsable: faker.person.fullName(),
        cargo:       faker.person.jobTitle(),
        telefono:    phone(),
        correo:      faker.internet.email(),
      },
      adminServidor: {
        proveedor:   faker.company.name(),
        dependencia: faker.helpers.arrayElement(dependencias),
        responsable: faker.person.fullName(),
        cargo:       faker.person.jobTitle(),
        telefono:    phone(),
        correo:      faker.internet.email(),
      },
      descripcion: {
        descripcionServidor:       faker.lorem.sentence(),
        nombreServidor:            `SRV-${faker.string.alphanumeric(6).toUpperCase()}`,
        nombreAplicacion:          faker.commerce.productName(),
        tipoUso:                   faker.helpers.arrayElement(['interno', 'publicado']),
        fechaArranque:             fechaFutura,
        vigencia:                  faker.helpers.arrayElement(['6 meses', '1 año', '2 años']),
        caracteristicasEspeciales: faker.lorem.sentence(),
      },
      specs: {
        tipoRequerimiento: 'especifico',
        arquitectura:      faker.helpers.arrayElement(['virtual', 'fisica', 'hibrida', 'nube']),
        modalidad:         'nuevo',
        sistemaOperativo:  faker.helpers.arrayElement(['windows', 'linux']),
        vCores:            faker.helpers.arrayElement([2, 4, 8, 16]),
        memoriaRam:        faker.helpers.arrayElement([4, 8, 16, 32]),
        motorBD:           faker.helpers.arrayElement(['MySQL', 'PostgreSQL', 'SQL Server']),
        puertos:           '80, 443, 8080',
        integraciones:     faker.lorem.sentence(),
        otrasSpecs:        faker.lorem.sentence(),
      },
      infraestructura: {
        requiereSSL: true,
      },
      responsiva: {
        firmante:       faker.person.fullName(),
        numEmpleado:    faker.string.numeric(6),
        puestoFirmante: faker.person.jobTitle(),
        aceptaTerminos: true,
      },
    });

    // Subdominios: limpiar y añadir demo
    while (this.subdominiosArray.length > 0) this.subdominiosArray.removeAt(0);
    this.subdominiosArray.push(this.fb.group({
      subdominio: [`app${faker.string.numeric(2)}.sonora.gob.mx`],
      puerto:     [faker.helpers.arrayElement(['80', '443', '8080'])],
    }));

    // Primer disco duro
    (this.discosDurosArray.at(0) as FormGroup).patchValue({
      capacidad: faker.helpers.arrayElement([50, 100, 200, 500]),
      tipo:      faker.helpers.arrayElement(['SSD', 'HDD', 'NVMe']),
      etiqueta:  'Sistema operativo',
    });

    // Primera entrada VPN
    (this.vpnsArray.at(0) as FormGroup).patchValue({
      tipoVpn:        'dependencia',
      vpnResponsable: faker.person.fullName(),
      vpnCargo:       faker.person.jobTitle(),
      vpnTelefono:    phone(),
      vpnCorreo:      faker.internet.email(),
    });

    this.cdr.markForCheck();
  }

  // Convierte cadenas vacías a null para los campos opcionales del backend
  private orNull(val: any): any {
    return val === '' || val === null || val === undefined ? null : val;
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.enviando   = true;
    this.errorEnvio = null;

    const v          = this.form.value;
    const today      = new Date().toISOString().split('T')[0];
    const idUsuario  = this.cartaService.obtenerIdUsuario();

    const VPN_TIPO: Record<string, string> = {
      dependencia:   'Usuario VPN de dependencia',
      proveedor:     'Usuario VPN para proveedor',
      actualizacion: 'Actualizacion de usuario VPN',
    };

    const vpnFechaExp = (vigencia: string | null): string | null => {
      if (!vigencia) return null;
      const d = new Date();
      d.setDate(d.getDate() + parseInt(vigencia, 10));
      return d.toISOString().split('T')[0];
    };

    const sistemaOperativo = v.specs.sistemaOperativo === 'otro'
      ? (v.specs.sistemaOperativoOtro || 'otro')
      : v.specs.sistemaOperativo;

    const discosDuros = (v.specs.discosDuros as any[]).map((d: any) => ({
      capacidad: d.capacidad ?? 0,
      tipo:      d.tipo,
      etiqueta:  d.etiqueta || null,
    }));

    const payload = {
      // — Meta —
      idUsuario,
      usuarioUltimaActualizacion: v.areaRequirente.responsable,
      fechaActualizacion:         today,

      // — I. Área requirente —
      sector:              v.areaRequirente.sector,
      dependencia:         v.areaRequirente.dependencia,
      responsableActual:   v.areaRequirente.responsable,
      cargoResponsable:    v.areaRequirente.cargo,
      telefonoResponsable: v.areaRequirente.telefono,
      correoResponsable:   v.areaRequirente.correo,

      // — II. Administrador del servidor —
      proveedor:        v.adminServidor.proveedor,
      dependenciaAdmin: v.adminServidor.dependencia,
      cargoAdmin:       v.adminServidor.cargo,
      telefonoAdmin:    v.adminServidor.telefono,
      correoAdmin:      v.adminServidor.correo,

      // — III. Descripción (nivel solicitud) —
      titulo:                 v.descripcion.nombreAplicacion,
      descripcion:            v.descripcion.descripcionServidor,
      fechaRequerida:         v.descripcion.fechaArranque,
      comentariosSeguimiento: this.orNull(v.descripcion.caracteristicasEspeciales),

      servidores: [{
        idSolicitud: null,

        // — III. Descripción del servidor —
        hostname:    v.descripcion.nombreServidor,
        tipoUso:     v.descripcion.tipoUso,
        vigencia:    v.descripcion.vigencia,
        funcion:     v.descripcion.nombreAplicacion,
        descripcion: v.descripcion.descripcionServidor,

        // — IV. Especificaciones técnicas —
        plantillaRecursos:          v.specs.tipoRequerimiento,
        modalidad:                  v.specs.modalidad,
        sistemaOperativo,
        requiereLlaveLicencia:      false,
        nucleos:                    v.specs.vCores,
        ram:                        v.specs.memoriaRam,
        discosDuros,
        motorBD:                    this.orNull(v.specs.motorBD),
        puertos:                    this.orNull(v.specs.puertos),
        integraciones:              this.orNull(v.specs.integraciones),
        otrasSpecs:                 this.orNull(v.specs.otrasSpecs),
        responsableInfraestructura: v.adminServidor.responsable,
        solicitudPublicacion:       v.descripcion.tipoUso === 'publicado',

        // — V. Infraestructura —
        subdominios: (v.infraestructura.subdominios as { subdominio: string; puerto: string }[])
          .filter(e => e.subdominio.trim())
          .map(e => ({
            idUsuario:       idUsuario,
            nombreUrl:       e.subdominio.trim(),
            puerto:          this.orNull(e.puerto),
            fechaAsignacion: today,
            fechaExpiracion: null,
            estado:          'Solicitado',
          })),
        requiereSSL: true,
        vpNs: (v.infraestructura.vpns as any[]).map((vpn: any) => ({
          tipo:           VPN_TIPO[vpn.tipoVpn] ?? vpn.tipoVpn,
          responsable:    this.orNull(vpn.vpnResponsable),
          cargo:          this.orNull(vpn.vpnCargo),
          telefono:       this.orNull(vpn.vpnTelefono),
          correo:         this.orNull(vpn.vpnCorreo),
          perfilAnterior: this.orNull(vpn.vpnPerfilAnterior),
          servidores:     this.orNull(vpn.vpnServidores),
          folio:          this.orNull(vpn.vpnFolio),
          ip:             this.orNull(vpn.vpnIp),
          empresa:        this.orNull(vpn.vpnEmpresa),
          vigencia:       this.orNull(vpn.vpnVigencia),
          fechaAsignacion: today,
          fechaExpiracion: vpnFechaExp(vpn.vpnVigencia),
          estado:          'Pendiente',
        })),
        waFs:              [],
        evidenciasPruebas: [],
      }],

      // — VI. Responsiva —
      firmante:       v.responsiva.firmante,
      numEmpleado:    v.responsiva.numEmpleado,
      puestoFirmante: v.responsiva.puestoFirmante,
      aceptaTerminos: v.responsiva.aceptaTerminos,
    };

    this.cartaService.crearSolicitudCompleta(payload).subscribe({
      next: (res) => {
        this.enviando = false;
        this.folio    = res.folio;

        const solicitudId  = res.solicitudId ?? res.id ?? 0;
        const senderUserId = this.cartaService.obtenerIdUsuario() ?? 0;
        this.notifService.notificarRol(
          'admin-cd', 'solicitud_nueva',
          'Nueva solicitud de aprovisionamiento',
          `Se registró la solicitud ${res.folio}. Requiere validación de recursos.`,
          solicitudId,
          senderUserId,
        ).subscribe();

        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.enviando   = false;
        this.errorEnvio = err?.error?.message ?? 'Ocurrió un error al enviar la solicitud. Intenta de nuevo.';
        this.cdr.markForCheck();
      },
    });
  }
}
