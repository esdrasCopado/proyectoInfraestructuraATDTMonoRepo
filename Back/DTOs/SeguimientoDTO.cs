namespace SolicitudServidores.DTOs
{
    public class AvanzarEtapaRequest
    {
        // en_proceso | completado | rechazado
        public string Status { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public long? CompletadoBy { get; set; }
    }

    /// <summary>Payload para regresar una solicitud a una etapa anterior por rechazo.</summary>
    public class RegresarEtapaRequest
    {
        /// <summary>Motivo del rechazo o descripción del problema encontrado.</summary>
        /// <example>Recursos de RAM insuficientes para el ambiente requerido.</example>
        public string? Motivo { get; set; }

        /// <summary>ID del usuario administrador que ejecuta el rechazo.</summary>
        /// <example>42</example>
        public long? RechazadoBy { get; set; }
    }
}
