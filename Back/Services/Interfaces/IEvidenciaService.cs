using SolicitudServidores.DTOs;
using SolicitudServidores.Models;

namespace SolicitudServidores.Services.Interfaces
{
    public interface IEvidenciaService
    {
        Task<EvidenciaResponseDto> SubirAsync(long solicitudId, string proposito, IFormFile archivo, long userId);
        Task<IEnumerable<EvidenciaResponseDto>> GetBySolicitudAsync(long solicitudId);
        Task<EvidenciaResponseDto?> GetByIdAsync(long id);
        Task<EvidenciaResponseDto?> ValidarAsync(long id, string estado, string? motivo, long validadoPor);
        Task<Evidencia?> EliminarAsync(long id);
        Task<int> EliminarPorSolicitudYPropositoAsync(long solicitudId, string proposito);
        Task<int> RechazarEvidenciasAsync(List<EvidenciaRechazadaItem> items, long rechazadoBy);
        Task<IEnumerable<EvidenciaResponseDto>> GetAllBySolicitudAsync(long solicitudId);
    }
}
