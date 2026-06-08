using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Repositories.Implementaciones
{
    public class AnalisisVulnerabilidadRepository : IAnalisisVulnerabilidadRepository
    {
        private readonly DataContext _context;

        public AnalisisVulnerabilidadRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<AnalisisVulnerabilidades>> GetBySolicitudId(long solicitudId)
        {
            return await _context.AnalisisVulnerabilidades
                .Where(a => a.SolicitudId == solicitudId)
                .OrderBy(a => a.Ronda)
                .ToListAsync();
        }

        public async Task<AnalisisVulnerabilidades?> GetById(long analisisId)
        {
            return await _context.AnalisisVulnerabilidades
                .Include(a => a.Solicitud)
                .Include(a => a.SolicitadoPor)
                .Include(a => a.AnalizadoPor)
                .FirstOrDefaultAsync(a => a.AnalisisId == analisisId);
        }

        public async Task<AnalisisVulnerabilidades?> GetUltimaPorSolicitud(long solicitudId)
        {
            return await _context.AnalisisVulnerabilidades
                .Include(a => a.Solicitud)
                .Include(a => a.SolicitadoPor)
                .Include(a => a.AnalizadoPor)
                .Where(a => a.SolicitudId == solicitudId)
                .OrderByDescending(a => a.Ronda)
                .FirstOrDefaultAsync();
        }

        public async Task<AnalisisVulnerabilidades> Create(AnalisisVulnerabilidades analisis)
        {
            await _context.AnalisisVulnerabilidades.AddAsync(analisis);
            await _context.SaveChangesAsync();
            return (await GetById(analisis.AnalisisId))!;
        }

        public async Task<AnalisisVulnerabilidades?> Update(AnalisisVulnerabilidades analisis)
        {
            var model = await _context.AnalisisVulnerabilidades
                .FirstOrDefaultAsync(a => a.AnalisisId == analisis.AnalisisId);
            if (model == null) return null;

            model.Estado           = analisis.Estado;
            model.Hallazgos        = analisis.Hallazgos;
            model.Recomendaciones  = analisis.Recomendaciones;
            model.AnalyzedBy       = analisis.AnalyzedBy;
            model.AnalyzedAt       = analisis.AnalyzedAt;
            model.PublicadoAt      = analisis.PublicadoAt;
            model.PublicadoBy      = analisis.PublicadoBy;
            model.UpdatedAt        = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetById(model.AnalisisId);
        }

        public async Task<int> GetNextRonda(long solicitudId)
        {
            var max = await _context.AnalisisVulnerabilidades
                .Where(a => a.SolicitudId == solicitudId)
                .MaxAsync(a => (int?)a.Ronda);
            return (max ?? 0) + 1;
        }
    }
}
