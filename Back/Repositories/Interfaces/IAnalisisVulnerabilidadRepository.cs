using SolicitudServidores.Models;

namespace SolicitudServidores.Repositories.Interfaces
{
    public interface IAnalisisVulnerabilidadRepository
    {
        Task<List<AnalisisVulnerabilidades>> GetBySolicitudId(long solicitudId);
        Task<AnalisisVulnerabilidades?> GetById(long analisisId);
        Task<AnalisisVulnerabilidades?> GetUltimaPorSolicitud(long solicitudId);
        Task<AnalisisVulnerabilidades> Create(AnalisisVulnerabilidades analisis);
        Task<AnalisisVulnerabilidades?> Update(AnalisisVulnerabilidades analisis);
        Task<int> GetNextRonda(long solicitudId);
    }
}
