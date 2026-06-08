namespace SolicitudServidores.DTOs
{
    public class CartaRequestDTO
    {
        public AreaRequirenteDTO? AreaRequirente { get; set; }
        public AdminServidorDTO? AdminServidor { get; set; }
        public DescripcionDTO? Descripcion { get; set; }
        public SpecsDTO? Specs { get; set; }
        public InfraestructuraDTO? Infraestructura { get; set; }
        public ResponsivaDTO? Responsiva { get; set; }
    }

    public class AreaRequirenteDTO
    {
        public string? Sector { get; set; }
        public string? Dependencia { get; set; }
        public string? Responsable { get; set; }
        public string? Cargo { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
    }

    public class AdminServidorDTO
    {
        public string? Proveedor { get; set; }
        public string? Dependencia { get; set; }
        public string? Responsable { get; set; }
        public string? Cargo { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
    }

    public class DescripcionDTO
    {
        public string? DescripcionServidor { get; set; }
        public string? NombreServidor { get; set; }
        public string? NombreAplicacion { get; set; }
        public string? TipoUso { get; set; }
        public string? FechaArranque { get; set; }
        public string? Vigencia { get; set; }
        public string? CaracteristicasEspeciales { get; set; }
    }

    public class SpecsDTO
    {
        public string? TipoRequerimiento { get; set; }
        /// <summary>virtual | fisica | hibrida | nube</summary>
        public string? Arquitectura { get; set; }
        /// <summary>nuevo | renovacion | clonacion | serverBase</summary>
        public string? Modalidad { get; set; }
        /// <summary>windows | linux | otro</summary>
        public string? SistemaOperativo { get; set; }
        public string? SistemaOperativoOtro { get; set; }
        public int VCores { get; set; }
        public int MemoriaRam { get; set; }
        /// <summary>Total de almacenamiento en GB (plano, retrocompatibilidad).</summary>
        public int Almacenamiento { get; set; }
        /// <summary>Detalle de cada disco.  Cuando se envía, el total reemplaza a Almacenamiento.</summary>
        public List<DiscoDuroDTO>? DiscosDuros { get; set; }
        public string? MotorBD { get; set; }
        public string? Puertos { get; set; }
        public string? Integraciones { get; set; }
        public string? OtrasSpecs { get; set; }
        // Solo obligatorios cuando Modalidad = "renovacion"
        public string? IpActual { get; set; }
        public string? NombreServidorActual { get; set; }
        /// <summary>renovacion_hardware | renovacion_software | migracion</summary>
        public string? TipoRenovacion { get; set; }
    }

    public class DiscoDuroDTO
    {
        public int Capacidad { get; set; }
        /// <summary>SSD | HDD | NVMe</summary>
        public string? Tipo { get; set; }
        public string? Etiqueta { get; set; }
    }

    public class InfraestructuraDTO
    {
        /// <summary>Lista de subdominios solicitados (URLs completas).</summary>
        public List<string>? Subdominios { get; set; }
        public string? Puerto { get; set; }
        public bool RequiereSSL { get; set; }
        public List<VpnCartaDTO>? Vpns { get; set; }
    }

    public class VpnCartaDTO
    {
        /// <summary>Usuario VPN de dependencia | Usuario VPN para proveedor | Actualizacion de usuario VPN</summary>
        public string? TipoVpn { get; set; }
        public string? VpnResponsable { get; set; }
        public string? VpnCargo { get; set; }
        public string? VpnTelefono { get; set; }
        public string? VpnCorreo { get; set; }
        /// <summary>Solo para tipo "Actualizacion de usuario VPN".</summary>
        public string? VpnPerfilAnterior { get; set; }
        /// <summary>Nombre(s) de servidor(es) asociados.</summary>
        public string? VpnServidores { get; set; }
        /// <summary>Folio de la VPN existente (solo en actualizaciones o renovaciones).</summary>
        public string? VpnFolio { get; set; }
        public string? VpnIp { get; set; }
        public string? VpnEmpresa { get; set; }
        /// <summary>Vigencia en días: "30" | "60" | "90".</summary>
        public string? VpnVigencia { get; set; }
    }

    public class ResponsivaDTO
    {
        public string? Firmante { get; set; }
        public string? NumEmpleado { get; set; }
        public string? PuestoFirmante { get; set; }
        public bool AceptaTerminos { get; set; }
    }
}
