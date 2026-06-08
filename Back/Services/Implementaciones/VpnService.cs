using SolicitudServidores.DTOs;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;
using SolicitudServidores.Services.Interfaces;

namespace SolicitudServidores.Services.Implementaciones
{
    public class VpnService : IVpnService
    {
        private readonly IVpnRepository _repo;

        public VpnService(IVpnRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<VPN>> GetAllAsync() => _repo.GetAll();

        public Task<IEnumerable<VPN>> BuscarAsync(string? folio, long? userId)
            => _repo.Buscar(folio, userId);

        public Task<VPN?> GetByIdAsync(int id) => _repo.GetById(id);

        public Task<IEnumerable<VPN>> GetByServidorAsync(long serverId) => _repo.GetByServerId(serverId);

        public Task<IEnumerable<VPN>> GetByFolioAsync(string folio) => _repo.GetByFolio(folio);

        public Task<IEnumerable<VPN>> GetByUsuarioAsync(long userId) => _repo.GetByUsuario(userId);

        public Task<VPN?> ActualizarFolioAsync(int vpnId, string folio) => _repo.ActualizarFolio(vpnId, folio);

        public async Task<VPN> CreateAsync(CreateVpnRequest request)
        {
            ValidarTipo(request.VpnType);

            var vpn = new VPN
            {
                VpnType        = request.VpnType.Trim(),
                Responsable    = request.Responsable.Trim(),
                Cargo          = request.Cargo?.Trim(),
                Phone          = request.Phone?.Trim(),
                Email          = request.Email?.Trim(),
                VpnIp          = request.VpnIp?.Trim(),
                ExternalId     = request.ExternalId?.Trim(),
                Empresa        = request.Empresa?.Trim(),
                VigenciaDias   = request.VigenciaDias,
                PerfilAnterior = request.PerfilAnterior?.Trim(),
                CreatedAt      = DateTime.UtcNow,
                UpdatedAt      = DateTime.UtcNow,
            };

            var creada = await _repo.Create(vpn);

            if (request.ServerId.HasValue)
                await _repo.AsignarAServidor(creada.VpnId, request.ServerId.Value);

            return await _repo.GetById(creada.VpnId) ?? creada;
        }

        public async Task<VPN?> UpdateAsync(int id, UpdateVpnRequest request)
        {
            var existente = await _repo.GetById(id);
            if (existente == null) return null;

            if (request.VpnType != null)
            {
                ValidarTipo(request.VpnType);
                existente.VpnType = request.VpnType.Trim();
            }

            existente.Responsable    = request.Responsable?.Trim()    ?? existente.Responsable;
            existente.Cargo          = request.Cargo?.Trim()          ?? existente.Cargo;
            existente.Phone          = request.Phone?.Trim()          ?? existente.Phone;
            existente.Email          = request.Email?.Trim()          ?? existente.Email;
            existente.VpnIp          = request.VpnIp?.Trim()          ?? existente.VpnIp;
            existente.ExternalId     = request.ExternalId?.Trim()     ?? existente.ExternalId;
            existente.Empresa        = request.Empresa?.Trim()        ?? existente.Empresa;
            existente.VigenciaDias   = request.VigenciaDias           ?? existente.VigenciaDias;
            existente.PerfilAnterior = request.PerfilAnterior?.Trim() ?? existente.PerfilAnterior;

            if (request.Estado != null)
            {
                if (string.IsNullOrWhiteSpace(request.Estado))
                    throw new ArgumentException("El estado no puede estar vacío.");

                existente.Estado = request.Estado.Trim();
            }

            return await _repo.Update(existente);
        }

        public async Task<VPN?> UpdateEstadoAsync(int id, string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException("El estado no puede estar vacío.");

            var existente = await _repo.GetById(id);
            if (existente == null) return null;

            existente.Estado = estado.Trim();
            return await _repo.Update(existente);
        }

        public async Task<VPN?> DeleteAsync(int id)
        {
            var existente = await _repo.GetById(id);
            if (existente == null) return null;
            return await _repo.Delete(id);
        }

        public async Task<ServerVpn> AsignarAServidorAsync(int vpnId, long serverId)
        {
            if (await _repo.ExisteAsignacion(vpnId, serverId))
                throw new InvalidOperationException("La VPN ya está asignada a ese servidor.");

            return await _repo.AsignarAServidor(vpnId, serverId);
        }

        public Task<bool> DesasignarDeServidorAsync(int vpnId, long serverId)
            => _repo.DesasignarDeServidor(vpnId, serverId);

        private static void ValidarTipo(string tipo)
        {
            if (tipo != "dependencia" && tipo != "proveedor" && tipo != "actualizacion")
                throw new ArgumentException("vpn_type debe ser 'dependencia', 'proveedor' o 'actualizacion'.");
        }
    }
}
