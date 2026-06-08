using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Repositories.Implementaciones
{
    public class ServidorRepository : IServidorRepository
    {
        private readonly DataContext _context;

        public ServidorRepository(DataContext context)
        {
            _context = context;
        }

        private IQueryable<Servidor> BuscarTodo()
        {
            return _context.Servidores
                .Include(s => s.Solicitud)
                .Include(s => s.Dependency)
                .Include(s => s.ServerVpns).ThenInclude(sv => sv.Vpn)
                .Include(s => s.ServerSubdominios).ThenInclude(ss => ss.Subdominio)
                .Include(s => s.Storages)
                .Include(s => s.WafConfig);
        }

        public async Task<Servidor> Create(Servidor servidor)
        {
            await _context.Servidores.AddAsync(servidor);
            await _context.SaveChangesAsync();
            return await GetById(servidor.Id) ?? servidor;
        }

        public async Task<Servidor?> Update(Servidor servidor)
        {
            _context.ChangeTracker.Clear();

            var servidorModel = await _context.Servidores
                .FirstOrDefaultAsync(s => s.Id == servidor.Id);

            if (servidorModel == null)
                return null;

            servidorModel.DependencyId                  = servidor.DependencyId;
            servidorModel.Estado                        = servidor.Estado;
            servidorModel.Expiracion                    = servidor.Expiracion;
            servidorModel.Hostname                      = servidor.Hostname;
            servidorModel.Ip                            = servidor.Ip;
            servidorModel.TipoUso                       = servidor.TipoUso;
            servidorModel.Funcion                       = servidor.Funcion;
            servidorModel.SistemaOperativo              = servidor.SistemaOperativo;
            servidorModel.RequiereLlaveLicencia         = servidor.RequiereLlaveLicencia;
            servidorModel.LlaveOS                       = servidor.LlaveOS;
            servidorModel.Nucleos                       = servidor.Nucleos;
            servidorModel.Ram                           = servidor.Ram;
            servidorModel.Almacenamiento                = servidor.Almacenamiento;
            servidorModel.Descripcion                   = servidor.Descripcion;
            servidorModel.PlantillaRecursos             = servidor.PlantillaRecursos;
            servidorModel.EtapaOperativa                = servidor.EtapaOperativa;
            servidorModel.ResponsableInfraestructura    = servidor.ResponsableInfraestructura;
            servidorModel.UsuarioUltimaActualizacion    = servidor.UsuarioUltimaActualizacion;
            servidorModel.FechaUltimaActualizacion      = servidor.FechaUltimaActualizacion;
            servidorModel.FechaAsignacionIp             = servidor.FechaAsignacionIp;
            servidorModel.TareasPendientes              = servidor.TareasPendientes;
            servidorModel.ObservacionesSeguimiento      = servidor.ObservacionesSeguimiento;
            servidorModel.EtapaVulnerabilidades         = servidor.EtapaVulnerabilidades;
            servidorModel.RequiereRevisionAnual         = servidor.RequiereRevisionAnual;
            servidorModel.UltimaRevisionAnual           = servidor.UltimaRevisionAnual;
            servidorModel.ComunicacionValidada          = servidor.ComunicacionValidada;
            servidorModel.FechaValidacionComunicacion   = servidor.FechaValidacionComunicacion;
            servidorModel.UsuarioValidacionComunicacion = servidor.UsuarioValidacionComunicacion;
            servidorModel.ParchesAplicados              = servidor.ParchesAplicados;
            servidorModel.FechaParches                  = servidor.FechaParches;
            servidorModel.UsuarioParches                = servidor.UsuarioParches;
            servidorModel.XdrInstalado                  = servidor.XdrInstalado;
            servidorModel.FechaXdr                      = servidor.FechaXdr;
            servidorModel.UsuarioXdr                    = servidor.UsuarioXdr;
            servidorModel.CredencialesEntregadas        = servidor.CredencialesEntregadas;
            servidorModel.FechaEntregaCredenciales      = servidor.FechaEntregaCredenciales;
            servidorModel.UsuarioCredenciales           = servidor.UsuarioCredenciales;
            servidorModel.WafConfigurado                = servidor.WafConfigurado;
            servidorModel.FechaConfiguracionWaf         = servidor.FechaConfiguracionWaf;
            servidorModel.UsuarioWaf                    = servidor.UsuarioWaf;
            servidorModel.EvidenciaValidada             = servidor.EvidenciaValidada;
            servidorModel.FechaValidacionEvidencia      = servidor.FechaValidacionEvidencia;
            servidorModel.UsuarioValidacionEvidencia    = servidor.UsuarioValidacionEvidencia;
            servidorModel.SolicitudPublicacion          = servidor.SolicitudPublicacion;
            servidorModel.FechaPublicacion              = servidor.FechaPublicacion;
            servidorModel.UsuarioPublicacion            = servidor.UsuarioPublicacion;
            servidorModel.FechaVulnerabilidades         = servidor.FechaVulnerabilidades;
            servidorModel.UsuarioVulnerabilidades       = servidor.UsuarioVulnerabilidades;

            await _context.SaveChangesAsync();
            return await GetById(servidorModel.Id);
        }

        public async Task<Servidor?> Delete(long id)
        {
            var servidorModel = await BuscarTodo().FirstOrDefaultAsync(s => s.Id == id);
            if (servidorModel == null)
                return null;

            _context.Servidores.Remove(servidorModel);
            await _context.SaveChangesAsync();
            return servidorModel;
        }

        public Task<Servidor?> GetById(long id)
        {
            return BuscarTodo().FirstOrDefaultAsync(s => s.Id == id);
        }

        public Task<List<Servidor>> GetAll()
        {
            return BuscarTodo().OrderBy(s => s.Id).ToListAsync();
        }

        public Task<List<Servidor>> GetBySolicitud(long idSolicitud)
        {
            return BuscarTodo()
                .Where(s => s.Solicitud != null && s.Solicitud.Id == idSolicitud)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public Task<bool> ExistsServidor(long id)
        {
            return _context.Servidores.AnyAsync(s => s.Id == id);
        }

        public Task<Servidor?> GetByIp(string ip)
        {
            return BuscarTodo().FirstOrDefaultAsync(s => s.Ip == ip);
        }

        public async Task<List<Subdominio>> UpdateSubdominiosStatusByIp(string ip, string nuevoStatus)
        {
            var servidor = await _context.Servidores
                .Include(s => s.ServerSubdominios)
                    .ThenInclude(ss => ss.Subdominio)
                .FirstOrDefaultAsync(s => s.Ip == ip);

            if (servidor == null)
                return new List<Subdominio>();

            var subdominios = servidor.ServerSubdominios
                .Select(ss => ss.Subdominio)
                .Where(sub => sub != null)
                .Cast<Subdominio>()
                .ToList();

            foreach (var sub in subdominios)
            {
                sub.Status = nuevoStatus;
                sub.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return subdominios;
        }
    }
}
