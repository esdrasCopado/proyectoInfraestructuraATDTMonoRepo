using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.Back.DTOs;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;
using System.Text;

namespace SolicitudServidores.Controllers
{
    [ApiController]
    [Route("api/servidor")]
    public class ServidorController : ControllerBase
    {
        private readonly IServidorRepository _repo;

        public ServidorController(IServidorRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repo.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var servidor = await _repo.GetById(id);
            if (servidor == null) return NotFound();
            return Ok(servidor);
        }

        [HttpGet("solicitud/{idSolicitud}")]
        public async Task<IActionResult> GetBySolicitud(long idSolicitud)
        {
            return Ok(await _repo.GetBySolicitud(idSolicitud));
        }

        [HttpGet("dashboard/resumen")]
        public async Task<IActionResult> GetDashboardResumen()
        {
            var servidores = await _repo.GetAll();
            var resumen = new
            {
                total                  = servidores.Count,
                comunicacionesValidadas = servidores.Count(s => s.ComunicacionValidada),
                parchesAplicados       = servidores.Count(s => s.ParchesAplicados),
                xdrInstalado           = servidores.Count(s => s.XdrInstalado),
                credencialesEntregadas = servidores.Count(s => s.CredencialesEntregadas),
                wafConfigurado         = servidores.Count(s => s.WafConfigurado),
                evidenciasValidadas    = servidores.Count(s => s.EvidenciaValidada),
                publicados             = servidores.Count(s => s.SolicitudPublicacion),
                porEtapa               = servidores
                    .GroupBy(s => string.IsNullOrWhiteSpace(s.EtapaOperativa) ? "Sin etapa" : s.EtapaOperativa)
                    .Select(g => new { etapa = g.Key, total = g.Count() })
                    .OrderByDescending(g => g.total),
                porVulnerabilidad      = servidores
                    .GroupBy(s => string.IsNullOrWhiteSpace(s.EtapaVulnerabilidades) ? "Sin etapa" : s.EtapaVulnerabilidades)
                    .Select(g => new { etapa = g.Key, total = g.Count() })
                    .OrderByDescending(g => g.total)
            };

            return Ok(resumen);
        }

        [HttpGet("recursos-predeterminados")]
        public IActionResult GetRecursosPredeterminados()
        {
            var recursos = new List<RecursoServidorPredeterminadoDTO>
            {
                new() { Nombre = "Básico",    Nucleos = 2, Ram = 4,  Almacenamiento = 80,  Descripcion = "Aplicaciones ligeras o ambientes de pruebas" },
                new() { Nombre = "Estándar",  Nucleos = 4, Ram = 8,  Almacenamiento = 160, Descripcion = "Servicios internos y apps administrativas" },
                new() { Nombre = "Avanzado",  Nucleos = 8, Ram = 16, Almacenamiento = 320, Descripcion = "Cargas publicadas o servicios críticos" }
            };

            return Ok(recursos);
        }

