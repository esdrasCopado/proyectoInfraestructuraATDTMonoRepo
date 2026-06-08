import { Pipe, PipeTransform } from '@angular/core';

const BADGE_CLASS: Record<string, string> = {
  pendiente:                    'badge-pendiente',
  recursos_asignados:           'badge-progreso',
  aprovisionado:                'badge-progreso',
  en_configuracion:             'badge-progreso',
  credenciales_entregadas:      'badge-progreso',
  en_pruebas:                   'badge-progreso',
  en_validacion_evidencias:     'badge-progreso',
  en_analisis_vulnerabilidades: 'badge-progreso',
  publicado:                    'badge-completado',
  finalizado:                   'badge-completado',
  rechazado:                    'badge-vencido',
  // legacy
  'en-progreso':                'badge-progreso',
  'en-validacion':              'badge-progreso',
  'en-pruebas':                 'badge-progreso',
  completada:                   'badge-completado',
};

export const ESTADO_LABEL: Record<string, string> = {
  pendiente:                    'Pendiente',
  recursos_asignados:           'Recursos asignados',
  aprovisionado:                'Aprovisionado',
  en_configuracion:             'En configuración',
  credenciales_entregadas:      'Credenciales entregadas',
  en_pruebas:                   'En pruebas',
  en_validacion_evidencias:     'Validación de evidencias',
  en_analisis_vulnerabilidades: 'Análisis de vulnerabilidades',
  publicado:                    'Publicado',
  finalizado:                   'Finalizado',
  rechazado:                    'Rechazado',
};

@Pipe({ name: 'estatusBadge', standalone: true })
export class EstatusBadgePipe implements PipeTransform {
  transform(estatus: string): string {
    return BADGE_CLASS[estatus] ?? BADGE_CLASS[(estatus || '').toLowerCase()] ?? 'badge-default';
  }
}

@Pipe({ name: 'estadoLabel', standalone: true })
export class EstadoLabelPipe implements PipeTransform {
  transform(estatus: string): string {
    return ESTADO_LABEL[estatus] ?? ESTADO_LABEL[(estatus || '').toLowerCase()] ?? estatus;
  }
}
