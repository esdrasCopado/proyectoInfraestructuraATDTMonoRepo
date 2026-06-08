using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Repositories.Implementaciones
{
    public class VpnRepository : IVpnRepository
    {
        private readonly DataContext _context;

        public VpnRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VPN>> GetAll()
        {
            return await QueryBase().ToListAsync();
        }

        public async Task<VPN?> GetById(int id)
        {
            return await QueryBase().FirstOrDefaultAsync(v => v.VpnId == id);
        }

        public async Task<IEnumerable<VPN>> GetByServerId(long serverId)
        {
            return await QueryBase()
                .Where(v => v.ServerVpns.Any(sv => sv.ServerId == serverId))
                .ToListAsync();
        }

        public async Task<IEnumerable<VPN>> GetByFolio(string folio)
        {
            var term = folio.Trim().ToLower();

            // Paso 1: obtener los IDs de servidor cuyo folio coincide
            var serverIds = await _context.Solicitudes
                .Where(sol => sol.ServerId != null && sol.Folio.ToLower().Contains(term))
                .Select(sol => sol.ServerId!.Value)
                .ToListAsync();

            if (serverIds.Count == 0)
                return Enumerable.Empty<VPN>();

            // Paso 2: todos los VPNs asignados a esos servidores (puede haber varios por servidor)
            return await QueryBase()
                .Where(v => v.ServerVpns.Any(sv => serverIds.Contains(sv.ServerId)))
                .ToListAsync();
        }

        public async Task<IEnumerable<VPN>> GetByUsuario(long userId)
        {
            var serverIds = await _context.Solicitudes
                .Where(sol => sol.ServerId != null && sol.CreatedBy == userId)
                .Select(sol => sol.ServerId!.Value)
                .ToListAsync();

            if (serverIds.Count == 0)
                return Enumerable.Empty<VPN>();

            return await QueryBase()
                .Where(v => v.ServerVpns.Any(sv => serverIds.Contains(sv.ServerId)))
                .ToListAsync();
        }

        public async Task<IEnumerable<VPN>> Buscar(string? folio, long? userId)
        {
            var query = QueryBase();

            // Filtro por usuario: solo VPNs de servidores de sus propias solicitudes
            if (userId.HasValue)
            {
                var serverIds = await _context.Solicitudes
                    .Where(sol => sol.ServerId != null && sol.CreatedBy == userId.Value)
                    .Select(sol => sol.ServerId!.Value)
                    .ToListAsync();

                query = query.Where(v => v.ServerVpns.Any(sv => serverIds.Contains(sv.ServerId)));
            }

            // Filtro parcial por folio de la VPN
            if (!string.IsNullOrWhiteSpace(folio))
            {
                var term = folio.Trim().ToLower();
                query = query.Where(v => v.Folio != null && v.Folio.ToLower().Contains(term));
            }

            return await query.ToListAsync();
        }

        public async Task<VPN?> ActualizarFolio(int vpnId, string folio)
        {
            var vpn = await _context.VPNs.FirstOrDefaultAsync(v => v.VpnId == vpnId);
            if (vpn == null) return null;

            vpn.Folio     = folio.Trim();
            vpn.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetById(vpnId);
        }

        public async Task<VPN> Create(VPN vpn)
        {
            await _context.VPNs.AddAsync(vpn);
            await _context.SaveChangesAsync();
            return await GetById(vpn.VpnId) ?? vpn;
        }

        public async Task<VPN?> Update(VPN vpn)
        {
            var existente = await _context.VPNs.FirstOrDefaultAsync(v => v.VpnId == vpn.VpnId);
            if (existente == null) return null;

            existente.VpnType       = vpn.VpnType;
            existente.Responsable   = vpn.Responsable;
            existente.Cargo         = vpn.Cargo;
            existente.Phone         = vpn.Phone;
            existente.Email         = vpn.Email;
            existente.VpnIp         = vpn.VpnIp;
            existente.ExternalId    = vpn.ExternalId;
            existente.Empresa       = vpn.Empresa;
            existente.VigenciaDias  = vpn.VigenciaDias;
            existente.PerfilAnterior = vpn.PerfilAnterior;
            existente.Estado        = vpn.Estado;
            existente.UpdatedAt     = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetById(existente.VpnId);
        }

        public async Task<VPN?> Delete(int id)
        {
            var vpn = await QueryBase().FirstOrDefaultAsync(v => v.VpnId == id);
            if (vpn == null) return null;

            _context.VPNs.Remove(vpn);
            await _context.SaveChangesAsync();
            return vpn;
        }

        public async Task<ServerVpn> AsignarAServidor(int vpnId, long serverId)
        {
            var asignacion = new ServerVpn
            {
                VpnId      = vpnId,
                ServerId   = serverId,
                AssignedAt = DateTime.UtcNow,
            };
            await _context.ServerVpns.AddAsync(asignacion);
            await _context.SaveChangesAsync();
            return asignacion;
        }

        public async Task<bool> DesasignarDeServidor(int vpnId, long serverId)
        {
            var asignacion = await _context.ServerVpns
                .FirstOrDefaultAsync(sv => sv.VpnId == vpnId && sv.ServerId == serverId);

            if (asignacion == null) return false;

            _context.ServerVpns.Remove(asignacion);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> ExisteAsignacion(int vpnId, long serverId)
        {
            return _context.ServerVpns.AnyAsync(sv => sv.VpnId == vpnId && sv.ServerId == serverId);
        }

        private IQueryable<VPN> QueryBase()
        {
            return _context.VPNs
                .Include(v => v.ServerVpns)
                    .ThenInclude(sv => sv.Servidor);
        }
    }
}
