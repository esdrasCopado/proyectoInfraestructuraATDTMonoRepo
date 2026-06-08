namespace SolicitudServidores.DTOs
{
    /// <summary>Datos de un subdominio tras actualizar su status.</summary>
    public class SubdominioStatusItemDTO
    {
        /// <summary>Identificador único del subdominio.</summary>
        public int SubdominioId { get; set; }

        /// <summary>Nombre solicitado originalmente.</summary>
        public string RequestedName { get; set; } = string.Empty;

        /// <summary>Nombre aprobado (puede ser nulo si aún no se aprueba).</summary>
        public string? ApprovedName { get; set; }

        /// <summary>Status actualizado: solicitado | aprobado | activo | expirado | revocado.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Fecha de la última modificación (UTC).</summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>Respuesta de la operación batch de actualización de status.</summary>
    public class SubdominiosBatchStatusResponseDTO
    {
        /// <summary>Mensaje descriptivo del resultado.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Número de subdominios efectivamente actualizados.</summary>
        public int Actualizados { get; set; }

        /// <summary>Lista de subdominios actualizados.</summary>
        public IEnumerable<SubdominioStatusItemDTO> Subdominios { get; set; } = Enumerable.Empty<SubdominioStatusItemDTO>();
    }
}
