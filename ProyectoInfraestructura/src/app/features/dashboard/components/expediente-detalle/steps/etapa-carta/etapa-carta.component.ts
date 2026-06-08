import { Component, Input } from '@angular/core';
import { DatePipe, LowerCasePipe } from '@angular/common';
import { Solicitud } from '../../../../models/solicitud.model';
import { DiscoDuro } from '../../../../../carta-aprovisionamiento/models/carta-aprovisionamiento.model';

@Component({
  selector: 'app-etapa-carta',
  standalone: true,
  imports: [DatePipe, LowerCasePipe],
  templateUrl: './etapa-carta.component.html',
  styleUrl: './etapa-carta.component.scss'
})
export class EtapaCartaComponent {

  @Input({ required: true }) solicitud!: Solicitud;

  vpnTipoLabel(tipo: string): string {
    return ({ dependencia: 'Usuario VPN de la Dependencia', actualizacion: 'Actualización de Usuario VPN', proveedor: 'Usuario VPN para Proveedor' } as Record<string, string>)[tipo] ?? tipo;
  }

  soLabel(so: 'windows' | 'linux' | 'otro'): string {
    return ({ windows: 'Windows', linux: 'Linux', otro: 'Otro' } as const)[so] ?? so;
  }

  totalAlmacenamiento(discos: DiscoDuro[]): number {
    return discos.reduce((acc, d) => acc + d.capacidad, 0);
  }
}
