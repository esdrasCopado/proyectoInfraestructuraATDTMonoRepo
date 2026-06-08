using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Repositories.Implementaciones
{
    public class CartaRepository : ICartaRepository
    {
        private readonly DataContext _context;

        public CartaRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Carta>> GetAll()
        {
            return await QueryBase()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Carta?> GetById(long id)
        {
            return await QueryBase().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Carta?> GetBySolicitudId(long solicitudId)
        {
            return await QueryBase().FirstOrDefaultAsync(c => c.SolicitudId == solicitudId);
        }

        public async Task<Carta> Create(Carta carta)
        {
            await _context.Cartas.AddAsync(carta);
            await _context.SaveChangesAsync();
            return await GetById(carta.Id) ?? carta;
        }

        public async Task<Carta?> Update(Carta carta)
        {
            var existente = await _context.Cartas.FirstOrDefaultAsync(c => c.Id == carta.Id);
            if (existente == null) return null;

            existente.FolioCarta                    = carta.FolioCarta;
            existente.FirmanteDependenciaNombre     = carta.FirmanteDependenciaNombre;
            existente.FirmanteDependenciaPuesto     = carta.FirmanteDependenciaPuesto;
            existente.FirmanteDependenciaEmpleado   = carta.FirmanteDependenciaEmpleado;
            existente.FirmaDependenciaAt            = carta.FirmaDependenciaAt;
            existente.FirmanteAtdtNombre            = carta.FirmanteAtdtNombre;
            existente.FirmanteAtdtPuesto            = carta.FirmanteAtdtPuesto;
            existente.FirmaAtdtAt                   = carta.FirmaAtdtAt;
            existente.PdfPath                       = carta.PdfPath;
            existente.PdfGeneratedAt                = carta.PdfGeneratedAt;
            existente.UpdatedAt                     = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetById(existente.Id);
        }

        private IQueryable<Carta> QueryBase()
        {
            return _context.Cartas
                .Include(c => c.Solicitud)
                    .ThenInclude(s => s!.Dependency)
                .Include(c => c.CreadoPor);
        }
    }
}
