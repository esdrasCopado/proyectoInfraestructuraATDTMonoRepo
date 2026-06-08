export interface ContactoArea {
  sector:      string;
  dependencia: string;
  responsable: string;
  cargo:       string;
  telefono:    string;
  correo:      string;
}

export interface AdminServidor extends ContactoArea {
  proveedor: string;
}

export interface Descripcion {
  descripcionServidor:       string;
  nombreServidor:            string;
  nombreAplicacion:          string;
  tipoUso:                   'interno' | 'publicado';
  fechaArranque:             string;
  vigencia:                  string;
  caracteristicasEspeciales: string;
}

export interface DiscoDuro {
  capacidad: number;
  tipo:      'SSD' | 'HDD' | 'NVMe';
  etiqueta:  string;
}

export interface Specs {
  tipoRequerimiento:    'estandar' | 'especifico';
  modalidad:            'nuevo' | 'renovacion';
  sistemaOperativo:     'windows' | 'linux' | 'otro';
  sistemaOperativoOtro: string;
  vCores:               number;
  memoriaRam:           number;
  discosDuros:          DiscoDuro[];
  motorBD:              string;
  puertos:              string;
  integraciones:        string;
  otrasSpecs:           string;
  // Campos de renovación (presentes solo cuando modalidad = 'renovacion')
  ipActual:             string;
  nombreServidorActual: string;
  tipoRenovacion:       'clonacion' | 'serverBase' | '';
}

export interface VpnEntry {
  tipoVpn:           'dependencia' | 'actualizacion' | 'proveedor';
  vpnResponsable:    string;
  vpnCargo:          string;
  vpnTelefono:       string;
  vpnCorreo:         string;
  vpnPerfilAnterior: string;
  vpnServidores:     string;
  vpnFolio:          string;
  vpnIp:             string;
  vpnEmpresa:        string;
  vpnVigencia:       '30' | '60' | '90' | '';
}

export interface SubdominioEntry {
  subdominio: string;
  puerto:     string;
}

export interface Infraestructura {
  subdominios: SubdominioEntry[];
  requiereSSL: boolean;
  vpns:        VpnEntry[];
}

export interface Responsiva {
  firmante:       string;
  numEmpleado:    string;
  puestoFirmante: string;
  aceptaTerminos: boolean;
}

export interface CartaAprovisionamiento {
  areaRequirente: ContactoArea;
  adminServidor:  AdminServidor;
  descripcion:    Descripcion;
  specs:          Specs;
  infraestructura: Infraestructura;
  responsiva:     Responsiva;
}
