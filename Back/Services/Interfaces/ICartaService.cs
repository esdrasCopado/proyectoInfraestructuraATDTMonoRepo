using SolicitudServidores.DTOs;
using SolicitudServidores.Models;

namespace SolicitudServidores.Services.Interfaces
{
    public interface ICartaService
    {
        Task<IEnumerable<Carta>> GetAllAsync();
        Task<Carta?> GetByIdAsync(long id);
        Task<Carta?> GetBySolicitudAsync(long solicitudId);
        Task<Carta> CreateAsync(CartaRequestDTO dto, long createdBy);
        Task<Carta?> FirmarDependenciaAsync(long id, FirmaDependenciaRequest request);
        Task<Carta?> FirmarAtdtAsync(long id, FirmaAtdtRequest request);
        Task<byte[]?> GenerarPdfAsync(long id);
    }
}
