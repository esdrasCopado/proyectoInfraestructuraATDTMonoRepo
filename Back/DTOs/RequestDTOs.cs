using SolicitudServidores.DTOs;

namespace SolicitudServidores.Back.DTOs
{
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginResponse
    {
        /// <summary>Token JWT. Incluir en el header: Authorization: Bearer {token}. Expira en 60 minutos.</summary>
        public string? Token { get; set; }

        /// <summary>
        /// Datos del usuario autenticado. Si <c>user.mustChangePassword</c> es <c>true</c>,
        /// redirigir al usuario a cambiar su contraseña antes de permitir acceso al sistema.
        /// </summary>
        public UsuarioDTO? User { get; set; }
    }

    public class UserDTO
    {
        public long Id { get; set; }
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Rol { get; set; }
    }

    /// <summary>
    /// Respuesta al crear un usuario. Incluye los datos del usuario y la contraseña temporal generada.
    /// Muéstrasela al administrador para que pueda compartirla manualmente si el correo no llega.
    /// </summary>
    public class CreateUsuarioResponse
    {
        /// <summary>Datos del usuario recién creado.</summary>
        public UsuarioDTO Usuario { get; set; } = null!;

        /// <summary>
        /// Contraseña temporal generada. Se expone una sola vez en esta respuesta.
        /// El usuario deberá cambiarla al primer ingreso.
        /// </summary>
        public string PasswordTemporal { get; set; } = string.Empty;

        /// <summary>Indica si el correo con las credenciales fue enviado exitosamente.</summary>
        public bool CorreoEnviado { get; set; }

        /// <summary>Mensaje de advertencia en caso de que el correo no haya podido enviarse.</summary>
        public string? AdvertenciaCorreo { get; set; }
    }

    /// <summary>
    /// Datos para crear un nuevo usuario. La contraseña se genera automáticamente
    /// y se envía al correo del usuario. No es necesario (ni posible) enviar contraseña.
    /// </summary>
    public class CreateUsuarioRequest
    {
        /// <summary>Nombre(s) del usuario. Requerido.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Apellidos del usuario. Requerido.</summary>
        public string Apellidos { get; set; } = string.Empty;

        /// <summary>Correo electrónico institucional. Se usa como nombre de usuario. Requerido y único.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>ID del rol asignado. Consultar GET /api/usuario/roles para obtener los IDs disponibles.</summary>
        public int RoleId { get; set; }

        /// <summary>ID de la dependencia. Requerido para rol "Dependencia / Cliente". Nulo para administradores.</summary>
        public int? DependencyId { get; set; }

        /// <summary>Número de empleado institucional.</summary>
        public string? NumeroEmpleado { get; set; }

        /// <summary>Puesto o cargo del usuario.</summary>
        public string? Cargo { get; set; }

        /// <summary>Teléfono de contacto.</summary>
        public string? Phone { get; set; }
    }

    public class UpdateUsuarioRequest
    {
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? RoleId { get; set; }
        public int? DependencyId { get; set; }
        public string? NumeroEmpleado { get; set; }
        public string? Cargo { get; set; }
        public string? Phone { get; set; }
        public bool? Activo { get; set; }
    }

