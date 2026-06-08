import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { UsuarioService } from '../../crearUsuario/services/usuario.service';
import { AuthService } from '../../dashboard/services/auth.service';
import { LoginService } from '../../login/data-access/login.service';

function passwordsMatch(group: AbstractControl) {
  const p = group.get('password')?.value;
  const c = group.get('confirmar')?.value;
  return p && c && p !== c ? { noCoinciden: true } : null;
}

@Component({
  selector: 'app-cambio-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatIconModule],
  templateUrl: './cambio-password.component.html',
  styleUrl: './cambio-password.component.scss',
})
export class CambioPasswordComponent {
  form: FormGroup;
  guardando = false;
  errorMsg = '';
  mostrarPassword = false;
  mostrarConfirm = false;

  constructor(
    private fb: FormBuilder,
    private usuarioService: UsuarioService,
    private authService: AuthService,
    private loginService: LoginService,
    private router: Router,
  ) {
    this.form = this.fb.group(
      {
        password: ['', [Validators.required, Validators.minLength(8)]],
        confirmar: ['', Validators.required],
      },
      { validators: passwordsMatch }
    );
  }

  guardar(): void {
    if (this.form.invalid) return;

    const usuario = this.authService.obtenerUsuario();
    if (!usuario) { this.router.navigate(['login']); return; }

    this.guardando = true;
    this.errorMsg = '';

    this.usuarioService.cambiarPassword(usuario.id, this.form.value.password).subscribe({
      next: () => {
        this.loginService.clearMustChangePassword();
        this.router.navigate(['dashboard']);
      },
      error: () => {
        this.guardando = false;
        this.errorMsg = 'No se pudo actualizar la contraseña. Inténtalo de nuevo.';
      },
    });
  }
}
