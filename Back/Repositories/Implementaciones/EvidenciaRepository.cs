using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Repositories.Implementaciones
{
    public class EvidenciaRepository : IEvidenciaRepository
    {
        private readonly DataContext _context;

        public EvidenciaRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Evidencia> Create(Evidencia evidencia)
        {
            await _context.Evidencias.AddAsync(evidencia);
            await _context.SaveChangesAsync();
            return await GetById(evidencia.Id) ?? evidencia;
        }

        public async Task<IEnumerable<Evidencia>> GetBySolicitud(long solicitudId)
        {
            return await _context.Evidencias
                .Include(e => e.SubidoPor)
                .Include(e => e.ValidadoPor)
                .Where(e => e.SolicitudId == solicitudId && e.DeletedAt == null)
                .OrderBy(e => e.Proposito)
                .ThenBy(e => e.Ronda)
                .ToListAsync();
        }

        public async Task<Evidencia?> GetById(long id)
        {
            return await _context.Evidencias
                .Include(e => e.SubidoPor)
                .Include(e => e.ValidadoPor)
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);
        }

        public Task<int> ContarPorPropositoYSolicitud(long solicitudId, string proposito)
        {
            return _context.Evidencias
                .CountAsync(e => e.SolicitudId == solicitudId
                              && e.Proposito == proposito
                              && e.DeletedAt == null);
        }

        public async Task<Evidencia?> Validar(long id, string estado, string? motivo, long validadoPor)
        {
            var evidencia = await _context.Evidencias
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);

            if (evidencia == null) return null;

            evidencia.EstadoValidacion = estado;
            evidencia.MotivoRechazo    = estado == "rechazada" ? motivo : null;
            evidencia.ValidatedBy      = validadoPor;
            evidencia.ValidatedAt      = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetById(id);
        }

        public async Task<Evidencia?> SoftDelete(long id)
        {
            var evidencia = await _context.Evidencias
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);

            if (evidencia == null) return null;

            evidencia.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return evidencia;
        }

        public async Task<IEnumerable<Evidencia>> EliminarPorSolicitudYProposito(long solicitudId, string proposito)
        {
            var evidencias = await _context.Evidencias
                .Where(e => e.SolicitudId == solicitudId && e.Proposito == proposito && e.DeletedAt == null)
                .ToListAsync();

            var ahora = DateTime.UtcNow;
            foreach (var e in evidencias)
                e.DeletedAt = ahora;

            await _context.SaveChangesAsync();
            return evidencias;
        }

        public async Task<IEnumerable<Evidencia>> RechazarYEliminar(List<(long Id, string Motivo)> items, long rechazadoBy)
        {
            var ids = items.Select(i => i.Id).ToList();
            var evidencias = await _context.Evidencias
                .Where(e => ids.Contains(e.Id) && e.DeletedAt == null)
                .ToListAsync();

            var ahora = DateTime.UtcNow;
            foreach (var e in evidencias)
            {
                var motivo = items.First(i => i.Id == e.Id).Motivo;
                e.EstadoValidacion = "rechazada";
                e.MotivoRechazo    = motivo;
                e.ValidatedBy      = rechazadoBy;
                e.ValidatedAt      = ahora;
                e.DeletedAt        = ahora;
            }

            await _context.SaveChangesAsync();
            return evidencias;
        }

        public async Task<IEnumerable<Evidencia>> GetAllBySolicitud(long solicitudId)
        {
            return await _context.Evidencias
                .Include(e => e.SubidoPor)
                .Include(e => e.ValidadoPor)
                .Where(e => e.SolicitudId == solicitudId)
                .OrderBy(e => e.Proposito)
                .ThenBy(e => e.Ronda)
                .ToListAsync();
        }
    }
}