    public class CreateSolicitudRequest
    {
        public long? IdUsuario { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Folio { get; set; }
        public string Arquitectura { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Servicios { get; set; } = string.Empty;
        public string? Estado { get; set; }
        public string? EtapaActual { get; set; }
        public string? Prioridad { get; set; }
        public string? ResponsableActual { get; set; }
        public string? UsuarioUltimaActualizacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaRequerida { get; set; }
        public string? CartaResponsivaFolio { get; set; }
        public string? ComentariosSeguimiento { get; set; }
        public bool NotificacionNueva { get; set; } = true;
        public string? TareasPendientes { get; set; }
        public List<CreateServidorRequest>? Servidores { get; set; }
    }

    public class UpdateSolicitudRequest
    {
        public long? IdUsuario { get; set; }
        public string? Titulo { get; set; }
        public string? Folio { get; set; }
        public string? Estado { get; set; }
        public string? EtapaActual { get; set; }
        public string? Prioridad { get; set; }
        public string? Arquitectura { get; set; }
        public string? Descripcion { get; set; }
        public string? Servicios { get; set; }
        public string? ResponsableActual { get; set; }
        public string? UsuarioUltimaActualizacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaRequerida { get; set; }
        public string? CartaResponsivaFolio { get; set; }
        public string? ComentariosSeguimiento { get; set; }
        public bool? NotificacionNueva { get; set; }
        public string? TareasPendientes { get; set; }
        public List<CreateServidorRequest>? Servidores { get; set; }
    }

    public class CreateServidorRequest
    {
        public long? IdSolicitud { get; set; }
        public int? DependencyId { get; set; }
        public string? Estado { get; set; }
        public DateTime? Expiracion { get; set; }
        public string? Hostname { get; set; }
        public string? Ip { get; set; }
        public string? TipoUso { get; set; }
        public string? Funcion { get; set; }
        public string? SistemaOperativo { get; set; }
        public bool? RequiereLlaveLicencia { get; set; }
        public string? LlaveOS { get; set; }
        public int? Nucleos { get; set; }
        public int? Ram { get; set; }
        public int? Almacenamiento { get; set; }
        public string? Descripcion { get; set; }
        public string? PlantillaRecursos { get; set; }
        public string? EtapaOperativa { get; set; }
        public string? ResponsableInfraestructura { get; set; }
        public string? UsuarioUltimaActualizacion { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; }
        public DateTime? FechaAsignacionIp { get; set; }
        public string? TareasPendientes { get; set; }
        public string? ObservacionesSeguimiento { get; set; }
        public string? EtapaVulnerabilidades { get; set; }
        public bool? RequiereRevisionAnual { get; set; }
        public DateTime? UltimaRevisionAnual { get; set; }
        public bool? ComunicacionValidada { get; set; }
        public DateTime? FechaValidacionComunicacion { get; set; }
        public string? UsuarioValidacionComunicacion { get; set; }
        public bool? ParchesAplicados { get; set; }
        public DateTime? FechaParches { get; set; }
        public string? UsuarioParches { get; set; }
        public bool? XdrInstalado { get; set; }
        public DateTime? FechaXdr { get; set; }
        public string? UsuarioXdr { get; set; }
        public bool? CredencialesEntregadas { get; set; }
        public DateTime? FechaEntregaCredenciales { get; set; }
        public string? UsuarioCredenciales { get; set; }
        public bool? WafConfigurado { get; set; }
        public DateTime? FechaConfiguracionWaf { get; set; }
        public string? UsuarioWaf { get; set; }
        public bool? EvidenciaValidada { get; set; }
        public DateTime? FechaValidacionEvidencia { get; set; }
        public string? UsuarioValidacionEvidencia { get; set; }
        public bool? SolicitudPublicacion { get; set; }
        public DateTime? FechaPublicacion { get; set; }
        public string? UsuarioPublicacion { get; set; }
        public DateTime? FechaVulnerabilidades { get; set; }
        public string? UsuarioVulnerabilidades { get; set; }
        public List<VpnRequestDTO>? VPNs { get; set; }
        public List<SubdominioRequestDTO>? Subdominios { get; set; }
        public List<WafRequestDTO>? WAFs { get; set; }
        public List<EvidenciaPruebaRequestDTO>? EvidenciasPruebas { get; set; }
    }

    public class UpdateServidorRequest : CreateServidorRequest
    {
    }

    public class VpnRequestDTO
    {
        public long? IdUsuarioResponsable { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public string? Estado { get; set; }
    }

    public class SubdominioRequestDTO
    {
        public long? IdUsuario { get; set; }
        public string NombreUrl { get; set; } = string.Empty;
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public string? Estado { get; set; }
    }

    public class WafRequestDTO
    {
        public long? IdUsuario { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Estado { get; set; }
        public string? Observaciones { get; set; }
    }

    public class EvidenciaPruebaRequestDTO
    {
        public long? IdUsuario { get; set; }
        public string RutaPdf { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public string? EstadoValidacion { get; set; }
        public string? Observaciones { get; set; }
    }

    public class ActualizarEstadoRequest
    {
        public string Estado { get; set; } = string.Empty;
        public string? EtapaActual { get; set; }
        public string? ResponsableActual { get; set; }
        public string? UsuarioUltimaActualizacion { get; set; }
        public string? ComentariosSeguimiento { get; set; }
    }

    public class RecursoServidorPredeterminadoDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public int Nucleos { get; set; }
        public int Ram { get; set; }
        public int Almacenamiento { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    /// <summary>Cuerpo para notificar al cliente/dependencia vinculado a una solicitud.</summary>
    public class NotificarClienteSolicitudRequest
    {
        /// <summary>ID de la solicitud cuyo creador (Dependencia / Cliente) recibirá la notificación.</summary>
        public long SolicitudId { get; set; }

        /// <summary>Tipo de evento. Ej: subdominio_asignado | evidencias_aprobadas | solicitud_publicacion.</summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>Título corto de la notificación.</summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>Cuerpo del mensaje.</summary>
        public string Mensaje { get; set; } = string.Empty;
    }

    /// <summary>Cuerpo para actualizar el status de un único subdominio.</summary>
    public class ActualizarStatusSubdominioRequest
    {
        /// <summary>Nuevo status del subdominio. Valores válidos: <c>solicitado</c> | <c>aprobado</c> | <c>activo</c> | <c>expirado</c> | <c>revocado</c>.</summary>
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>Cuerpo para actualizar el status de varios subdominios en lote.</summary>
    public class ActualizarStatusSubdominiosBatchRequest
    {
        /// <summary>Lista de IDs de los subdominios a actualizar. Los IDs inexistentes son ignorados.</summary>
        public List<int> Ids { get; set; } = new();

        /// <summary>Nuevo status a aplicar. Valores válidos: <c>solicitado</c> | <c>aprobado</c> | <c>activo</c> | <c>expirado</c> | <c>revocado</c>.</summary>
        public string Status { get; set; } = string.Empty;
    }
}
