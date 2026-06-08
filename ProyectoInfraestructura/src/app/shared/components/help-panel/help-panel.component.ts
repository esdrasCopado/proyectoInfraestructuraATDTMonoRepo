import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { trigger, transition, style, animate } from '@angular/animations';

export interface AyudaCampo {
  nombre: string;
  descripcion: string;
  ejemplo?: string;
  requerido: boolean;
}

export interface AyudaPaso {
  titulo: string;
  descripcion: string;
  campos: AyudaCampo[];
}

@Component({
  selector: 'app-help-panel',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(100%)' }),
        animate('250ms cubic-bezier(0.4, 0, 0.2, 1)', style({ transform: 'translateX(0)' }))
      ]),
      transition(':leave', [
        animate('200ms cubic-bezier(0.4, 0, 0.2, 1)', style({ transform: 'translateX(100%)' }))
      ])
    ])
  ],
  template: `
    <button mat-stroked-button class="help-btn" (click)="toggle()" type="button"
            [attr.aria-expanded]="abierto" aria-controls="help-panel">
      <mat-icon>help_outline</mat-icon>
      {{ abierto ? 'Cerrar ayuda' : '¿Cómo llenar este paso?' }}
    </button>

    @if (abierto) {
      <div class="help-backdrop" (click)="abierto = false"></div>
      <div id="help-panel" class="help-panel" @slideIn role="complementary"
           aria-label="Panel de ayuda">
        <div class="help-header">
          <h4 class="help-title">{{ ayuda.titulo }}</h4>
          <button mat-icon-button class="close-btn" (click)="abierto = false"
                  aria-label="Cerrar ayuda">
            <mat-icon>close</mat-icon>
          </button>
        </div>
        <p class="help-desc">{{ ayuda.descripcion }}</p>

        <div class="help-campos">
          @for (campo of ayuda.campos; track campo.nombre) {
            <div class="help-campo">
              <div class="help-campo-header">
                <span class="help-campo-nombre">{{ campo.nombre }}</span>
                @if (campo.requerido) {
                  <span class="help-badge-req">Obligatorio</span>
                } @else {
                  <span class="help-badge-opt">Opcional</span>
                }
              </div>
              <p class="help-campo-desc">{{ campo.descripcion }}</p>
              @if (campo.ejemplo) {
                <p class="help-campo-ejemplo">
                  <span class="ejemplo-label">Ejemplo:</span> {{ campo.ejemplo }}
                </p>
              }
            </div>
          }
        </div>
      </div>
    }
  `,
  styleUrl: './help-panel.component.scss'
})
export class HelpPanelComponent {
  @Input({ required: true }) ayuda!: AyudaPaso;
  abierto = false;

  toggle(): void {
    this.abierto = !this.abierto;
  }
}
