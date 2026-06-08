namespace SolicitudServidores.DTOs
{
    public class CreateSolicitudRequest
    {
        // ── Vínculos ──────────────────────────────────────────────────────────
        public int DependencyId { get; set; }
        public int? AdminContactId { get; set; }

        // ── Identificación (Apartado 2.1) ─────────────────────────────────────
        public string DescripcionUso { get; set; } = string.Empty;
        public string NombreServidor { get; set; } = string.Empty;
        public string? NombreAplicacion { get; set; }
        // interno | publicado
        public string TipoUso { get; set; } = "interno";

        // ── Planificación (Apartado 2.2) ──────────────────────────────────────
        public DateTime? FechaArranqueDeseada { get; set; }
        public int VigenciaMeses { get; set; } = 12;
        public string? CaracteristicasEspeciales { get; set; }

        // ── Requerimiento técnico (Apartado 3.1/3.2) ─────────────────────────
        // estandar | especifico
        public string TipoRequerimiento { get; set; } = "estandar";
        public bool EsClonacion { get; set; } = false;
        public string? IpServidorBase { get; set; }
        public string? NombreServidorBase { get; set; }
        public string? SistemaOperativo { get; set; }
        public int RamSolicitadaGb { get; set; }
        public int VcpuSolicitado { get; set; }
        public int AlmacenamientoSolicitadoGb { get; set; }

        // ── Conectividad (Apartado 3.3) ───────────────────────────────────────
        public string? MotorBaseDatos { get; set; }
        public string? ReglasFirewall { get; set; }
        public string? IntegracionesExternas { get; set; }
        public string? ConectividadOtras { get; set; }
    }

    public class UpdateSolicitudRequest
    {
        public int? AdminContactId { get; set; }
        public string? DescripcionUso { get; set; }
        public string? NombreServidor { get; set; }
        public string? NombreAplicacion { get; set; }
        public string? TipoUso { get; set; }
        public DateTime? FechaArranqueDeseada { get; set; }
        public int? VigenciaMeses { get; set; }
        public string? CaracteristicasEspeciales { get; set; }
        public string? TipoRequerimiento { get; set; }
        public bool? EsClonacion { get; set; }
        public string? IpServidorBase { get; set; }
        public string? NombreServidorBase { get; set; }
        public string? SistemaOperativo { get; set; }
        public int? RamSolicitadaGb { get; set; }
        public int? VcpuSolicitado { get; set; }
        public int? AlmacenamientoSolicitadoGb { get; set; }
        public string? MotorBaseDatos { get; set; }
        public string? ReglasFirewall { get; set; }
        public string? IntegracionesExternas { get; set; }
        public string? ConectividadOtras { get; set; }
    }

    public class ActualizarEstatusRequest
    {
        // pendiente | en_validacion | aprovisionado | en_pruebas | publicado | rechazado | finalizado
        public string Estatus { get; set; } = string.Empty;
    }

    public class SolicitudDashboardDto
    {
        public int Total { get; set; }
        public IEnumerable<SolicitudResumenDto> Solicitudes { get; set; } = [];
    }

    public class SolicitudResumenDto
    {
        public long Id { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Dependencia { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int? EtapaActual { get; set; }
        public string? NombreEtapa { get; set; }
        public DateTime Actualizacion { get; set; }
    }
}
