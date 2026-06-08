import { Component, Input, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { catchError, debounceTime, distinctUntilChanged, of, Subscription, switchMap, tap } from 'rxjs';
import { PhoneMaskDirective } from '../../../../../shared/directives/phone-mask.directive';
import { CartaService } from '../../../services/carta.service';

export interface VpnDisponible {
  folio:   string;
  tipo:    string;
  ip?:     string;
}

const PHONE = Validators.pattern(/^\(\d{3}\) \d{3}-\d{4}$/);
const VPN_FIELDS = ['vpnResponsable','vpnCargo','vpnTelefono','vpnCorreo',
                    'vpnPerfilAnterior','vpnServidores','vpnFolio','vpnIp',
                    'vpnEmpresa','vpnVigencia'];

@Component({
  selector: 'app-step-infraestructura',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonToggleModule,
    MatRadioModule,
    MatButtonModule,
    MatIconModule,
    MatAutocompleteModule,
    PhoneMaskDirective,
  ],
  templateUrl: './step-infraestructura.component.html',
  styleUrl: './step-infraestructura.component.scss',
})
export class StepInfraestructuraComponent implements OnInit, OnDestroy {
  @Input({ required: true }) form!: FormGroup;

  folioSearchCtrls: FormControl<string>[] = [];
  foliosFiltrados: VpnDisponible[][] = [];
  folioSinResultados: boolean[] = [];
  folioLoading: boolean[] = [];
  folioError: boolean[] = [];

  private subs = new Subscription();

  constructor(
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private cartaService: CartaService,
  ) {}

  get subdominiosArray(): FormArray {
    return this.form.get('subdominios') as FormArray;
  }

  subdominioAt(i: number): FormGroup {
    return this.subdominiosArray.at(i) as FormGroup;
  }

  agregarSubdominio(): void {
    this.subdominiosArray.push(this.fb.group({ subdominio: [''], puerto: [''] }));
    this.cdr.markForCheck();
  }

  eliminarSubdominio(i: number): void {
    this.subdominiosArray.removeAt(i);
    this.cdr.markForCheck();
  }

  get vpnsArray(): FormArray {
    return this.form.get('vpns') as FormArray;
  }

  vpnAt(i: number): FormGroup {
    return this.vpnsArray.at(i) as FormGroup;
  }

  tipoVpnAt(i: number): string {
    return this.vpnsArray.at(i).get('tipoVpn')?.value ?? 'dependencia';
  }

  ngOnInit(): void {
    Promise.resolve().then(() => {
      for (let i = 0; i < this.vpnsArray.length; i++) {
        const vpnGroup = this.vpnAt(i);
        this.aplicarValidadoresVpn(vpnGroup);
        this.suscribirTipoVpn(vpnGroup);
        this.registrarBusquedaFolio(i);
      }
      this.cdr.markForCheck();
    });
  }

  ngOnDestroy(): void { this.subs.unsubscribe(); }

  agregarVpn(): void {
    const vpnGroup = this.crearVpnGroup();
    this.vpnsArray.push(vpnGroup);
    Promise.resolve().then(() => {
      const i = this.vpnsArray.length - 1;
      this.aplicarValidadoresVpn(vpnGroup);
      this.suscribirTipoVpn(vpnGroup);
      this.registrarBusquedaFolio(i);
      this.cdr.markForCheck();
    });
  }

  eliminarVpn(i: number): void {
    if (this.vpnsArray.length > 1) {
      this.vpnsArray.removeAt(i);
      this.cdr.markForCheck();
    }
  }

  crearVpnGroup(): FormGroup {
    return this.fb.group({
      tipoVpn:           ['dependencia', Validators.required],
      vpnResponsable:    [''],
      vpnCargo:          [''],
      vpnTelefono:       [''],
      vpnCorreo:         [''],
      vpnPerfilAnterior: [''],
      vpnServidores:     [''],
      vpnFolio:          [''],
      vpnIp:             [''],
      vpnEmpresa:        [''],
      vpnVigencia:       [''],
    });
  }

  private registrarBusquedaFolio(i: number): void {
    const ctrl = new FormControl<string>('', { nonNullable: true });
    this.folioSearchCtrls[i]   = ctrl;
    this.foliosFiltrados[i]    = [];
    this.folioSinResultados[i] = false;
    this.folioLoading[i]       = false;
    this.folioError[i]         = false;

    this.subs.add(
      ctrl.valueChanges.pipe(
        debounceTime(300),
        distinctUntilChanged(),
        // When an autocomplete option is selected, Angular Material briefly
        // writes the VpnDisponible object into the control before (optionSelected)
        // resets it to ''. Skip those non-string emissions.
        switchMap(q => {
          if (typeof q !== 'string') return of(null);
          this.folioLoading[i] = true;
          this.folioError[i]   = false;
          this.cdr.markForCheck();
          return this.cartaService.buscarVpnsPorFolio(q.trim()).pipe(
            catchError(() => {
              this.folioError[i]   = true;
              this.folioLoading[i] = false;
              this.cdr.markForCheck();
              return of([] as VpnDisponible[]);
            })
          );
        })
      ).subscribe(results => {
        if (results === null) return;
        const term = typeof ctrl.value === 'string' ? ctrl.value.trim() : '';
        this.foliosFiltrados[i]    = results;
        this.folioLoading[i]       = false;
        this.folioError[i]         = false;
        this.folioSinResultados[i] = term.length > 0 && results.length === 0;
        this.cdr.markForCheck();
      })
    );
  }

