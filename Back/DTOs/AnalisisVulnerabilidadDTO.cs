namespace SolicitudServidores.DTOs
{
    /// <summary>Datos de un análisis de vulnerabilidades.</summary>
    public class AnalisisVulnerabilidadDTO
    {
        /// <summary>Identificador del análisis.</summary>
        public long AnalisisId { get; set; }

        /// <summary>ID de la solicitud relacionada.</summary>
        public long SolicitudId { get; set; }

        /// <summary>Folio de la solicitud relacionada.</summary>
        public string? Folio { get; set; }

        /// <summary>Número de ronda (1 = primera vez, N = re-análisis tras solventación).</summary>
        public int Ronda { get; set; }

        /// <summary>Estado del análisis: pendiente | aprobado | rechazado.</summary>
        public string Estado { get; set; } = "pendiente";

        /// <summary>Hallazgos registrados por el Administrador de Vulnerabilidades.</summary>
        public string? Hallazgos { get; set; }

        /// <summary>Recomendaciones emitidas.</summary>
        public string? Recomendaciones { get; set; }

        /// <summary>Nombre del usuario que solicitó la publicación.</summary>
        public string? SolicitadoPorNombre { get; set; }

        /// <summary>Fecha en que se solicitó la publicación (etapa 13 completada).</summary>
        public DateTime? SolicitudPublicacionAt { get; set; }

        /// <summary>Nombre del administrador que realizó el análisis.</summary>
        public string? AnalizadoPorNombre { get; set; }

        /// <summary>Fecha en que se completó el análisis.</summary>
        public DateTime? AnalyzedAt { get; set; }

        /// <summary>Fecha de publicación efectiva (tras aprobación).</summary>
        public DateTime? PublicadoAt { get; set; }

        /// <summary>Fecha de creación del registro.</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Datos para registrar el resultado de un análisis de vulnerabilidades.</summary>
    public class SubmitAnalisisRequest
    {
        /// <summary>
        /// Resultado del análisis. Valores permitidos: <c>aprobado</c> | <c>rechazado</c>.
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Descripción de los hallazgos encontrados. Requerido si se rechaza.</summary>
        public string? Hallazgos { get; set; }

        /// <summary>Recomendaciones para el equipo de dependencia o infraestructura.</summary>
        public string? Recomendaciones { get; set; }

        /// <summary>ID del administrador que realiza el análisis.</summary>
        public long? AnalyzedBy { get; set; }
    }
}
