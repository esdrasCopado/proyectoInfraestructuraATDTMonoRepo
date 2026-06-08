import { Component, Input, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Solicitud } from '../../../../models/solicitud.model';

@Component({
  selector: 'app-etapa-verificacion',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './etapa-verificacion.component.html',
  styleUrl: './etapa-verificacion.component.scss'
})
export class EtapaVerificacionComponent implements OnInit {

  @Input({ required: true }) solicitud!: Solicitud;
  @Input({ required: true }) etapa!: 4 | 5 | 6;

  form!: FormGroup;

  private get specs()  { return this.solicitud.carta?.specs; }
  private get etapa2() { return this.solicitud.etapas.find(e => e.numero === 2); }

  // ── Datos de referencia ───────────────────────────────────────────────────

  get ip(): string { return this.etapa2?.ip || '—'; }
  get puertos():       string { return this.specs?.puertos       || '—'; }
  get motorBD():       string { return this.specs?.motorBD       || '—'; }
  get integraciones(): string { return this.specs?.integraciones || '—'; }
  get nombreServidor():string { return this.solicitud.nombreServidor || '—'; }

  get sistemaOperativo(): string {
    const so = this.specs?.sistemaOperativo;
    if (!so || so === 'otro') {
      return this.specs?.sistemaOperativoOtro || 'Otro';
    }
    return { windows: 'Windows Server', linux: 'Linux' }[so];
  }

  // fecha en que se completó la Etapa 3 (creación del servidor)
  get fechaCreacionServidor(): string {
    return this.solicitud.etapas.find(e => e.numero === 3)?.fechaActualizacion ?? '—';
  }

  // ── Textos por etapa ──────────────────────────────────────────────────────

  get tituloBloque1(): string {
    return ({
      4: 'Datos de red a verificar',
      5: 'Sistema operativo y estado de actualización',
      6: 'Identificación del host en XDR',
    } as const)[this.etapa];
  }

  get hintBloque1(): string {
    return ({
      4: 'Solo lectura · Declarado en la carta de aprovisionamiento. Usa ping, traceroute, telnet o nmap para verificar conectividad y puertos antes de confirmar.',
      5: 'Solo lectura · Aplica actualizaciones con apt/yum o Windows Update/WSUS antes de confirmar. La fecha de creación te indica qué tan reciente es la imagen base.',
      6: 'Solo lectura · Busca el host en la consola XDR y confirma que aparece con estado "online" o "healthy" antes de marcar.',
    } as const)[this.etapa];
  }

  get checkboxText(): string {
    return ({
      4: 'Confirmo que las comunicaciones del servidor han sido verificadas y están operativas',
      5: 'Confirmo que el sistema operativo cuenta con las actualizaciones y parches al día',
      6: 'Confirmo que el agente XDR está instalado y reportando correctamente',
    } as const)[this.etapa];
  }

  get confirmadoInvalido(): boolean {
    const ctrl = this.form.get('confirmado');
    return !!ctrl?.invalid && !!ctrl?.touched;
  }

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.form = this.fb.group({ confirmado: [false, Validators.requiredTrue] });
  }
}