  onFolioFocus(i: number): void {
    if (this.folioLoading[i] || this.foliosFiltrados[i]?.length > 0) return;
    this.folioLoading[i] = true;
    this.folioError[i]   = false;
    this.cdr.markForCheck();
    this.subs.add(
      this.cartaService.buscarVpnsPorFolio('').pipe(
        catchError(() => {
          this.folioError[i]   = true;
          this.folioLoading[i] = false;
          this.cdr.markForCheck();
          return of([]);
        })
      ).subscribe(results => {
        this.foliosFiltrados[i] = results;
        this.folioLoading[i]    = false;
        this.cdr.markForCheck();
      })
    );
  }

  seleccionarFolio(i: number, vpn: VpnDisponible): void {
    this.vpnAt(i).patchValue({ vpnFolio: vpn.folio, vpnIp: vpn.ip ?? '' });
    this.folioSearchCtrls[i].setValue(vpn.folio, { emitEvent: false });
    this.folioSinResultados[i] = false;
    this.cdr.markForCheck();
  }

  agregarFolioVpn(i: number): void {
    const folio = this.folioSearchCtrls[i].value.trim();
    if (!folio) return;
    this.cartaService.agregarFolioVpn(folio).subscribe({
      next: vpn => {
        this.seleccionarFolio(i, vpn);
      },
      error: () => {},
    });
  }

  private suscribirTipoVpn(vpnGroup: FormGroup): void {
    this.subs.add(
      vpnGroup.get('tipoVpn')!.valueChanges.subscribe(() => {
        this.limpiarCamposVpn(vpnGroup);
        this.aplicarValidadoresVpn(vpnGroup);
        this.cdr.markForCheck();
      })
    );
  }

  private limpiarCamposVpn(vpnGroup: FormGroup): void {
    const reset: Record<string, unknown> = {};
    VPN_FIELDS.forEach(f => reset[f] = '');
    vpnGroup.patchValue(reset);
  }

  private aplicarValidadoresVpn(vpnGroup: FormGroup): void {
    VPN_FIELDS.forEach(f => {
      vpnGroup.get(f)?.clearValidators();
      vpnGroup.get(f)?.updateValueAndValidity({ emitEvent: false });
    });

    const req   = [Validators.required];
    const phone = [Validators.required, PHONE];
    const email = [Validators.required, Validators.email];

    switch (vpnGroup.get('tipoVpn')?.value) {
      case 'dependencia':
        this.setV(vpnGroup, 'vpnResponsable', req);
        this.setV(vpnGroup, 'vpnCargo',       req);
        this.setV(vpnGroup, 'vpnTelefono',    phone);
        this.setV(vpnGroup, 'vpnCorreo',      email);
        break;
      case 'actualizacion':
        this.setV(vpnGroup, 'vpnResponsable', req);
        this.setV(vpnGroup, 'vpnFolio',       req);
        this.setV(vpnGroup, 'vpnIp',          req);
        break;
      case 'proveedor':
        this.setV(vpnGroup, 'vpnResponsable', req);
        this.setV(vpnGroup, 'vpnCargo',       req);
        this.setV(vpnGroup, 'vpnTelefono',    phone);
        this.setV(vpnGroup, 'vpnCorreo',      email);
        this.setV(vpnGroup, 'vpnEmpresa',     req);
        this.setV(vpnGroup, 'vpnFolio',       req);
        this.setV(vpnGroup, 'vpnIp',          req);
        this.setV(vpnGroup, 'vpnVigencia',    req);
        break;
    }
  }

  private setV(vpnGroup: FormGroup, field: string, validators: any[]): void {
    vpnGroup.get(field)?.setValidators(validators);
    vpnGroup.get(field)?.updateValueAndValidity({ emitEvent: false });
  }

  hasErrorAt(i: number, field: string, error: string): boolean {
    const ctrl = this.vpnAt(i).get(field);
    return !!(ctrl?.touched && ctrl?.hasError(error));
  }

  isInvalidAt(i: number, field: string): boolean {
    const ctrl = this.vpnAt(i).get(field);
    return !!(ctrl?.touched && ctrl?.invalid);
  }
}
