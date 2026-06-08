using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Repositories.Implementaciones
{
    public class SeguimientoRepository : ISeguimientoRepository
    {
        private readonly DataContext _context;

        private static readonly (int Numero, string Nombre)[] EtapasCanónicas =
        {
            (1,  "carta_responsiva"),
            (2,  "validacion_recursos"),
            (3,  "creacion_servidor"),
            (4,  "comunicaciones"),
            (5,  "parches"),
            (6,  "xdr_agente"),
            (7,  "vpn"),
            (8,  "subdominio"),
            (9,  "credenciales"),
            (10, "waf"),
            (11, "evidencias"),
            (12, "validacion_evidencias"),
            (13, "solicitud_publicacion"),
            (14, "vulnerabilidades"),
        };

        public SeguimientoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Seguimiento>> GetBySolicitudId(long solicitudId)
        {
            return await _context.Seguimientos
                .Include(s => s.CompletadoPor)
                .Where(s => s.SolicitudId == solicitudId)
                .OrderBy(s => s.EtapaNumero)
                .ToListAsync();
        }

        public async Task<Seguimiento?> GetByEtapa(long solicitudId, int etapaNumero)
        {
            return await _context.Seguimientos
                .Include(s => s.CompletadoPor)
                .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId && s.EtapaNumero == etapaNumero);
        }

        public async Task<int?> GetEtapaActual(long solicitudId)
        {
            var enProceso = await _context.Seguimientos
                .Where(s => s.SolicitudId == solicitudId && s.Status == "en_proceso")
                .OrderBy(s => s.EtapaNumero)
                .FirstOrDefaultAsync();

            if (enProceso != null) return enProceso.EtapaNumero;

            var pendiente = await _context.Seguimientos
                .Where(s => s.SolicitudId == solicitudId && s.Status == "pendiente")
                .OrderBy(s => s.EtapaNumero)
                .FirstOrDefaultAsync();

            if (pendiente != null) return pendiente.EtapaNumero;

            return -1;
        }

        public async Task<Seguimiento?> GetById(long id)
        {
            return await _context.Seguimientos
                .Include(s => s.CompletadoPor)
                .FirstOrDefaultAsync(s => s.SeguimientoId == id);
        }

        public async Task<List<Seguimiento>> InicializarEtapas(long solicitudId)
        {
            var etapas = EtapasCanónicas.Select(e => new Seguimiento
            {
                SolicitudId  = solicitudId,
                EtapaNumero  = e.Numero,
                EtapaNombre  = e.Nombre,
                Status       = "pendiente",
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow,
            }).ToList();

            await _context.Seguimientos.AddRangeAsync(etapas);
            await _context.SaveChangesAsync();
            return etapas;
        }

        public async Task ActualizarEtapaActualSolicitud(long solicitudId, int etapaActual)
        {
            var solicitud = await _context.Solicitudes
                .FirstOrDefaultAsync(s => s.Id == solicitudId);

            if (solicitud == null) return;

            solicitud.EtapaActual = etapaActual;
            solicitud.UpdatedAt   = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<Seguimiento?> RegresarAEtapa(long solicitudId, int etapaDestino, string? motivo, long? rechazadoBy)
        {
            var etapasAResetear = await _context.Seguimientos
                .Where(s => s.SolicitudId == solicitudId && s.EtapaNumero >= etapaDestino)
                .ToListAsync();

            if (!etapasAResetear.Any()) return null;

            foreach (var etapa in etapasAResetear)
            {
                etapa.Status          = "pendiente";
                etapa.FechaInicio     = null;
                etapa.FechaCompletado = null;
                etapa.CompletadoBy    = null;
                etapa.UpdatedAt       = DateTime.UtcNow;
            }

            var etapaDestinada = etapasAResetear.First(e => e.EtapaNumero == etapaDestino);
            etapaDestinada.Observaciones = motivo ?? etapaDestinada.Observaciones;
            etapaDestinada.CompletadoBy  = rechazadoBy;

            await _context.SaveChangesAsync();

            await ActualizarEtapaActualSolicitud(solicitudId, etapaDestino);

            return await GetByEtapa(solicitudId, etapaDestino);
        }

        public async Task<Seguimiento?> Update(Seguimiento seguimiento)
        {
            var existente = await _context.Seguimientos
                .FirstOrDefaultAsync(s => s.SeguimientoId == seguimiento.SeguimientoId);

            if (existente == null) return null;

            existente.Status           = seguimiento.Status;
            existente.FechaInicio      = seguimiento.FechaInicio;
            existente.FechaCompletado  = seguimiento.FechaCompletado;
            existente.CompletadoBy     = seguimiento.CompletadoBy;
            existente.Observaciones    = seguimiento.Observaciones;
            existente.UpdatedAt        = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetById(existente.SeguimientoId);
        }
    }
}
