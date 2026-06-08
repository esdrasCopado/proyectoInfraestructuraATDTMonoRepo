using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Repositories.Implementaciones
{
    public class SolicitudRepository : ISolicitudRepository
    {
        private readonly DataContext _context;

        public SolicitudRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Solicitud>> GetAll()
        {
            return await QueryBase()
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Solicitud>> GetAllPaged(int page, int size)
        {
            return await QueryBase()
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<Solicitud?> GetById(long id)
        {
            return await QueryBase().FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Solicitud?> GetByFolio(string folio)
        {
            return await QueryBase().FirstOrDefaultAsync(s => s.Folio == folio);
        }

        public async Task<IEnumerable<Solicitud>> GetByDependency(int dependencyId)
        {
            return await QueryBase()
                .Where(s => s.DependencyId == dependencyId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Solicitud>> GetByEstatus(string estatus)
        {
            return await QueryBase()
                .Where(s => s.Estatus == estatus)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Solicitud>> GetByCreatedBy(long userId)
        {
            return await QueryBase()
                .Where(s => s.CreatedBy == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public Task<bool> ExistsFolio(string folio)
        {
            return _context.Solicitudes.AnyAsync(s => s.Folio == folio && s.DeletedAt == null);
        }

        public async Task<Solicitud> Create(Solicitud solicitud)
        {
            await _context.Solicitudes.AddAsync(solicitud);
            await _context.SaveChangesAsync();
            return await GetById(solicitud.Id) ?? solicitud;
        }

        public async Task<Solicitud?> Update(Solicitud solicitud)
        {
            var existente = await _context.Solicitudes
                .FirstOrDefaultAsync(s => s.Id == solicitud.Id && s.DeletedAt == null);

            if (existente == null) return null;

            existente.AdminContactId            = solicitud.AdminContactId;
            existente.DescripcionUso            = solicitud.DescripcionUso;
            existente.NombreServidor            = solicitud.NombreServidor;
            existente.NombreAplicacion          = solicitud.NombreAplicacion;
            existente.TipoUso                   = solicitud.TipoUso;
            existente.FechaArranqueDeseada      = solicitud.FechaArranqueDeseada;
            existente.VigenciaMeses             = solicitud.VigenciaMeses;
            existente.CaracteristicasEspeciales = solicitud.CaracteristicasEspeciales;
            existente.TipoRequerimiento         = solicitud.TipoRequerimiento;
            existente.EsClonacion               = solicitud.EsClonacion;
            existente.IpServidorBase            = solicitud.IpServidorBase;
            existente.NombreServidorBase         = solicitud.NombreServidorBase;
            existente.SistemaOperativo          = solicitud.SistemaOperativo;
            existente.RamSolicitadaGb           = solicitud.RamSolicitadaGb;
            existente.VcpuSolicitado            = solicitud.VcpuSolicitado;
            existente.AlmacenamientoSolicitadoGb = solicitud.AlmacenamientoSolicitadoGb;
            existente.MotorBaseDatos            = solicitud.MotorBaseDatos;
            existente.ReglasFirewall            = solicitud.ReglasFirewall;
            existente.IntegracionesExternas     = solicitud.IntegracionesExternas;
            existente.ConectividadOtras         = solicitud.ConectividadOtras;
            existente.Estatus                   = solicitud.Estatus;
            existente.UpdatedBy                 = solicitud.UpdatedBy;
            existente.UpdatedAt                 = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetById(existente.Id);
        }

        public async Task<Solicitud?> AsignarServidor(long solicitudId, long serverId)
        {
            var solicitud = await _context.Solicitudes
                .FirstOrDefaultAsync(s => s.Id == solicitudId && s.DeletedAt == null);

            if (solicitud == null) return null;

            solicitud.ServerId  = serverId;
            solicitud.Estatus   = "aprovisionado";
            solicitud.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetById(solicitudId);
        }

        public async Task<Solicitud?> SoftDelete(long id)
        {
            var solicitud = await _context.Solicitudes
                .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null);

            if (solicitud == null) return null;

            solicitud.DeletedAt = DateTime.UtcNow;
            solicitud.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return solicitud;
        }

        private IQueryable<Solicitud> QueryBase()
        {
            return _context.Solicitudes
                .Where(s => s.DeletedAt == null)
                .Include(s => s.Dependency)
                .Include(s => s.AdminContact)
                .Include(s => s.CreadoPor)
                .Include(s => s.Servidor)
                    .ThenInclude(srv => srv!.ServerVpns)
                        .ThenInclude(sv => sv.Vpn)
                .Include(s => s.Servidor)
                    .ThenInclude(srv => srv!.ServerSubdominios)
                        .ThenInclude(ss => ss.Subdominio)
                .Include(s => s.Servidor)
                    .ThenInclude(srv => srv!.Storages)
                .Include(s => s.Carta)
                .Include(s => s.Seguimientos.OrderBy(seg => seg.EtapaNumero));
        }
    }
}
