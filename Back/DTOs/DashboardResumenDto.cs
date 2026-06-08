namespace SolicitudServidores.DTOs
{
    public class DashboardResumenDto
    {
        public int Total { get; set; }
        public int Nuevas { get; set; }
        public int Pendientes { get; set; }
        public int EnProceso { get; set; }
        public int Terminadas { get; set; }
        public IEnumerable<EtapaCountDto> PorEtapa { get; set; } = new List<EtapaCountDto>();
        public IEnumerable<PrioridadCountDto> PorPrioridad { get; set; } = new List<PrioridadCountDto>();
    }

    public class EtapaCountDto
    {
        public string Etapa { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class PrioridadCountDto
    {
        public string Prioridad { get; set; } = string.Empty;
        public int Total { get; set; }
    }
}
