namespace SolicitudServidores.DTOs
{
    public class CreateSolicitudCompletaRequest
    {
        // Identificación
        public long IdUsuario { get; set; }
        public string? ResponsableActual { get; set; }
        public string? UsuarioUltimaActualizacion { get; set; }
        public string? FechaActualizacion { get; set; }

        // Área requirente → Dependency
        public string? Sector { get; set; }
        public string? Dependencia { get; set; }
        public string? CargoResponsable { get; set; }
        public string? TelefonoResponsable { get; set; }
        public string? CorreoResponsable { get; set; }

        // Admin / proveedor → AdminDepContactInformation
        public string? Proveedor { get; set; }
        public string? DependenciaAdmin { get; set; }
        public string? CargoAdmin { get; set; }
        public string? TelefonoAdmin { get; set; }
        public string? CorreoAdmin { get; set; }

        // Solicitud
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? FechaRequerida { get; set; }
        public string? ComentariosSeguimiento { get; set; }

        // Servidores (se procesa el primero del array)
        public List<ServidorCompletaDto> Servidores { get; set; } = new();

        // Carta / firma → Carta
        public string? Firmante { get; set; }
        public string? NumEmpleado { get; set; }
        public string? PuestoFirmante { get; set; }
        public bool AceptaTerminos { get; set; }
    }

    public class ServidorCompletaDto
    {
        public string? Hostname { get; set; }
        public string? TipoUso { get; set; }
        public string? Vigencia { get; set; }
        public string? Funcion { get; set; }
        public string? Descripcion { get; set; }
        public string? PlantillaRecursos { get; set; }
        public string? Modalidad { get; set; }
        public string? SistemaOperativo { get; set; }
        public bool RequiereLlaveLicencia { get; set; }
        public int Nucleos { get; set; }
        public int Ram { get; set; }
        public List<DiscoDuroCompletaDto> DiscosDuros { get; set; } = new();
        public string? MotorBD { get; set; }
        public string? Puertos { get; set; }
        public string? Integraciones { get; set; }
        public string? OtrasSpecs { get; set; }
        public string? ResponsableInfraestructura { get; set; }
        public bool SolicitudPublicacion { get; set; }
        public string? Puerto { get; set; }
        public bool RequiereSSL { get; set; }
        public List<VpnCompletaDto> VpNs { get; set; } = new();
        public List<SubdominioCompletaDto> Subdominios { get; set; } = new();
    }

    public class DiscoDuroCompletaDto
    {
        public int Capacidad { get; set; }
        public string? Tipo { get; set; }
        public string? Etiqueta { get; set; }
    }

    public class VpnCompletaDto
    {
        public string? Tipo { get; set; }
        public string? Responsable { get; set; }
        public string? Cargo { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? PerfilAnterior { get; set; }
        public string? Ip { get; set; }
        public string? Empresa { get; set; }
        public string? Vigencia { get; set; }
        public string? FechaAsignacion { get; set; }
        public string? FechaExpiracion { get; set; }
        public string? Estado { get; set; }
        public string? Folio { get; set; }
    }

    public class SubdominioCompletaDto
    {
        public string? NombreUrl { get; set; }
        public string? Puerto { get; set; }
        public bool RequiereSSL { get; set; }
        public string? FechaAsignacion { get; set; }
        public string? FechaExpiracion { get; set; }
        public string? Estado { get; set; }
    }
}
