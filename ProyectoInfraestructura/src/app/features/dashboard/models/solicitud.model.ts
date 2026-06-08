import { CartaAprovisionamiento } from '../../carta-aprovisionamiento/models/carta-aprovisionamiento.model';

export type EstadoEtapa = 'completada' | 'en-curso' | 'pendiente-respuesta' | 'sin-iniciar';
export type EstadoSolicitud =
  | 'pendiente'
  | 'recursos_asignados'
  | 'aprovisionado'
  | 'en_configuracion'
  | 'credenciales_entregadas'
  | 'en_pruebas'
  | 'en_validacion_evidencias'
  | 'en_analisis_vulnerabilidades'
  | 'publicado'
  | 'finalizado'
  | 'rechazado';

export interface EtapaSolicitud {
  numero: number;
  nombre: string;
  estado: EstadoEtapa;
  fechaActualizacion?: string;
  responsable?: string;
  vCores?: number;
  memoriaRam?: number;
  almacenamiento?: number;
  ip?: string;
}

export interface VpnServidor {
  id?:             number;
  vpnType:         string;
  responsable:     string;
  cargo?:          string;
  phone?:          string;
  email?:          string;
  folio?:          string;
  vpnIp?:          string;
  perfilAnterior?: string;
  servidores?:     string;
  empresa?:        string;
  vigenciaDias?:   number;
  estado?:         string;
  fechaAsignacion?: string;
  fechaExpiracion?: string;
  assignedAt:      string;
}

export interface SubdominioServidor {
  id?:           number;
  requestedName: string;
  port: number;
  sslRequired: boolean;
  status: string;
  assignedAt: string;
}

export interface Solicitud {
  id: string;
  folio: string;
  dependencia: string;
  idUsuario?: number;
  nombreServidor: string;
  servidorId?: string;
  estado: EstadoSolicitud;
  etapaActual: number;
  etapas: EtapaSolicitud[];
  fechaRegistro: string;
  fechaActualizacion: string;
  carta?: CartaAprovisionamiento;
  vpns?: VpnServidor[];
  subdominios?: SubdominioServidor[];
}

export interface DashboardMetricas {
  total: number;
  enProgreso: number;
  pendientes: number;
  completadas: number;
}

export interface DashboardResponse {
  solicitudes: Solicitud[];
  metricas: DashboardMetricas;
}
