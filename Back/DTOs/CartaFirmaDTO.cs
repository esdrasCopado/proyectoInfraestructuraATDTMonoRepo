namespace SolicitudServidores.DTOs
{
    public class FirmaDependenciaRequest
    {
        public string FirmanteNombre { get; set; } = string.Empty;
        public string FirmantePuesto { get; set; } = string.Empty;
        public string FirmanteNumEmpleado { get; set; } = string.Empty;
    }

    public class FirmaAtdtRequest
    {
        // Opcionales: si se omiten se usan los valores por defecto del modelo
        public string? FirmanteNombre { get; set; }
        public string? FirmantePuesto { get; set; }
    }
}
