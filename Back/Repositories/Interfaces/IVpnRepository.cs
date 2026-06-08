using SolicitudServidores.Models;

namespace SolicitudServidores.Repositories.Interfaces
{
    public interface IVpnRepository
    {
        Task<IEnumerable<VPN>> GetAll();
        Task<VPN?> GetById(int id);
        Task<IEnumerable<VPN>> GetByServerId(long serverId);
        Task<IEnumerable<VPN>> GetByFolio(string folio);
        Task<IEnumerable<VPN>> GetByUsuario(long userId);
        Task<IEnumerable<VPN>> Buscar(string? folio, long? userId);
        Task<VPN?> ActualizarFolio(int vpnId, string folio);
        Task<VPN> Create(VPN vpn);
        Task<VPN?> Update(VPN vpn);
        Task<VPN?> Delete(int id);
        Task<ServerVpn> AsignarAServidor(int vpnId, long serverId);
        Task<bool> DesasignarDeServidor(int vpnId, long serverId);
        Task<bool> ExisteAsignacion(int vpnId, long serverId);
    }
}
