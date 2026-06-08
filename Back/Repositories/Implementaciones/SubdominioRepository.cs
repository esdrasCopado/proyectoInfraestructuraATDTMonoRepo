using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Repositories.Implementaciones
{
    public class SubdominioRepository : ISubdominioRepository
    {
        private readonly DataContext _context;

        public SubdominioRepository(DataContext context)
        {
            _context = context;
        }

        public Task<Subdominio?> GetById(int id)
            => _context.Subdominios.FirstOrDefaultAsync(s => s.SubdominioId == id);

        public async Task<Subdominio?> UpdateStatus(int id, string status)
        {
            var sub = await _context.Subdominios.FirstOrDefaultAsync(s => s.SubdominioId == id);
            if (sub == null) return null;

            sub.Status = status;
            sub.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return sub;
        }

        public async Task<List<Subdominio>> UpdateStatusBatch(IEnumerable<int> ids, string status)
        {
            var idList = ids.Distinct().ToList();
            var subdominios = await _context.Subdominios
                .Where(s => idList.Contains(s.SubdominioId))
                .ToListAsync();

            foreach (var sub in subdominios)
            {
                sub.Status = status;
                sub.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return subdominios;
        }
    }
}
