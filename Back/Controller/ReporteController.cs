using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolicitudServidores.Back.DTOs;
using SolicitudServidores.DBContext;

namespace SolicitudServidores.Controllers
{
    [ApiController]
    [Route("api/reporte")]
    [Authorize(Roles = "Administrador General,Administrador de Centro de Datos,Administrador de Infraestructura,Administrador de Vulnerabilidades")]
    public class ReporteController : ControllerBase
    {
        private readonly DataContext _db;

        public ReporteController(DataContext db)
        {
            _db = db;
        }

        // ── 1. Admin Centro de Datos ──────────────────────────────────────────

        /// <summary>1.1 Solicitudes por dependencia</summary>
        [HttpGet("solicitudes/por-dependencia")]
        [ProducesResponseType(typeof(List<Reporte11ItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SolicitudesPorDependencia(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var query = _db.Solicitudes
                .Include(s => s.Dependency)
                .Include(s => s.CreadoPor)
                .Where(s => s.DeletedAt == null)
                .AsQueryable();

            if (fechaInicio.HasValue) query = query.Where(s => s.CreatedAt >= fechaInicio.Value);
            if (fechaFin.HasValue)    query = query.Where(s => s.CreatedAt <= fechaFin.Value);

            var result = await query
                .OrderBy(s => s.Dependency!.Name)
                .ThenBy(s => s.CreatedAt)
                .Select(s => new Reporte11ItemDto
                {
                    FolioSolicitud      = s.Folio,
                    Sector              = s.Dependency != null ? s.Dependency.Sector : null,
                    Dependencia         = s.Dependency != null ? s.Dependency.Name : string.Empty,
                    Responsable         = s.CreadoPor != null
                                            ? s.CreadoPor.Nombre + " " + s.CreadoPor.Apellidos
                                            : string.Empty,
                    Contacto            = s.CreadoPor != null ? s.CreadoPor.Email : string.Empty,
                    EstatusProcesamieto = s.Estatus,
                    FechaCreacion       = s.CreatedAt,
                })
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>1.2 Recursos solicitados (totalizado)</summary>
        [HttpGet("solicitudes/recursos-solicitados")]
        [ProducesResponseType(typeof(Reporte12ResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecursosSolicitados(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var query = _db.Solicitudes
                .Include(s => s.Dependency)
                .Include(s => s.CreadoPor)
                .Include(s => s.Servidor)
                .Where(s => s.DeletedAt == null && s.Servidor != null)
                .AsQueryable();

            if (fechaInicio.HasValue) query = query.Where(s => s.CreatedAt >= fechaInicio.Value);
            if (fechaFin.HasValue)    query = query.Where(s => s.CreatedAt <= fechaFin.Value);

            var solicitudes = await query.ToListAsync();

            var items = solicitudes.Select(s => new Reporte12ItemDto
            {
                FolioSolicitud        = s.Folio,
                Sector                = s.Dependency?.Sector,
                Dependencia           = s.Dependency?.Name ?? string.Empty,
                Responsable           = s.CreadoPor != null
                                            ? s.CreadoPor.Nombre + " " + s.CreadoPor.Apellidos
                                            : string.Empty,
                Contacto              = s.CreadoPor?.Email ?? string.Empty,
                EstatusProcesamieto   = s.Estatus,
                IpServidor            = s.Servidor!.Ip,
                AdministradorServidor = s.Servidor.ResponsableInfraestructura,
                DescripcionProyecto   = s.DescripcionUso,
                SistemaOperativo      = s.Servidor.SistemaOperativo,
                Vcpu                  = s.Servidor.Nucleos,
                Ram                   = s.Servidor.Ram,
                Almacenamiento        = s.Servidor.Almacenamiento,
                FechaCreacion         = s.CreatedAt,
            }).ToList();

            return Ok(new Reporte12ResponseDto
            {
                Items               = items,
                TotalVcpu           = items.Sum(i => i.Vcpu),
                TotalRam            = items.Sum(i => i.Ram),
                TotalAlmacenamiento = items.Sum(i => i.Almacenamiento),
            });
        }

        /// <summary>1.3 Reporte por IP — sin ?ip= devuelve todos los servidores.</summary>
        [HttpGet("solicitudes/por-ip")]
        [ProducesResponseType(typeof(List<Reporte13ItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReportePorIp([FromQuery] string? ip)
        {
            var query = _db.Servidores
                .Include(s => s.Solicitud)
                    .ThenInclude(sol => sol!.Dependency)
                .Include(s => s.Solicitud)
                    .ThenInclude(sol => sol!.CreadoPor)
                .Include(s => s.ServerSubdominios)
                    .ThenInclude(ss => ss.Subdominio)
                .Include(s => s.ServerVpns)
                    .ThenInclude(sv => sv.Vpn)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(ip))
                query = query.Where(s => s.Ip == ip);

            var servidores = await query.ToListAsync();

            var result = servidores.Select(srv => new Reporte13ItemDto
            {
                FolioSolicitud        = srv.Solicitud?.Folio ?? string.Empty,
                Sector                = srv.Solicitud?.Dependency?.Sector,
                Dependencia           = srv.Solicitud?.Dependency?.Name ?? string.Empty,
                Responsable           = srv.Solicitud?.CreadoPor != null
                                            ? srv.Solicitud.CreadoPor.Nombre + " " + srv.Solicitud.CreadoPor.Apellidos
                                            : string.Empty,
                ContactoResponsable   = srv.Solicitud?.CreadoPor?.Email ?? string.Empty,
                EstatusProcesamieto   = srv.Solicitud?.Estatus ?? string.Empty,
                IpServidor            = srv.Ip,
                AdministradorServidor = srv.ResponsableInfraestructura,
                DescripcionProyecto   = srv.Solicitud?.DescripcionUso,
                SistemaOperativo      = srv.SistemaOperativo,
                Vcpu                  = srv.Nucleos,
                Ram                   = srv.Ram,
                Almacenamiento        = srv.Almacenamiento,
                SubdominiosAprobados  = srv.ServerSubdominios
                    .Where(ss => ss.Subdominio != null && ss.Subdominio.Status == "aprobado")
                    .Select(ss => ss.Subdominio.ApprovedName ?? ss.Subdominio.RequestedName)
                    .ToList(),
                Vpns = srv.ServerVpns
                    .Select(sv => sv.Vpn.Folio ?? sv.Vpn.VpnId.ToString())
                    .ToList(),
            }).ToList();

            return Ok(result);
        }

        // ── 2. Admin Infraestructura ──────────────────────────────────────────

        /// <summary>2.1 Reporte de VPN</summary>
        [HttpGet("infraestructura/vpn")]
        [ProducesResponseType(typeof(List<Reporte21ItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReporteVpn(
            [FromQuery] string?   estado,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var query = _db.VPNs
                .Include(v => v.ServerVpns)
                    .ThenInclude(sv => sv.Servidor)
                        .ThenInclude(s => s!.Solicitud)
                            .ThenInclude(sol => sol!.Dependency)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado)) query = query.Where(v => v.Estado == estado);
            if (fechaInicio.HasValue) query = query.Where(v => v.CreatedAt >= fechaInicio.Value);
            if (fechaFin.HasValue)    query = query.Where(v => v.CreatedAt <= fechaFin.Value);

            var vpns = await query.ToListAsync();

            var result = vpns.Select(v =>
            {
                var servidor  = v.ServerVpns.Select(sv => sv.Servidor).FirstOrDefault();
                var solicitud = servidor?.Solicitud;
                return new Reporte21ItemDto
                {
                    FolioSolicitud      = solicitud?.Folio ?? string.Empty,
                    Sector              = solicitud?.Dependency?.Sector,
                    Dependencia         = solicitud?.Dependency?.Name ?? string.Empty,
                    ResponsableServidor = servidor?.ResponsableInfraestructura,
                    ContactoResponsable = v.Email,
                    EstatusProcesamieto = v.Estado ?? string.Empty,
                    IpServidor          = servidor?.Ip,
                    IdentificadorVpn    = v.Folio,
                    UsuarioAsignado     = v.ExternalId,
                    FechaCreacionVpn    = v.FechaAsignacion,
                    FechaVencimientoVpn = v.FechaExpiracion,
                    Vigencia            = v.VigenciaDias,
                    TipoVpn             = v.VpnType,
                };
            }).ToList();

            return Ok(result);
        }

        /// <summary>2.2 Reporte de subdominios</summary>
        [HttpGet("infraestructura/subdominios")]
        [ProducesResponseType(typeof(List<Reporte22ItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReporteSubdominios(
            [FromQuery] string?   estado,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var query = _db.Subdominios
                .Include(sub => sub.ServerSubdominios)
                    .ThenInclude(ss => ss.Servidor)
                        .ThenInclude(s => s!.Solicitud)
                            .ThenInclude(sol => sol!.Dependency)
                .Include(sub => sub.ServerSubdominios)
                    .ThenInclude(ss => ss.Servidor)
                        .ThenInclude(s => s!.Solicitud)
                            .ThenInclude(sol => sol!.CreadoPor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(sub => sub.Status == estado);
            if (fechaInicio.HasValue)
                query = query.Where(sub => sub.AssignedAt >= fechaInicio.Value);
            if (fechaFin.HasValue)
                query = query.Where(sub => sub.AssignedAt <= fechaFin.Value);

            var subdominios = await query.ToListAsync();

            var result = subdominios.Select(sub =>
            {
                var servidor  = sub.ServerSubdominios.Select(ss => ss.Servidor).FirstOrDefault();
                var solicitud = servidor?.Solicitud;
                return new Reporte22ItemDto
                {
                    FolioSolicitud      = solicitud?.Folio ?? string.Empty,
                    Sector              = solicitud?.Dependency?.Sector,
                    Dependencia         = solicitud?.Dependency?.Name ?? string.Empty,
                    ResponsableServidor = servidor?.ResponsableInfraestructura,
                    Contacto            = solicitud?.CreadoPor?.Email,
                    EstatusProcesamieto = sub.Status,
                    IpServidor          = servidor?.Ip,
                    SubdominioAprobado  = sub.ApprovedName ?? sub.RequestedName,
                    ProxyAsignado       = sub.ApprovedName,
                    TipoDespliegue      = servidor?.TipoUso,
                    Puerto              = sub.Port,
                    FechaAsignacion     = sub.AssignedAt,
                };
            }).ToList();

            return Ok(result);
        }

        // ── 3. Admin Vulnerabilidades ─────────────────────────────────────────

        /// <summary>3.1 Resultados de análisis de vulnerabilidades</summary>
        [HttpGet("seguridad/vulnerabilidades")]
        [ProducesResponseType(typeof(List<Reporte31ItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReporteVulnerabilidades(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var query = _db.AnalisisVulnerabilidades
                .Include(a => a.Solicitud)
                    .ThenInclude(sol => sol!.Dependency)
                .Include(a => a.Solicitud)
                    .ThenInclude(sol => sol!.CreadoPor)
                .Include(a => a.Solicitud)
                    .ThenInclude(sol => sol!.Servidor)
                        .ThenInclude(srv => srv!.ServerSubdominios)
                            .ThenInclude(ss => ss.Subdominio)
                .AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(a => a.SolicitudPublicacionAt >= fechaInicio.Value);
            if (fechaFin.HasValue)
                query = query.Where(a => a.SolicitudPublicacionAt <= fechaFin.Value);

            var analisis = await query
                .OrderByDescending(a => a.SolicitudPublicacionAt)
                .ToListAsync();

            var result = analisis.Select(a =>
            {
                var solicitud = a.Solicitud;
                var servidor  = solicitud?.Servidor;
                return new Reporte31ItemDto
                {
                    FolioSolicitud         = solicitud?.Folio ?? string.Empty,
                    Sector                 = solicitud?.Dependency?.Sector,
                    Dependencia            = solicitud?.Dependency?.Name ?? string.Empty,
                    ResponsableServidor    = servidor?.ResponsableInfraestructura,
                    TelefonoContacto       = solicitud?.CreadoPor?.Phone,
                    EmailContacto          = solicitud?.CreadoPor?.Email,
                    EstatusProcesamieto    = solicitud?.Estatus ?? string.Empty,
                    IpServidor             = servidor?.Ip,
                    SubdominiosAprobados   = servidor?.ServerSubdominios
                        .Where(ss => ss.Subdominio != null && ss.Subdominio.Status == "aprobado")
                        .Select(ss => ss.Subdominio.ApprovedName ?? ss.Subdominio.RequestedName)
                        .ToList() ?? new(),
                    FechaSolicitudAnalisis = a.SolicitudPublicacionAt,
                    FechaAplicacionPrueba  = a.AnalyzedAt,
                    ResultadoPrueba        = a.Estado,
                    Hallazgos              = a.Hallazgos,
                    Ronda                  = a.Ronda,
                };
            }).ToList();

            return Ok(result);
        }

        /// <summary>3.2 Comunicaciones y aplicativos por IP — sin ?ip= devuelve todos.</summary>
        [HttpGet("seguridad/comunicaciones-por-ip")]
        [ProducesResponseType(typeof(List<Reporte32ItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ComunicacionesPorIp([FromQuery] string? ip)
        {
            var query = _db.Servidores
                .Include(s => s.Solicitud)
                    .ThenInclude(sol => sol!.Dependency)
                .Include(s => s.Solicitud)
                    .ThenInclude(sol => sol!.CreadoPor)
                .Include(s => s.ServerSubdominios)
                    .ThenInclude(ss => ss.Subdominio)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(ip))
                query = query.Where(s => s.Ip == ip);

            var servidores = await query.ToListAsync();

            var result = servidores.Select(srv =>
            {
                var solicitud = srv.Solicitud;
                return new Reporte32ItemDto
                {
                    FolioSolicitud        = solicitud?.Folio ?? string.Empty,
                    Sector                = solicitud?.Dependency?.Sector,
                    Dependencia           = solicitud?.Dependency?.Name ?? string.Empty,
                    ResponsableServidor   = srv.ResponsableInfraestructura,
                    ContactoResponsable   = solicitud?.CreadoPor?.Email,
                    EstatusProcesamieto   = solicitud?.Estatus ?? string.Empty,
                    IpServidor            = srv.Ip,
                    SubdominiosAprobados  = srv.ServerSubdominios
                        .Where(ss => ss.Subdominio != null && ss.Subdominio.Status == "aprobado")
                        .Select(ss => ss.Subdominio.ApprovedName ?? ss.Subdominio.RequestedName)
                        .ToList(),
                    TipoDespliegue        = srv.TipoUso,
                    ReglasFirewall        = solicitud?.ReglasFirewall,
                    IntegracionesExternas = solicitud?.IntegracionesExternas,
                    Otras                 = solicitud?.ConectividadOtras,
                };
            }).ToList();

            return Ok(result);
        }

        // ── 4. Admin General ──────────────────────────────────────────────────

        /// <summary>4.1 Estatus de solicitudes (fila por solicitud)</summary>
        [HttpGet("resumen/estatus-solicitudes")]
        [ProducesResponseType(typeof(Reporte41ResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> EstatusSolicitudes(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var query = _db.Solicitudes
                .Include(s => s.Dependency)
                .Include(s => s.CreadoPor)
                .Include(s => s.Servidor)
                .Include(s => s.Seguimientos)
                    .ThenInclude(seg => seg.CompletadoPor)
                .Where(s => s.DeletedAt == null)
                .AsQueryable();

            if (fechaInicio.HasValue) query = query.Where(s => s.CreatedAt >= fechaInicio.Value);
            if (fechaFin.HasValue)    query = query.Where(s => s.CreatedAt <= fechaFin.Value);

            var solicitudes = await query
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var items = solicitudes.Select(s =>
            {
                var etapaActual = s.Seguimientos
                    .Where(seg => seg.EtapaNumero == s.EtapaActual)
                    .FirstOrDefault();

                return new Reporte41ItemDto
                {
                    Sector                  = s.Dependency?.Sector,
                    Dependencia             = s.Dependency?.Name ?? string.Empty,
                    Responsable             = s.CreadoPor != null
                                                ? s.CreadoPor.Nombre + " " + s.CreadoPor.Apellidos
                                                : string.Empty,
                    EmailContacto           = s.CreadoPor?.Email ?? string.Empty,
                    DescripcionServidor     = s.DescripcionUso,
                    Ip                      = s.Servidor?.Ip,
                    FechaSolicitud          = s.CreatedAt,
                    EstatusProcesamieto     = s.Estatus,
                    EtapaActual             = s.EtapaActual,
                    NombreEtapaActual       = etapaActual?.EtapaNombre,
                    FechaProcesamientoEtapa = etapaActual?.FechaInicio,
                    RolResponsableEtapa     = etapaActual?.CompletadoPor != null
                                                ? etapaActual.CompletadoPor.Nombre + " " + etapaActual.CompletadoPor.Apellidos
                                                : null,
                    FechaPublicacion        = s.Servidor?.FechaPublicacion,
                    TipoDespliegue          = s.Servidor?.TipoUso,
                };
            }).ToList();

            return Ok(new Reporte41ResponseDto
            {
                Items            = items,
                TotalSolicitudes = items.Count(),
            });
        }

        /// <summary>
        /// 4.2 Recursos solicitados (totalizado).
        /// Filtros opcionales: ?dependencia=&sistemaOperativo=&ip=
        /// </summary>
        [HttpGet("resumen/recursos-totalizados")]
        [ProducesResponseType(typeof(Reporte42ResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecursosTotalizados(
            [FromQuery] string?   dependencia,
            [FromQuery] string?   sistemaOperativo,
            [FromQuery] string?   ip,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var query = _db.Solicitudes
                .Include(s => s.Dependency)
                .Include(s => s.CreadoPor)
                .Include(s => s.Servidor)
                    .ThenInclude(srv => srv!.ServerSubdominios)
                        .ThenInclude(ss => ss.Subdominio)
                .Include(s => s.Servidor)
                    .ThenInclude(srv => srv!.ServerVpns)
                        .ThenInclude(sv => sv.Vpn)
                .Where(s => s.DeletedAt == null && s.Servidor != null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dependencia))
                query = query.Where(s => s.Dependency != null &&
                    EF.Functions.ILike(s.Dependency.Name, $"%{dependencia}%"));

            if (!string.IsNullOrWhiteSpace(sistemaOperativo))
                query = query.Where(s => s.Servidor != null &&
                    EF.Functions.ILike(s.Servidor.SistemaOperativo, $"%{sistemaOperativo}%"));

            if (!string.IsNullOrWhiteSpace(ip))
                query = query.Where(s => s.Servidor != null && s.Servidor.Ip == ip);

            if (fechaInicio.HasValue) query = query.Where(s => s.CreatedAt >= fechaInicio.Value);
            if (fechaFin.HasValue)    query = query.Where(s => s.CreatedAt <= fechaFin.Value);

            var solicitudes = await query.OrderBy(s => s.Folio).ToListAsync();

            var items = solicitudes.Select(s => new Reporte42ItemDto
            {
                FolioSolicitud        = s.Folio,
                Sector                = s.Dependency?.Sector,
                Dependencia           = s.Dependency?.Name ?? string.Empty,
                Responsable           = s.CreadoPor != null
                                            ? s.CreadoPor.Nombre + " " + s.CreadoPor.Apellidos
                                            : string.Empty,
                Contacto              = s.CreadoPor?.Email ?? string.Empty,
                EstatusProcesamieto   = s.Estatus,
                IpServidor            = s.Servidor!.Ip,
                AdministradorServidor = s.Servidor.ResponsableInfraestructura,
                DescripcionProyecto   = s.DescripcionUso,
                SistemaOperativo      = s.Servidor.SistemaOperativo,
                Vcpu                  = s.Servidor.Nucleos,
                Ram                   = s.Servidor.Ram,
                Almacenamiento        = s.Servidor.Almacenamiento,
                SubdominiosAprobados  = s.Servidor.ServerSubdominios
                    .Where(ss => ss.Subdominio != null && ss.Subdominio.Status == "aprobado")
                    .Select(ss => ss.Subdominio.ApprovedName ?? ss.Subdominio.RequestedName)
                    .ToList(),
                Vpns = s.Servidor.ServerVpns
                    .Select(sv => sv.Vpn.Folio ?? sv.Vpn.VpnId.ToString())
                    .ToList(),
                FechaCreacion = s.CreatedAt,
            }).ToList();

            return Ok(new Reporte42ResponseDto
            {
                Items               = items,
                TotalVcpu           = items.Sum(i => i.Vcpu),
                TotalRam            = items.Sum(i => i.Ram),
                TotalAlmacenamiento = items.Sum(i => i.Almacenamiento),
                TotalServidores     = items.Count,
            });
        }
    }
}
