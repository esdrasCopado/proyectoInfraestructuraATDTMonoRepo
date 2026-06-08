using SolicitudServidores.Models;

namespace SolicitudServidores.Repositories.Interfaces
{
    public interface ISolicitudRepository
    {
        Task<IEnumerable<Solicitud>> GetAll();
        Task<IEnumerable<Solicitud>> GetAllPaged(int page, int size);
        Task<Solicitud?> GetById(long id);
        Task<Solicitud?> GetByFolio(string folio);
        Task<IEnumerable<Solicitud>> GetByDependency(int dependencyId);
        Task<IEnumerable<Solicitud>> GetByEstatus(string estatus);
        Task<IEnumerable<Solicitud>> GetByCreatedBy(long userId);
        Task<bool> ExistsFolio(string folio);
        Task<Solicitud> Create(Solicitud solicitud);
        Task<Solicitud?> Update(Solicitud solicitud);
        Task<Solicitud?> AsignarServidor(long solicitudId, long serverId);
        Task<Solicitud?> SoftDelete(long id);
    }
}
