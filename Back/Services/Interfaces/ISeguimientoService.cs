using SolicitudServidores.DTOs;
using SolicitudServidores.Models;

namespace SolicitudServidores.Services.Interfaces
{
    public interface ISeguimientoService
    {
        Task<IEnumerable<Seguimiento>> GetBySolicitudAsync(long solicitudId);
        Task<Seguimiento?> GetEtapaAsync(long solicitudId, int etapaNumero);
        Task<int?> GetEtapaActualAsync(long solicitudId);
        Task<List<Seguimiento>> InicializarEtapasAsync(long solicitudId);
        Task<Seguimiento?> AvanzarEtapaAsync(long solicitudId, int etapaNumero, AvanzarEtapaRequest request);
        Task<Seguimiento?> RegresarEtapaAsync(long solicitudId, int etapaDestino, RegresarEtapaRequest request);
        Task<Seguimiento?> RechazarValidacionEvidenciasAsync(long solicitudId, RechazarEvidenciasRequest request);
    }
}
