import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { Subscription } from 'rxjs';
import { UsuarioService, CreateUsuarioResponse, RolOpcion, DependenciaOpcion } from '../services/usuario.service';

@Component({
  selector: 'app-nuevo-usuario',
  templateUrl: './nuevoUsuario.component.html',
  styleUrls: ['./nuevoUsuario.component.scss'],
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, MatIconModule],
})
export class NuevoUsuarioComponent implements OnInit, OnDestroy {
  form: FormGroup;
  roles: RolOpcion[] = [];
  dependencias: DependenciaOpcion[] = [];
  mostrarDependencias = false;
  cargandoDeps = false;
  guardando = false;
  resultado: CreateUsuarioResponse | null = null;
  errorMsg = '';
  copiado = false;

  mostrarFormDep = false;
  nombreNuevaDep = '';
  guardandoDep = false;
  errorDep = '';

  private rolSub?: Subscription;

  constructor(
    private fb: FormBuilder,
    private usuarioService: UsuarioService,
    private cdr: ChangeDetectorRef,
  ) {
    this.form = this.fb.group({
      nombre:         ['', [Validators.required, Validators.minLength(2)]],
      apellidos:      ['', [Validators.required, Validators.minLength(2)]],
      email:          ['', [Validators.required, Validators.email]],
      roleId:         [null, Validators.required],
      dependencyId:   [null],
      cargo:          [''],
      numeroEmpleado: [''],
      phone:          [''],
    });
  }

  ngOnInit(): void {
    this.usuarioService.getRoles().subscribe(r => { this.roles = r; this.cdr.detectChanges(); });

    this.rolSub = this.form.get('roleId')!.valueChanges.subscribe(roleId => {
      const esDep = this.esRolDependencia(roleId);
      this.mostrarDependencias = esDep;

      const depCtrl = this.form.get('dependencyId')!;
      if (esDep) {
        depCtrl.setValidators(Validators.required);
        if (this.dependencias.length === 0) this.cargarDependencias();
      } else {
        depCtrl.clearValidators();
        depCtrl.setValue(null);
        this.ocultarFormDep();
      }
      depCtrl.updateValueAndValidity();
      this.cdr.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.rolSub?.unsubscribe();
  }

  private esRolDependencia(roleId: number | null): boolean {
    if (!roleId) return false;
    const rol = this.roles.find(r => r.roleId === Number(roleId));
    if (!rol) return false;
    const nombre = rol.nombre.toLowerCase();
    return nombre.includes('dependencia') || nombre.includes('cliente');
  }

  private cargarDependencias(): void {
    this.cargandoDeps = true;
    this.usuarioService.getDependencias().subscribe(d => {
      this.dependencias = d;
      this.cargandoDeps = false;
      this.cdr.detectChanges();
    });
  }

  campo(name: string) { return this.form.get(name); }

  mostrarAgregarDep(): void {
    this.mostrarFormDep = true;
    this.nombreNuevaDep = '';
    this.errorDep = '';
  }

  ocultarFormDep(): void {
    this.mostrarFormDep = false;
    this.nombreNuevaDep = '';
    this.errorDep = '';
    this.guardandoDep = false;
  }

  agregarDependencia(): void {
    const nombre = this.nombreNuevaDep.trim();
    if (!nombre || this.guardandoDep) return;
    this.guardandoDep = true;
    this.errorDep = '';
    this.usuarioService.crearDependencia(nombre).subscribe({
      next: (nueva) => {
        this.dependencias = [...this.dependencias, nueva];
        this.form.get('dependencyId')!.setValue(nueva.dependencyId);
        this.ocultarFormDep();
        this.cdr.detectChanges();
      },
      error: () => {
        this.guardandoDep = false;
        this.errorDep = 'No se pudo crear la dependencia. Intenta de nuevo.';
        this.cdr.detectChanges();
      },
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.guardando) return;
    this.guardando = true;
    this.errorMsg = '';
    const v = this.form.value;
    this.usuarioService.crearUsuario({
      nombre:         v.nombre.trim(),
      apellidos:      v.apellidos.trim(),
      email:          v.email.trim(),
      roleId:         Number(v.roleId),
      dependencyId:   v.dependencyId ? Number(v.dependencyId) : undefined,
      cargo:          v.cargo?.trim()          || undefined,
      numeroEmpleado: v.numeroEmpleado?.trim() || undefined,
      phone:          v.phone?.trim()          || undefined,
    }).subscribe({
      next: (res) => {
        this.resultado = res;
        this.guardando = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.guardando = false;
        if (err.status === 409) {
          this.errorMsg = 'Ya existe un usuario registrado con ese correo electrónico.';
        } else if (err.status === 400) {
          this.errorMsg = 'Verifica que todos los campos obligatorios estén completos y sean correctos.';
        } else {
          this.errorMsg = 'Ocurrió un error al crear el usuario. Intenta de nuevo.';
        }
        this.cdr.detectChanges();
      },
    });
  }

  nuevoUsuario(): void {
    this.resultado = null;
    this.errorMsg = '';
    this.copiado = false;
    this.mostrarDependencias = false;
    this.ocultarFormDep();
    this.form.reset();
  }

  copiarPassword(): void {
    const pw = this.resultado?.passwordTemporal;
    if (!pw) return;
    navigator.clipboard.writeText(pw).then(() => {
      this.copiado = true;
      this.cdr.detectChanges();
      setTimeout(() => { this.copiado = false; this.cdr.detectChanges(); }, 2000);
    });
  }
}
