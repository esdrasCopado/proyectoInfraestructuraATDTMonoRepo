namespace SolicitudServidores.DTOs
{
    public class CreateVpnRequest
    {
        // "dependencia" | "proveedor"
        public string VpnType { get; set; } = string.Empty;
        public string Responsable { get; set; } = string.Empty;
        public string? Cargo { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? VpnIp { get; set; }
        public string? ExternalId { get; set; }

        // Solo si VpnType = "proveedor"
        public string? Empresa { get; set; }
        // 30, 60 o 90
        public int? VigenciaDias { get; set; }
        public string? PerfilAnterior { get; set; }

        // Opcional: asignar al servidor al momento de crear
        public long? ServerId { get; set; }
    }

    public class ActualizarFolioVpnRequest
    {
        public string Folio { get; set; } = string.Empty;
    }

    public class UpdateVpnRequest
    {
        public string? VpnType { get; set; }
        public string? Responsable { get; set; }
        public string? Cargo { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? VpnIp { get; set; }
        public string? ExternalId { get; set; }
        public string? Empresa { get; set; }
        public int? VigenciaDias { get; set; }
        public string? PerfilAnterior { get; set; }
        public string? Estado { get; set; }
    }

    public class ActualizarEstadoVpnRequest
    {
        public string Estado { get; set; } = string.Empty;
    }
}
