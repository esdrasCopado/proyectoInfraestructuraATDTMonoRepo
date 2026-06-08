namespace SolicitudServidores.DTOs
{
    /// <summary>Evidencia individual rechazada con su motivo.</summary>
    public class EvidenciaRechazadaItem
    {
        /// <summary>ID de la evidencia rechazada.</summary>
        /// <example>3</example>
        public long Id { get; set; }

        /// <summary>Motivo por el que se rechaza esta evidencia.</summary>
        /// <example>El archivo está ilegible.</example>
        public string Motivo { get; set; } = string.Empty;
    }

    /// <summary>Payload para rechazar la validación de evidencias (etapa 12 → 11).</summary>
    public class RechazarEvidenciasRequest
    {
        /// <summary>Lista de evidencias rechazadas con su motivo individual.</summary>
        public List<EvidenciaRechazadaItem> Evidencias { get; set; } = new();

        /// <summary>Observación general que se guarda en la etapa 12.</summary>
        /// <example>Se rechaza la ronda por documentación incompleta.</example>
        public string? ObservacionEtapa { get; set; }

        /// <summary>ID del administrador que realiza el rechazo.</summary>
        /// <example>2</example>
        public long? RechazadoBy { get; set; }
    }

    public class ValidarEvidenciaRequest
    {
        // aprobada | rechazada
        public string EstadoValidacion { get; set; } = string.Empty;
        public string? MotivoRechazo { get; set; }
    }

    public class EvidenciaResponseDto
    {
        public long Id { get; set; }
        public long SolicitudId { get; set; }
        public string Proposito { get; set; } = string.Empty;
        public int Ronda { get; set; }
        public string ArchivoNombre { get; set; } = string.Empty;
        public string ArchivoPath { get; set; } = string.Empty;
        public int? ArchivoSizeKb { get; set; }
        public string EstadoValidacion { get; set; } = string.Empty;
        public string? MotivoRechazo { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public UsuarioResumenDto? SubidoPor { get; set; }
        public UsuarioResumenDto? ValidadoPor { get; set; }
    }

    public class UsuarioResumenDto
    {
        public long Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Cargo { get; set; }
    }
}