        [HttpGet("reportes/vulnerabilidades-pendientes")]
        public async Task<IActionResult> GetVulnerabilidadesPendientes()
        {
            var servidores = await _repo.GetAll();
            var pendientes = servidores.Where(s =>
                !string.IsNullOrWhiteSpace(s.EtapaVulnerabilidades) &&
                !string.Equals(s.EtapaVulnerabilidades, "Completado", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(s.EtapaVulnerabilidades, "Cerrado",    StringComparison.OrdinalIgnoreCase));

            return Ok(pendientes);
        }

        [HttpGet("reportes/revision-anual")]
        public async Task<IActionResult> GetPendientesRevisionAnual()
        {
            var limite = DateTime.UtcNow.Date.AddYears(-1);
            var servidores = await _repo.GetAll();
            var pendientes = servidores.Where(s =>
                s.RequiereRevisionAnual &&
                (!s.UltimaRevisionAnual.HasValue || s.UltimaRevisionAnual.Value.Date <= limite));

            return Ok(pendientes);
        }

        [HttpGet("reportes/vpns-activas")]
        public async Task<IActionResult> GetVpnsActivas()
        {
            var servidores = await _repo.GetAll();
            var result = servidores
                .Where(s => s.ServerVpns.Any())
                .Select(s => new
                {
                    Servidor = new { s.Id, s.Hostname, s.Ip },
                    VPNs     = s.ServerVpns.Select(sv => new
                    {
                        sv.Vpn.VpnId,
                        sv.Vpn.VpnType,
                        sv.Vpn.Responsable,
                        sv.Vpn.Email,
                        sv.Vpn.VpnIp
                    })
                });

            return Ok(result);
        }

        [HttpGet("reportes/detallado")]
        public async Task<IActionResult> GetReporteDetallado(
            [FromQuery] string? estado,
            [FromQuery] string? etapa,
            [FromQuery] string? tipoUso,
            [FromQuery] string? buscar,
            [FromQuery] bool soloPendientes = false)
        {
            var servidores = AplicarFiltrosReporte(await _repo.GetAll(), estado, etapa, tipoUso, buscar, soloPendientes)
                .Select(s => new
                {
                    s.Id,
                    s.Hostname,
                    s.Estado,
                    s.TipoUso,
                    s.EtapaOperativa,
                    s.EtapaVulnerabilidades,
                    s.ResponsableInfraestructura,
                    s.Ip,
                    s.ComunicacionValidada,
                    s.ParchesAplicados,
                    s.XdrInstalado,
                    s.CredencialesEntregadas,
                    s.WafConfigurado,
                    s.EvidenciaValidada,
                    s.SolicitudPublicacion,
                    Solicitud = s.Solicitud == null ? null : new
                    {
                        s.Solicitud.Id,
                        s.Solicitud.Folio,
                        s.Solicitud.NombreServidor,
                        s.Solicitud.Estatus
                    }
                });

            return Ok(servidores);
        }

        [HttpGet("reportes/exportar-csv")]
        public async Task<IActionResult> ExportarReporteCsv(
            [FromQuery] string? estado,
            [FromQuery] string? etapa,
            [FromQuery] string? tipoUso,
            [FromQuery] string? buscar,
            [FromQuery] bool soloPendientes = false)
        {
            var servidores = AplicarFiltrosReporte(await _repo.GetAll(), estado, etapa, tipoUso, buscar, soloPendientes).ToList();
            var csv = new StringBuilder();
            csv.AppendLine("Id,Folio,NombreServidor,Hostname,Estado,TipoUso,EtapaOperativa,EtapaVulnerabilidades,ResponsableInfraestructura,IP,ComunicacionValidada,ParchesAplicados,XdrInstalado,CredencialesEntregadas,WafConfigurado,EvidenciaValidada,SolicitudPublicacion");

            foreach (var servidor in servidores)
            {
                csv.AppendLine(string.Join(",", new[]
                {
                    servidor.Id.ToString(),
                    EscapeCsv(servidor.Solicitud?.Folio),
                    EscapeCsv(servidor.Solicitud?.NombreServidor),
                    EscapeCsv(servidor.Hostname),
                    EscapeCsv(servidor.Estado),
                    EscapeCsv(servidor.TipoUso),
                    EscapeCsv(servidor.EtapaOperativa),
                    EscapeCsv(servidor.EtapaVulnerabilidades),
                    EscapeCsv(servidor.ResponsableInfraestructura),
                    EscapeCsv(servidor.Ip),
                    servidor.ComunicacionValidada ? "Sí" : "No",
                    servidor.ParchesAplicados     ? "Sí" : "No",
                    servidor.XdrInstalado         ? "Sí" : "No",
                    servidor.CredencialesEntregadas ? "Sí" : "No",
                    servidor.WafConfigurado        ? "Sí" : "No",
                    servidor.EvidenciaValidada     ? "Sí" : "No",
                    servidor.SolicitudPublicacion  ? "Sí" : "No"
                }));
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv; charset=utf-8", $"reporte-servidores-{DateTime.UtcNow:yyyyMMddHHmm}.csv");
        }

        /// <summary>Devuelve el servidor cuya IP coincide exactamente.</summary>
        [HttpGet("ip/{ip}")]
        public async Task<IActionResult> GetByIp(string ip)
        {
            var servidor = await _repo.GetByIp(ip);
            if (servidor == null) return NotFound(new { error = $"No existe un servidor con IP '{ip}'." });
            return Ok(servidor);
        }

        /// <summary>
        /// Actualiza el status de todos los subdominios asociados al servidor con la IP indicada.
        /// Valores válidos de status: solicitado | aprobado | activo | expirado | revocado.
        /// </summary>
        [HttpPatch("ip/{ip}/subdominios/status")]
        public async Task<IActionResult> ActualizarStatusSubdominiosPorIp(string ip, [FromBody] ActualizarStatusSubdominioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(new { error = "El campo 'status' es requerido." });

            var servidor = await _repo.GetByIp(ip);
            if (servidor == null) return NotFound(new { error = $"No existe un servidor con IP '{ip}'." });

            var subdominios = await _repo.UpdateSubdominiosStatusByIp(ip, request.Status.Trim());

            if (subdominios.Count == 0)
                return Ok(new { message = "El servidor no tiene subdominios asociados.", actualizados = 0 });

            return Ok(new
            {
                message = $"{subdominios.Count} subdominio(s) actualizado(s).",
                actualizados = subdominios.Count,
                subdominios = subdominios.Select(s => new
                {
                    s.SubdominioId,
                    s.RequestedName,
                    s.ApprovedName,
                    s.Status,
                    s.UpdatedAt
                })
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServidorRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Hostname))
                return BadRequest("El hostname del servidor es requerido.");

            var servidor = MapServidor(request);
            var creado = await _repo.Create(servidor);
            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateServidorRequest request)
        {
            var existente = await _repo.GetById(id);
            if (existente == null) return NotFound();

            ApplyServidorChanges(existente, request);
            var actualizado = await _repo.Update(existente);
            return Ok(actualizado);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var eliminado = await _repo.Delete(id);
            if (eliminado == null) return NotFound();
            return Ok(eliminado);
        }

        private static Servidor MapServidor(CreateServidorRequest request)
        {
            var servidor = new Servidor();
            ApplyServidorChanges(servidor, request);
            return servidor;
        }

        private static void ApplyServidorChanges(Servidor servidor, CreateServidorRequest request)
        {
            if (request.DependencyId.HasValue)
                servidor.DependencyId = request.DependencyId.Value;

            servidor.Estado = string.IsNullOrWhiteSpace(request.Estado)
                ? (string.IsNullOrWhiteSpace(servidor.Estado) ? "Pendiente" : servidor.Estado)
                : request.Estado!;

            servidor.Expiracion = request.Expiracion ?? servidor.Expiracion;

            if (request.Hostname != null)
                servidor.Hostname = request.Hostname.Trim();

            if (request.Ip != null)
                servidor.Ip = string.IsNullOrWhiteSpace(request.Ip) ? null : request.Ip.Trim();

            if (request.TipoUso != null)
                servidor.TipoUso = string.IsNullOrWhiteSpace(request.TipoUso) ? "Interno" : request.TipoUso!;

            if (request.Funcion != null)
                servidor.Funcion = request.Funcion.Trim();

            if (request.SistemaOperativo != null)
                servidor.SistemaOperativo = request.SistemaOperativo.Trim();

            if (request.RequiereLlaveLicencia.HasValue)
                servidor.RequiereLlaveLicencia = request.RequiereLlaveLicencia.Value;

            if (request.LlaveOS != null)
                servidor.LlaveOS = string.IsNullOrWhiteSpace(request.LlaveOS) ? null : request.LlaveOS.Trim();

            servidor.Nucleos        = request.Nucleos        ?? (servidor.Nucleos        == 0 ? 2   : servidor.Nucleos);
            servidor.Ram            = request.Ram            ?? (servidor.Ram            == 0 ? 8   : servidor.Ram);
            servidor.Almacenamiento = request.Almacenamiento ?? (servidor.Almacenamiento == 0 ? 100 : servidor.Almacenamiento);
            servidor.Descripcion    = request.Descripcion    ?? servidor.Descripcion;

            if (request.PlantillaRecursos != null)
                servidor.PlantillaRecursos = string.IsNullOrWhiteSpace(request.PlantillaRecursos) ? "General" : request.PlantillaRecursos!;

            if (request.EtapaOperativa != null)
                servidor.EtapaOperativa = string.IsNullOrWhiteSpace(request.EtapaOperativa) ? "Provisionamiento" : request.EtapaOperativa!;

            if (request.ResponsableInfraestructura != null)
                servidor.ResponsableInfraestructura = request.ResponsableInfraestructura;

            servidor.UsuarioUltimaActualizacion = request.UsuarioUltimaActualizacion
                ?? request.ResponsableInfraestructura
                ?? servidor.UsuarioUltimaActualizacion;

            servidor.FechaUltimaActualizacion = request.FechaUltimaActualizacion ?? DateTime.UtcNow;

            if (request.FechaAsignacionIp.HasValue)
                servidor.FechaAsignacionIp = request.FechaAsignacionIp;

            if (request.TareasPendientes != null)
                servidor.TareasPendientes = request.TareasPendientes;

            if (request.ObservacionesSeguimiento != null)
                servidor.ObservacionesSeguimiento = request.ObservacionesSeguimiento;

            if (request.EtapaVulnerabilidades != null)
                servidor.EtapaVulnerabilidades = request.EtapaVulnerabilidades;

            if (request.RequiereRevisionAnual.HasValue)
                servidor.RequiereRevisionAnual = request.RequiereRevisionAnual.Value;

            if (request.UltimaRevisionAnual.HasValue)
                servidor.UltimaRevisionAnual = request.UltimaRevisionAnual;

            if (request.ComunicacionValidada.HasValue)
                servidor.ComunicacionValidada = request.ComunicacionValidada.Value;

            if (request.FechaValidacionComunicacion.HasValue)
                servidor.FechaValidacionComunicacion = request.FechaValidacionComunicacion;

            if (request.UsuarioValidacionComunicacion != null)
                servidor.UsuarioValidacionComunicacion = request.UsuarioValidacionComunicacion;

            if (request.ParchesAplicados.HasValue)
                servidor.ParchesAplicados = request.ParchesAplicados.Value;

            if (request.FechaParches.HasValue)
                servidor.FechaParches = request.FechaParches;

            if (request.UsuarioParches != null)
                servidor.UsuarioParches = request.UsuarioParches;

            if (request.XdrInstalado.HasValue)
                servidor.XdrInstalado = request.XdrInstalado.Value;

            if (request.FechaXdr.HasValue)
                servidor.FechaXdr = request.FechaXdr;

            if (request.UsuarioXdr != null)
                servidor.UsuarioXdr = request.UsuarioXdr;

            if (request.CredencialesEntregadas.HasValue)
                servidor.CredencialesEntregadas = request.CredencialesEntregadas.Value;

            if (request.FechaEntregaCredenciales.HasValue)
                servidor.FechaEntregaCredenciales = request.FechaEntregaCredenciales;

            if (request.UsuarioCredenciales != null)
                servidor.UsuarioCredenciales = request.UsuarioCredenciales;

            if (request.WafConfigurado.HasValue)
                servidor.WafConfigurado = request.WafConfigurado.Value;

            if (request.FechaConfiguracionWaf.HasValue)
                servidor.FechaConfiguracionWaf = request.FechaConfiguracionWaf;

            if (request.UsuarioWaf != null)
                servidor.UsuarioWaf = request.UsuarioWaf;

            if (request.EvidenciaValidada.HasValue)
                servidor.EvidenciaValidada = request.EvidenciaValidada.Value;

            if (request.FechaValidacionEvidencia.HasValue)
                servidor.FechaValidacionEvidencia = request.FechaValidacionEvidencia;

            if (request.UsuarioValidacionEvidencia != null)
                servidor.UsuarioValidacionEvidencia = request.UsuarioValidacionEvidencia;

            if (request.SolicitudPublicacion.HasValue)
                servidor.SolicitudPublicacion = request.SolicitudPublicacion.Value;

            if (request.FechaPublicacion.HasValue)
                servidor.FechaPublicacion = request.FechaPublicacion;

            if (request.UsuarioPublicacion != null)
                servidor.UsuarioPublicacion = request.UsuarioPublicacion;

            if (request.FechaVulnerabilidades.HasValue)
                servidor.FechaVulnerabilidades = request.FechaVulnerabilidades;

            if (request.UsuarioVulnerabilidades != null)
                servidor.UsuarioVulnerabilidades = request.UsuarioVulnerabilidades;
        }

        private static IEnumerable<Servidor> AplicarFiltrosReporte(
            IEnumerable<Servidor> servidores,
            string? estado, string? etapa, string? tipoUso, string? buscar, bool soloPendientes)
        {
            var query = servidores.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(s => string.Equals(s.Estado, estado, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(etapa))
                query = query.Where(s =>
                    string.Equals(s.EtapaOperativa,       etapa, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.EtapaVulnerabilidades, etapa, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(tipoUso))
                query = query.Where(s => string.Equals(s.TipoUso, tipoUso, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(s =>
                    (s.Hostname?.Contains(buscar, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.Funcion?.Contains(buscar, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.Solicitud?.NombreServidor?.Contains(buscar, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.Solicitud?.Folio?.Contains(buscar, StringComparison.OrdinalIgnoreCase) ?? false));

            if (soloPendientes)
                query = query.Where(s =>
                    !s.ComunicacionValidada ||
                    !s.ParchesAplicados     ||
                    !s.XdrInstalado         ||
                    !s.CredencialesEntregadas ||
                    !s.WafConfigurado       ||
                    !s.EvidenciaValidada    ||
                    (!string.IsNullOrWhiteSpace(s.EtapaVulnerabilidades) &&
                        !string.Equals(s.EtapaVulnerabilidades, "Completado", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(s.EtapaVulnerabilidades, "Cerrado",    StringComparison.OrdinalIgnoreCase)));

            return query.OrderBy(s => s.Hostname).ThenBy(s => s.Id);
        }

        private static string EscapeCsv(string? value)
        {
            var sanitized = (value ?? string.Empty).Replace("\"", "\"\"");
            return $"\"{sanitized}\"";
        }
    }
}
