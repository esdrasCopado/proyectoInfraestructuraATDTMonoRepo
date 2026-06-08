namespace SolicitudServidores.DTOs
{
    /// <summary>Notificación devuelta al cliente.</summary>
    public class NotificationDTO
    {
        /// <summary>Identificador único de la notificación.</summary>
        public long NotificationId { get; set; }

        /// <summary>ID del usuario destinatario.</summary>
        public long RecipientUserId { get; set; }

        /// <summary>ID del usuario que envió la notificación. Null si fue generada por el sistema.</summary>
        public long? SenderUserId { get; set; }

        /// <summary>Tipo de evento. Ej: subdominio_asignado | evidencias_aprobadas | solicitud_publicacion.</summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>ID de la solicitud relacionada (opcional).</summary>
        public long? SolicitudId { get; set; }

        /// <summary>Tipo de entidad relacionada. Ej: evidencia | vpn | subdominio (opcional).</summary>
        public string? EntityType { get; set; }

        /// <summary>ID de la entidad relacionada (opcional).</summary>
        public long? EntityId { get; set; }

        /// <summary>Título corto de la notificación.</summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>Cuerpo del mensaje.</summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>Indica si el destinatario ya leyó la notificación.</summary>
        public bool Leida { get; set; }

        /// <summary>Fecha en que fue marcada como leída (UTC). Null si aún no se ha leído.</summary>
        public DateTime? LeidaAt { get; set; }

        /// <summary>Fecha de creación (UTC).</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Cuerpo para crear una notificación dirigida a un usuario específico.</summary>
    public class CreateNotificationRequest
    {
        /// <summary>ID del usuario que recibirá la notificación.</summary>
        public long RecipientUserId { get; set; }

        /// <summary>ID del usuario remitente. Omitir si es generada por el sistema.</summary>
        public long? SenderUserId { get; set; }

        /// <summary>
        /// Tipo de evento que originó la notificación.
        /// Valores sugeridos: <c>subdominio_asignado</c> | <c>evidencias_cargadas</c> | <c>evidencias_aprobadas</c> |
        /// <c>evidencias_rechazadas</c> | <c>solicitud_publicacion</c> | <c>vulnerabilidades_aprobadas</c> | <c>vulnerabilidades_rechazadas</c>.
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>ID de la solicitud relacionada (opcional).</summary>
        public long? SolicitudId { get; set; }

        /// <summary>Tipo de entidad relacionada. Ej: evidencia | vpn | subdominio (opcional).</summary>
        public string? EntityType { get; set; }

        /// <summary>ID de la entidad relacionada (opcional).</summary>
        public long? EntityId { get; set; }

        /// <summary>Título corto visible en la campana de notificaciones.</summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>Cuerpo completo del mensaje.</summary>
        public string Mensaje { get; set; } = string.Empty;
    }

    /// <summary>Cuerpo para notificar a un usuario específico (el ID va en la ruta).</summary>
    public class NotificarUsuarioRequest
    {
        /// <summary>
        /// Tipo de evento que originó la notificación.
        /// Valores sugeridos: <c>subdominio_asignado</c> | <c>evidencias_cargadas</c> | <c>evidencias_aprobadas</c> |
        /// <c>evidencias_rechazadas</c> | <c>solicitud_publicacion</c> | <c>vulnerabilidades_aprobadas</c> | <c>vulnerabilidades_rechazadas</c>.
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>ID de la solicitud relacionada (opcional).</summary>
        public long? SolicitudId { get; set; }

        /// <summary>Tipo de entidad relacionada. Ej: evidencia | vpn | subdominio (opcional).</summary>
        public string? EntityType { get; set; }

        /// <summary>ID de la entidad relacionada (opcional).</summary>
        public long? EntityId { get; set; }

        /// <summary>Título corto visible en la campana de notificaciones.</summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>Cuerpo completo del mensaje.</summary>
        public string Mensaje { get; set; } = string.Empty;
    }

    /// <summary>Cuerpo para marcar una notificación como leída o no leída.</summary>
    public class MarkReadRequest
    {
        /// <summary>true = marcar como leída; false = marcar como no leída.</summary>
        public bool Leida { get; set; } = true;
    }

    /// <summary>Cuerpo para enviar una notificación a todos los usuarios activos de un rol.</summary>
    public class NotificarPorRolRequest
    {
        /// <summary>
        /// Nombre exacto del rol destinatario:
        /// <c>Administrador de Centro de Datos</c> | <c>Administrador de Infraestructura</c> |
        /// <c>Administrador de Vulnerabilidades</c> | <c>Administrador General</c> | <c>Dependencia / Cliente</c>.
        /// </summary>
        public string RolNombre { get; set; } = string.Empty;

        /// <summary>Tipo de evento. Ej: subdominio_asignado | evidencias_aprobadas.</summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>ID de la solicitud relacionada (opcional).</summary>
        public long? SolicitudId { get; set; }

        /// <summary>Título corto visible en la campana de notificaciones.</summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>Cuerpo completo del mensaje.</summary>
        public string Mensaje { get; set; } = string.Empty;
    }
}
