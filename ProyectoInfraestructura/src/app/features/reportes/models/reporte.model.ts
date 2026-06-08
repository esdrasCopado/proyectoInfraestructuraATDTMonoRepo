export interface FiltrosReporte {
  fechaInicio?: string;
  fechaFin?: string;
  ip?: string;
  dependencia?: string;
  sistemaOperativo?: string;
  agrupacion?: string;
  estado?: string;
}

export interface FilaBase {
  folioSolicitud: string;
  dependencia: string;
  responsable: string;
  contacto: string;
  estatusProcesamieto: string;
  fechaCreacion: string;
}

export interface FilaConServidor extends FilaBase {
  ipServidor: string;
  administradorServidor: string;
  descripcionProyecto: string;
  sistemaOperativo: string;
  vcpu: number;
  ram: number;
  almacenamiento: number;
}

// 1.1 Solicitudes por dependencia
export interface Reporte11Fila {
  folioSolicitud: string;
  sector: string;
  dependencia: string;
  responsable: string;
  contacto: string;
  estatusProcesamieto: string;
  fechaCreacion: string;
}

// 1.2 Recursos solicitados totalizados
export type Reporte12Fila = FilaConServidor;

export interface Reporte12Response {
  items: Reporte12Fila[];
  totalVcpu: number;
  totalRam: number;
  totalAlmacenamiento: number;
}

// 1.3 Por IP
export interface Reporte13Fila extends FilaConServidor {
  contactoResponsable: string;
  subdominiosAprobados: string[];
  vpns: string[];
}

// 2.1 VPN
export interface Reporte21Fila {
  folioSolicitud: string;
  sector: string;
  dependencia: string;
  responsableServidor: string;
  contactoResponsable: string;
  estatusProcesamieto: string;
  ipServidor: string;
  identificadorVpn: string;
  usuarioAsignado: string;
  fechaCreacionVpn: string;
  fechaVencimientoVpn: string;
  vigencia: string;
  tipoVpn: string;
}

// 2.2 Subdominios
export interface Reporte22Fila {
  folioSolicitud: string;
  sector: string;
  dependencia: string;
  responsableServidor: string;
  contacto: string;
  estatusProcesamieto: string;
  ipServidor: string;
  subdominioAprobado: string;
  proxyAsignado: string;
  tipoDespliegue: string;
  puerto: string;
  fechaAsignacion: string;
}

// 3.1 Vulnerabilidades
export interface Reporte31Fila {
  folioSolicitud: string;
  sector: string;
  dependencia: string;
  responsableServidor: string;
  telefonoContacto: string;
  emailContacto: string;
  estatusProcesamieto: string;
  ipServidor: string;
  subdominiosAprobados: string[];
  fechaSolicitudAnalisis: string;
  fechaAplicacionPrueba: string;
  resultadoPrueba: string;
  hallazgos: string;
  ronda: number;
}

// 3.2 Comunicaciones y aplicativos por IP
export interface Reporte32Fila {
  folioSolicitud: string;
  sector: string;
  dependencia: string;
  responsableServidor: string;
  contactoResponsable: string;
  estatusProcesamieto: string;
  ipServidor: string;
  subdominiosAprobados: string[];
  tipoDespliegue: string;
  puertosSolicitados: number[];
  reglasFirewall: string;
  integracionesExternas: string;
  otras: string;
}

// 4.1 Estatus de solicitudes
export interface Reporte41Fila {
  sector: string;
  dependencia: string;
  responsable: string;
  emailContacto: string;
  descripcionServidor: string;
  ip: string;
  fechaSolicitud: string;
  estatusProcesamieto: string;
  etapaActual: number;
  nombreEtapaActual: string;
  fechaProcesamientoEtapa: string;
  rolResponsableEtapa: string;
  fechaPublicacion: string;
  tipoDespliegue: string;
}

export interface Reporte41Response {
  items: Reporte41Fila[];
  totalSolicitudes: number;
}

// 4.2 Recursos totalizados general
export interface Reporte42Fila {
  folioSolicitud: string;
  sector: string;
  dependencia: string;
  responsable: string;
  contacto: string;
  estatusProcesamieto: string;
  ipServidor: string;
  administradorServidor: string;
  descripcionProyecto: string;
  sistemaOperativo: string;
  vcpu: number;
  ram: number;
  almacenamiento: number;
  subdominiosAprobados: string[];
  vpns: string[];
  fechaCreacion: string;
}

export interface Reporte42Response {
  items: Reporte42Fila[];
  totalVcpu: number;
  totalRam: number;
  totalAlmacenamiento: number;
  totalServidores: number;
}
