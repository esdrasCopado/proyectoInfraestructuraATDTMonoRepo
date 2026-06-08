using SolicitudServidores.DTOs;
using SolicitudServidores.Models;

namespace SolicitudServidores.Services.Interfaces
{
    public interface ISolicitudService
    {
        Task<IEnumerable<Solicitud>> GetAllAsync(int pagina = 0, int cantidad = 20);
        Task<Solicitud?> GetByIdAsync(long id);
        Task<Solicitud?> GetByFolioAsync(string folio);
        Task<IEnumerable<Solicitud>> GetByDependencyAsync(int dependencyId);
        Task<IEnumerable<Solicitud>> GetByEstatusAsync(string estatus);
        Task<IEnumerable<Solicitud>> GetByCreatedByAsync(long userId);
        Task<SolicitudDashboardDto> GetDashboardAsync();
        Task<SolicitudDashboardDto> GetDashboardByUserAsync(long userId);
        Task<Solicitud> CreateAsync(CreateSolicitudRequest request, long createdBy);
        Task<Solicitud> CrearCompletaAsync(CreateSolicitudCompletaRequest request, long createdBy);
        Task<Solicitud?> UpdateAsync(long id, UpdateSolicitudRequest request, long updatedBy);
        Task<Solicitud?> ActualizarEstatusAsync(long id, ActualizarEstatusRequest request, long updatedBy);
        Task<Solicitud?> AsignarServidorAsync(long solicitudId, long serverId, long updatedBy);
        Task<Solicitud?> SoftDeleteAsync(long id);
    }
}
