using SolicitudServidores.DTOs;
using SolicitudServidores.Models;

namespace SolicitudServidores.Services.Interfaces
{
    public interface IVpnService
    {
        Task<IEnumerable<VPN>> GetAllAsync();
        Task<IEnumerable<VPN>> BuscarAsync(string? folio, long? userId);
        Task<VPN?> GetByIdAsync(int id);
        Task<IEnumerable<VPN>> GetByServidorAsync(long serverId);
        Task<IEnumerable<VPN>> GetByFolioAsync(string folio);
        Task<IEnumerable<VPN>> GetByUsuarioAsync(long userId);
        Task<VPN?> ActualizarFolioAsync(int vpnId, string folio);
        Task<VPN> CreateAsync(CreateVpnRequest request);
        Task<VPN?> UpdateAsync(int id, UpdateVpnRequest request);
        Task<VPN?> UpdateEstadoAsync(int id, string estado);
        Task<VPN?> DeleteAsync(int id);
        Task<ServerVpn> AsignarAServidorAsync(int vpnId, long serverId);
        Task<bool> DesasignarDeServidorAsync(int vpnId, long serverId);
    }
}
