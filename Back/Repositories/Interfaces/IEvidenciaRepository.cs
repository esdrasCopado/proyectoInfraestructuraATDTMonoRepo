using SolicitudServidores.Models;

namespace SolicitudServidores.Repositories.Interfaces
{
    public interface IEvidenciaRepository
    {
        Task<Evidencia> Create(Evidencia evidencia);
        Task<IEnumerable<Evidencia>> GetBySolicitud(long solicitudId);
        Task<Evidencia?> GetById(long id);
        Task<int> ContarPorPropositoYSolicitud(long solicitudId, string proposito);
        Task<Evidencia?> Validar(long id, string estado, string? motivo, long validadoPor);
        Task<Evidencia?> SoftDelete(long id);
        Task<IEnumerable<Evidencia>> EliminarPorSolicitudYProposito(long solicitudId, string proposito);
        Task<IEnumerable<Evidencia>> RechazarYEliminar(List<(long Id, string Motivo)> items, long rechazadoBy);
        Task<IEnumerable<Evidencia>> GetAllBySolicitud(long solicitudId);
    }
}
